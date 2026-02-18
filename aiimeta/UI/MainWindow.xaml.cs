using Microsoft.Win32;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;

using aiimeta.Formats;
using aiimeta.Reader;

namespace aiimeta.UI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>Format of the title string to be shown on the app title bar.</summary>
        /// <remarks>
        /// <c>{0}</c> is a filename, and <c>{1}</c> is <see cref="OriginalTitle"/>.</remarks>
        private const string TitleFormat = "{0} ― {1}";

        /// <summary>App title as defined in XAML.</summary>
        private string OriginalTitle;

        public MainWindow()
        {
            InitializeComponent();
            OriginalTitle = Title;

            // Creates an ImageFactory instance.
            // Since we don't use a DI framework,
            // we need to keep its subcomponents and dispose them appropriately.
            HttpClient = new HttpClient();
            MetadataReader = new MetadataReader(HttpClient);
            var parser = new AggregateMetadataParser();
            ImageFactory = new ImageFactory(MetadataReader, parser, HttpClient)
            {
                MaxPreviewWidth  = SystemParameters.PrimaryScreenWidth  * 0.5,
                MaxPreviewHeight = SystemParameters.PrimaryScreenHeight * 0.5,
            };
        }

        private HttpClient HttpClient;

        private MetadataReader MetadataReader;

        private ImageFactory ImageFactory;

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // In this version, we do all required initialization in the constructor,
            // and we have nothing to do here.
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            MetadataReader.Dispose();
            HttpClient.Dispose();
        }

        /// <summary>Checks if the current clipboard content is suitable for pasting as an image.</summary>
        /// <remarks>Current version only checks the clipboard data format.</remarks>
        private void Image_Paste_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            var data = Clipboard.GetDataObject();
            if (data is null) return;
            e.CanExecute |=
                data.GetDataPresent(DataFormats.FileDrop) ||
                data.GetDataPresent(CFStr.FILEDESCRIPTOR) ||
                data.GetDataPresent(CFStr.INETURL);
            e.Handled = true;
        }

        /// <summary>Checks if the current drag-and-drop content is suitable for dropping as an image.</summary>
        /// <remarks>Current version only checks the clipboard data format.</remarks>
        private void Window_PreviewDragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop) ||
                e.Data.GetDataPresent(CFStr.FILEDESCRIPTOR) ||
                e.Data.GetDataPresent(CFStr.INETURL))
            {
                e.Effects = DragDropEffects.Copy;
                e.Handled = true;
            }
            // We don't set e.Handled = true in this method
            // so that other controls on this Window can investigate and handle other types of drop request.
            // In this version, it is "filename" TextBox that may handle it.
        }

        private void Window_DragOver(object sender, DragEventArgs e)
        {
            // Invokation of this event handler means
            // no control on this Window wanted to handle this drop request.
            // Tell that fact to the user.
            e.Effects = DragDropEffects.None;
            e.Handled = true;
        }

        /// <summary>Pastes an image file from the clipboard.</summary>
        private async void Image_Paste_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var data = Clipboard.GetDataObject();
            if (data is null) return;
            if (!await LoadDataObjectAsImageAsync(data))
            {
                MessageBox.Show(
                    "Unable to load the pasted file as an image.",
                    OriginalTitle, MessageBoxButton.OK);
            }
            e.Handled = true;
        }

        /// <summary>Receives a file/URL drag-and-drop.</summary>
        /// <remarks>When more than one files are dropped, uses only the first one and ignores the rest.</remarks>
        private async void Window_PreviewDrop(object sender, DragEventArgs e)
        {
            if (!await LoadDataObjectAsImageAsync(e.Data))
            {
                MessageBox.Show(
                    "Unable to load the dropped file as an image.",
                    OriginalTitle, MessageBoxButton.OK);
            }
            e.Handled = true;
        }

        /// <summary>Loads the Clipboard/DragDrop data object as an image.</summary>
        /// <param name="original_data">IDataObject instance likely containing an image.</param>
        /// <returns>True if an image is loaded. False otherwise.</returns>
        /// <remarks>
        /// If the data object content represents a file-like object,
        /// this method tries to decode and load it as an image file.
        /// </remarks>
        private async Task<bool> LoadDataObjectAsImageAsync(IDataObject original_data)
        {
            var data = new OutlookDataObject(original_data);
            if (data.GetData(DataFormats.FileDrop) is string[] paths
                && paths.Length >= 1
                && await LoadImageAsync(paths[0]))
            {
                return true;
            }
            if (data.GetData(CFStr.FILEDESCRIPTOR) is string[] names
                && names.Length >= 1)
            {
                // CFSTR_FILEDESCEIPTOR-based drag-and-drop sends only file names,
                // and directory paths or other information on their locations
                // are unavailable.
                // If CFSTR_INETURL is also present,
                // it is likely that the file is from the internet,
                // and the CFSTR_INETURL content is (by specification) an absolute URL.
                // So, we try to grab the URL and handle it like a full path name.
                var stream = data.GetData(CFStr.FILECONTENTS, 0);
                var full_name = data.GetData(CFStr.INETURL)?.AsString() ?? names[0];
                if (await LoadImageAsync(stream, names[0], full_name))
                {
                    return true; 
                }
            }
            if (data.GetData(CFStr.INETURL)?.AsString() is string url
                && await LoadImageAsync(new Uri(url)))
            {
                return true;
            }
            return false;
        }

        private async void FileOpenButton_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog
            {
                CheckFileExists = true,
                CheckPathExists = true,
                DefaultExt = "png",
                Filter = "Image files|*.png;*.jpg;*.jpeg;*.webp",
                Multiselect = false,
            };
            if (dlg.ShowDialog() == true)
            {
                await LoadImageAsync(dlg.FileName);
            }
        }

        #region Image file loading

        private Task<bool> LoadImageAsync(string path)
        {
            return LoadImageCoreAsync(() => ImageFactory.Create(path));
        }

        private Task<bool> LoadImageAsync(Uri uri)
        {
            return LoadImageCoreAsync(() => ImageFactory.Create(uri));
        }

        private Task<bool> LoadImageAsync(Stream stream, string name, string full_name)
        {
            return LoadImageCoreAsync(() => ImageFactory.Create(stream, name, full_name));
        }

        private async Task<bool> LoadImageCoreAsync(Func<IImageObject> create_image)
        {
            bool result = false;
            Mouse.OverrideCursor = Cursors.Wait;
            IsEnabled = false;
            try
            {
                await LoadImageCoreCoreAsync(create_image);
                result = true;
            }
            catch (Exception)
            {
                // MessageBox.Show(exception.ToString());
            }
            IsEnabled = true;
            Mouse.OverrideCursor = null;
            return result;
        }

        private async Task LoadImageCoreCoreAsync(Func<IImageObject> create_image)
        {
            var image_object = await Task.Run(create_image);
            var metadata = image_object.Metadata;
            var parsed = image_object.ParsedMetadata;

            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.StreamSource = image_object.GetPreviewStream();
            bitmap.EndInit();
            bitmap.Freeze();
            image.Source = bitmap;

            filename.Text = image_object.FullName;
            Title = string.Format(TitleFormat, image_object.Name, OriginalTitle);
            
            parameters.Text    = metadata.Parameters    ?? string.Empty;
            comfyWorkflow.Text = metadata.ComfyWorkflow ?? string.Empty;
            comfyPrompt.Text   = metadata.ComfyPrompt   ?? string.Empty;

            positive.Text = parsed.PositivePromptText ?? string.Empty;
            negative.Text = parsed.NegativePromptText ?? string.Empty;

            metadataList.ItemsSource = parsed.Properties;

            // Avoid showing unused tab item.
            if (metadataArea.SelectedIndex > 0 &&
               (metadataArea.SelectedItem as TabItem)?.Visibility != Visibility.Visible)
            {
                // Tab item at index 0 ("Metadata") is always used and visible.
                metadataArea.SelectedIndex = 0;
            }
        }

        #endregion

        private void MetadataList_Command_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var view = sender as ListView;
            if (view is null) return;

            Func<KeyValuePair<string, string>, string> mapper =
                e.Command == Commands.Copy ? item => item.Value + Environment.NewLine :
                e.Command == Commands.CopyRow ? item => Escape(item.Key) + "\t" + Escape(item.Value) + Environment.NewLine :
                item => throw new NotImplementedException();

            var list = view.SelectedItems.OfType<KeyValuePair<string, string>>().Select(mapper);
            var text = string.Concat(list);
            Clipboard.SetData(DataFormats.UnicodeText, text);
        }

        private static string Escape(string text)
            => text
                .Replace("\\", "\\\\")
                .Replace("\r\n", "\\n")
                .Replace("\n", "\\n")
                .Replace("\r", "\\r")
                .Replace("\t", "\\t");

        private void MetadataList_Command_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = (sender as ListView)?.SelectedIndex >= 0;
        }

        /// <summary>A safe margin when calculating the available width.</summary>
        /// <remarks>This value was decided by trial and error.</remarks>
        private const double ColumnWidthMargin = 2.0;

        /// <summary>The design value of the Width property for the last column in the metadataList.</summary>
        /// <remarks>The value 0.0 is a flag indicating "not initialized yet".</remarks>
        private double InitialColumnWidth = 0.0;

        private void MetadataList_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            // Let the last (i.e., "Value" in our case) column occupy all available space.
            // This code should work fine as long as the user doesn't modify column widths manually.
            // If they did and later resized the UI, the column width could become strange...

            // Get the applicable components.
            var view = (ListView)sender;
            var grid_view = (GridView)view.View;

            // If this is the first SizeChanged event, 
            // get and keep the design value for the Width of the last column,
            // so that we can use it as the minimum column width.
            var column_count = grid_view.Columns.Count;
            if (InitialColumnWidth <= 0)
            {
                InitialColumnWidth = grid_view.Columns[column_count - 1].Width;
            }

            // Calculate the width of the space available for the last column.
            // As we use Fluent UI style,
            // SystemParameters.VerticalScrollBarWidth may be different
            // from the actual width, but it should be a good estimation.
            double available_width = view.ActualWidth 
                - SystemParameters.VerticalScrollBarWidth
                - ColumnWidthMargin;
            for (int i = 0; i < column_count - 1; i++)
            {
                available_width -= grid_view.Columns[i].Width;
            }

            // Set the new width if it is wide enough.
            grid_view.Columns[column_count - 1].Width = Math.Max(available_width, InitialColumnWidth);
        }

        /// <summary>Focuses an UIElement when the mouse button is pressed on it.</summary>
        /// <remarks>
        /// This is a general purpose event handler to focus an UIElement
        /// that usually does not get a keyboard focus.
        /// </remarks>
        private void Any_PreviewMouseDown_ToFocus(object sender, MouseButtonEventArgs e)
        {
            (sender as UIElement)?.Focus();
        }
    }
}
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

        public MainWindow(IImageFactory image_factory)
        {
            InitializeComponent();
            OriginalTitle = Title;
            ImageFactory = image_factory;
        }

        private readonly IImageFactory ImageFactory;

        private void image_DragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop) ||
                e.Data.GetDataPresent(CFStr.FILEDESCRIPTOR) ||
                e.Data.GetDataPresent(CFStr.INETURL))
            {
                e.Effects = DragDropEffects.Copy;
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }
            e.Handled = true;
        }

        /// <summary>Receives a file drag-and-drop'ed on the image area.</summary>
        /// <remarks>When more than one files are dropped, uses only the first one, ignoring others.</remarks>
        private async void image_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                var paths = e.Data.GetData(DataFormats.FileDrop) as string[];
                if (paths?.Length >= 1)
                {
                    await LoadImageAsync(paths[0]);
                    e.Handled = true;
                    return;
                }
            }
            if (e.Data.GetDataPresent(CFStr.FILEDESCRIPTOR))
            {
                var data = new OutlookDataObject(e.Data);
                var names = data.GetData(CFStr.FILEDESCRIPTOR) as string[];
                if (names?.Length >= 1)
                {
                    // CFSTR_FILEDESCEIPTOR-based drag-and-drop sends only file names,
                    // and directory paths or other information on their locations
                    // are unavailable.
                    // If CFSTR_INETURL is also present,
                    // it is likely that the file is from the internet,
                    // and the CFSTR_INETURL content is (by specification) an absolute URL.
                    // So, we try to grab the URL and handle it like a full path name.
                    var stream = data.GetData(CFStr.FILECONTENTS, 0);
                    var url = data.GetData(CFStr.INETURL)?.AsString() ?? names[0];
                    await LoadImageAsync(stream, names[0], url);
                    e.Handled = true;
                    return;
                }
            }
            if (e.Data.GetDataPresent(CFStr.INETURL))
            {
                var url = e.Data.GetData(CFStr.INETURL)?.AsString();
                if (url is not null)
                {
                    await LoadImageAsync(new Uri(url));
                    e.Handled = true;
                    return;
                }
            }
        }

        private async void fileOpenButton_Click(object sender, RoutedEventArgs e)
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

        private Task LoadImageAsync(string path)
        {
            return LoadImageCoreAsync(() => ImageFactory.Create(path));
        }

        private Task LoadImageAsync(Uri uri)
        {
            return LoadImageCoreAsync(() => ImageFactory.Create(uri));
        }

        private Task LoadImageAsync(Stream stream, string name, string full_name)
        {
            return LoadImageCoreAsync(() => ImageFactory.Create(stream, name, full_name));
        }

        private async Task LoadImageCoreAsync(Func<IImageObject> create_image)
        {
            Mouse.OverrideCursor = Cursors.Wait;
            IsEnabled = false;
            try
            {
                await LoadImageCoreCoreAsync(create_image);
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.ToString());
            }
            IsEnabled = true;
            Mouse.OverrideCursor = null;
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

        private void metadataList_Command_Executed(object sender, ExecutedRoutedEventArgs e)
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

        private void metadataList_Command_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = (sender as ListView)?.SelectedIndex >= 0;
        }

        /// <summary>A safe margin when calculating the available width.</summary>
        /// <remarks>This value was decided by trial and error.</remarks>
        private const double ColumnWidthMargin = 2.0;

        /// <summary>The design value of the Width property for the last column in the metadataList.</summary>
        /// <remarks>The value 0.0 is a flag indicating "not initialized yet".</remarks>
        private double InitialColumnWidth = 0.0;

        private void metadataList_SizeChanged(object sender, SizeChangedEventArgs e)
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
    }
}
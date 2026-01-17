using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace aiimeta.UI
{
    /// <summary>TextBox with a copy button on its upper right.</summary>
    /// <remarks>
    /// <para>
    /// This is not a general purpose control, but it is just an app-specific component.
    /// It is not adviced to reuse it in another project.
    /// </para>
    /// <para>
    /// When the button is pressed,
    /// this control copies the content of the <see cref="Text"/> to the clipboard.
    /// </para>
    /// </remarks>
    public partial class TextBoxWithCopyButton : UserControl
    {
        public static DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(TextBoxWithCopyButton),
                new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public TextBoxWithCopyButton()
        {
            InitializeComponent();
        }

        public string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(Text);
        }
    }
}

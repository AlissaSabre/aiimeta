using System.Configuration;
using System.Data;
using System.Windows;

namespace aiimeta.UI
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    /// <remarks>
    /// Unlike usual WPF apps, the main entry point of the aiimeta app is not in this class.
    /// It is defined in <see cref="Program"/>.
    /// </remarks>
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
        }
    }

}

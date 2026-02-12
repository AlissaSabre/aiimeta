using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using aiimeta.UI;

namespace aiimeta
{
    /// <summary>Provides the main entry point of the aiimeta app.</summary>
    public class Program
    {
        [STAThread]
        public static void Main()
        {
            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
            CultureInfo.CurrentUICulture = CultureInfo.InvariantCulture;
            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
            CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;

            var app = new App();
            var main = new MainWindow();
            app.Run(main);
        }
    }
}

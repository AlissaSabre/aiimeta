using aiimeta.Formats;
using aiimeta.Reader;
using Microsoft.Extensions.DependencyInjection;
using System.Configuration;
using System.Data;
using System.Globalization;
using System.Net.Http;
using System.Windows;

namespace aiimeta.UI
{
    /// <summary>Custom startup code.</summary>
    public partial class App : Application
    {
        private readonly ServiceProvider ServiceProvider;

        public App()
        {
            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
            CultureInfo.CurrentUICulture = CultureInfo.InvariantCulture;
            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
            CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;

            ServiceProvider = new ServiceCollection()
                .AddSingleton<MainWindow, MainWindow>()
                .AddSingleton<ImageFactory>(provider =>
                    new ImageFactory(
                        provider.GetRequiredService<MetadataReader>(),
                        provider.GetRequiredService<AggregateMetadataParser>(),
                        provider.GetRequiredService<HttpClient>())
                    {
                        MaxPreviewWidth = SystemParameters.PrimaryScreenWidth * 0.5,
                        MaxPreviewHeight = SystemParameters.PrimaryScreenHeight * 0.5,
                    })
                .AddSingleton<MetadataReader, MetadataReader>()
                .AddSingleton<AggregateMetadataParser, AggregateMetadataParser>()
                .AddSingleton<HttpClient, HttpClient>()
                .BuildServiceProvider();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            MainWindow = ServiceProvider.GetRequiredService<MainWindow>();
            MainWindow.Show();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
            ServiceProvider.Dispose();
        }
    }

}

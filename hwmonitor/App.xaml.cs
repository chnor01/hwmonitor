using System.Configuration;
using System.Data;
using System.Windows;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using SkiaSharp;
using Application = System.Windows.Application;


namespace hwmonitor
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            LiveCharts.Configure(config =>
                config
                .AddDarkTheme()

                );
        }

    }

}

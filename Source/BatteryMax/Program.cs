using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BatteryMax
{
    static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        public static async Task Main(string[] args)
        {
            try
            {
                using IHost host = CreateHostBuilder(args).Build();

                Application.SetHighDpiMode(HighDpiMode.SystemAware);
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                var applicationContext = new CustomApplicationContext();
                //await applicationContext.InitializeContextAsync(new TestBatteryData());
                await applicationContext.InitializeContextAsync();

                Application.Run(applicationContext);

                //await host.RunAsync(); This... is not needed?
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "BatteryMax error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static IHostBuilder CreateHostBuilder(string[] args) => Host.CreateDefaultBuilder(args).
            ConfigureAppConfiguration((hostingContext, configuration) =>
            {
                var configRoot = configuration.Build();

                var config = BatteryMaxConfiguration.StaticDefaults();

                configRoot.Bind(config);

                Settings.Initialize(config);
            });
    }

    public class TestBatteryData : BatteryData
    {
        public TestBatteryData() : base(null)
        {
            IsNotAvailable = false;
            CurrentCharge = 15;
            CurrentTime = TimeSpan.FromMinutes(12);
            IsBelowMinimumCharge = true;
        }
    }
}

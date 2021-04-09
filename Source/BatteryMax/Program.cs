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

                //await applicationContext.InitializeContextAsync(new TestBatteryDraining());
                await applicationContext.InitializeContextAsync();

                Application.Run(applicationContext);
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

    public class TestBatteryDraining : BatteryData
    {
        private int count = 0;
        private const int interval = 5;

        private const int currentChargeStep = -2;

        private TestBatteryDraining testBatteryDraining;

        public TestBatteryDraining() : base(null)
        {
            IsNotAvailable = false;

            CurrentCharge = 100;
            TotalSecondsRemaining = (CurrentCharge / currentChargeStep) * interval;
            SetChargeMinimumMaximum();
            CalculateRemainingTime();

            testBatteryDraining = this;
        }

        public override BatteryData GetNextTestData()
        {
            if (++count % interval == 0)
            {
                var newCurrentCharge = testBatteryDraining.CurrentCharge + currentChargeStep;
                var newTotalSecondsRemaining = newCurrentCharge / currentChargeStep * interval;
                testBatteryDraining = new TestBatteryDraining
                {
                    CurrentCharge = newCurrentCharge,
                    TotalSecondsRemaining = newTotalSecondsRemaining
                };
                testBatteryDraining.SetChargeMinimumMaximum();
                testBatteryDraining.CalculateRemainingTime();
                if (newCurrentCharge < 10)
                {
                    testBatteryDraining.IsCriticalCharge = true;
                }
            }

            return testBatteryDraining;
        }
    }
}

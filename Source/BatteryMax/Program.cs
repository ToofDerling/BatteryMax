using System;
using System.Windows.Forms;

namespace BatteryMax
{
    static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        public static void Main()
        {
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            try
            {
                Settings.Initialize();

                var applicationContext = new CustomApplicationContext(new TestBattery());
                //var applicationContext = new CustomApplicationContext();
                Application.Run(applicationContext);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Program Terminated Unexpectedly", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    public class TestBattery : Battery
    {
        public TestBattery() : base(initialize: false)
        {
            CurrentCharge = 100;
        }
    }
}

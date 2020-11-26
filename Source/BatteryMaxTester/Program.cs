using BatteryMax;
using System;
using System.Configuration;
using System.Drawing;

namespace BatteryMaxTester
{
    class Program
    {
        static void Main(string[] _)
        {


            //var info = BatteryInfo.GetBatteryInformation();

            //Console.WriteLine($"Charge/discharge rate: {info.Rate}");
            //Console.WriteLine($"Current capacity: {info.CurrentCapacity}");
            //Console.WriteLine($"Full capacity: {info.FullChargeCapacity}");
            ////Console.WriteLine($"Designed capacity: {info.DesignedCapacity}");
            ////Console.WriteLine($"PowerState: {info.PowerState}");

            //Console.WriteLine();

            //// Remaining Battery Life[h] = Battery Remaining Capacity[mAh / mWh] / Battery Present Drain Rate[mA / mW]

            //var capacity = (float)info.FullChargeCapacity - info.CurrentCapacity;
            //Console.WriteLine(capacity);

            //var hour = capacity / info.Rate;
            //Console.WriteLine(hour);

            //var ts = TimeSpan.FromHours(hour);
            //Console.WriteLine(ts.Minutes);

            ///*Console.WriteLine(info.Voltage);
            //Console.WriteLine(info.CycleCount);*/

            var settings = IconSettings.GetSettings(new Size(24, 24));
            var builder = new IconBuilder(settings);

            var battery = new TestBattery();

            builder.BuildIcon(battery, 0);

            Console.ReadLine();
        }

        private class TestBattery : Battery
        {
            public TestBattery()
            {
                CurrentCharge = 50;
            }

        }
    }
}

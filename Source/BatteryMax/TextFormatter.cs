using BatteryMax.Properties;
using System.Text;

namespace BatteryMax
{
    public class TextFormatter
    {
        public static string FormatBatteryUpdateText(Battery battery)
        {
            var currentCharge = battery.CurrentCharge;

            var sb = new StringBuilder();

            //Handle the special case with no time
            if (battery.IsPluggedInNotCharging)
            {
                sb.AppendFormat(Resources.PluggedIn, currentCharge);
                sb.AppendLine();
                sb.Append(Resources.NotCharging);

                return sb.ToString();
            }

            int targetCharge;

            if (battery.IsCharging)
            {
                sb.AppendFormat(Resources.Charging, currentCharge);
                targetCharge = Settings.MaximumCharge;

                if (battery.IsAboveMaximumCharge)
                {
                    sb.AppendLine();
                    sb.AppendFormat(Resources.AboveMaximumCharge, Settings.MaximumCharge);
                    targetCharge = 100;
                }
            }
            else // Not charging
            {
                sb.AppendFormat(Resources.Remaining, currentCharge);
                targetCharge = Settings.MinimumCharge;

                if (battery.IsBelowMinimumCharge)
                {
                    sb.AppendLine();
                    sb.AppendFormat(Resources.BelowMinimumCharge, Settings.MinimumCharge);
                    targetCharge = 0;
                }
            }

            if (battery.CurrentTime.TotalSeconds > 0)
            {
                sb.AppendLine();
                FormatTime(sb, battery, targetCharge);
            }

            return sb.ToString();
        }

        private static void FormatTime(StringBuilder sb, Battery battery, int targetCharge)
        {
            if (battery.CurrentTime.Hours > 0 && battery.CurrentTime.Minutes > 0)
            {
                sb.AppendFormat(Resources.HourMin, battery.CurrentTime.Hours, battery.CurrentTime.Minutes, targetCharge);
            }
            else if (battery.CurrentTime.Hours > 0)
            {
                sb.AppendFormat(Resources.Hour, battery.CurrentTime.Hours, targetCharge);
            }
            else // This can return "0 min" and that's fine
            {
                sb.AppendFormat(Resources.Min, battery.CurrentTime.Minutes, targetCharge);
            }
        }
    }
}

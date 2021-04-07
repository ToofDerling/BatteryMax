using System;
using System.Drawing;
using System.Windows.Forms;

namespace BatteryMax
{
    public class IconSettings
    {
        private readonly BatteryIcon batteryIcon;

        public IconSettings(BatteryIcon batteryIcon, WindowsTheme theme)
        {
            this.batteryIcon = batteryIcon;

            ForegroundColor = theme == WindowsTheme.Light ? Settings.ForegroundColorLightTheme : Settings.ForegroundColorDarkTheme;
        }

        public int Width => batteryIcon.Size;
        public int Height => batteryIcon.Size;

        public Color BackgroundColor => Settings.BackgroundColor;
        public Color ForegroundColor { get; private set; }

        public DrawRectangle[] Rectangles => batteryIcon.Rectangles;

        public float PercentPerLevel => 100f / batteryIcon.Levels.Maximum;

        public static IconSettings GetSettings(WindowsTheme theme)
        {
            var size = SystemInformation.SmallIconSize;
            Log.Write($"{nameof(SystemInformation.SmallIconSize)}={size.Width}x{size.Height}");

            return GetSettings(size, theme);
        }

        public static IconSettings GetSettings(Size size, WindowsTheme theme)
        {
            var check = size.Width;
            if (size.Width != size.Height)
            {
                check = Math.Min(size.Width, size.Height);
            }

            // Assume that reducing is better than enlarging
            if (check < 20)
            {
                return new IconSettings(Settings.BatteryIcon100, theme);
            }

            if (check < 24)
            {
                return null; //settings150;
            }

            return null;// settings150;
        }

        public Color GetColor(BatteryData battery)
        {
            if (battery.IsCriticalCharge)
            {
                return Settings.CriticalColor;
            }

            if (battery.IsBelowMinimumCharge || battery.IsAboveMaximumCharge)
            {
                return Settings.WarningColor;
            }

            if (battery.IsCharging || battery.IsPluggedInNotCharging)
            {
                return Settings.ChargingColor;
            }

            return Settings.DrainingColor;
        }

        public Rectangle GetChargeLevelsRectangle(int levels)
        {
            if (batteryIcon.Levels.Height.HasValue)
            {
                return new Rectangle(batteryIcon.Levels.X, batteryIcon.Levels.Y, levels, batteryIcon.Levels.Height.Value);
            }

            if (batteryIcon.Levels.Width.HasValue)
            {
                // X,Y are bottom left
                return new Rectangle(batteryIcon.Levels.X, batteryIcon.Levels.Y - levels, batteryIcon.Levels.Width.Value, levels);
            }

            throw new ArgumentException("Cannot create a chargelevels rectangle without a width or a height");
        }


        /*      private static readonly IconSettings settings100 = new()
              {
                  Width = 16,
                  Height = 16,

                  X = 3,
                  Y = 6,
                  Y2 = 11,

                  Levels = 10,

                  Template = @"icons\template_16.png",
              };
        */
        //TODO:
        /*
                private static readonly IconSettings settings150 = new IconSettings
                {
                    Width = 24,
                    Height = 24,


                    Template = @"icons\template_24.png",
                    X = 3,
                    Y = 8,
                    Y2 = 17,
                    Levels = 18
                };
        */
    }
}

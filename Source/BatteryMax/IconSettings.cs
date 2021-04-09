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

        public Rectangle[] Rectangles => batteryIcon.Rectangles;

        public float PercentPerLevel => 100f / batteryIcon.Levels.Maximum;

        public static IconSettings GetSettings(WindowsTheme theme)
        {
            var size = SystemInformation.SmallIconSize;
            Log.Write($"{nameof(SystemInformation.SmallIconSize)}: {size.Width}x{size.Height}");

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
                return new IconSettings(Settings.BatteryIcon150, theme);
            }

            // Return the largest setting we have
            return new IconSettings(Settings.BatteryIcon150, theme);
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
            var levelsConfig = batteryIcon.Levels;

            return levelsConfig.Direction switch
            {
                BatteryIconLevelsDirection.LeftRight => new Rectangle(levelsConfig.X, levelsConfig.Y, levels, levelsConfig.WidthOrHeight),
                // X,Y of config are bottom left
                BatteryIconLevelsDirection.BottomUp => new Rectangle(levelsConfig.X, levelsConfig.Y - levels, levelsConfig.WidthOrHeight, levels),
                _ => throw new NotImplementedException(levelsConfig.Direction.ToString()),
            };
        }
    }
}

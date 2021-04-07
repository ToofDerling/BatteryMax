using System;
using System.Drawing;

namespace BatteryMax
{
    public class Settings
    {
        public static int MaximumCharge { get; private set; }

        public static int MinimumCharge { get; private set; }

        public static Color BackgroundColor { get; private set; }

        public static Color ForegroundColorDarkTheme { get; private set; }

        public static Color ForegroundColorLightTheme { get; private set; }

        public static Color ChargingColor { get; private set; }

        public static Color DrainingColor { get; private set; }

        public static Color WarningColor { get; private set; }

        public static Color CriticalColor { get; private set; }

        public static BatteryIcon BatteryIcon100 { get; private set; }

        public static void Initialize(BatteryMaxConfiguration config)
        {
            MinimumCharge = GetChargeLevel(config.ChargeLevels.Minimum, nameof(MinimumCharge));
            MaximumCharge = GetChargeLevel(config.ChargeLevels.Maximum, nameof(MaximumCharge));
            ValidateChargeLevels();

            BackgroundColor = GetIconColor(config.IconColors.Background, nameof(BackgroundColor));
            ForegroundColorDarkTheme = GetIconColor(config.IconColors.ForegroundDarkTheme, nameof(ForegroundColorDarkTheme));
            ForegroundColorLightTheme = GetIconColor(config.IconColors.ForegroundLightTheme, nameof(ForegroundColorLightTheme));

            ChargingColor = GetIconColor(config.IconColors.Charging, nameof(ChargingColor));
            DrainingColor = GetIconColor(config.IconColors.Draining, nameof(DrainingColor));
            WarningColor = GetIconColor(config.IconColors.Warning, nameof(WarningColor));
            CriticalColor = GetIconColor(config.IconColors.Critical, nameof(CriticalColor));

            var batteryIconDefaults = BatteryMaxConfiguration.BatteryIconDefaults();
            BatteryIcon100 = config.BatteryIcon100 ?? batteryIconDefaults.BatteryIcon100;
        }

        private static int GetChargeLevel(int value, string name)
        {
            if (value is > 100 or < 0)
            {
                throw new ApplicationException($"Value of '{value}' is not valid for {name}. {name} must be greater than or equal to 0 and less than or equal to 100");
            }

            return value;
        }

        private static void ValidateChargeLevels()
        {
            if (MinimumCharge >= MaximumCharge)
            {
                throw new ApplicationException($"{nameof(MinimumCharge)} '{MinimumCharge}' must be less than {nameof(MaximumCharge)} '{MaximumCharge}'");
            }
        }

        private static Color GetIconColor(Rgb rgb, string name)
        {
            try
            {
                if (!string.IsNullOrEmpty(rgb.Name))
                {
                    if (!Enum.TryParse(typeof(KnownColor), rgb.Name, ignoreCase: true, out object _))
                    {
                        throw new ApplicationException($"{name}: \"{rgb.Name}\" is not a valid color name");
                    }

                    return Color.FromName(rgb.Name);
                }

                if (rgb.Alpha.HasValue)
                {
                    return Color.FromArgb(rgb.Alpha.Value, rgb.Red.Value, rgb.Green.Value, rgb.Blue.Value);
                }

                return Color.FromArgb(rgb.Red.Value, rgb.Green.Value, rgb.Blue.Value);
            }
            catch (ArgumentException ex)
            {
                throw new ApplicationException($"{name}: {ex.Message}");
            }
        }
    }
}

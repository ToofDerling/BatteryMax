using System.Configuration;
using System.Drawing;

namespace BatteryMax
{
    public class Settings
    {
        private static class Defaults
        {
            public const int MaximumCharge = 80;
            public const int MinimumCharge = 20;

            public const string ChargingColor = "0,173,239"; // Blue
            public const string DrainingColor = "0,130,100"; // Green
            public const string WarningColor = "255,140,0"; // Orange
            public const string CriticalColor = "204,0,0"; // Red
        }

        public static int MaximumCharge { get; private set; }

        public static int MinimumCharge { get; private set; }

        public static Color ChargingColor { get; private set; }

        public static Color DrainingColor { get; private set; }

        public static Color WarningColor { get; private set; }

        public static Color CriticalColor { get; private set; }

        public static void Initialize()
        {
            var appSettings = ConfigurationManager.AppSettings;

            MaximumCharge = int.TryParse(appSettings[nameof(MaximumCharge)], out var maximumCharge) ? maximumCharge : Defaults.MaximumCharge;
            MinimumCharge = int.TryParse(appSettings[nameof(MinimumCharge)], out var minimumcharge) ? minimumcharge : Defaults.MinimumCharge;

            ChargingColor = ParseColor(appSettings[nameof(ChargingColor)], Defaults.ChargingColor);
            DrainingColor = ParseColor(appSettings[nameof(DrainingColor)], Defaults.DrainingColor);
            WarningColor = ParseColor(appSettings[nameof(WarningColor)], Defaults.WarningColor);
            CriticalColor = ParseColor(appSettings[nameof(CriticalColor)], Defaults.CriticalColor);
        }

        private static Color ParseColor(string configRgb, string defaultRgb)
        {
            if (string.IsNullOrWhiteSpace(configRgb))
            {
                return ParseColor(defaultRgb, null);
            }

            const int sz = 3;

            var rgbTokens = configRgb.Split(',');
            if (rgbTokens.Length != sz)
            {
                return ParseColor(defaultRgb, null);
            }

            var rgb = new int[sz];
            for (int i = 0; i < sz; i++)
            {
                if (!int.TryParse(rgbTokens[i], out var rgbVal))
                {
                    return ParseColor(defaultRgb, null);
                }

                rgb[i] = rgbVal;
            }

            return Color.FromArgb(rgb[0], rgb[1], rgb[2]);
        }

        public static void Update(params (string key, object value)[] keyValues)
        {
            var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

            foreach (var (key, value) in keyValues)
            {
                config.AppSettings.Settings[key].Value = value.ToString();
            }

            config.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection(config.AppSettings.SectionInformation.Name);

            Initialize();
        }
    }
}

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
            public const string ChargingColor = "0,173,239";
            public const string DrainingColor = "0,130,100";
            public const string WarningColor = "255,140,0";
            public const string CriticalColor = "204,0,0";
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
            var tokens = configRgb.Split(',');

            if (tokens.Length != 3
                || !int.TryParse(tokens[0], out var r) || !int.TryParse(tokens[1], out var g) || !int.TryParse(tokens[2], out var b))
            {
                return ParseColor(defaultRgb, null);
            }

            return Color.FromArgb(r, g, b);
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

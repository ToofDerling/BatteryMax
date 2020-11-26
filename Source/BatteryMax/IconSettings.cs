using System.Drawing;
using System.Windows.Forms;

namespace BatteryMax
{
    public class IconSettings
    {
        public string Template { get; set; }

        public int X { get; set; }
        public int Y { get; set; }
        public int Y2 { get; set; }

        public int Levels { get; set; }
        public float PercentPerLevel => 100f / Levels;

        public static IconSettings GetSettings()
        {
            var size = SystemInformation.SmallIconSize;
            Log.Write($"{nameof(SystemInformation.SmallIconSize)}={size.Width}x{size.Height}");

            return GetSettings(size);
        }

        public static IconSettings GetSettings(Size size)
        {
            if (size.Width != size.Height)
            {
                //Can this happen?
            }

            // Assume that reducing is better than enlarging
            if (size.Width < 20)
            {
                return settings100;
            }

            if (size.Width < 24)
            {
                return settings150;
            }

            return settings150;
        }

        private static readonly IconSettings settings100 = new IconSettings
        {
            Template = @"icons\template_16.png",
            X = 3,
            Y = 6,
            Y2 = 11,
            Levels = 10
        };

        //TODO:
        private static readonly IconSettings settings125 = new IconSettings
        {
            Template = @"icons\template_20.png",

            X = 3,
            Y = 5,
            Y2 = 6,
            Levels = 10
        };

        private static readonly IconSettings settings150 = new IconSettings
        {
            Template = @"icons\template_24.png",
            X = 3,
            Y = 8,
            Y2 = 17,
            Levels = 18
        };

        //TODO:
        private static readonly IconSettings settings175 = new IconSettings
        {
            Template = @"icons\template_28.png",

            X = 3,
            Y = 5,
            Y2 = 6,
            Levels = 10
        };
    }
}

namespace BatteryMax
{
    public class ChargeLevels
    {
        public int Minimum { get; set; }
        public int Maximum { get; set; }
    }

    public class Rgb
    {
        public int? Red { get; set; }
        public int? Green { get; set; }
        public int? Blue { get; set; }

        public int? Alpha { get; set; }

        public string Name { get; set; }
    }

    public class IconColors
    {
        public Rgb Background { get; set; }

        public Rgb ForegroundDarkTheme { get; set; }
        public Rgb ForegroundLightTheme { get; set; }

        public Rgb Charging { get; set; }
        public Rgb Draining { get; set; }
        public Rgb Warning { get; set; }
        public Rgb Critical { get; set; }
    }

    public class DrawRectangle
    {
        public int X { get; set; }
        public int Y { get; set; }

        public int Width { get; set; }
        public int Height { get; set; }
    }

    public class BatteryIconLevels
    {
        public int X { get; set; }
        public int Y { get; set; }

        public int? Width { get; set; }
        public int? Height { get; set; }

        public int Maximum { get; set; }
    }

    public class BatteryIcon
    {
        public int Size { get; set; }

        public DrawRectangle[] Rectangles { get; set; }

        public BatteryIconLevels Levels { get; set; }
    }

    public record BatteryMaxConfiguration
    {
        /// <summary>
        /// This provides the default values for the ChargeLevels and IconColors properties
        /// </summary>
        /// <returns></returns>
        public static BatteryMaxConfiguration StaticDefaults()
        {
            return new BatteryMaxConfiguration
            {
                ChargeLevels = new ChargeLevels { Minimum = 20, Maximum = 80 },
                
                IconColors = new IconColors
                {
                    Background = new Rgb { Name = "Transparent" },
                    
                    ForegroundDarkTheme = new Rgb { Name = "White" }, // Foreground for dark theme
                    ForegroundLightTheme = new Rgb { Name = "Black" }, // Foreground for light theme

                    Charging = new Rgb { Red = 0, Green = 173, Blue = 239 }, // Blue
                    Draining = new Rgb { Red = 0, Green = 130, Blue = 100 }, // Green 
                    Warning = new Rgb { Red = 255, Green = 140, Blue = 0 }, // Orange
                    Critical = new Rgb { Red = 204, Green = 0, Blue = 0 } // Red
                },
            };
        }

        /// <summary>
        /// This provides the default values for the BatteryIconNNN properties
        /// /// </summary>
        /// <returns></returns>
        public static BatteryMaxConfiguration BatteryIconDefaults()
        {
            return new BatteryMaxConfiguration
            {
                BatteryIcon100 = new BatteryIcon
                {
                    Size = 16,
                    Rectangles = new[]
                    {
                        new DrawRectangle { X = 1, Y = 4, Width = 13, Height = 9 },
                        new DrawRectangle { X = 15, Y = 7, Width = 1, Height = 4 }
                    },
                    Levels = new BatteryIconLevels 
                    { 
                        Maximum = 10,
                        X = 3,
                        Y = 6,
                        Height = 6
                    }
                }
            };
        }

        public ChargeLevels ChargeLevels { get; init; }
        public IconColors IconColors { get; init; }

        public BatteryIcon BatteryIcon100 { get; set; }
        public BatteryIcon BatteryIcon125 { get; set; }
        public BatteryIcon BatteryIcon150 { get; set; }
        public BatteryIcon BatteryIcon175 { get; set; }
    }
}

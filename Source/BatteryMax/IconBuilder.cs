using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BatteryMax
{
    public class IconBuilder
    {
        private IconSettings IconSettings { get; set; }

        public IconBuilder(IconSettings iconSettings)
        {
            IconSettings = iconSettings;
        }

        public int GetDrawingWidth(Battery battery)
        {
            var drawWidth = Convert.ToInt32(battery.CurrentCharge / IconSettings.PercentPerLevel);

            // Always draw something if charge > 0
            if (drawWidth == 0 && battery.CurrentCharge > 0)
            {
                drawWidth = 1;
            }

            return drawWidth;
        }

        private static Color GetDrawingColor(Battery battery)
        {
            if (battery.IsCharging || battery.IsPluggedInNotCharging)
            {
                return Settings.ChargingColor;
            }

            //TODO: IsAboveMaximumCharge should draw ChargingColor + WarningColor

            if (battery.IsBelowMinimumCharge)
            {
                return Settings.WarningColor;
            }

            if (battery.IsCriticalCharge)
            {
                return Settings.CriticalColor;
            }

            return Settings.DrainingColor;
        }

        public Icon BuildIcon(Battery battery, int drawWidth)
        {
            using var image = Image.FromFile(IconSettings.Template);

            if (drawWidth > 0)
            {
                DrawChargeLevel(battery, image, drawWidth);
            }
#if DEBUG
            image.Save(@"c:\temp\batterymax.png");
#endif

            using var bmp = new Bitmap(image);
            return Icon.FromHandle(bmp.GetHicon());
        }

        private void DrawChargeLevel(Battery battery, Image image, int levels)
        {
            using var graphics = Graphics.FromImage(image);
            using var pen = new Pen(default(Color));

            for (var level = 0; level < levels; level++)
            {
                var color = GetDrawingColor(battery);
                if (pen.Color != color)
                {
                    pen.Color = color;
                }

                var x = IconSettings.X + level;
                graphics.DrawLine(pen, x, IconSettings.Y, x, IconSettings.Y2);
            }
        }
    }
}

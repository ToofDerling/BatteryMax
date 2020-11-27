using System;
using System.Drawing;

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

        private Color GetDrawingColor(Battery battery, int level)
        {
            if (battery.IsCriticalCharge)
            {
                return Settings.CriticalColor;
            }

            if (battery.IsBelowMinimumCharge)
            {
                return Settings.WarningColor;
            }

            var levelPercent = level * IconSettings.PercentPerLevel;
            if (levelPercent > Settings.MaximumCharge)
            {
                return Settings.WarningColor;
            }

            if (battery.IsCharging || battery.IsPluggedInNotCharging)
            {
                return Settings.ChargingColor;
            }

            return Settings.DrainingColor;
        }

        /// <summary>
        /// Image will be disposed here.
        /// </summary>
        /// <param name="image"></param>
        /// <returns></returns>
        public Icon CreateIcon(Image image)
        {
            var bitmap = new Bitmap(image);
            var icon = Icon.FromHandle(bitmap.GetHicon());

            bitmap.Dispose();
            image.Dispose();

            return icon;
        }

        public Image DrawImage(Battery battery, int drawWidth)
        {
            var image = Image.FromFile(IconSettings.Template);

            if (drawWidth > 0)
            {
                DrawChargeLevel(battery, image, drawWidth);
            }

            return image;
        }

        private void DrawChargeLevel(Battery battery, Image image, int levels)
        {
            using var graphics = Graphics.FromImage(image);
            using var pen = new Pen(default(Color));

            for (var xPos = 0; xPos < levels; xPos++)
            {
                var level = xPos + 1;
                var color = GetDrawingColor(battery, level);
                if (pen.Color != color)
                {
                    pen.Color = color;
                }

                var x = IconSettings.X + xPos;
                graphics.DrawLine(pen, x, IconSettings.Y, x, IconSettings.Y2);
            }
        }
    }
}

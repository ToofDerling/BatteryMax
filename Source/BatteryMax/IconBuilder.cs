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
                //DrawChargeLevels(battery, image, drawWidth);
                DrawChargeLevelsSolid(battery, image, drawWidth);
            }

            return image;
        }

        private void DrawChargeLevelsSolid(Battery battery, Image image, int levels)
        {
            using var graphics = Graphics.FromImage(image);
            using var brush = new SolidBrush(GetColor());

            var height = IconSettings.Y2 - IconSettings.Y + 1;
            var width = levels;

            graphics.FillRectangle(brush, IconSettings.X, IconSettings.Y, width, height);

            Color GetColor()
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
        }

        #region DrawChargeLevelsColors - make this an option?

        private void DrawChargeLevelsColors(Battery battery, Image image, int levels)
        {
            using var graphics = Graphics.FromImage(image);
            using var pen = new Pen(default(Color));

            for (var xPos = 0; xPos < levels; xPos++)
            {
                var level = xPos + 1;

                SetPen(level);

                var x = IconSettings.X + xPos;
                graphics.DrawLine(pen, x, IconSettings.Y, x, IconSettings.Y2);
            }

            void SetPen(int level)
            {
                Color color;

                if (battery.IsCriticalCharge)
                {
                    color = Settings.CriticalColor;
                }
                else if (battery.IsBelowMinimumCharge || level * IconSettings.PercentPerLevel > Settings.MaximumCharge)
                {
                    color = Settings.WarningColor;
                }
                else if (battery.IsCharging || battery.IsPluggedInNotCharging)
                {
                    color = Settings.ChargingColor;
                }
                else
                {
                    color = Settings.DrainingColor;
                }

                if (pen.Color != color)
                {
                    pen.Color = color;
                }
            }
        }

        #endregion
    }
}

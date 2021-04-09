using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace BatteryMax
{
    public class IconBuilder
    {
        private IconSettings IconSettings { get; set; }

        public IconBuilder(IconSettings iconSettings)
        {
            IconSettings = iconSettings;
        }

        public int GetDrawingWidth(BatteryData battery)
        {
            var drawWidth = Convert.ToInt32(battery.CurrentCharge / IconSettings.PercentPerLevel);

            // Always draw something if charge > 0
            if (drawWidth == 0 && battery.CurrentCharge > 0)
            {
                drawWidth = 1;
            }

            return drawWidth;
        }

        public Icon DrawIcon(BatteryData battery, int drawWidth)
        {
            // Format32bppArgb is required for transparancy
            using var bitmap = new Bitmap(IconSettings.Width, IconSettings.Height, PixelFormat.Format32bppArgb);
            using var graphics = Graphics.FromImage(bitmap);

            DrawBackground(graphics, bitmap);
            DrawForeground(graphics);
            DrawChargeLevels(graphics, battery, drawWidth);
#if DEBUG
            bitmap.Save(@$"c:\system\temp\batterymax-{drawWidth}.png");
#endif
            var icon = Icon.FromHandle(bitmap.GetHicon());
            return icon;
        }

        private void DrawBackground(Graphics graphics, Bitmap bitmap)
        {
            if (IconSettings.BackgroundColor == Color.Transparent)
            {
                bitmap.MakeTransparent();
            }
            else
            {
                using var backgroundBrush = new SolidBrush(IconSettings.BackgroundColor);
                graphics.FillRectangle(backgroundBrush, 0, 0, IconSettings.Width, IconSettings.Height);
            }
        }

        private void DrawForeground(Graphics graphics)
        {
            if (IconSettings.Rectangles == null || IconSettings.Rectangles.Length == 0)
            {
                return;
            }

            using var pen = new Pen(IconSettings.ForegroundColor);
            foreach (var drawRectangle in IconSettings.Rectangles)
            {
                graphics.DrawRectangle(pen, drawRectangle.X, drawRectangle.Y, drawRectangle.Width, drawRectangle.Height);
            }
        }

        private void DrawChargeLevels(Graphics graphics, BatteryData battery, int levels)
        {
            if (levels <= 0)
            {
                return;
            }

            var rect = IconSettings.GetChargeLevelsRectangle(levels);

            using var brush = new SolidBrush(IconSettings.GetColor(battery));
            graphics.FillRectangle(brush, rect);
        }
    }
}

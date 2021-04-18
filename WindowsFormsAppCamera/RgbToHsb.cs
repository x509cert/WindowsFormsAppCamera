using System;
using System.Numerics;

namespace WindowsFormsAppCamera
{
    public class RgbToHsb
    {
        public enum Color { Unknown, Black, Gray, White, Red, Yellow, Green, Cyan, Blue, Purple};

        public static Color GetColorFromRgbHsb(int r, int g, int b, float h, float s, float br)
        {
            if (br < 10) return Color.Black;
            if (br >= 11 && br < 30) return Color.Gray;
            if (br > 90 && s < 10) return Color.White;

            // if RG and B are all close to each other, then this is gray
            float range = 0.15F;
            float rr1 = r - (r * range), rr2 = r + (r * range);
            float gr1 = g - (g * range), gr2 = g + (g * range);
            float br1 = b - (b * range), br2 = b + (b * range);
            if (r >= gr1 && r <= gr2 &&
                g >= br1 && g <= br2 &&
                b >= rr1 && b <= rr2)
                return Color.Gray;

            if (h > 330 || h <= 30) return Color.Red;
            if (h > 30 && h <= 90) return Color.Yellow;
            if (h > 90 && h <= 150) return Color.Green;
            if (h > 150 && h <= 210) return Color.Cyan;
            if (h > 210 && h <= 270) return Color.Blue;
            if (h > 270 && h <= 330) return Color.Purple;

            return Color.Unknown;
        }

        public static void RGBtoHSB(int red, int green, int blue, ref float h, ref float s, ref float bright)
        {
            // normalize red, green and blue values
            float r = ((float)red / 255);
            float g = ((float)green / 255);
            float b = ((float)blue / 255);

            // conversion start
            float max = Math.Max(r, Math.Max(g, b));
            float min = Math.Min(r, Math.Min(g, b));

            h = 0;
            if (max == r && g >= b)
            {
                h = 60 * (g - b) / (max - min);
            }
            else if (max == r && g < b)
            {
                h = 60 * (g - b) / (max - min) + 360;
            }
            else if (max == g)
            {
                h = 60 * (b - r) / (max - min) + 120;
            }
            else if (max == b)
            {
                h = 60 * (r - g) / (max - min) + 240;
            }

            s = 100 * ((max == 0) ? 0 : (1 - (min / max)));
            bright = max * 100;
        }
    }
}

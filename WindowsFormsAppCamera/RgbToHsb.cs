﻿using System;
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

            // if RG and B are all close to each other, then this is gray(ish)
            float range = 0.15F;
            float rr1 = r - (r * range), rr2 = r + (r * range);
            float gr1 = g - (g * range), gr2 = g + (g * range);
            float br1 = b - (b * range), br2 = b + (b * range);
            if (r >= gr1 && r <= gr2 &&
                g >= br1 && g <= br2 &&
                b >= rr1 && b <= rr2 &&
                g >= rr1 && g <= rr2)
                return Color.Gray;

            if (h > 330 || h <= 30)  return Color.Red;
            if (h > 30  && h <= 90)  return Color.Yellow;
            if (h > 90  && h <= 150) return Color.Green;
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

    // added in case it's needed
    // uses Vector4 to take advantage of SIMD instructions https://docs.microsoft.com/en-us/dotnet/standard/simd
    class RgbToLab
    {
        public static Vector4 RGBToLab(Vector4 color)
        {
            float[] xyz = new float[3];
            float[] lab = new float[3];
            float[] rgb = new float[] { color.X, color.Y, color.Z, color.W };

            rgb[0] = color.X / 255.0f;
            rgb[1] = color.Y / 255.0f;
            rgb[2] = color.Z / 255.0f;

            if (rgb[0] > .04045f)
            {
                rgb[0] = (float)Math.Pow((rgb[0] + .055) / 1.055, 2.4);
            }
            else
            {
                rgb[0] = rgb[0] / 12.92f;
            }

            if (rgb[1] > .04045f)
            {
                rgb[1] = (float)Math.Pow((rgb[1] + .055) / 1.055, 2.4);
            }
            else
            {
                rgb[1] = rgb[1] / 12.92f;
            }

            if (rgb[2] > .04045f)
            {
                rgb[2] = (float)Math.Pow((rgb[2] + .055) / 1.055, 2.4);
            }
            else
            {
                rgb[2] = rgb[2] / 12.92f;
            }
            rgb[0] = rgb[0] * 100.0f;
            rgb[1] = rgb[1] * 100.0f;
            rgb[2] = rgb[2] * 100.0f;


            xyz[0] = ((rgb[0] * .412453f) + (rgb[1] * .357580f) + (rgb[2] * .180423f));
            xyz[1] = ((rgb[0] * .212671f) + (rgb[1] * .715160f) + (rgb[2] * .072169f));
            xyz[2] = ((rgb[0] * .019334f) + (rgb[1] * .119193f) + (rgb[2] * .950227f));


            xyz[0] = xyz[0] / 95.047f;
            xyz[1] = xyz[1] / 100.0f;
            xyz[2] = xyz[2] / 108.883f;

            if (xyz[0] > .008856f)
            {
                xyz[0] = (float)Math.Pow(xyz[0], (1.0 / 3.0));
            }
            else
            {
                xyz[0] = (xyz[0] * 7.787f) + (16.0f / 116.0f);
            }

            if (xyz[1] > .008856f)
            {
                xyz[1] = (float)Math.Pow(xyz[1], 1.0 / 3.0);
            }
            else
            {
                xyz[1] = (xyz[1] * 7.787f) + (16.0f / 116.0f);
            }

            if (xyz[2] > .008856f)
            {
                xyz[2] = (float)Math.Pow(xyz[2], 1.0 / 3.0);
            }
            else
            {
                xyz[2] = (xyz[2] * 7.787f) + (16.0f / 116.0f);
            }

            lab[0] = (116.0f * xyz[1]) - 16.0f;
            lab[1] = 500.0f * (xyz[0] - xyz[1]);
            lab[2] = 200.0f * (xyz[1] - xyz[2]);

            return new Vector4(lab[0], lab[1], lab[2], color.W);
        }
    }
}

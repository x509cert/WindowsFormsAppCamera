using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;

namespace WindowsFormsAppCamera
{
    // Converts RGB to closet known color
    // Uses the algorithm here https://en.wikipedia.org/wiki/Color_difference
    public static class RgbToClosest
    {
        private static readonly Color[] ColorArray = {
                Color.White,    Color.Gray,     Color.DarkGray, Color.Black,
                Color.Red,      Color.DarkRed,  Color.Pink,     Color.Orange,
                Color.Blue,     Color.DarkBlue,
                Color.Green,    Color.DarkGreen,
                Color.Purple,
                Color.Yellow
            };

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Color GetClosestColorFromRgb(int r, int g, int b)
        {
            return GetClosestColor(Color.FromArgb(255, r, g, b));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Color GetClosestColor(Color baseColor)
        {
            System.Collections.Generic.List<(Color Value, int Diff)> colors = ColorArray.Select(x => (Value: x, Diff: GetDiff(x, baseColor))).ToList();
            int min = colors.Min(x => x.Diff);

            return colors.Find(x => x.Diff == min).Value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int GetDiff(Color color, Color baseColor)
        {
            int a = color.A - baseColor.A,
                r = color.R - baseColor.R,
                g = color.G - baseColor.G,
                b = color.B - baseColor.B;

            return (a * a) + (r * r) + (g * g) + (b * b);
        }
    }
}

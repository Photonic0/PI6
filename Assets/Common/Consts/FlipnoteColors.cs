using UnityEngine;

namespace Assets.Common.Consts
{
    public static class FlipnoteColors
    {
        public enum ColorID
        {
            Orange = 0,
            Yellow = 1,
            DarkGreen = 2,
            Blue = 3,
            Magenta = 4
        }
        public static Color Orange => colors[0];
        public static Color Yellow => colors[1];
        public static Color DarkGreen => colors[2];
        public static Color Blue => colors[3];
        public static Color Magenta => colors[4];

        static readonly Color[] colors = new Color[] { new(1, 16f / 255f, 16f / 255f), new(1, 230f / 255f, 0), new(0, 132f / 255f, 48f / 255f), new(0, 58f / 255f, 206f / 255f), new(1, 0, 1) };
        public static Color GetColor(ColorID colorID)
        {
            return colors[(int)colorID];
        }
    }
}

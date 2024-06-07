// Created by LunarEclipse on 2024-6-5 2:27.

using UnityEngine;

namespace Luna.Extensions.Unity
{
    public static class ColorExtensions
    {
        public static float GetHue(this Color color)
        {
            float min = Mathf.Min(color.r, Mathf.Min(color.g, color.b));
            float max = Mathf.Max(color.r, Mathf.Max(color.g, color.b));

            if (min == max) return 0;

            float hue = 0;
            if (max == color.r)
                hue = (color.g - color.b) / (max - min);
            else if (max == color.g)
                hue = 2 + (color.b - color.r) / (max - min);
            else
                hue = 4 + (color.r - color.g) / (max - min);

            hue *= 60;
            if (hue < 0) hue += 360;

            return hue;
        }
        
        public static float GetSaturation(this Color color)
        {
            float min = Mathf.Min(color.r, Mathf.Min(color.g, color.b));
            float max = Mathf.Max(color.r, Mathf.Max(color.g, color.b));

            if (max == 0) return 0;
            return 1 - min / max;
        }
        
        public static float GetBrightness(this Color color)
        {
            return Mathf.Max(color.r, Mathf.Max(color.g, color.b));
        }

        /// Returns a new Color that is the inversion of this Color.
        public static Color Invert(this Color color)
        {
            return new Color(1 - color.r, 1 - color.g, 1 - color.b, color.a);
        }

        /// Returns a new Color with the same settings and a new alpha.
        public static Color WithAlpha(this Color color, float alpha)
        {
            return new Color(color.r, color.g, color.b, alpha);
        }
    }
}
// Created by LunarEclipse on 2024-7-14 7:23.

using UnityEngine;

namespace Luna.Extensions.Unity
{
    public static class ColorExtensions
    {
        public static Color RandomColor(float brightness = 1, float saturation = 1)
        {
            return Color.HSVToRGB(Random.value, saturation, brightness);
        }
        
        public static Color WithAlpha(this Color self, float a)
        {
            return new Color(self.r, self.g, self.b, a);
        }
        
        public static Color WithHue(this Color self, float h)
        {
            Color.RGBToHSV(self, out _, out float s, out float v);
            return Color.HSVToRGB(h, s, v);
        }
        
        public static Color WithSaturation(this Color self, float s)
        {
            Color.RGBToHSV(self, out float h, out _, out float v);
            return Color.HSVToRGB(h, s, v);
        }
        
        public static Color WithBrightness(this Color self, float v)
        {
            Color.RGBToHSV(self, out float h, out float s, out _);
            return Color.HSVToRGB(h, s, v);
        }
        
        public static Color With(this Color self, float? brightness = null, float? saturation = null, float? hue = null)
        {
            Color.RGBToHSV(self, out float h, out float s, out float v);
            return Color.HSVToRGB(hue ?? h, saturation ?? s, brightness ?? v);
        }
    }
}
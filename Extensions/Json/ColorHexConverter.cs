// Created by LunarEclipse on 2024-09-29 17:09.
using System;
using System.Drawing;
using Newtonsoft.Json;
using System.Globalization;


namespace USEN.Games.Roulette
{
    public class ColorHexConverter : JsonConverter<Color>
    {
        public override void WriteJson(JsonWriter writer, Color value, JsonSerializer serializer)
        {
            // Serialize the color as a hex string (e.g., #FF0000 for red)
            string hexColor = $"#{value.A:X2}{value.R:X2}{value.G:X2}{value.B:X2}";
            writer.WriteValue(hexColor);
        }

        public override Color ReadJson(JsonReader reader, Type objectType, Color existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            // Read the hex string and convert it back to a Color object
            string hexColor = (string)reader.Value;

            // Remove the '#' if it's present
            if (hexColor.StartsWith("#"))
            {
                hexColor = hexColor.Substring(1);
            }

            // Parse the hex string into RGB components
            if (hexColor.Length == 6) // RGB
            {
                byte r = byte.Parse(hexColor.Substring(0, 2), NumberStyles.HexNumber);
                byte g = byte.Parse(hexColor.Substring(2, 4).Substring(0, 2), NumberStyles.HexNumber);
                byte b = byte.Parse(hexColor.Substring(4, 6).Substring(0, 2), NumberStyles.HexNumber);
                return Color.FromArgb(r, g, b);
            }
            else if (hexColor.Length == 8) // ARGB
            {
                byte a = byte.Parse(hexColor.Substring(0, 2), NumberStyles.HexNumber);
                byte r = byte.Parse(hexColor.Substring(2, 4).Substring(0, 2), NumberStyles.HexNumber);
                byte g = byte.Parse(hexColor.Substring(4, 6).Substring(0, 2), NumberStyles.HexNumber);
                byte b = byte.Parse(hexColor.Substring(6, 8).Substring(0, 2), NumberStyles.HexNumber);
                return Color.FromArgb(a, r, g, b);
            }

            throw new Exception("Invalid hex color format.");
        }
    }

    public class ColorArgbConverter : JsonConverter<Color>
    {
        public override void WriteJson(JsonWriter writer, Color value, JsonSerializer serializer)
        {
            // Serialize the color as a argb string (e.g., "255,255,255,255" for white)
            string argbColor = $"{value.A},{value.R},{value.G},{value.B}";
            writer.WriteValue(argbColor);
        }

        public override Color ReadJson(JsonReader reader, Type objectType, Color existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            // Read the argb string and convert it back to a Color object
            string argbColor = (string)reader.Value;
            argbColor = argbColor.Replace(" ", "");

            // Parse the argb string into ARGB components
            string[] components = argbColor.Split(',');
            if (components.Length == 4)
            {
                byte a = byte.Parse(components[0]);
                byte r = byte.Parse(components[1]);
                byte g = byte.Parse(components[2]);
                byte b = byte.Parse(components[3]);
                return Color.FromArgb(a, r, g, b);
            }

            throw new Exception("Invalid argb color format.");
        }
    }
}
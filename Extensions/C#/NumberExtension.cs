// Created by LunarEclipse on 2024-01-07 0:55.

namespace Luna.Extensions
{
    public static class NumberExtension
    {
        public static float Mod(this float x, float m)
        {
            return (x % m + m) % m;
        }
        
        public static double Mod(this double x, double m)
        {
            return (x % m + m) % m;
        }
        
        public static int Mod(this int x, int m)
        {
            return (x % m + m) % m;
        }
        
        public static long Mod(this long x, long m)
        {
            return (x % m + m) % m;
        }
        
        public static uint Mod(this uint x, uint m)
        {
            return (x % m + m) % m;
        }
        
        public static ulong Mod(this ulong x, ulong m)
        {
            return (x % m + m) % m;
        }
        
        public static decimal Mod(this decimal x, decimal m)
        {
            return (x % m + m) % m;
        }
        
        public static byte Mod(this byte x, byte m)
        {
            return (byte)((x % m + m) % m);
        }
        
        public static sbyte Mod(this sbyte x, sbyte m)
        {
            return (sbyte)((x % m + m) % m);
        }
        
        public static short Mod(this short x, short m)
        {
            return (short)((x % m + m) % m);
        }
        
        public static ushort Mod(this ushort x, ushort m)
        {
            return (ushort)((x % m + m) % m);
        }
    }
}
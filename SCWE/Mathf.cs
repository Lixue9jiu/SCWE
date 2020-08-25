using System;
using System.Runtime.CompilerServices;

namespace SCWE
{
    public static class Mathf
    {
        public const float PI = (float)Math.PI;
        public const float Rad = (float)(Math.PI / 180f);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Sin(float f)
        {
            return (float)Math.Sin(f);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Cos(float f)
        {
            return (float)Math.Cos(f);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Clamp(float value, float min, float max)
        {
            if (value < min) return min;

            return value > max ? max : value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Max(float a, float b)
        {
            return a > b ? a : b;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Max(float a, float b, float c)
        {
            return Max(Max(a, b), c);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Max(params float[] fs)
        {
            float result = float.NegativeInfinity;
            foreach (float f in fs)
            {
                if (f > result)
                    result = f;
            }
            return result;
        }
    }
}

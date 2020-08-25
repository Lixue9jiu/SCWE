using System;

namespace SCWE
{
    public struct Vector2
    {
        public float x;
        public float y;

        public Vector2(float x, float y)
        {
            this.x = x;
            this.y = y;
        }

        public static Vector2 operator +(Vector2 a, Vector2 b)
        {
            return new Vector2(a.x + b.x, a.y + b.y);
        }

        public static Vector2 operator -(Vector2 a, Vector2 b)
        {
            return new Vector2(a.x - b.x, a.y - b.y);
        }

        public static float Distance(Vector2 a, Vector2 b)
        {
            var aMb = a - b;
            return (float)Math.Sqrt(aMb.x * aMb.x + aMb.y * aMb.y);
        }
    }
}

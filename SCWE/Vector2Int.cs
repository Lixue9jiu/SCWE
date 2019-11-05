using System;
using System.Collections.Generic;
using System.Text;

namespace SCWE
{
    public struct Vector2Int
    {
        public int x;
        public int y;

        public Vector2Int(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public override int GetHashCode()
        {
            return x + y << 16;
        }

        public override string ToString()
        {
            return string.Format("{0}, {1}", x, y);
        }

        public static implicit operator Vector2(Vector2Int v) => new Vector2(v.x, v.y);
        public static explicit operator Vector2Int(Vector2 v) => new Vector2Int((int)v.x, (int)v.y);
    }
}

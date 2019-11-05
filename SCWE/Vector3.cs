using System;
using System.Collections.Generic;
using System.Text;

namespace SCWE
{
    public struct Vector3
    {
        public float x;
        public float y;
        public float z;

        public static Vector3 zero => new Vector3();
        public static Vector3 left => new Vector3(1, 0, 0);
        public static Vector3 up => new Vector3(0, 1, 0);
        public static Vector3 backward => new Vector3(0, 0, 1);
        public static Vector3 one => new Vector3(1, 1, 1);

        public Vector3(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public static Vector3 operator +(Vector3 a, Vector3 b)
        {
            return new Vector3(a.x + b.x, a.y + b.y, a.z + b.z);
        }

        public static Vector3 operator /(Vector3 a, float f)
        {
            return new Vector3(a.x / f, a.y / f, a.z / f);
        }

        public override string ToString()
        {
            return string.Format("{0}, {1}, {2}", x, y, z);
        }
    }
}

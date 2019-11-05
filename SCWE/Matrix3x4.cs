using System;
using System.Collections.Generic;
using System.Text;

namespace SCWE
{
    public struct Matrix3x4
    {
        float M00;
        float M01;
        float M02;
        float M03;
        float M10;
        float M11;
        float M12;
        float M13;
        float M20;
        float M21;
        float M22;
        float M23;

        public Matrix3x4 Identity => new Matrix3x4(
            1, 0, 0, 0,
            0, 1, 0, 0,
            0, 0, 1, 0
        );

        public Matrix3x4(
            float m00, float m01, float m02, float m03,
            float m10, float m11, float m12, float m13,
            float m20, float m21, float m22, float m23)
        {
            M00 = m00;
            M01 = m01;
            M02 = m02;
            M03 = m03;
            M10 = m10;
            M11 = m11;
            M12 = m12;
            M13 = m13;
            M20 = m20;
            M21 = m21;
            M22 = m22;
            M23 = m23;
        }

        public static Matrix3x4 Transform(Vector3 pos)
        {
            return new Matrix3x4(
                1, 0, 0, pos.x,
                0, 1, 0, pos.y,
                0, 0, 1, pos.z);
        }

        public static Vector3 operator *(Matrix3x4 m, Vector3 p)
        {
            return new Vector3(
                m.M00 * p.x + m.M01 * p.y + m.M02 * p.z + m.M03,
                m.M10 * p.x + m.M11 * p.y + m.M12 * p.z + m.M13,
                m.M20 * p.x + m.M21 * p.y + m.M22 * p.z + m.M23);
        }
    }
}
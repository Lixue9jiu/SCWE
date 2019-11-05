using System;
using System.Collections.Generic;
using System.Text;
using System.Numerics;

namespace SCWE
{
    public struct Matrix4x4
    {
        float M11;
        float M12;
        float M13;
        float M14;
        float M21;
        float M22;
        float M23;
        float M24;
        float M31;
        float M32;
        float M33;
        float M34;
        float M41;
        float M42;
        float M43;
        float M44;

        public static Matrix4x4 Identity => new Matrix4x4(
            1, 0, 0, 0,
            0, 1, 0, 0,
            0, 0, 1, 0,
            0, 0, 0, 1
        );

        public Matrix4x4 Inverse
        {
            get
            {
                Invert(this, out Matrix4x4 result);
                return result;
            }
        }

        public Matrix4x4(
            float m00, float m01, float m02, float m03,
            float m10, float m11, float m12, float m13,
            float m20, float m21, float m22, float m23,
            float m30, float m31, float m32, float m33)
        {
            M11 = m00;
            M12 = m01;
            M13 = m02;
            M14 = m03;
            M21 = m10;
            M22 = m11;
            M23 = m12;
            M24 = m13;
            M31 = m20;
            M32 = m21;
            M33 = m22;
            M34 = m23;
            M41 = m30;
            M42 = m31;
            M43 = m32;
            M44 = m33;
        }

        public static Matrix4x4 Translate(Vector3 pos)
        {
            return new Matrix4x4(
                1, 0, 0, pos.x,
                0, 1, 0, pos.y,
                0, 0, 1, pos.z,
                0, 0, 0, 1);
        }

        public static Matrix4x4 Scale(Vector3 scale)
        {
            return new Matrix4x4(
                scale.x, 0, 0, 0,
                0, scale.y, 0, 0,
                0, 0, scale.z, 0,
                0, 0, 0, 1);
        }

        public static Matrix4x4 Euler(float x, float y, float z)
        {
            x *= Mathf.Rad;
            y *= Mathf.Rad;
            z *= Mathf.Rad;
            return new Matrix4x4(
                Mathf.Cos(z), -Mathf.Sin(z), 0, 0,
                Mathf.Sin(z), Mathf.Cos(z), 0, 0,
                0, 0, 1, 0,
                0, 0, 0, 1) * 
                new Matrix4x4(
                    Mathf.Cos(y), 0, Mathf.Sin(y), 0,
                    0, 1, 0, 0,
                    -Mathf.Sin(y), 0, Mathf.Cos(y), 0,
                    0, 0, 0, 1) * 
                    new Matrix4x4(
                        1, 0, 0, 0,
                        0, Mathf.Cos(x), -Mathf.Sin(x), 0,
                        0, Mathf.Sin(x), Mathf.Cos(x), 0,
                        0, 0, 0, 1);
        }

        public static Matrix4x4 TRS(Vector3 translate, Vector3 euler, Vector3 scale)
        {
            return Translate(translate) * Euler(euler.x, euler.y, euler.z) * Scale(scale);
        }

        // axis-angles rotation, the angle is given in radius
        public static Matrix4x4 Rotation(Vector3 axis, float angle)
        {
            angle *= Mathf.Rad;
            float cos = (float)Math.Cos(angle);
            float sin = (float)Math.Sin(angle);
            float cos1 = 1 - cos;
            return new Matrix4x4
            {
                M11 = cos + axis.x * axis.x * cos1,
                M12 = axis.x * axis.y * cos1 - axis.z * sin,
                M13 = axis.x * axis.z * cos1 + axis.y * sin,
                M21 = axis.y * axis.x * cos1 + axis.z * sin,
                M22 = cos + axis.y * axis.y * cos1,
                M23 = axis.y * axis.z * cos1 - axis.x * sin,
                M31 = axis.z * axis.x * cos1 - axis.y * sin,
                M32 = axis.z * axis.y * cos1 + axis.x * sin,
                M33 = cos + axis.z * axis.z * cos1,
                M44 = 1
            };
        }

        public static Vector3 MultiplyPosition(Matrix4x4 m, Vector3 p)
        {
            float w = m.M41 * p.x + m.M42 * p.y + m.M43 * p.z + m.M44;
            return new Vector3(m.M11 * p.x + m.M12 * p.y + m.M13 * p.z + m.M14,
                                m.M21 * p.x + m.M22 * p.y + m.M23 * p.z + m.M24,
                                m.M31 * p.x + m.M32 * p.y + m.M33 * p.z + m.M34) / w;
        }

        public static Vector3 MultiplyPosition3x4(Matrix4x4 m, Vector3 p)
        {
            return new Vector3(m.M11 * p.x + m.M12 * p.y + m.M13 * p.z + m.M14,
                                m.M21 * p.x + m.M22 * p.y + m.M23 * p.z + m.M24,
                                m.M31 * p.x + m.M32 * p.y + m.M33 * p.z + m.M34);
        }

        public static implicit operator Matrix3x4(Matrix4x4 m) => new Matrix3x4(
            m.M11, m.M12, m.M13, m.M14,
            m.M21, m.M22, m.M23, m.M24,
            m.M31, m.M32, m.M33, m.M34);

        public static Matrix4x4 operator *(Matrix4x4 a, Matrix4x4 b)
        {
            return Multiply(a, b);
        }

        public static Matrix4x4 Multiply(Matrix4x4 value1, Matrix4x4 value2)
        {
            var result = new Matrix4x4
            {
                M11 = value1.M11 * value2.M11 + value1.M12 * value2.M21 + value1.M13 * value2.M31 +
                      value1.M14 * value2.M41,
                M12 = value1.M11 * value2.M12 + value1.M12 * value2.M22 + value1.M13 * value2.M32 +
                      value1.M14 * value2.M42,
                M13 = value1.M11 * value2.M13 + value1.M12 * value2.M23 + value1.M13 * value2.M33 +
                      value1.M14 * value2.M43,
                M14 = value1.M11 * value2.M14 + value1.M12 * value2.M24 + value1.M13 * value2.M34 +
                      value1.M14 * value2.M44,
                M21 = value1.M21 * value2.M11 + value1.M22 * value2.M21 + value1.M23 * value2.M31 +
                      value1.M24 * value2.M41,
                M22 = value1.M21 * value2.M12 + value1.M22 * value2.M22 + value1.M23 * value2.M32 +
                      value1.M24 * value2.M42,
                M23 = value1.M21 * value2.M13 + value1.M22 * value2.M23 + value1.M23 * value2.M33 +
                      value1.M24 * value2.M43,
                M24 = value1.M21 * value2.M14 + value1.M22 * value2.M24 + value1.M23 * value2.M34 +
                      value1.M24 * value2.M44,
                M31 = value1.M31 * value2.M11 + value1.M32 * value2.M21 + value1.M33 * value2.M31 +
                      value1.M34 * value2.M41,
                M32 = value1.M31 * value2.M12 + value1.M32 * value2.M22 + value1.M33 * value2.M32 +
                      value1.M34 * value2.M42,
                M33 = value1.M31 * value2.M13 + value1.M32 * value2.M23 + value1.M33 * value2.M33 +
                      value1.M34 * value2.M43,
                M34 = value1.M31 * value2.M14 + value1.M32 * value2.M24 + value1.M33 * value2.M34 +
                      value1.M34 * value2.M44,
                M41 = value1.M41 * value2.M11 + value1.M42 * value2.M21 + value1.M43 * value2.M31 +
                      value1.M44 * value2.M41,
                M42 = value1.M41 * value2.M12 + value1.M42 * value2.M22 + value1.M43 * value2.M32 +
                      value1.M44 * value2.M42,
                M43 = value1.M41 * value2.M13 + value1.M42 * value2.M23 + value1.M43 * value2.M33 +
                      value1.M44 * value2.M43,
                M44 = value1.M41 * value2.M14 + value1.M42 * value2.M24 + value1.M43 * value2.M34 +
                      value1.M44 * value2.M44
            };
            return result;
        }

        public static bool Invert(Matrix4x4 matrix, out Matrix4x4 result)
        {
            float m = matrix.M11;
            float m2 = matrix.M12;
            float m3 = matrix.M13;
            float m4 = matrix.M14;
            float m5 = matrix.M21;
            float m6 = matrix.M22;
            float m7 = matrix.M23;
            float m8 = matrix.M24;
            float m9 = matrix.M31;
            float m10 = matrix.M32;
            float m11 = matrix.M33;
            float m12 = matrix.M34;
            float m13 = matrix.M41;
            float m14 = matrix.M42;
            float m15 = matrix.M43;
            float m16 = matrix.M44;
            float num = m11 * m16 - m12 * m15;
            float num2 = m10 * m16 - m12 * m14;
            float num3 = m10 * m15 - m11 * m14;
            float num4 = m9 * m16 - m12 * m13;
            float num5 = m9 * m15 - m11 * m13;
            float num6 = m9 * m14 - m10 * m13;
            float num7 = m6 * num - m7 * num2 + m8 * num3;
            float num8 = -(m5 * num - m7 * num4 + m8 * num5);
            float num9 = m5 * num2 - m6 * num4 + m8 * num6;
            float num10 = -(m5 * num3 - m6 * num5 + m7 * num6);
            float num11 = m * num7 + m2 * num8 + m3 * num9 + m4 * num10;
            if (Math.Abs(num11) < 1.401298E-45f)
            {
                result = new Matrix4x4(float.NaN, float.NaN, float.NaN, float.NaN, float.NaN, float.NaN, float.NaN,
                    float.NaN, float.NaN, float.NaN, float.NaN, float.NaN, float.NaN, float.NaN, float.NaN, float.NaN);
                return false;
            }
            float num12 = 1f / num11;
            result.M11 = num7 * num12;
            result.M21 = num8 * num12;
            result.M31 = num9 * num12;
            result.M41 = num10 * num12;
            result.M12 = -(m2 * num - m3 * num2 + m4 * num3) * num12;
            result.M22 = (m * num - m3 * num4 + m4 * num5) * num12;
            result.M32 = -(m * num2 - m2 * num4 + m4 * num6) * num12;
            result.M42 = (m * num3 - m2 * num5 + m3 * num6) * num12;
            float num13 = m7 * m16 - m8 * m15;
            float num14 = m6 * m16 - m8 * m14;
            float num15 = m6 * m15 - m7 * m14;
            float num16 = m5 * m16 - m8 * m13;
            float num17 = m5 * m15 - m7 * m13;
            float num18 = m5 * m14 - m6 * m13;
            result.M13 = (m2 * num13 - m3 * num14 + m4 * num15) * num12;
            result.M23 = -(m * num13 - m3 * num16 + m4 * num17) * num12;
            result.M33 = (m * num14 - m2 * num16 + m4 * num18) * num12;
            result.M43 = -(m * num15 - m2 * num17 + m3 * num18) * num12;
            float num19 = m7 * m12 - m8 * m11;
            float num20 = m6 * m12 - m8 * m10;
            float num21 = m6 * m11 - m7 * m10;
            float num22 = m5 * m12 - m8 * m9;
            float num23 = m5 * m11 - m7 * m9;
            float num24 = m5 * m10 - m6 * m9;
            result.M14 = -(m2 * num19 - m3 * num20 + m4 * num21) * num12;
            result.M24 = (m * num19 - m3 * num22 + m4 * num23) * num12;
            result.M34 = -(m * num20 - m2 * num22 + m4 * num24) * num12;
            result.M44 = (m * num21 - m2 * num23 + m3 * num24) * num12;
            return true;
        }

    }
}

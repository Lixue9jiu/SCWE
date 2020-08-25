using System;
using System.Collections.Generic;

namespace SCWE
{
    public struct Mesh
    {
        public Vector3[] vertices;
        public uint[] triangles;
        public Vector2[] uv;
        public Color[] colors;

        public static Mesh CreateEmpty()
        {
            return new Mesh
            {
                vertices = new Vector3[0],
                triangles = new uint[0],
                uv = new Vector2[0],
                colors = new Color[0],
            };
        }

        public static Mesh Combine(IEnumerable<Mesh> meshes)
        {
            List<Vector3> vertices = new List<Vector3>();
            List<uint> tri = new List<uint>();
            List<Vector2> uvs = new List<Vector2>();
            List<Color> colors = new List<Color>();

            var iter = meshes.GetEnumerator();
            if (iter.MoveNext())
            {
                uint vertexCount = (uint)iter.Current.vertices.LongLength;
                vertices.AddRange(iter.Current.vertices);
                tri.AddRange(iter.Current.triangles);
                uvs.AddRange(iter.Current.uv);
                colors.AddRange(iter.Current.colors);
                while (iter.MoveNext())
                {
                    var m = iter.Current;
                    vertices.AddRange(m.vertices);
                    var m_tri = m.triangles;
                    var count = m_tri.Length;
                    for (int i = 0; i < count; i++)
                    {
                        tri.Add(m_tri[i] + vertexCount);
                    }
                    uvs.AddRange(m.uv);
                    colors.AddRange(m.colors);
                    vertexCount += (uint)m.vertices.LongLength;
                }
            }

            return new Mesh
            {
                vertices = vertices.ToArray(),
                triangles = tri.ToArray(),
                uv = uvs.ToArray(),
                colors = colors.ToArray()
            };
        }

        public void Append(Mesh data)
        {
            uint vCount = (uint)vertices.LongLength;
            vertices = MergeArray(vertices, data.vertices);

            uint[] tri = new uint[triangles.Length + data.triangles.Length];
            int start = triangles.Length;
            int count = data.triangles.Length;
            System.Array.Copy(triangles, tri, triangles.Length);
            for (int i = 0; i < count; i++)
            {
                tri[start + i] = data.triangles[i] + vCount;
            }
            triangles = tri;

            uv = MergeArray(uv, data.uv);
            colors = MergeArray(colors, data.colors);
        }

        public void FlipWindingOrder()
        {
            uint[] tri = new uint[triangles.Length];
            for (int i = 0; i < tri.Length; i += 3)
            {
                tri[i] = triangles[i];
                tri[i + 1] = triangles[i + 2];
                tri[i + 2] = triangles[i + 1];
            }
            triangles = tri;
        }

        public Mesh Clone()
        {
            return new Mesh
            {
                vertices = CloneArray(vertices),
                triangles = CloneArray(triangles),
                uv = CloneArray(uv),
                colors = CloneArray(colors)
            };
        }

        public Mesh(Mesh m)
        {
            vertices = CloneArray(m.vertices);
            triangles = CloneArray(m.triangles);
            uv = CloneArray(m.uv);
            colors = CloneArray(m.colors);
        }

        public static T[] CloneArray<T>(T[] src)
        {
            T[] dst = new T[src.Length];
            System.Array.Copy(src, dst, src.Length);
            return dst;
        }

        public static T[] MergeArray<T>(T[] a1, T[] a2)
        {
            T[] dst = new T[a1.Length + a2.Length];
            Array.Copy(a1, 0, dst, 0, a1.Length);
            Array.Copy(a2, 0, dst, a1.Length, a2.Length);
            return dst;
        }

        public void Transform(Matrix4x4 matrix)
        {
            for (int i = 0; i < vertices.Length; i++)
            {
                vertices[i] = Matrix4x4.MultiplyPosition3x4(matrix, vertices[i]);
            }
        }

        public void WrapInTextureSlot(int texSlot)
        {
            Vector2 uvPos = new Vector2((texSlot % 16) / 16f, -((texSlot >> 4)) / 16f);

            for (int i = 0; i < uv.Length; i++)
            {
                uv[i] += uvPos;
            }
        }

        public void FlipVertical()
        {
            Vector3[] vec = vertices;
            uint[] tri = triangles;

            for (int i = 0; i < vertices.Length; i++)
            {
                Vector3 v = vec[i];
                v.y = 1 - v.y;
                vec[i] = v;
            }
            for (int i = 0; i < tri.Length; i += 3)
            {
                uint tmp = tri[i];
                tri[i] = tri[i + 1];
                tri[i + 1] = tmp;
            }
        }
    }
}

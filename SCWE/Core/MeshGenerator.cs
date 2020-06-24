using System;
using System.Collections;
using System.Collections.Generic;

namespace SCWE
{
    public class MeshGenerator
    {
        public readonly TerrainMesh TerrainMesh = new TerrainMesh();

        public void GenerateChunkMesh(int chunkx, int chunkz, Terrain terrain)
        {
            int bx = chunkx << TerrainChunk.SizeXShift;
            int bz = chunkz << TerrainChunk.SizeZShift;
            TerrainChunk c20 = terrain.GetChunk(chunkx + 1, chunkz - 1);
            TerrainChunk c21 = terrain.GetChunk(chunkx + 1, chunkz);
            TerrainChunk c22 = terrain.GetChunk(chunkx + 1, chunkz + 1);
            TerrainChunk c10 = terrain.GetChunk(chunkx, chunkz - 1);
            TerrainChunk c11 = terrain.GetChunk(chunkx, chunkz);
            TerrainChunk c12 = terrain.GetChunk(chunkx, chunkz + 1);
            TerrainChunk c00 = terrain.GetChunk(chunkx - 1, chunkz - 1);
            TerrainChunk c01 = terrain.GetChunk(chunkx - 1, chunkz);
            TerrainChunk c02 = terrain.GetChunk(chunkx - 1, chunkz + 1);

            for (int z = 0; z < TerrainChunk.SizeZ; z++)
            {
                for (int x = 0; x < TerrainChunk.SizeX; x++)
                {
                    switch (x)
                    {
                        case 0:
                            if (c01 == null || z == 0 && c00 == null || z == TerrainChunk.SizeZMinusOne && c02 == null) continue;
                            goto default;
                        case 15:
                            if (c21 == null || z == 0 && c20 == null || z == TerrainChunk.SizeZMinusOne && c22 == null) continue;
                            goto default;
                        default:
                            if (z == 0 && c10 == null || z == TerrainChunk.SizeZMinusOne && c12 == null) continue;

                            // if (x == 0) Debug.Log("0");
                            for (int y = 1; y < TerrainChunk.SizeYMinusOne; y++)
                            {
#if DEBUG
                                try
                                {
#endif
                                    GenerateBlockMesh(bx + x, y, bz + z, c11.GetCellValue(bx + x, y, bz + z), terrain);
#if DEBUG
                                }
                                catch (Exception e)
                                {
                                    Console.WriteLine("{0}, {1}, {2}: {3}", bx + x, y, bz + z, c11.GetCellValue(x, y, z));
                                    Console.WriteLine(e);
                                    Console.ReadLine();
                                }
#endif
                            }
                            break;
                    }
                }
            }
        }

        public void GenerateBlockMesh(int x, int y, int z, int value, Terrain terrain)
        {
            int content = TerrainChunk.GetContent(value);
            if (BlocksManager.IsTransparent[content])
            {
                BlocksManager.NormalBlocks[content].GenerateTerrain(x, y, z, value, terrain.GetChunk(x >> TerrainChunk.SizeXShift, z >> TerrainChunk.SizeZShift), this);
            }
            else
            {
                Vector3 v000 = new Vector3(x, y, z);
                Vector3 v001 = new Vector3(x, y, z + 1.0f);
                Vector3 v010 = new Vector3(x, y + 1.0f, z);
                Vector3 v011 = new Vector3(x, y + 1.0f, z + 1.0f);
                Vector3 v100 = new Vector3(x + 1.0f, y, z);
                Vector3 v101 = new Vector3(x + 1.0f, y, z + 1.0f);
                Vector3 v110 = new Vector3(x + 1.0f, y + 1.0f, z);
                Vector3 v111 = new Vector3(x + 1.0f, y + 1.0f, z + 1.0f);

                TerrainChunk c11 = terrain.GetChunkWithBlock(x, z);
                TerrainChunk c21 = terrain.GetChunkWithBlock(x + 1, z);
                TerrainChunk c01 = terrain.GetChunkWithBlock(x - 1, z);
                TerrainChunk c12 = terrain.GetChunkWithBlock(x, z + 1);
                TerrainChunk c10 = terrain.GetChunkWithBlock(x, z - 1);

                var cellFace = new CellFace();
                var block = BlocksManager.StandardCubeBlocks[content];
                int neighbor = c01.GetCellContent(x - 1, y, z);
                if (BlocksManager.IsTransparent[neighbor])
                {
                    block.GenerateTerrain(x, y, z, value, CellFace.BACK, c01, ref cellFace);
                    TerrainMesh.Quad(v000, v001, v011, v010, cellFace.TextureSlot, cellFace.Color);
                }

                neighbor = c11.GetCellContent(x, y - 1, z);
                if (BlocksManager.IsTransparent[neighbor])
                {
                    block.GenerateTerrain(x, y, z, value, CellFace.BOTTOM, c11, ref cellFace);
                    TerrainMesh.Quad(v001, v000, v100, v101, cellFace.TextureSlot, cellFace.Color);
                }

                neighbor = c10.GetCellContent(x, y, z - 1);
                if (BlocksManager.IsTransparent[neighbor])
                {
                    block.GenerateTerrain(x, y, z, value, CellFace.LEFT, c10, ref cellFace);
                    TerrainMesh.Quad(v100, v000, v010, v110, cellFace.TextureSlot, cellFace.Color);
                }

                neighbor = c21.GetCellContent(x + 1, y, z);
                if (BlocksManager.IsTransparent[neighbor])
                {
                    block.GenerateTerrain(x, y, z, value, CellFace.FRONT, c21, ref cellFace);
                    TerrainMesh.Quad(v101, v100, v110, v111, cellFace.TextureSlot, cellFace.Color);
                }

                neighbor = c11.GetCellContent(x, y + 1, z);
                if (BlocksManager.IsTransparent[neighbor])
                {
                    block.GenerateTerrain(x, y, z, value, CellFace.TOP, c11, ref cellFace);
                    TerrainMesh.Quad(v011, v111, v110, v010, cellFace.TextureSlot, cellFace.Color);
                }

                neighbor = c12.GetCellContent(x, y, z + 1);
                if (BlocksManager.IsTransparent[neighbor])
                {
                    block.GenerateTerrain(x, y, z, value, CellFace.RIGHT, c12, ref cellFace);
                    TerrainMesh.Quad(v001, v101, v111, v011, cellFace.TextureSlot, cellFace.Color);
                }
            }
        }

        public static void GenerateFurnitureMesh(Furniture furniture, out Mesh mesh)
        {
            var terrain = new TerrainMesh();
            Block[] blocks = BlocksManager.Blocks;

            int res = furniture.Resolution;
            Matrix4x4 matrix = Matrix4x4.Scale(Vector3.one / res);
            float uvBlockSize = 0.0625f / res;

            int[] mask = new int[res * res];
            int u, v, n, w, h, j, i, l, k;

            int[] off;
            int[] x;

            for (int d = 0; d < 3; d++)
            {
                off = new int[] { 0, 0, 0 };
                x = new int[] { 0, 0, 0 };
                u = (d + 1) % 3;
                v = (d + 2) % 3;

                off[d] = 1;

                for (x[d] = -1; x[d] < res;)
                {
                    //Debug.LogFormat("x[d]: {0}", x[d]);
                    mask = new int[res * res];
                    for (n = 0; n < mask.Length; n++)
                    {
                        mask[n] = -1;
                    }

                    n = 0;
                    for (x[v] = 0; x[v] < res; x[v]++)
                    {
                        for (x[u] = 0; x[u] < res; x[u]++)
                        {
                            int va = furniture.GetCellValue(x[0], x[1], x[2]);
                            int vb = furniture.GetCellValue(x[0] + off[0], x[1] + off[1], x[2] + off[2]);
                            int ia = TerrainChunk.GetContent(va);
                            int ib = TerrainChunk.GetContent(vb);
                            //Debug.LogFormat("{0} and {1}: {2}, {3}", new Point3(x[0], x[1], x[2]), new Point3(x[0] + off[0], x[1] + off[1], x[2] + off[2]), a.Name, b.Name);
                            if (ia == 0 && ib != 0)
                            {
                                Block b = blocks[ib];
                                mask[n] = b.TextureSlot;
                                mask[n] |= BlocksManager.GetBlockColorInt(b, vb) << 8;
                                mask[n] |= 4096;
                            }
                            else if (ia != 0 && ib == 0)
                            {
                                Block a = blocks[ia];
                                mask[n] = a.TextureSlot;
                                mask[n] |= BlocksManager.GetBlockColorInt(a, va) << 8;
                            }
                            n++;
                        }
                    }

                    ++x[d];
                    n = 0;
                    for (j = 0; j < res; j++)
                    {
                        for (i = 0; i < res;)
                        {
                            if (mask[n] != -1)
                            {
                                for (w = 1; i + w < res && mask[w + n] == mask[n]; w++)
                                {
                                }
                                for (h = 1; h + j < res; h++)
                                {
                                    for (k = 0; k < w; k++)
                                    {
                                        if (mask[n + k + h * res] != mask[n])
                                        {
                                            goto Done;
                                        }
                                    }
                                }
                            Done:
                                //Debug.LogFormat("quard: {0}, {1}, {2}, {3}; {4}", j, i, h, w, x[d]);

                                x[u] = i;
                                x[v] = j;
                                int[] du = new int[] { 0, 0, 0 };
                                int[] dv = new int[] { 0, 0, 0 };
                                du[u] = w;
                                dv[v] = h;

                                int textureSlot = mask[n] & 255;
                                if (mask[n] >> 12 == 0)
                                {
                                    terrain.FurnitureQuad(
                                        new Vector3(x[0], x[1], x[2]),
                                        new Vector3(x[0] + du[0], x[1] + du[1], x[2] + du[2]),
                                        new Vector3(x[0] + du[0] + dv[0], x[1] + du[1] + dv[1], x[2] + du[2] + dv[2]),
                                        new Vector3(x[0] + dv[0], x[1] + dv[1], x[2] + dv[2]),
                                        x[u], x[v], w, h, uvBlockSize,
                                        textureSlot,
                                        BlocksManager.DEFAULT_COLORS[(mask[n] >> 8) & 15]
                                    );
                                }
                                else
                                {
                                    terrain.FurnitureQuad(
                                        new Vector3(x[0], x[1], x[2]),
                                        new Vector3(x[0] + dv[0], x[1] + dv[1], x[2] + dv[2]),
                                        new Vector3(x[0] + du[0] + dv[0], x[1] + du[1] + dv[1], x[2] + du[2] + dv[2]),
                                        new Vector3(x[0] + du[0], x[1] + du[1], x[2] + du[2]),
                                        x[v], x[u], h, w, uvBlockSize,
                                        textureSlot,
                                        BlocksManager.DEFAULT_COLORS[(mask[n] >> 8) & 15]
                                    );
                                }

                                for (l = 0; l < h; l++)
                                {
                                    for (k = 0; k < w; k++)
                                    {
                                        mask[n + k + l * res] = -1;
                                    }
                                }
                                i += w;
                                n += w;
                            }
                            else
                            {
                                i++;
                                n++;
                            }
                        }
                    }
                }
            }

            //Debug.LogFormat("{0}, {1}, {2}", vertices.Count, colors.Count, uvs.Count);

            mesh = terrain.PushToMesh();
            mesh.Transform(matrix);
        }
    }

    public struct CellFace
    {
        public const int FRONT = 0;
        public const int TOP = 1;
        public const int RIGHT = 2;
        public const int BACK = 3;
        public const int BOTTOM = 4;
        public const int LEFT = 5;

        public int TextureSlot;
        public bool IsOpposite;
        public Color Color;

        public static int[] opposite = { 3, 4, 5, 0, 1, 2 };

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return (obj is CellFace) && Equals((CellFace)obj);
        }

        public bool Equals(CellFace face)
        {
            return face.TextureSlot == TextureSlot && face.IsOpposite == IsOpposite && face.Color == Color;
        }

        public static bool operator ==(CellFace a, CellFace b)
        {
            return Equals(a, b);
        }

        public static bool operator !=(CellFace a, CellFace b)
        {
            return !Equals(a, b);
        }
    }
}
using System;
using System.Collections.Generic;
using System.Text;

namespace SCWE
{
    public class SingleThreadGenerationManager : IMeshGenerationManager
    {
        public IEnumerator<TerrainMesh> GenerateMeshes(int chunkx, int chunkz, int radius)
        {
            var terrain = WorldManager.World.Terrain;
            MeshGenerator generator = new MeshGenerator();
            foreach (var pos in ChunksInRange(terrain, chunkx, chunkz, radius))
            {
                generator.GenerateChunkMesh(pos.x, pos.y, terrain);
                yield return generator.TerrainMesh;
            }
        }

        public static IEnumerable<Vector2Int> ChunksInRange(Terrain terrain, int chunkx, int chunkz, int radius)
        {
            Dictionary<Vector2Int, byte> masks = new Dictionary<Vector2Int, byte>();

            var center = new Vector2Int(chunkx, chunkz);
            foreach (var pos in MeshGenerator.SpiralIter(center, radius))
            {
                if (!masks.ContainsKey(pos))
                {
                    AddMask(masks, pos, terrain);
                }

                if (terrain.ChunkExists(pos.x, pos.y) && Vector2.Distance(pos, center) < radius)
                {
                    foreach (var offset in MeshGenerator.neighbors)
                    {
                        var pos2 = pos + offset;
                        terrain.LoadChunk(pos2.x, pos2.y);
                    }
                    terrain.LoadChunk(pos.x, pos.y);
                    yield return pos;
                }

                for (int i = 0; i < MeshGenerator.neighbors.Length; i++)
                {
                    var pos2 = pos + MeshGenerator.neighbors[i];
                    if (!masks.ContainsKey(pos2))
                    {
                        AddMask(masks, pos2, terrain);
                    }
                    masks[pos2] |= (byte)(1 << MeshGenerator.opposite[i]);

                    if (masks[pos2] == 0xff)
                    {
                        terrain.DisposeChunk(pos2.x, pos2.y);
                    }
                }
            }
        }

        private static void AddMask(Dictionary<Vector2Int, byte> masks, Vector2Int pos, Terrain terrain)
        {
            int mask = 0;
            for (int i = 0; i < MeshGenerator.neighbors.Length; i++)
            {
                var pos2 = pos + MeshGenerator.neighbors[i];
                if (!terrain.ChunkExists(pos2.x, pos2.y))
                {
                    mask |= (byte)(1 << i);
                }
            }
            masks.Add(pos, 0);
        }

        public void GenerateMeshes(int chunkx, int chunkz, int radius, int maxVertexCount, Action progress, Action<Mesh> callback)
        {
            var iter = GenerateMeshes(chunkx, chunkz, radius);
            while (iter.MoveNext())
            {
                progress();
                if (iter.Current.VertexCount > maxVertexCount)
                {
                    callback(iter.Current.ToMesh());
                    iter.Current.Clear();
                }
            }
            if (iter.Current.VertexCount > 0)
            {
                callback(iter.Current.ToMesh());
            }
        }

        public bool PollEvents()
        {
            return false;
        }
    }
}

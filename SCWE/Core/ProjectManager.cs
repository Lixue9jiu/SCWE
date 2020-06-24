using System;
using System.Collections.Generic;
using System.IO;

namespace SCWE
{
    public static class ProjectManager
    {
        const int VertexCountThreshold = int.MaxValue - 16 * 16 * 128;

        public struct Config
        {
            public string dataPath;
        }

        public static string DataPath { get; private set; }

        public static void Initialize(Config config)
        {
            DataPath = config.dataPath;
            BlockMeshesManager.LoadAllMeshes(config.dataPath);
        }

        // user is responsable for creating an writable dir, passing in as tmpDir
        public static void LoadWorld(string tmpDir, string worldPath)
        {
            using (Stream s = File.OpenRead(worldPath))
            {
                WorldManager.LoadWorld(tmpDir, s);
            }
        }

        public static void GenerateMesh(int chunkx, int chunkz, int radius, string outputDir)
        {
            if (!Directory.Exists(outputDir))
                Directory.CreateDirectory(outputDir);
            IEnumerator<TerrainMesh> ms = GenerateMesh(chunkx, chunkz, radius);

            Console.Write("generating chunk mesh...");
            var cursorPos = new Vector2Int(Console.CursorLeft, Console.CursorTop);

            int count = 1;
            if (ms.MoveNext())
            {
                TerrainMesh last = ms.Current;
                List<Mesh> output = new List<Mesh>();
                while (ms.MoveNext())
                {
                    Console.Write(++count);
                    Console.SetCursorPosition(cursorPos.x, cursorPos.y);

                    if (ms.Current.VertexCount > VertexCountThreshold)
                    {
                        output.Add(last.PushToMesh());
                    }
                }
                output.Add(last.PushToMesh());

                for (int i = 0; i < output.Count; i++)
                {
                    output[i].Transform(new Matrix4x4(
                        1, 0, 0, 0,
                        0, 0, 1, 0,
                        0, 1, 0, 0,
                        0, 0, 0, 1) * Matrix4x4.Translate(new Vector3(-(chunkx << TerrainChunk.SizeXShift), 0, -(chunkz << TerrainChunk.SizeZShift))));
                    using (Stream s = File.OpenWrite(Path.Combine(outputDir, i + ".ply")))
                    {
                        ModelExporter.ExportPly(output[i], s);
                    }
                }
            }
        }

        public static int CheckChunkCount(int chunkx, int chunkz, int radius)
        {
            var terrain = WorldManager.World.Terrain;

            int count = 0;
            var center = new Vector2(chunkx, chunkz);
            int startx = chunkx - radius;
            int startz = chunkz - radius;
            int size = 2 * radius;

            for (int z = 0; z < size; z++)
            {
                for (int x = 0; x < size; x++)
                {
                    var pos = new Vector2Int(x + startx, z + startz);
                    if (terrain.ChunkExists(x + startx, z + startz) && Vector2.Distance(pos, center) < radius)
                        count++;
                }
            }

            return count;
        }

        public static IEnumerator<TerrainMesh> GenerateMesh(int chunkx, int chunkz, int radius)
        {
            var terrain = WorldManager.World.Terrain;
            terrain.DisposeChunksOutOfRadius(chunkx, chunkz, radius);

            var center = new Vector2(chunkx, chunkz);
            int startx = chunkx - radius;
            int startz = chunkz - radius;
            int size = 2 * radius;

            for (int z = 0; z < size; z++)
            {
                for (int x = 0; x < size; x++)
                {
                    var pos = new Vector2Int(x + startx, z + startz);
                    if (!terrain.ChunkLoaded(pos.x, pos.y) && Vector2.Distance(pos, center) < radius)
                        terrain.LoadChunk(pos.x, pos.y);
                }
            }

            MeshGenerator generator = new MeshGenerator();
            for (int z = 0; z < size; z++)
            {
                for (int x = 0; x < size; x++)
                {
                    var pos = new Vector2Int(x + startx, z + startz);
                    if (terrain.ChunkLoaded(pos.x, pos.y) && Vector2.Distance(pos, center) < radius)
                    {
                        generator.GenerateChunkMesh(pos.x, pos.y, terrain);
                        yield return generator.TerrainMesh;
                    }
                }
            }
        }

        public static void Dispose()
        {
            WorldManager.World?.Dispose();
        }
    }
}

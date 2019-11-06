using System;
using System.Collections.Generic;
using System.IO;

namespace SCWE
{
    public static class ProjectManager
    {
        public struct Config
        {
            public string blocksDataPath;
            public string blockMeshesPath;
        }

        public static void Initialize(Config config)
        {
            BlockMeshesManager.LoadAllMeshes(config.blockMeshesPath);
            using (Stream s = File.OpenRead(config.blocksDataPath))
            {
                BlocksManager.Initialize(s);
            }
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
            Mesh[] ms = GenerateMesh(chunkx, chunkz, radius);
            for (int i = 0; i < ms.Length; i++)
            {
                ms[i].Transform(new Matrix4x4(
                        1, 0, 0, 0,
                        0, 0, 1, 0,
                        0, 1, 0, 0,
                        0, 0, 0, 1) * Matrix4x4.Translate(new Vector3(-(chunkx << TerrainChunk.SizeXShift), 0, -(chunkz << TerrainChunk.SizeZShift))));
            }

            if (ms.Length > 0)
            {
                List<Mesh> list = new List<Mesh>(ms);
                Console.WriteLine(list.Count);
                List<Mesh> output = new List<Mesh>();
                while (list.Count > 1)
                {
                    if (list[0].vertices.LongLength + list[1].vertices.LongLength > uint.MaxValue)
                    {
                        output.Add(list[0]);
                        list.RemoveAt(0);
                    }
                    else
                    {
                        var m = list[0];
                        m.Append(list[1]);
                        list[0] = m;
                        list.RemoveAt(1);
                    }
                }
                output.Add(list[0]);

                for (int i = 0; i < output.Count; i++)
                {
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

        public static Mesh[] GenerateMesh(int chunkx, int chunkz, int radius)
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

            List<Mesh> result = new List<Mesh>();
            MeshGenerator generator = new MeshGenerator();
            for (int z = 0; z < size; z++)
            {
                for (int x = 0; x < size; x++)
                {
                    var pos = new Vector2Int(x + startx, z + startz);
                    if (terrain.ChunkLoaded(pos.x, pos.y) && Vector2.Distance(pos, center) < radius)
                    {
                        generator.GenerateChunkMesh(pos.x, pos.y, terrain);
                        Console.WriteLine($"generated chunk mesh: {pos.x}, {pos.y}");
                        result.Add(generator.TerrainMesh.PushToMesh());
                    }
                }
            }
            return result.ToArray();
        }

        public static void Dispose()
        {
            WorldManager.World.Dispose();
        }
    }
}

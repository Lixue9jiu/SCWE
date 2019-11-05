using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using SCWE;

namespace SCWEWindows
{
    class Test
    {
        static void TestMain(string[] args)
        {
            //MeshTest();
            //ChunkTest();
            //BlockLoadingTest();
            //MeshImportTest();
            WorldLoadingTest();
        }

        static void MeshImportTest()
        {
            Mesh m = ModelImporter.ImportPly("outputb.ply");
            ModelExporter.ExportPly(m, "outoutputb.ply", ModelExporter.DefaultOptions);
        }

        static void BlockLoadingTest()
        {
            ProjectManager.Initialize(new ProjectManager.Config { blocksDataPath = "../../data/BlocksData.csv", blockMeshesPath = "../../data" });
            Console.ReadLine();
        }

        static void WorldLoadingTest()
        {
            ProjectManager.Initialize(new ProjectManager.Config { blocksDataPath = "../../data/BlocksData.csv", blockMeshesPath = "../../data" });
            ProjectManager.LoadWorld("temp", "../../data/Lancelot.scworld");
            var chunk = WorldManager.Project.PlayerPosition;
            ProjectManager.GenerateMesh((int)chunk.x >> 4, (int)chunk.z >> 4, 20, "output");
            Console.ReadLine();
        }

        static void ChunkTest()
        {
            ProjectManager.Initialize(new ProjectManager.Config { blocksDataPath = "BlocksData.csv" });
            Terrain terrain = new Terrain();
            TerrainChunk chunk = new TerrainChunk();
            for (int z = 0; z < 16; z++)
            {
                for (int x = 0; x < 16; x++)
                {
                    for (int y = 0; y < 128; y++)
                    {
                        if ((x + y + z) % 2 == 0)
                            chunk.SetCellValue(x, y, z, 1);
                    }
                }
            }
            terrain.SetChunk(0, 0, chunk);
            MeshGenerator g = new MeshGenerator();
            g.GenerateChunkMesh(0, 0, terrain);
            Mesh mesh = g.TerrainMesh.PushToMesh();

            ModelExporter.ExportPly(mesh, "output.ply", ModelExporter.DefaultOptions ^ ModelExporter.Options.UseBinary);
            ModelExporter.ExportPly(mesh, "outputb.ply");
        }

        static void MeshTest()
        {
            TerrainMesh m = new TerrainMesh();
            m.Quad(new Vector3(0, 0, 0), new Vector3(0, 1, 0), new Vector3(1, 1, 0), new Vector3(1, 0, 0), 0, Color.white);
            m.Quad(new Vector3(0, 0, 0), new Vector3(0, 1, 0), new Vector3(0, 1, 1), new Vector3(0, 0, 1), 0, Color.white);
            m.Quad(new Vector3(0, 0, 0), new Vector3(1, 0, 0), new Vector3(1, 0, 1), new Vector3(0, 0, 1), 0, Color.white);
            Mesh mesh = m.PushToMesh();
            ModelExporter.ExportPly(mesh, "output.ply");

            Console.WriteLine(File.ReadAllText("output.ply"));
            Console.ReadLine();
        }
    }
}

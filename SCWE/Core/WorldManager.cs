using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using SCWE.Utils;

namespace SCWE
{
    public static class WorldManager
    {
        public static ProjectData Project;
        public static string ChunkDat;

        public static string CurrentWorldDir;

        public static World World;

        public static event Action OnWorldLoaded;

        public static string CurrentEmbeddedContent
        {
            get
            {
                return Path.Combine(CurrentWorldDir, "EmbeddedContent");
            }
        }

        public static void LoadWorld(string tempFolder, Stream stream)
        {
            string dir = Path.Combine(tempFolder, "World");
            int i = 0;
            while (Directory.Exists(dir + i))
            {
                i++;
            }
            dir += i;
            Directory.CreateDirectory(dir);
            ZipUtils.Unzip(stream, dir);

            CurrentWorldDir = dir;
            if (!File.Exists(Path.Combine(dir, "Project.xml"))) throw new Exception("Project.xml not presented");
            Project = new ProjectData(dir);
            string chunkFileName = Project.Version >= 2.2f ? "Chunks32h.dat" : Project.Version >= 1.29f ? "Chunks32.dat" : "Chunks.dat";
            ChunkDat = Path.Combine(dir, chunkFileName);
            if (!File.Exists(ChunkDat)) 
            {
                Project = null;
                ChunkDat = null;
                throw new Exception("Chunks.dat not presented");
            }

            string blockDataPath = Path.Combine(ProjectManager.DataPath, "BlocksData129.csv");
            if (chunkFileName == "Chunks32h.dat")
            {
                TerrainChunk.SetDimensions(16, 256, 16);
            }
            else
            {
                TerrainChunk.SetDimensions(16, 128, 16);
            }
            Console.WriteLine("initializing blocks...");
            using (var s = File.OpenRead(blockDataPath))
            {
                BlocksManager.Initialize(s);
            }

            Console.WriteLine("loading world...");
            World = new World();
            World.Load();
            OnWorldLoaded?.Invoke();
        }
    }
}

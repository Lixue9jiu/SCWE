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
            if (!File.Exists(Path.Combine(dir, "Project.xml"))) throw new Exception("invalid world file");
            Project = new ProjectData(dir);
            ChunkDat = Path.Combine(dir, Project.Version >= 1.29f ? "Chunks32.dat" : "Chunk.dat");
            if (!File.Exists(ChunkDat)) 
            {
                Project = null;
                ChunkDat = null;
                throw new Exception("invalid world file");
            }

            World = new World();
            World.Load();
            OnWorldLoaded();
        }
    }
}

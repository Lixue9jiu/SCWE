using System.IO;

namespace SCWE
{
    public static class ProjectManager
    {
        public struct Config
        {
            public string DataPath;
            public int VertexCountThreshold;
        }

        public static Config Settings { get; private set; }

        public static void Initialize(Config config)
        {
            Settings = config;
            BlockMeshesManager.LoadAllMeshes(config.DataPath);
        }

        // user is responsable for creating an writable dir, passing in as tmpDir
        public static void LoadWorld(string tmpDir, string worldPath)
        {
            using (Stream s = File.OpenRead(worldPath))
            {
                WorldManager.LoadWorld(tmpDir, s);
            }
        }

        public static void Dispose()
        {
            WorldManager.World?.Dispose();
        }
    }
}

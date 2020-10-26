using SCWE.Utils;
using System;
using System.IO;

namespace SCWE
{
    public enum LoadingResult
    {
        Success,
        ProjectNotFound,
        ChunksDatNotFound,
        OtherError
    }

    public struct LoadingInfo
    {
        public LoadingResult result;
        public string msg;
        public Exception e;
        public float? worldVersion;
        public GameInfo? gameInfo;
    }

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

        public static LoadingInfo LoadWorld(string tempFolder, Stream stream)
        {
            try {
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
                if (!File.Exists(Path.Combine(dir, "Project.xml")))
                {
                    CurrentWorldDir = null;
                    return new LoadingInfo
                    {
                        result = LoadingResult.ProjectNotFound,
                        msg = "Project.xml not presented",
                        worldVersion = null,
                        gameInfo = null
                    };
                }
                Project = new ProjectData(dir);
                string chunkFileName = Project.Version >= 2.2f ? "Chunks32h.dat" : Project.Version >= 1.29f ? "Chunks32.dat" : "Chunks.dat";
                ChunkDat = Path.Combine(dir, chunkFileName);
                if (!File.Exists(ChunkDat))
                {
                    var res = new LoadingInfo
                    {
                        result = LoadingResult.ChunksDatNotFound,
                        msg = chunkFileName + " not presented",
                        worldVersion = Project.Version,
                        gameInfo = Project.GameInfo
                    };
                    CurrentWorldDir = null;
                    Project = null;
                    ChunkDat = null;
                    return res;
                }

                string blockDataPath = Path.Combine(ProjectManager.Settings.DataPath, "BlocksData.csv");
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

                return new LoadingInfo
                {
                    result = LoadingResult.Success,
                    msg = "success",
                    worldVersion = Project.Version,
                    gameInfo = Project.GameInfo
                };
            }
            catch (Exception e)
            {
                var res = new LoadingInfo
                {
                    result = LoadingResult.OtherError,
                    msg = e.Message,
                    worldVersion = Project?.Version,
                    gameInfo = Project?.GameInfo,
                    e = e
                };
                World?.Dispose();
                World = null;
                CurrentWorldDir = null;
                Project = null;
                ChunkDat = null;
                return res;
            }
        }
    }
}

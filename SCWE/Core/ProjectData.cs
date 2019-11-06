using SCWE.Utils;
using System.IO;
using System.Xml.Linq;

namespace SCWE
{
    public class ProjectData
    {
        public readonly float Version;

        public XElement Root;

        public readonly GameInfo GameInfo;

        public static string GetWorldName(string path)
        {
            ProjectData p = new ProjectData(path);
            return p.GetSubsystem("GameInfo").GetValue<string>("WorldName");
        }

        public Vector3 PlayerPosition
        {
            get
            {
                if (Version >= 2.1f)
                {
                    Vector3 v = GetSubsystem("Players")
                    .GetValues("Players")
                    .GetValues("1")
                    .GetValue<Vector3>("SpawnPosition");
                    v.x = -v.x;
                    return v + new Vector3(0, 1.7f, 0);
                }
                else
                {
                    Vector3 v = GetSubsystem("Player").GetValue<Vector3>("SpawnPosition");
                    v.x = -v.x;
                    return v + new Vector3(0, 1.7f, 0);
                }
            }
        }

        public ProjectData(XDocument doc)
        {
            Root = doc.Root;
            Version = float.Parse(Root.Attribute("Version").Value);
            GameInfo = new GameInfo(this);
        }

        public ProjectData(string worldPath) : this(XDocument.Load(Path.Combine(worldPath, "Project.xml")))
        {
        }

        public XElement GetSubsystem(string name)
        {
            return XMLUtils.FindValuesByName(Root.Element("Subsystems"), name);
        }
    }

    public struct GameInfo
    {
        public string WorldName;
        public string WorldSeed;
        public string TerrainGenerationMode;
        public int TerrainLevel;
        public int TerrainBlockIndex;
        public int TerrainOceanBlockIndex;
        public int TemperatureOffset;
        public int HumidityOffset;
        public int SeaLevelOffset;
        public int BiomeSize;
        public string BlockTextureName;
        public string[] Colors;

        public GameInfo(ProjectData project)
        {
            XElement e = project.GetSubsystem("GameInfo");
            e.GetValueOrDefault("WorldName", out WorldName, "");
            e.GetValueOrDefault("WorldSeedString", out WorldSeed, "");
            e.GetValueOrDefault("TerrainGenerationMode", out TerrainGenerationMode, "");
            e.GetValueOrDefault("TerrainLevel", out TerrainLevel);
            e.GetValueOrDefault("TerrainBlockIndex", out TerrainBlockIndex);
            e.GetValueOrDefault("TerrainOceanBlockIndex", out TerrainOceanBlockIndex);
            e.GetValueOrDefault("TemperatureOffset", out TemperatureOffset);
            e.GetValueOrDefault("HumidityOffset", out HumidityOffset);
            e.GetValueOrDefault("SeaLevelOffset", out SeaLevelOffset);
            e.GetValueOrDefault("BiomeSize", out BiomeSize, 0);
            e.GetValueOrDefault("BlockTextureName", out BlockTextureName, "");

            string str;
            XElement palette = XMLUtils.FindValuesByName(e, "Palette");
            if (palette != null)
            {
                palette.GetValue("Colors", out str);
                Colors = str.Split(';');
            }
            else
            {
                Colors = new string[16];
            }
        }
    }

}

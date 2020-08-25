using System;
using System.Collections.Generic;
using System.IO;

namespace SCWE
{
    public static class BlocksManager
    {
        struct BlockData
        {
            public int Index;
            public string Name;
            public int TextureSlot;
            public string BlockType;
            public string ExtraData;

            public BlockData(string src)
            {
                string[] strs = src.Split(',');
                Index = int.Parse(strs[0]);
                Name = strs[1];
                TextureSlot = int.Parse(strs[2]);
                BlockType = strs[3];
                ExtraData = strs[4];
            }
        }

        public static readonly Color[] DEFAULT_COLORS =
        {
            new Color (255, 255, 255, 255),
            new Color (181, 255, 255, 255),
            new Color (255, 181, 255, 255),
            new Color (160, 181, 255, 255),
            new Color (255, 240, 160, 255),
            new Color (181, 255, 181, 255),
            new Color (255, 181, 160, 255),
            new Color (181, 181, 181, 255),
            new Color (112, 112, 112, 255),
            new Color (32, 112, 112, 255),
            new Color (112, 32, 112, 255),
            new Color (26, 52, 128, 255),
            new Color (87, 54, 31, 255),
            new Color (24, 116, 24, 255),
            new Color (136, 32, 32, 255),
            new Color (24, 24, 24, 255)
        };

        public static readonly Dictionary<int, int> paintedTextures = new Dictionary<int, int>
        {
            {4, 23},
            {70, 39},
            {6, 40},
            {176, 64},
            {7, 51},
            {54, 50},
            {8, 147},
            {16, 69},
            {1, 24},
            {58, 58}
        };

        public static Block[] Blocks { get; private set; }
        public static ICubeBlock[] StandardCubeBlocks { get; private set; }
        public static INormalBlock[] NormalBlocks { get; private set; }

        public static bool[] IsTransparent { get; private set; }

        public static Color ColorFromInt(int? i)
        {
            return i.HasValue ? DEFAULT_COLORS[i.Value] : Color.white;
        }

        public static int GetBlockColorInt(Block block, int value)
        {
            IPaintableBlock paintable = block as IPaintableBlock;
            if (paintable == null)
                return 0;
            return paintable.GetColor(TerrainChunk.GetData(value)) ?? 0;
        }

        public static void Initialize(Stream s)
        {
            if (WorldManager.Project != null)
            {
                string[] strs = WorldManager.Project.GameInfo.Colors;
                for (int i = 0; i < 16; i++)
                {
                    if (!string.IsNullOrEmpty(strs[i]))
                    {
                        string[] rgb = strs[i].Split(',');
                        DEFAULT_COLORS[i] = new Color(byte.Parse(rgb[0]), byte.Parse(rgb[1]), byte.Parse(rgb[2]));
                    }
                }
            }
            Load(s);
        }

        static void Load(Stream s)
        {
            Dictionary<int, BlockData> blockData = new Dictionary<int, BlockData>();
            using (TextReader reader = new StreamReader(s))
            {
                reader.ReadLine();
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    BlockData data = new BlockData(line);
                    blockData[data.Index] = data;
                }
            }

            Dictionary<string, System.Type> definedBlocks = new Dictionary<string, System.Type>();
            foreach (System.Type t in System.Reflection.Assembly.GetAssembly(typeof(Block)).GetTypes())
            {
                if (t.IsSubclassOf(typeof(Block)))
                {
                    definedBlocks.Add(t.Name, t);
                }
            }

            List<Block> b = new List<Block>();
            int i = 0;
            int count = blockData.Count;
            while (i < count)
            {
                if (blockData.ContainsKey(i))
                {
                    Block block = (Block)System.Activator.CreateInstance(definedBlocks[blockData[i].BlockType]);
                    try
                    {
                        InitializeBlock(block, blockData[i]);
                    }
                    catch
                    {
                        Console.WriteLine($"error loading block: {i}, falling back to air");
                        block = new CubeBlock();
                        InitializeBlock(block, blockData[0]);
                    }
                    b.Add(block);
                }
                else
                {
                    Block block = new CubeBlock();
                    InitializeBlock(block, blockData[0]);
                    b.Add(block);
                }
                i++;
            }

            Blocks = b.ToArray();
            Console.WriteLine($"blocks loaded: {Blocks.Length}");

            StandardCubeBlocks = new ICubeBlock[Blocks.Length];
            NormalBlocks = new INormalBlock[Blocks.Length];
            IsTransparent = new bool[Blocks.Length];
            for (int k = 0; k < Blocks.Length; k++)
            {
                var standard = Blocks[k] as ICubeBlock;
                var normal = Blocks[k] as INormalBlock;

                if (standard == null && normal == null)
                    Console.WriteLine("block {0} does not define GenerateTerrain", Blocks[k].GetType().Name);

                StandardCubeBlocks[k] = standard;
                NormalBlocks[k] = normal;
                IsTransparent[k] = standard == null;
            }
        }

        static void InitializeBlock(Block block, BlockData data)
        {
            block.Index = data.Index;
            block.Name = data.Name;
            block.TextureSlot = data.TextureSlot;
            block.Initialize(data.ExtraData);
        }
    }
}

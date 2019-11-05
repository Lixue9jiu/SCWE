namespace SCWE
{
    public abstract class FluidBlock : Block, INormalBlock
    {
        readonly float MaxLevel;
        float[] m_heightByLevel = new float[16];

        protected FluidBlock(int maxLevel)
        {
            MaxLevel = maxLevel;
            for (int i = 0; i < 16; i++)
            {
                float num = 0.875f * Mathf.Clamp(1f - i / MaxLevel, 0f, 1f);
                m_heightByLevel[i] = num;
            }
        }

        public void GenerateFluidTerrain(int x, int y, int z, int value, TerrainChunk chunk, TerrainMesh terrainMesh, Color topColor, Color sideColor)
        {
            float height1, height2, height3, height4;

            int data = TerrainChunk.GetData(value);
            if (GetIsTop(data))
            {
                int cellValueFast = chunk.GetCellValue(x - 1, y, z - 1);
                int cellValueFast2 = chunk.GetCellValue(x, y, z - 1);
                int cellValueFast3 = chunk.GetCellValue(x + 1, y, z - 1);
                int cellValueFast4 = chunk.GetCellValue(x - 1, y, z);
                int cellValueFast5 = chunk.GetCellValue(x + 1, y, z);
                int cellValueFast6 = chunk.GetCellValue(x - 1, y, z + 1);
                int cellValueFast7 = chunk.GetCellValue(x, y, z + 1);
                int cellValueFast8 = chunk.GetCellValue(x + 1, y, z + 1);
                float h = CalculateNeighborHeight(cellValueFast);
                float num = CalculateNeighborHeight(cellValueFast2);
                float h2 = CalculateNeighborHeight(cellValueFast3);
                float num2 = CalculateNeighborHeight(cellValueFast4);
                float num3 = CalculateNeighborHeight(cellValueFast5);
                float h3 = CalculateNeighborHeight(cellValueFast6);
                float num4 = CalculateNeighborHeight(cellValueFast7);
                float h4 = CalculateNeighborHeight(cellValueFast8);
                float levelHeight = GetLevelHeight(GetLevel(data));
                height1 = CalculateFluidVertexHeight(h, num, num2, levelHeight);
                height2 = CalculateFluidVertexHeight(num, h2, levelHeight, num3);
                height3 = CalculateFluidVertexHeight(levelHeight, num3, num4, h4);
                height4 = CalculateFluidVertexHeight(num2, levelHeight, h3, num4);
            }
            else
            {
                height1 = 1f;
                height2 = 1f;
                height3 = 1f;
                height4 = 1f;
            }

            Vector3 v000 = new Vector3(x, y, z);
            Vector3 v001 = new Vector3(x, y, z + 1f);
            Vector3 v010 = new Vector3(x, y + height1, z);
            Vector3 v011 = new Vector3(x, y + height4, z + 1f);
            Vector3 v100 = new Vector3(x + 1.0f, y, z);
            Vector3 v101 = new Vector3(x + 1.0f, y, z + 1f);
            Vector3 v110 = new Vector3(x + 1.0f, y + height2, z);
            Vector3 v111 = new Vector3(x + 1.0f, y + height3, z + 1f);

            int c = chunk.GetCellContent(x - 1, y, z);
            if (c != Index && BlocksManager.IsTransparent[c])
            {
                terrainMesh.Quad(v001, v011, v010, v000, TextureSlot, sideColor);
            }

            c = chunk.GetCellContent(x, y - 1, z);
            if (c != Index && BlocksManager.IsTransparent[c])
            {
                terrainMesh.Quad(v000, v100, v101, v001, TextureSlot, sideColor);
            }

            c = chunk.GetCellContent(x, y, z - 1);
            if (c != Index && BlocksManager.IsTransparent[c])
            {
                terrainMesh.Quad(v000, v010, v110, v100, TextureSlot, sideColor);
            }

            c = chunk.GetCellContent(x + 1, y, z);
            if (c != Index && BlocksManager.IsTransparent[c])
            {
                terrainMesh.Quad(v100, v110, v111, v101, TextureSlot, sideColor);
            }

            c = chunk.GetCellContent(x, y + 1, z);
            if (c != Index && BlocksManager.IsTransparent[c])
            {
                terrainMesh.Quad(v111, v110, v010, v011, TextureSlot, topColor);
            }

            c = chunk.GetCellContent(x, y, z + 1);
            if (c != Index && BlocksManager.IsTransparent[c])
            {
                terrainMesh.Quad(v101, v111, v011, v001, TextureSlot, sideColor);
            }
        }

        public float GetLevelHeight(int level)
        {
            return m_heightByLevel[level];
        }

        public bool IsTheSameFluid(int content)
        {
            return content == Index;
        }

        public static bool GetIsTop(int data)
        {
            return (data & 0x10) != 0;
        }

        public static int GetLevel(int data)
        {
            return data & 0xF;
        }

        float CalculateNeighborHeight(int value)
        {
            int num = TerrainChunk.GetContent(value);
            if (IsTheSameFluid(num))
            {
                int data = TerrainChunk.GetData(value);
                if (GetIsTop(data))
                {
                    return GetLevelHeight(GetLevel(data));
                }
                return 1f;
            }
            if (num == 0)
            {
                return 0.01f;
            }
            return 0f;
        }

        static float CalculateFluidVertexHeight(float h1, float h2, float h3, float h4)
        {
            float num = Mathf.Max(h1, h2, h3, h4);
            if (num < 1f)
            {
                if (h1 != 0.01f && h2 != 0.01f && h3 != 0.01f && h4 != 0.01f)
                {
                    return num;
                }
                return 0f;
            }
            return 1f;
        }

        public abstract void GenerateTerrain(int x, int y, int z, int value, TerrainChunk chunk, MeshGenerator g);
    }
}
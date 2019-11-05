using System.Collections;
using System.Collections.Generic;

namespace SCWE
{
    public class TallGrassBlock : Block, INormalBlock
    {
        public static bool GetIsSmall(int data)
        {
            return (data & 8) != 0;
        }

        public void GenerateTerrain(int x, int y, int z, int value, TerrainChunk chunk, MeshGenerator g)
        {
            int textureSlot = GetIsSmall(TerrainChunk.GetData(value)) ? 84 : 85;
            Color color = GrassBlock.map.Lookup(chunk.GetShiftValue(x, z));

            g.TerrainMesh.TwoSidedQuad(new Vector3(x, y, z),
                                     new Vector3(x + 1.0f, y, z + 1.0f),
                                     new Vector3(x + 1.0f, y + 1.0f, z + 1.0f),
                                     new Vector3(x, y + 1.0f, z),
                                     textureSlot, color);

            g.TerrainMesh.TwoSidedQuad(new Vector3(x, y, z + 1.0f),
                                     new Vector3(x + 1.0f, y, z),
                                     new Vector3(x + 1.0f, y + 1.0f, z),
                                     new Vector3(x, y + 1.0f, z + 1.0f),
                                     textureSlot, color);
        }
    }
}
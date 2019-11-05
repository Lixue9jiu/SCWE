using System.Collections;

namespace SCWE
{
    public class GrassBlock : Block, ICubeBlock
    {
        public static ColorMap map = new ColorMap(new Color(141, 198, 166, 255), new Color(210, 201, 93, 255), new Color(141, 198, 166, 255), new Color(79, 225, 56, 255));

        public void GenerateTerrain(int x, int y, int z, int value, int face, TerrainChunk chunk, ref CellFace data)
        {
            switch (face)
            {
                case CellFace.TOP:
                    data.TextureSlot = 0;
                    data.Color = map.Lookup(chunk.GetShiftValue(x, z));
                    break;
                case CellFace.BOTTOM:
                    data.TextureSlot = 2;
                    data.Color = Color.white;
                    break;
                default:
                    data.TextureSlot = 3;
                    data.Color = Color.white;
                    break;
            }
        }
    }
}
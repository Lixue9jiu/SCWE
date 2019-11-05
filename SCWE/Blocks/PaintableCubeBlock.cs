using System;

namespace SCWE
{
    public class PaintableCubeBlock : Block, ICubeBlock, IPaintableBlock
    {
        public int? GetColor(int data)
        {
            if ((data & 1) != 0)
            {
                return new int?(data >> 1 & 15);
            }
            return null;
        }

        public void GenerateTerrain(int x, int y, int z, int value, int face, TerrainChunk chunk, ref CellFace data)
        {
            int? color = GetColor(TerrainChunk.GetData(value));
            data.TextureSlot = color.HasValue ? BlocksManager.paintedTextures[TextureSlot] : TextureSlot;
            data.Color = BlocksManager.ColorFromInt(color);
        }
    }
}
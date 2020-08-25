namespace SCWE
{
    public class TreeBlock : Block, ICubeBlock
    {
        public void GenerateTerrain(int x, int y, int z, int value, int face, TerrainChunk chunk, ref CellFace data)
        {
            data.TextureSlot = face == CellFace.TOP || face == CellFace.BOTTOM ? 21 : TextureSlot;
            data.Color = Color.white;
        }
    }
}
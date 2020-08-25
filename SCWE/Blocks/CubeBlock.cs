namespace SCWE
{
    public class CubeBlock : Block, ICubeBlock
    {
        public void GenerateTerrain(int x, int y, int z, int value, int face, TerrainChunk chunk, ref CellFace data)
        {
            data.TextureSlot = TextureSlot;
            data.Color = Color.white;
        }
    }
}

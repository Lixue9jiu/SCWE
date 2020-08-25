namespace SCWE
{
    public interface INormalBlock
    {
        void GenerateTerrain(int x, int y, int z, int value, TerrainChunk chunk, MeshGenerator g);
    }
}

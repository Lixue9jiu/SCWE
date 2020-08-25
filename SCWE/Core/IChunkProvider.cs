namespace SCWE
{
    public interface IChunkProvider
    {
        TerrainChunk GetChunkWithBlock(int x, int z);
        TerrainChunk GetChunk(int x, int z);
    }
}

namespace SCWE
{
    public class ChunkCluster : IChunkProvider
    {
        private Vector2Int center;
        private TerrainChunk[] chunks = new TerrainChunk[9];

        public ChunkCluster(Vector2Int center, Terrain terrain)
        {
            this.center = center;
            chunks[4] = terrain.GetChunk(center.x, center.y);
            foreach (Vector2Int offset in MeshGenerator.neighbors)
            {
                var pos = center + offset;
                chunks[GetIndex(pos.x, pos.y)] = terrain.GetChunk(pos.x, pos.y);
            }
        }

        public TerrainChunk GetChunk(int x, int z)
        {
            return chunks[GetIndex(x, z)];
        }

        public TerrainChunk GetChunkWithBlock(int x, int z)
        {
            return GetChunk(x >> TerrainChunk.SizeXShift, z >> TerrainChunk.SizeZShift);
        }

        private int GetIndex(int chunkx, int chunkz)
        {
            return (chunkx - center.x + 1) + (chunkz - center.y + 1) * 3;
        }
    }
}

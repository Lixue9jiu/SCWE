using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SCWE
{
    public class Terrain : IDisposable, IChunkProvider
    {
        ITerrainReader terrainReader;
        Dictionary<Vector2Int, TerrainChunk> chunks = new Dictionary<Vector2Int, TerrainChunk>();

        HashSet<Vector2Int> garbagedChunks = new HashSet<Vector2Int>();

        public IEnumerable<Vector2Int> LoadedChunks => chunks.Keys;

        public void Load(string datFile)
        {
            var fileName = Path.GetFileNameWithoutExtension(datFile);
            if (fileName == "Chunks32h")
            {
                terrainReader = new TerrainReader22();
            }
            else if (fileName == "Chunks32")
            {
                terrainReader = new TerrainReader129();
            }
            else if (fileName == "Chunks")
            {
                terrainReader = new TerrainReader124();
            }
            else
            {
                throw new Exception("unexpected chunk file name: " + fileName);
            }

            terrainReader.Load(File.OpenRead(datFile));
        }

        public bool ChunkExists(int x, int z)
        {
            return terrainReader.ChunkExist(x, z);
        }

        public bool ChunkLoaded(int x, int z)
        {
            var pos = new Vector2Int(x, z);
            return chunks.ContainsKey(pos) && !garbagedChunks.Contains(pos);
        }

        public TerrainChunk GetChunk(int x, int z)
        {
            var pos = new Vector2Int(x, z);
            if (ChunkLoaded(x, z))
            {
                return chunks[pos];
            }
            return null;
        }

        public void SetChunk(int x, int z, TerrainChunk chunk)
        {
            var pos = new Vector2Int(x, z);
            if (ChunkLoaded(x, z))
            {
                DisposeChunk(x, z);
            }
            chunks[pos] = chunk;
        }

        public TerrainChunk GetChunkWithBlock(int x, int z)
        {
            return GetChunk(x >> TerrainChunk.SizeXShift, z >> TerrainChunk.SizeZShift);
        }

        public TerrainChunk LoadChunk(int x, int z)
        {
            var pos = new Vector2Int(x, z);
            if (chunks.ContainsKey(pos))
            {
                garbagedChunks.Remove(pos);
                return chunks[pos];
            }

            if (terrainReader.ChunkExist(x, z))
            {
                TerrainChunk chunk = FindGarbageChunk();
                terrainReader.ReadChunk(x, z, chunk);
                chunks[pos] = chunk;
                return chunk;
            }
            return null;
        }

        public void DisposeChunksOutOfRadius(int x, int z, int radius)
        {
            var center = new Vector2(x, z);
            List<Vector2Int> keys = new List<Vector2Int>(chunks.Keys);
            foreach (Vector2Int p in keys)
            {
                if (Vector2.Distance(center, p) > radius)
                {
                    DisposeChunk(p.x, p.y);
                }
            }
        }

        public void DisposeChunk(int x, int z)
        {
            var pos = new Vector2Int(x, z);
            if (chunks.ContainsKey(pos))
            {
                garbagedChunks.Add(pos);
            }
        }

        public void Dispose()
        {
            terrainReader?.Dispose();
        }

        private TerrainChunk FindGarbageChunk()
        {
            if (garbagedChunks.Count > 0)
            {
                var f = garbagedChunks.First();
                garbagedChunks.Remove(f);
                var res = chunks[f];
                chunks.Remove(f);
                return res;
            }
            return new TerrainChunk();
        }
    }
}

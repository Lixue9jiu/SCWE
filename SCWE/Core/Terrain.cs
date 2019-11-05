using System;
using System.Collections.Generic;
using System.IO;

namespace SCWE
{
    public class Terrain : IDisposable
    {
        ITerrainReader terrainReader;
        Dictionary<Vector2Int, TerrainChunk> chunks = new Dictionary<Vector2Int, TerrainChunk>();

        Queue<WeakReference<TerrainChunk>> garbagedChunks = new Queue<WeakReference<TerrainChunk>>();

        public void Load(string datFile)
        {
            var fileName = Path.GetFileNameWithoutExtension(datFile);
            if (fileName == "Chunks32")
            {
                terrainReader = new TerrainReader129();
            }
            else
            {
                throw new NotImplementedException("does not support dat file type: " + fileName);
            }

            terrainReader.Load(File.OpenRead(datFile));
        }

        public bool ChunkLoaded(int x, int z)
        {
            return chunks.ContainsKey(new Vector2Int(x, z));
        }

        public TerrainChunk GetChunk(int x, int z)
        {
            var pos = new Vector2Int(x, z);
            if (chunks.ContainsKey(pos))
            {
                return chunks[pos];
            }
            return null;
        }

        public void SetChunk(int x, int z, TerrainChunk chunk)
        {
            var pos = new Vector2Int(x, z);
            if (chunks.ContainsKey(pos))
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
            if (terrainReader.ChunkExist(x, z))
            {
                var pos = new Vector2Int(x, z);
                if (chunks.ContainsKey(pos)) return chunks[pos];

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
            if (terrainReader.ChunkExist(x, z))
            {
                var pos = new Vector2Int(x, z);
                garbagedChunks.Enqueue(new WeakReference<TerrainChunk>(chunks[pos]));
                chunks.Remove(pos);
            }
        }

        public void Dispose()
        {
            terrainReader.Dispose();
        }

        private TerrainChunk FindGarbageChunk()
        {
            while (garbagedChunks.Count > 0)
            {
                var reference = garbagedChunks.Dequeue();
                if (reference.TryGetTarget(out TerrainChunk chunk))
                {
                    return chunk;
                }
            }
            return new TerrainChunk();
        }
    }
}

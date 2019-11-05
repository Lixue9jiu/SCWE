using System;
using System.Collections.Generic;
using System.IO;

namespace SCWE
{
    public interface ITerrainReader : IDisposable
    {
        bool ChunkExist(int x, int z);
        void ReadChunk(int x, int z, TerrainChunk chunk);
        void Load(Stream s);
    }
}

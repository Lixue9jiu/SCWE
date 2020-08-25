using System;
using System.Collections.Generic;
using System.Text;

namespace SCWE
{
    public interface IChunkProvider
    {
        TerrainChunk GetChunkWithBlock(int x, int z);
        TerrainChunk GetChunk(int x, int z);
    }
}

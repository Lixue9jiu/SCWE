using System;
using System.Collections.Generic;
using System.Text;

namespace SCWE
{
    public interface ICubeBlock
    {
        void GenerateTerrain(int x, int y, int z, int value, int face, TerrainChunk chunk, ref CellFace data);
    }
}

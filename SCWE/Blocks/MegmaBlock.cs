using System.Collections;

namespace SCWE
{
    public class MegmaBlock : FluidBlock
    {
        public MegmaBlock() : base(4)
        {
        }

        public override void GenerateTerrain(int x, int y, int z, int value, TerrainChunk chunk, MeshGenerator g)
        {
            GenerateFluidTerrain(x, y, z, value, chunk, g.TerrainMesh, Color.white, Color.white);
        }
    }
}
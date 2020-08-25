namespace SCWE
{
    public class WaterBlock : FluidBlock
    {
        static ColorMap map = new ColorMap(new Color(0, 0, 128, 255), new Color(0, 80, 100, 255), new Color(0, 45, 85, 255), new Color(0, 113, 97, 255));

        public WaterBlock() : base(7)
        {
        }

        public override void GenerateTerrain(int x, int y, int z, int value, TerrainChunk chunk, MeshGenerator g)
        {
            Color color = map.Lookup(chunk.GetShiftValue(x, z));
            GenerateFluidTerrain(x, y, z, value, chunk, g.TerrainMesh, color, color);
        }
    }
}
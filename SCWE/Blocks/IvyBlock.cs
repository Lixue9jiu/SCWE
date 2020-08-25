namespace SCWE
{
    public class IvyBlock : Block, INormalBlock
    {
        private static ColorMap map = new ColorMap(new Color(96, 161, 123, 255), new Color(174, 164, 42, 255), new Color(96, 161, 123, 255), new Color(30, 191, 1, 255));

        public static int GetFace(int data)
        {
            return data & 3;
        }

        public void GenerateTerrain(int x, int y, int z, int value, TerrainChunk chunk, MeshGenerator g)
        {
            switch (GetFace(TerrainChunk.GetData(value)))
            {
                case 1:
                    g.TerrainMesh.TwoSidedQuad(
                        new Vector3(x, y, z),
                        new Vector3(x, y + 1.0f, z),
                        new Vector3(x, y + 1.0f, z + 1.0f),
                        new Vector3(x, y, z + 1.0f),
                        TextureSlot, map.Lookup(chunk.GetShiftValue(x, z))
                    );
                    break;
                case 0:
                    g.TerrainMesh.TwoSidedQuad(
                        new Vector3(x, y, z + 1.0f),
                        new Vector3(x, y + 1.0f, z + 1.0f),
                        new Vector3(x + 1.0f, y + 1.0f, z + 1.0f),
                        new Vector3(x + 1.0f, y, z + 1.0f),
                        TextureSlot, map.Lookup(chunk.GetShiftValue(x, z))
                    );
                    break;
                case 3:
                    g.TerrainMesh.TwoSidedQuad(
                        new Vector3(x + 1.0f, y, z + 1.0f),
                        new Vector3(x + 1.0f, y + 1.0f, z + 1.0f),
                        new Vector3(x + 1.0f, y + 1.0f, z),
                        new Vector3(x + 1.0f, y, z),
                        TextureSlot, map.Lookup(chunk.GetShiftValue(x, z))
                    );
                    break;
                case 2:
                    g.TerrainMesh.TwoSidedQuad(
                        new Vector3(x + 1.0f, y, z),
                        new Vector3(x + 1.0f, y + 1.0f, z),
                        new Vector3(x, y + 1.0f, z),
                        new Vector3(x, y, z),
                        TextureSlot, map.Lookup(chunk.GetShiftValue(x, z))
                    );
                    break;
                default:
                    throw new System.Exception("undefined face: " + GetFace(TerrainChunk.GetData(value)));
            }

        }
    }
}
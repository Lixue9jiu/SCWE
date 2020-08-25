namespace SCWE
{
    public class MeshBlock : Block, INormalBlock
    {
        Mesh blockMesh;

        public override void Initialize(string extraData)
        {
            base.Initialize(extraData);
            blockMesh = BlockMeshesManager.FindMesh(extraData);
        }

        public void GenerateTerrain(int x, int y, int z, int value, TerrainChunk chunk, MeshGenerator g)
        {
            g.TerrainMesh.Mesh(x, y, z, blockMesh, Color.white);
        }
    }
}

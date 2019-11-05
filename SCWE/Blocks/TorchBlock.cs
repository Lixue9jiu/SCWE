using System.Collections;

namespace SCWE
{
    public class TorchBlock : Block, INormalBlock
    {
        Mesh[] meshes = new Mesh[5];

        public override void Initialize(string extraData)
        {
            Mesh mesh = new Mesh(BlockMeshesManager.FindMesh("Torch"));
            meshes[0] = mesh.Clone();
            meshes[0].Transform(Matrix4x4.Euler(34, 0, 0) * Matrix4x4.Translate(new Vector3(0.5f, 0.15f, -0.05f)));
            meshes[1] = mesh.Clone();
            meshes[1].Transform(Matrix4x4.Euler(34, 90, 0) * Matrix4x4.Translate(new Vector3(-0.05f, 0.15f, 0.5f)));
            meshes[2] = mesh.Clone();
            meshes[2].Transform(Matrix4x4.Euler(34, 180, 0) * Matrix4x4.Translate(new Vector3(0.5f, 0.15f, 1.05f)));
            meshes[3] = mesh.Clone();
            meshes[3].Transform(Matrix4x4.Euler(34, 270, 0) * Matrix4x4.Translate(new Vector3(1.05f, 0.15f, 0.5f)));
            meshes[4] = mesh.Clone();
            meshes[4].Transform(Matrix4x4.Translate(new Vector3(0.5f, 0f, 0.5f)));
        }

        public void GenerateTerrain(int x, int y, int z, int value, TerrainChunk chunk, MeshGenerator g)
        {
            g.TerrainMesh.Mesh(x, y, z, meshes[TerrainChunk.GetData(value)], Color.white);
        }
    }
}
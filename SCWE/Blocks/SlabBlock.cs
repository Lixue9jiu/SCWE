namespace SCWE
{
    public class SlabBlock : Block, INormalBlock, IPaintableBlock
    {
        Mesh[] blockMeshes = new Mesh[2];
        Mesh[] paintedBlockMeshes = new Mesh[2];

        public override void Initialize(string extraData)
        {
            base.Initialize(extraData);
            //IsCubic = false;

            Mesh slab = BlockMeshesManager.FindMesh("Slab");
            Matrix4x4 matrix = Matrix4x4.Translate(new Vector3(0.5f, 0f, 0.5f));

            blockMeshes[0] = new Mesh(slab);
            blockMeshes[0].Transform(matrix);

            blockMeshes[1] = new Mesh(slab);
            blockMeshes[1].Transform(matrix);
            blockMeshes[1].FlipVertical();

            paintedBlockMeshes[0] = blockMeshes[0].Clone();
            paintedBlockMeshes[1] = blockMeshes[1].Clone();

            blockMeshes[0].WrapInTextureSlot(TextureSlot);
            blockMeshes[1].WrapInTextureSlot(TextureSlot);
            paintedBlockMeshes[0].WrapInTextureSlot(BlocksManager.paintedTextures[TextureSlot]);
            paintedBlockMeshes[1].WrapInTextureSlot(BlocksManager.paintedTextures[TextureSlot]);
        }

        public int? GetColor(int data)
        {
            if ((data & 2) != 0)
            {
                return new int?(data >> 2 & 15);
            }
            return null;
        }

        public static bool GetIsTop(int data)
        {
            return (data & 1) != 0;
        }

        public void GenerateTerrain(int x, int y, int z, int value, TerrainChunk chunk, MeshGenerator g)
        {
            int? color = GetColor(TerrainChunk.GetData(value));
            if (color.HasValue)
                g.TerrainMesh.Mesh(x, y, z, paintedBlockMeshes[GetIsTop(TerrainChunk.GetData(value)) ? 1 : 0], BlocksManager.DEFAULT_COLORS[color.Value]);
            else
                g.TerrainMesh.Mesh(x, y, z, blockMeshes[GetIsTop(TerrainChunk.GetData(value)) ? 1 : 0], Color.white);
        }
    }
}
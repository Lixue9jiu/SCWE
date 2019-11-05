using System.Collections;

namespace SCWE
{
    public class StairBlock : Block, INormalBlock, IPaintableBlock
    {
        Mesh[] blockMeshes = new Mesh[24];
        Mesh[] paintedBlockMeshes = new Mesh[24];

        public override void Initialize(string extraData)
        {
            base.Initialize(extraData);
            //IsCubic = false;

            Matrix4x4 matrix = Matrix4x4.Translate(new Vector3(0.5f, 0f, 0.5f));

            float y;
            Matrix4x4 m;
            Mesh mesh;
            for (int i = 0; i < 24; i++)
            {
                y = 0;

                y -= GetRotation(i) * 90;

                m = Matrix4x4.Euler(0, y, 0);
                switch ((i >> 3) & 3)
                {
                    case 1:
                        mesh = BlockMeshesManager.FindMesh("Stair0");
                        break;
                    case 0:
                        mesh = BlockMeshesManager.FindMesh("Stair1");
                        break;
                    case 2:
                        mesh = BlockMeshesManager.FindMesh("Stair2");
                        break;
                    default:
                        throw new System.Exception("unknown stair module: " + ((i >> 3) & 3));
                }

                blockMeshes[i] = new Mesh(mesh);
                blockMeshes[i].Transform(matrix * m);
                if ((i & 4) != 0)
                {
                    blockMeshes[i].FlipVertical();
                }
                paintedBlockMeshes[i] = blockMeshes[i].Clone();

                blockMeshes[i].WrapInTextureSlot(TextureSlot);
                paintedBlockMeshes[i].WrapInTextureSlot(BlocksManager.paintedTextures[TextureSlot]);
            }
        }

        public static int GetRotation(int data)
        {
            return data & 3;
        }

        public int? GetColor(int data)
        {
            if ((data & 32) != 0)
            {
                return new int?(data >> 6 & 15);
            }
            return null;
        }

        public static int GetVariant(int data)
        {
            return data & 31;
        }

        public void GenerateTerrain(int x, int y, int z, int value, TerrainChunk chunk, MeshGenerator g)
        {
            int? color = GetColor(TerrainChunk.GetData(value));
            if (color.HasValue)
                g.TerrainMesh.Mesh(x, y, z, paintedBlockMeshes[GetVariant(TerrainChunk.GetData(value))], BlocksManager.DEFAULT_COLORS[color.Value]);
            else
                g.TerrainMesh.Mesh(x, y, z, blockMeshes[GetVariant(TerrainChunk.GetData(value))], Color.white);
        }
    }
}
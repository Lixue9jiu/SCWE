using System.Collections;
using System.Collections.Generic;

namespace SCWE
{
    public class PostedSignBlock : Block, INormalBlock, IPaintableBlock
    {
        Mesh[] blockMeshes = new Mesh[16];
        Mesh[] paintedBlockMeshes = new Mesh[16];

        public override void Initialize(string extraData)
        {
            base.Initialize(extraData);

            string[] strs = extraData.Split(' ');

            Mesh sign = new Mesh(BlockMeshesManager.FindMesh(strs[0] + "_Sign"));
            Mesh post = new Mesh(BlockMeshesManager.FindMesh(strs[0] + "_Post"));

            int paintedTexSlot = int.Parse(strs[1]);

            for (int i = 0; i < 16; i++)
            {
                bool hanging = (i & 8) != 0;
                Matrix4x4 matrix = Matrix4x4.Euler(0, 45 * i, 0);
                if (hanging)
                {
                    matrix *= Matrix4x4.Scale(new Vector3(1f, -1f, 1f)) * Matrix4x4.Translate(new Vector3(0f, 1f, 0f));
                }
                matrix = Matrix4x4.Translate(new Vector3(0.5f, 0f, 0.5f)) * matrix;
                blockMeshes[i] = sign.Clone();
                blockMeshes[i].Append(post);
                blockMeshes[i].Transform(matrix);
                blockMeshes[i].WrapInTextureSlot(TextureSlot);

                paintedBlockMeshes[i] = sign.Clone();
                paintedBlockMeshes[i].Append(post);
                paintedBlockMeshes[i].Transform(matrix);
                paintedBlockMeshes[i].WrapInTextureSlot(paintedTexSlot);
            }
        }

        public void GenerateTerrain(int x, int y, int z, int value, TerrainChunk chunk, MeshGenerator g)
        {
            int data = TerrainChunk.GetData(value);
            int? i = GetColor(data);
            if (i.HasValue)
                g.TerrainMesh.Mesh(x, y, z, paintedBlockMeshes[GetVariant(data)], BlocksManager.DEFAULT_COLORS[i.Value]);
            else
                g.TerrainMesh.Mesh(x, y, z, blockMeshes[GetVariant(data)], Color.white);
        }

        public int? GetColor(int data)
        {
            if ((data & 0x10) != 0)
            {
                return data >> 5 & 0xF;
            }
            return null;
        }

        public static int GetVariant(int data)
        {
            return data & 0xF;
        }
    }
}
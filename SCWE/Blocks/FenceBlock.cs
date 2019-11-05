using System.Collections;
using System.Collections.Generic;

namespace SCWE
{
    public class FenceBlock : Block, INormalBlock, IPaintableBlock
    {
        Mesh[] blockMeshes = new Mesh[16];
        Mesh[] paintedBlockMeshes = new Mesh[16];

        Color unpaintedColor;

        public override void Initialize(string extraData)
        {
            base.Initialize(extraData);

            string[] strs = extraData.Split(' ');

            string modelName = strs[0];
            //bool useAlphaTest = bool.Parse(strs[1]);
            bool doubleSidedPlanks = bool.Parse(strs[2]);
            unpaintedColor = new Color(byte.Parse(strs[3]), byte.Parse(strs[4]), byte.Parse(strs[5]), 255);

            Mesh post = new Mesh(BlockMeshesManager.FindMesh(modelName + "_Post"));
            post.Transform(Matrix4x4.Translate(new Vector3(0.5f, 0f, 0.5f)));
            Mesh plank = new Mesh(BlockMeshesManager.FindMesh(modelName + "_Planks"));

            for (int i = 0; i < 16; i++)
            {
                Mesh data = Mesh.CreateEmpty();

                if ((i & 1) != 0)
                {
                    Mesh data1 = plank.Clone();
                    data1.Transform(Matrix4x4.Translate(new Vector3(0.5f, 0f, 0.5f)));
                    data.Append(data1);
                    if (doubleSidedPlanks)
                    {
                        data1.FlipWindingOrder();
                        data.Append(data1);
                    }
                }
                if ((i & 2) != 0)
                {
                    Mesh data1 = plank.Clone();
                    data1.Transform(Matrix4x4.Translate(new Vector3(0.5f, 0f, 0.5f)) * Matrix4x4.Euler(0, -180, 0));
                    data.Append(data1);
                    if (doubleSidedPlanks)
                    {
                        data1.FlipWindingOrder();
                        data.Append(data1);
                    }
                }
                if ((i & 4) != 0)
                {
                    Mesh data1 = plank.Clone();
                    data1.Transform(Matrix4x4.Translate(new Vector3(0.5f, 0f, 0.5f)) * Matrix4x4.Euler(0, 90, 0));
                    data.Append(data1);
                    if (doubleSidedPlanks)
                    {
                        data1.FlipWindingOrder();
                        data.Append(data1);
                    }
                }
                if ((i & 8) != 0)
                {
                    Mesh data1 = plank.Clone();
                    data1.Transform(Matrix4x4.Translate(new Vector3(0.5f, 0f, 0.5f)) * Matrix4x4.Euler(0, 270, 0));
                    data.Append(data1);
                    if (doubleSidedPlanks)
                    {
                        data1.FlipWindingOrder();
                        data.Append(data1);
                    }
                }

                blockMeshes[i] = post.Clone();
                blockMeshes[i].Append(data);

                paintedBlockMeshes[i] = post.Clone();
                paintedBlockMeshes[i].Append(data);

                blockMeshes[i].WrapInTextureSlot(TextureSlot);
                paintedBlockMeshes[i].WrapInTextureSlot(BlocksManager.paintedTextures[TextureSlot]);
            }
        }

        public void GenerateTerrain(int x, int y, int z, int value, TerrainChunk chunk, MeshGenerator g)
        {
            int data = TerrainChunk.GetData(value);
            int? i = GetColor(data);
            var terrainMesh = g.TerrainMesh;
            if (i.HasValue)
                terrainMesh.Mesh(x, y, z, paintedBlockMeshes[GetVariant(data)], BlocksManager.DEFAULT_COLORS[i.Value]);
            else
                terrainMesh.Mesh(x, y, z, blockMeshes[GetVariant(data)], unpaintedColor);
        }

        public static int GetVariant(int data)
        {
            return data & 0xF;
        }

        public int? GetColor(int data)
        {
            if ((data & 0x10) != 0)
            {
                return data >> 5 & 0xF;
            }
            return null;
        }
    }
}
using System.Collections;
using System;

namespace SCWE
{
    public class ElectricGateBlock : Block, INormalBlock
    {
        Mesh[] meshes = new Mesh[24];

        public override void Initialize(string extraData)
        {
            Mesh blockMesh;
            switch (Index)
            {
                case 134:
                    blockMesh = BlockMeshesManager.FindMesh("NandGate");
                    break;
                case 135:
                    blockMesh = BlockMeshesManager.FindMesh("NorGate");
                    break;
                case 137:
                    blockMesh = BlockMeshesManager.FindMesh("AndGate");
                    break;
                case 140:
                    blockMesh = BlockMeshesManager.FindMesh("NotGate");
                    break;
                case 143:
                    blockMesh = BlockMeshesManager.FindMesh("OrGate");
                    break;
                case 145:
                    blockMesh = BlockMeshesManager.FindMesh("DelayGate");
                    break;
                case 146:
                    blockMesh = BlockMeshesManager.FindMesh("SRLatch");
                    break;
                case 156:
                    blockMesh = BlockMeshesManager.FindMesh("XorGate");
                    break;
                case 157:
                    blockMesh = BlockMeshesManager.FindMesh("RandomGenerator");
                    break;
                case 179:
                    blockMesh = BlockMeshesManager.FindMesh("MotionDetector");
                    break;
                case 180:
                    blockMesh = BlockMeshesManager.FindMesh("DigitalToAnalogConverter");
                    break;
                case 181:
                    blockMesh = BlockMeshesManager.FindMesh("AnalogToDigitalConverter");
                    break;
                case 183:
                    blockMesh = BlockMeshesManager.FindMesh("SoundGenerator");
                    break;
                case 184:
                    blockMesh = BlockMeshesManager.FindMesh("Counter");
                    break;
                case 186:
                    blockMesh = BlockMeshesManager.FindMesh("MemoryBank");
                    break;
                case 187:
                    blockMesh = BlockMeshesManager.FindMesh("RealTimeClock");
                    break;
                case 188:
                    blockMesh = BlockMeshesManager.FindMesh("TruthTableCircuit");
                    break;
                default:
                    throw new System.Exception("unsupported electric gate: " + Index);
            }

            Matrix4x4 m;
            Matrix4x4 m2;

            Matrix4x4 half = Matrix4x4.Translate(new Vector3(0.5f, 0.5f, 0.5f));
            Matrix4x4 inverseHalf = half.Inverse;

            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 6; j++)
                {
                    switch (j)
                    {
                        case 0:
                            m = Matrix4x4.Euler(0, 0, i * 90);
                            m2 = Matrix4x4.TRS(new Vector3(0.5f, -0.5f, 0f), new Vector3(90, 0, 0), Vector3.one);
                            break;
                        case 1:
                            m = Matrix4x4.Euler(i * -90, 0, 0);
                            m2 = Matrix4x4.TRS(new Vector3(0f, -0.5f, 0.5f), new Vector3(90, 0, -270), Vector3.one);
                            break;
                        case 2:
                            m = Matrix4x4.Euler(0, 0, i * -90);
                            m2 = Matrix4x4.TRS(new Vector3(-0.5f, -0.5f, 0f), new Vector3(90, 0, -180), Vector3.one);
                            break;
                        case 3:
                            m = Matrix4x4.Euler(i * 90, 0, 0);
                            m2 = Matrix4x4.TRS(new Vector3(0f, -0.5f, -0.5f), new Vector3(90, 0, -90), Vector3.one);
                            break;
                        case 4:
                            m = Matrix4x4.Euler(0, i * 90, 0);
                            m2 = Matrix4x4.Translate(new Vector3(0.5f, 0f, 0.5f));
                            break;
                        case 5:
                            m = Matrix4x4.Euler(0, i * -90, 0);
                            m2 = Matrix4x4.TRS(new Vector3(0.5f, 0f, -0.5f), new Vector3(180, 0, 0), Vector3.one);
                            break;
                        default:
                            throw new System.Exception();
                    }

                    Mesh data = blockMesh.Clone();
                    data.Transform(half * m * m2 * inverseHalf);
                    meshes[(j << 2) + i] = data;
                }
            }
        }

        public static int GetFace(int value)
        {
            return TerrainChunk.GetData(value) >> 2 & 7;
        }

        public void GenerateTerrain(int x, int y, int z, int value, TerrainChunk chunk, MeshGenerator g)
        {
            g.TerrainMesh.Mesh(x, y, z, meshes[TerrainChunk.GetData(value) & 31], Color.white);
        }
    }
}

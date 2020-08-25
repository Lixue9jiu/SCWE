namespace SCWE
{
    public class FurnitureBlock : Block, INormalBlock
    {
        FurnitureSet furnitures;

        public override void Initialize(string extraData)
        {
            WorldManager.OnWorldLoaded += OnWorldLoaded;
        }

        private void OnWorldLoaded()
        {
            furnitures = WorldManager.World.Furnitures;
        }

        public static int GetDesignIndex(int data)
        {
            return data >> 2 & 1023;
        }

        public static int GetRotation(int data)
        {
            return data & 3;
        }

        public void GenerateTerrain(int x, int y, int z, int value, TerrainChunk chunk, MeshGenerator g)
        {
            int d = TerrainChunk.GetData(value);
            g.TerrainMesh.Mesh(x, y, z, furnitures.GetFurniture(GetDesignIndex(d), GetRotation(d)));
        }
    }
}
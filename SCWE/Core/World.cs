using System;
using System.Collections.Generic;
using System.Text;

namespace SCWE
{
    public class World : IDisposable
    {
        public readonly Terrain Terrain = new Terrain();
        public readonly FurnitureSet Furnitures = new FurnitureSet();

        public void Load()
        {
            Terrain.Load(WorldManager.ChunkDat);
            Furnitures.Load(WorldManager.Project);
        }

        public void Dispose()
        {
            Terrain.Dispose();
        }
    }
}

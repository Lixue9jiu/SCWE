using System;
using System.Collections.Generic;
using System.Text;

namespace SCWE
{
    public class Furniture
    {
        public int Index;
        public int Resolution;
        public int[] Data;
        public int TerrainUseCount;

        public int GetCellValue(int x, int y, int z)
        {
            if (x >= 0 && x < Resolution && y >= 0 && y < Resolution && z >= 0 && z < Resolution)
            {
                return Data[Resolution - x - 1 + y * Resolution + z * Resolution * Resolution];
            }
            return 0;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace SCWE
{
    public class TerrainChunk
    {
        public const int SizeXShift = 4;
        public const int SizeYShift = 7;
        public const int SizeZShift = 4;

        public const int SizeX = 16;
        public const int SizeY = 128;
        public const int SizeZ = 16;

        public const int SizeXMinusOne = 15;
        public const int SizeYMinusOne = 127;
        public const int SizeZMinusOne = 15;

        int[] cells;
        int[] shifts;
        public TerrainChunk()
        {
            cells = new int[SizeZ * SizeX * SizeY];
            shifts = new int[SizeZ * SizeX];
        }

        // will clip x, y, z before fetching the cell value
        public int GetCellValue(int x, int y, int z)
        {
            return cells[GetCellIndex(x & SizeXMinusOne, y & SizeYMinusOne, z & SizeZMinusOne)];
        }

        public int GetCellContent(int x, int y, int z)
        {
            return GetContent(GetCellValue(x, y, z));
        }

        public int GetShiftValue(int x, int z)
        {
            return shifts[GetShiftIndex(x & SizeXMinusOne, z & SizeZMinusOne)];
        }

        public void SetCellValue(int x, int y, int z, int value)
        {
            SetCellValue(GetCellIndex(x, y, z), value);
        }

        public void SetCellValue(int index, int value)
        {
            cells[index] = value;
        }

        public void SetShiftValue(int index, int value)
        {
            shifts[index] = value;
        }

        public static int GetShiftIndex(int x, int y)
        {
            return y + SizeZ * x;
        }

        public static int GetCellIndex(int x, int y, int z)
        {
            return y + SizeY * x + SizeX * SizeY * z;
        }

        public static int GetContent(int value)
        {
            return value & 1023;
        }

        public static int GetData(int value)
        {
            return (value & -16384) >> 14;
        }

        public static int MakeBlockValue(int contents, int light, int data)
        {
            return (contents & 1023) | (light << 10 & 15360) | (data << 14 & -16384);
        }

        public static int ReplaceData(int value, int data)
        {
            return value ^ ((value ^ data << 14) & -16384);
        }

        public static int GetTemperature(int value)
        {
            return (value & 3840) >> 8;
        }

        public static int GetHumidity(int value)
        {
            return (value & 61440) >> 12;
        }
    }
}

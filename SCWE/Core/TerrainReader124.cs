using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SCWE
{
    public class TerrainReader124 : ITerrainReader
    {
        Stream stream;

        Dictionary<Vector2Int, int> chunkOffsets = new Dictionary<Vector2Int, int>();

        byte[] buffer = new byte[65536];

        object locker = new object();

        public void Load(Stream stream)
        {
            this.stream = stream;
            while (true)
            {
                int x;
                int y;
                int offset;
                ReadChunkEntry(stream, out x, out y, out offset);
                if (offset == 0)
                    break;
                chunkOffsets[new Vector2Int(-x, y)] = offset;
            }
        }

        public bool ChunkExist(int chunkx, int chunky)
        {
            return chunkOffsets.ContainsKey(new Vector2Int(chunkx, chunky));
        }

        public unsafe void ReadChunk(int chunkx, int chunky, TerrainChunk chunk)
        {
            lock (locker)
            {
                Vector2Int p = new Vector2Int(chunkx, chunky);
                int value;
                if (chunkOffsets.TryGetValue(p, out value))
                {
                    stream.Seek(value, SeekOrigin.Begin);
                    ReadChunkHeader(stream);

                    stream.Read(buffer, 0, 65536);

                    fixed (byte* bptr = &buffer[0])
                    {
                        byte* ptr = bptr;
                        for (int x = 0; x < 16; x++)
                        {
                            for (int y = 0; y < 16; y++)
                            {
                                int index = TerrainChunk.GetCellIndex(15 - x, 0, y);
                                int h = 0;
                                while (h < 128)
                                {
                                    var val = *ptr;
                                    ptr++;
                                    var data = *ptr;
                                    ptr++;
                                    chunk.SetCellValue(index, val | (data << 10));
                                    h++;
                                    index++;
                                }
                            }
                        }
                    }

                    stream.Read(buffer, 0, 1024);

                    fixed (byte* bptr = &buffer[0])
                    {
                        int* iptr = (int*)bptr;
                        for (int x = 0; x < 16; x++)
                        {
                            int index = TerrainChunk.GetShiftIndex(15 - x, 0);
                            int h = 0;
                            while (h < 16)
                            {
                                chunk.SetShiftValue(index, *iptr);
                                iptr++;
                                h++;
                                index++;
                            }
                        }
                    }
                }
            }
        }

        public void Dispose()
        {
            stream.Close();
            stream.Dispose();
        }

        static int ReadInt(Stream stream)
        {
            return stream.ReadByte() + (stream.ReadByte() << 8) + (stream.ReadByte() << 16) + (stream.ReadByte() << 24);
        }

        public static void ReadChunkEntry(Stream stream, out int chunkx, out int chunky, out int index)
        {
            chunkx = ReadInt(stream);
            chunky = ReadInt(stream);
            index = ReadInt(stream);
        }

        public static void ReadChunkHeader(Stream stream)
        {
            int v1 = ReadInt(stream);
            int v2 = ReadInt(stream);
            int chunkx = ReadInt(stream);
            int chunky = ReadInt(stream);
            if (v1 != unchecked((int)0xDEADBEEF) || v2 != unchecked((int)0xFFFFFFFF))
            {
                throw new System.Exception(string.Format("invalid chunk header at: {0}, {1}", chunkx, chunky));
            }
        }
    }
}

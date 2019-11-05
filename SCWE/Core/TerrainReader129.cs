using System;
using System.Collections.Generic;
using System.IO;

namespace SCWE
{
    public class TerrainReader129 : ITerrainReader
    {
        Stream stream;

        Dictionary<Vector2Int, long> chunkOffsets = new Dictionary<Vector2Int, long>();

        byte[] buffer = new byte[131072];

        object locker = new object();

        //public BlockTerrain.Chunk[] AlaphaTest(BlockTerrain terrain)
        //{
        //    BlockTerrain.Chunk chunk;

        //    Dictionary<Point2, long>.Enumerator enu = chunkOffsets.GetEnumerator();
        //    List<BlockTerrain.Chunk> c = new List<BlockTerrain.Chunk>();

        //    while (enu.MoveNext())
        //    {
        //        Point2 p = enu.Current.Key;
        //        chunk = ReadChunk(p.X, p.Y, terrain);
        //        c.Add(chunk);
        //    }
        //    return c.ToArray();
        //}

        public void Load(Stream stream)
        {
            this.stream = stream;
            while (true)
            {
                int x;
                int y;
                int index;
                ReadChunkEntry(stream, out x, out y, out index);
                if (index < 0)
                    break;
                chunkOffsets[new Vector2Int(-x, y)] = 786444L + 132112L * (long)index;
            }
        }

        public bool ChunkExist(int chunkx, int chunky)
        {
            return chunkOffsets.ContainsKey(new Vector2Int(chunkx, chunky));
        }

        //	public void SaveChunkEntries ()
        //	{
        //		Point2[] entries = new Point2[chunkOffsets.Count];
        //		foreach (KeyValuePair<Point2, long> p in chunkOffsets) {
        //			entries [(int)((p.Value - 786444L) / 132112L)] = p.Key;
        //		}
        //
        //		stream.Seek (0, SeekOrigin.Begin);
        //		for (int i = 0; i < entries.Length; i++) {
        //			WriteChunkEntry (stream, -entries [i].X, entries [i].Y, i);
        //		}
        //	}

        public unsafe void ReadChunk(int chunkx, int chunky, TerrainChunk chunk)
        {
            lock (locker)
            {
                Vector2Int p = new Vector2Int(chunkx, chunky);
                long value;
                if (chunkOffsets.TryGetValue(p, out value))
                {
                    stream.Seek(value, SeekOrigin.Begin);
                    ReadChunkHeader(stream);

                    stream.Read(buffer, 0, 131072);

                    fixed (byte* bptr = &buffer[0])
                    {
                        int* iptr = (int*)bptr;
                        for (int x = 0; x < 16; x++)
                        {
                            for (int y = 0; y < 16; y++)
                            {
                                int index = TerrainChunk.GetCellIndex(15 - x, 0, y);
                                int h = 0;
                                while (h < 128)
                                {
                                    chunk.SetCellValue(index, *iptr);
                                    iptr++;
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

        static void WriteInt(Stream stream, int value)
        {
            stream.WriteByte((byte)value);
            stream.WriteByte((byte)(value >> 8));
            stream.WriteByte((byte)(value >> 16));
            stream.WriteByte((byte)(value >> 24));
        }

        public static void ReadChunkEntry(Stream stream, out int chunkx, out int chunky, out int index)
        {
            chunkx = ReadInt(stream);
            chunky = ReadInt(stream);
            index = ReadInt(stream);
        }

        static void WriteChunkEntry(Stream stream, int chunkx, int chunky, int index)
        {
            WriteInt(stream, chunkx);
            WriteInt(stream, chunky);
            WriteInt(stream, index);
        }

        public static void ReadChunkHeader(Stream stream)
        {
            int v1 = ReadInt(stream);
            int v2 = ReadInt(stream);
            int chunkx = ReadInt(stream);
            int chunky = ReadInt(stream);
            if (v1 != -559038737 || v2 != -2)
            {
                throw new System.Exception(string.Format("invalid chunk header at: {0}, {1}", chunkx, chunky));
            }
        }

        public static void WriteChunkHeader(Stream stream, int chunkx, int chunky)
        {
            WriteInt(stream, -559038737);
            WriteInt(stream, -2);
            WriteInt(stream, chunkx);
            WriteInt(stream, chunky);
        }
    }
}

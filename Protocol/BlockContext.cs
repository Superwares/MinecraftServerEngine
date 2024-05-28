

using Common;
using Containers;
using static System.Collections.Specialized.BitVector32;

namespace Protocol
{
    internal sealed class BlockContext : System.IDisposable
    {
        private sealed class ChunkData : System.IDisposable
        {
            private const int _WIDTH = ChunkLocation.WIDTH;
            private const int _MAX_SECTION_COUNT = 16;

            private sealed class SectionData : System.IDisposable
            {
                private bool _disposed = false;

                public const int WIDTH = _WIDTH;
                public const int HEIGHT = WIDTH;
                private const int _TOTAL_BLOCK_COUNT = WIDTH * WIDTH * HEIGHT;

                private int _bitsPerBlock;

                private (int, int)[] _palette;

                private const int _BITS_PER_DATA_UNIT = sizeof(long) * 8; // TODO: Change to appropriate name.
                private long[] _data;

                private byte[] _blockLights, _skyLights;

                private static int GetDataLength(int _bitsPerBlock)
                {
                    int a = _TOTAL_BLOCK_COUNT * _bitsPerBlock;
                    System.Diagnostics.Debug.Assert(a % _BITS_PER_DATA_UNIT == 0);
                    int length = a / _BITS_PER_DATA_UNIT;
                    return length;
                }

                public SectionData(int defaultId)
                {
                    _bitsPerBlock = 4;

                    _palette = [(defaultId, _TOTAL_BLOCK_COUNT)];

                    {
                        int length = GetDataLength(_bitsPerBlock);
                        _data = new long[length];

                        int i;
                        long value = Conversions.ToLong(defaultId);

                        int start, offset, end;

                        for (int y = 0; y < HEIGHT; ++y)
                        {
                            for (int z = 0; z < WIDTH; ++z)
                            {
                                for (int x = 0; x < WIDTH; ++x)
                                {
                                    i = (((y * HEIGHT) + z) * WIDTH) + x;

                                    start = (i * _bitsPerBlock) / _BITS_PER_DATA_UNIT;
                                    offset = (i * _bitsPerBlock) % _BITS_PER_DATA_UNIT;
                                    end = (((i + 1) * _bitsPerBlock) - 1) / _BITS_PER_DATA_UNIT;

                                    System.Diagnostics.Debug.Assert(
                                        (value & ~((1L << _bitsPerBlock) - 1L)) == 0);
                                    _data[start] |= (value << offset);

                                    if (start != end)
                                    {
                                        _data[end] = value >> (_BITS_PER_DATA_UNIT - offset);
                                    }

                                }
                            }
                        }

                    }

                    {
                        int length = _TOTAL_BLOCK_COUNT / 2;
                        _blockLights = new byte[length];
                        System.Array.Fill<byte>(_blockLights, byte.MaxValue);
                        _skyLights = new byte[length];
                        System.Array.Fill<byte>(_skyLights, byte.MaxValue);
                    }

                }

                ~SectionData() => System.Diagnostics.Debug.Assert(false);


                public int GetId(int x, int y, int z)
                {
                    System.Diagnostics.Debug.Assert(!_disposed);

                    System.Diagnostics.Debug.Assert(x >= 0 && x <= WIDTH);
                    System.Diagnostics.Debug.Assert(z >= 0 && z <= WIDTH);
                    System.Diagnostics.Debug.Assert(y >= 0 && y <= HEIGHT);

                    long mask = (1L << _bitsPerBlock) - 1L;

                    int i;
                    long value;

                    int start, offset, end;

                    i = (((y * HEIGHT) + z) * WIDTH) + x;
                    start = (i * _bitsPerBlock) / _BITS_PER_DATA_UNIT;
                    offset = (i * _bitsPerBlock) % _BITS_PER_DATA_UNIT;
                    end = ((i + 1) * _bitsPerBlock - 1) / _BITS_PER_DATA_UNIT;

                    if (start == end)
                    {
                        value = (_data[start] >> offset);
                    }
                    else
                    {
                        value = (_data[start] >> offset | _data[end] << (_BITS_PER_DATA_UNIT - offset));
                    }

                    value &= mask;

                    if (_bitsPerBlock == 13)
                    {
                        return Conversions.ToInt(value);
                    }
                    else
                    {
                        System.Diagnostics.Debug.Assert(_palette != null);

                        (int id, var _) = _palette[value];
                        return id;
                    }
                }

                private void ExpandData(int bitsPerBlock)
                {
                    System.Diagnostics.Debug.Assert(!_disposed);

                    System.Diagnostics.Debug.Assert(_palette != null);
                    System.Diagnostics.Debug.Assert(bitsPerBlock > _bitsPerBlock);

                    int length = GetDataLength(bitsPerBlock);
                    var data = new long[length];

                    long mask = (1L << bitsPerBlock) - 1L;

                    int i;
                    long value;

                    int start, offset, end;

                    if (bitsPerBlock == 13)
                    {
                        int id;

                        for (int y = 0; y < HEIGHT; ++y)
                        {
                            for (int z = 0; z < WIDTH; ++z)
                            {
                                for (int x = 0; x < WIDTH; ++x)
                                {
                                    i = (((y * HEIGHT) + z) * WIDTH) + x;

                                    {
                                        start = (i * _bitsPerBlock) / _BITS_PER_DATA_UNIT;
                                        offset = (i * _bitsPerBlock) % _BITS_PER_DATA_UNIT;
                                        end = (((i + 1) * _bitsPerBlock) - 1) / _BITS_PER_DATA_UNIT;

                                        if (start == end)
                                        {
                                            value = (_data[start] >> offset);
                                        }
                                        else
                                        {
                                            value = 
                                                (_data[start] >> offset) | 
                                                (_data[end] << (_BITS_PER_DATA_UNIT - offset));
                                        }

                                        value &= mask;
                                        (id, int count) = _palette[value];
                                        System.Diagnostics.Debug.Assert(count > 0);
                                    }

                                    {
                                        start = (i * bitsPerBlock) / _BITS_PER_DATA_UNIT;
                                        offset = (i * bitsPerBlock) % _BITS_PER_DATA_UNIT;
                                        end = (((i + 1) * bitsPerBlock) - 1) / _BITS_PER_DATA_UNIT;

                                        value = Conversions.ToLong(id);

                                        System.Diagnostics.Debug.Assert(
                                            (value & ~((1L << bitsPerBlock) - 1L)) == 0);
                                        data[start] |= (value << offset);

                                        if (start != end)
                                        {
                                            data[end] = (value >> (_BITS_PER_DATA_UNIT - offset));
                                        }
                                    }

                                }
                            }
                        }

                    }
                    else
                    {
                        System.Diagnostics.Debug.Assert(
                            bitsPerBlock > 4 && bitsPerBlock <= 8);

                        for (int y = 0; y < HEIGHT; ++y)
                        {
                            for (int z = 0; z < WIDTH; ++z)
                            {
                                for (int x = 0; x < WIDTH; ++x)
                                {
                                    i = (((y * HEIGHT) + z) * WIDTH) + x;

                                    {
                                        start = (i * _bitsPerBlock) / _BITS_PER_DATA_UNIT;
                                        offset = (i * _bitsPerBlock) % _BITS_PER_DATA_UNIT;
                                        end = (((i + 1) * _bitsPerBlock) - 1) / _BITS_PER_DATA_UNIT;

                                        if (start == end)
                                        {
                                            value = (_data[start] >> offset);
                                        }
                                        else
                                        {
                                            value = 
                                                (_data[start] >> offset | 
                                                _data[end] << (_BITS_PER_DATA_UNIT - offset));
                                        }

                                        value &= mask;
                                    }

                                    {
                                        start = (i * bitsPerBlock) / _BITS_PER_DATA_UNIT;
                                        offset = (i * bitsPerBlock) % _BITS_PER_DATA_UNIT;
                                        end = (((i + 1) * bitsPerBlock) - 1) / _BITS_PER_DATA_UNIT;

                                        System.Diagnostics.Debug.Assert(
                                            (value & ~((1L << bitsPerBlock) - 1L)) == 0);
                                        data[start] |= (value << offset);

                                        if (start != end)
                                        {
                                            data[end] = value >> (_BITS_PER_DATA_UNIT - offset);
                                        }
                                    }

                                }
                            }
                        }
                    }

                    _bitsPerBlock = bitsPerBlock;
                    _data = data;
                }

                private long GetValue(int id)
                {
                    System.Diagnostics.Debug.Assert(!_disposed);

                    long value;

                    if (_bitsPerBlock == 13)
                    {
                        System.Diagnostics.Debug.Assert(_palette == null);

                        value = Conversions.ToLong(id);
                    }
                    else
                    {
                        System.Diagnostics.Debug.Assert(_palette != null);
                        System.Diagnostics.Debug.Assert(_bitsPerBlock >= 4 && _bitsPerBlock <= 8);
                        System.Diagnostics.Debug.Assert(_palette.Length > 0);

                        int indexPalette = -1;

                        System.Diagnostics.Debug.Assert(_palette != null);
                        for (int i = 0; i < _palette.Length; ++i)
                        {
                            (int idPalette, int count) = _palette[i];
                            System.Diagnostics.Debug.Assert(count >= 0);

                            if (id == idPalette)
                            {
                                indexPalette = i;
                                _palette[i] = (idPalette, ++count);
                            }
                        }

                        if (indexPalette >= 0)
                        {
                            value = Conversions.ToLong(indexPalette);
                        }
                        else
                        {
                            System.Diagnostics.Debug.Assert(indexPalette == -1);

                            int length = _palette.Length;
                            int lengthNew = length + 1;

                            int bitsPerBlock;
                            if (lengthNew <= 0b1111)
                            {
                                bitsPerBlock = 4;
                            }
                            else if (lengthNew <= 0b1_1111)
                            {
                                bitsPerBlock = 5;
                            }
                            else if (lengthNew <= 0b11_1111)
                            {
                                bitsPerBlock = 6;
                            }
                            else if (lengthNew <= 0b111_1111)
                            {
                                bitsPerBlock = 7;
                            }
                            else if (lengthNew <= 0b1111_1111)
                            {
                                bitsPerBlock = 8;
                            }
                            else
                            {
                                bitsPerBlock = 13;
                            }

                            if (bitsPerBlock > _bitsPerBlock)
                            {
                                ExpandData(bitsPerBlock);
                            }

                            {
                                var paletteNew = new (int, int)[lengthNew];

                                System.Array.Copy(_palette, paletteNew, length);
                                paletteNew[length] = (id, 1);

                                _palette = paletteNew;
                            }

                            if (bitsPerBlock == 13)
                            {
                                value = Conversions.ToLong(id);
                            }
                            else
                            {
                                System.Diagnostics.Debug.Assert(bitsPerBlock >= 4 && bitsPerBlock <= 8);

                                value = Conversions.ToLong(length);
                            }

                        }
                    }

                    return value;
                }

                public bool SetId(int x, int y, int z, int id)
                {
                    System.Diagnostics.Debug.Assert(!_disposed);

                    System.Diagnostics.Debug.Assert(x >= 0 && x <= WIDTH);
                    System.Diagnostics.Debug.Assert(y >= 0 && y <= WIDTH);
                    System.Diagnostics.Debug.Assert(z >= 0 && z <= HEIGHT);

                    long value = GetValue(id);

                    int i = (((y * HEIGHT) + z) * WIDTH) + x;
                    int start = (i * _bitsPerBlock) / _BITS_PER_DATA_UNIT;
                    int offset = (i * _bitsPerBlock) % _BITS_PER_DATA_UNIT;
                    int end = ((i + 1) * _bitsPerBlock - 1) / _BITS_PER_DATA_UNIT;

                    System.Diagnostics.Debug.Assert(
                        (value & ~((Conversions.ToLong(1) << _bitsPerBlock) - 1)) == 0);
                    _data[start] |= (value << offset);

                    if (start != end)
                    {
                        _data[end] = (value >> (_BITS_PER_DATA_UNIT - offset));
                    }

                    throw new System.NotImplementedException();
                }

                internal static void Write(Buffer buffer, SectionData sectionData)
                {
                    int bitCount = sectionData._bitsPerBlock;
                    buffer.WriteByte(Conversions.ToByte(bitCount));

                    (int, int)[] palette = sectionData._palette;
                    System.Diagnostics.Debug.Assert(palette != null);
                    if (bitCount == 13)
                    {
                        buffer.WriteInt(0, true);
                    }
                    else
                    {
                        System.Diagnostics.Debug.Assert(bitCount >= 4 && bitCount <= 8);

                        int length = palette.Length;
                        buffer.WriteInt(length, true);

                        for (int i = 0; i < length; ++i)
                        {
                            (int id, _) = palette[i];
                            buffer.WriteInt(id, true);
                        }
                    }

                    long[] data = sectionData._data;
                    buffer.WriteInt(data.Length, true);
                    for (int i = 0; i < data.Length; ++i)
                    {
                        buffer.WriteLong(data[i]);
                    }

                    buffer.WriteData(sectionData._blockLights);
                    buffer.WriteData(sectionData._skyLights);

                }

                public void Dispose()
                {
                    // Assertion.
                    System.Diagnostics.Debug.Assert(!_disposed);

                    // Release resources.
                    _palette = null;

                    _data = null;

                    _blockLights = null; _skyLights = null;

                    // Finish.
                    System.GC.SuppressFinalize(this);
                    _disposed = true;
                }


            }

            private bool _disposed = false;

            private int _count = 0;
            private SectionData?[] _sections = new SectionData?[_MAX_SECTION_COUNT];  // from bottom to top
            /*public int Count => _count;*/

            internal static (int, byte[]) Write(ChunkData chunkData)
            {
                using Buffer buffer = new();

                int mask = 0;
                for (int i = 0; i < _MAX_SECTION_COUNT; ++i)
                {
                    SectionData? section = chunkData._sections[i];
                    if (section == null) continue;

                    mask |= (1 << i);  // TODO;
                    SectionData.Write(buffer, section);
                }

                // TODO
                for (int z = 0; z < _WIDTH; ++z)
                {
                    for (int x = 0; x < _WIDTH; ++x)
                    {
                        buffer.WriteByte(127);  // Void Biome
                    }
                }

                return (mask, buffer.ReadData());
            }

            internal static (int, byte[]) Write()
            {
                Buffer buffer = new();

                int mask = 0;
                System.Diagnostics.Debug.Assert(_MAX_SECTION_COUNT == 16);

                // TODO: biomes
                for (int z = 0; z < _WIDTH; ++z)
                {
                    for (int x = 0; x < _WIDTH; ++x)
                    {
                        buffer.WriteByte(127);  // Void Biome
                    }
                }

                return (mask, buffer.ReadData());
            }

            public ChunkData() { }

            public void SetId(int defaultId, int x, int y, int z, int id)
            {
                System.Diagnostics.Debug.Assert(!_disposed);

                System.Diagnostics.Debug.Assert(x >= 0 && x < SectionData.WIDTH);
                System.Diagnostics.Debug.Assert(z >= 0 && z < SectionData.WIDTH);

                System.Diagnostics.Debug.Assert(y >= 0);

                int ySection = y / SectionData.HEIGHT;
                System.Diagnostics.Debug.Assert(ySection < _MAX_SECTION_COUNT);

                int yPrime = y - (ySection * SectionData.HEIGHT);
                System.Diagnostics.Debug.Assert(yPrime >= 0 && yPrime < SectionData.HEIGHT);

                SectionData? section = _sections[ySection];
                if (section == null)
                {
                    if (id == defaultId)
                    {
                        return;
                    }

                    section = new SectionData(defaultId);
                    _sections[ySection] = section;
                }

                section.SetId(x, yPrime, z, id);
                return;
            }

            public int GetId(int defaultId, int x, int y, int z)
            {
                System.Diagnostics.Debug.Assert(!_disposed);

                System.Diagnostics.Debug.Assert(x >= 0 && x < SectionData.WIDTH);
                System.Diagnostics.Debug.Assert(z >= 0 && z < SectionData.WIDTH);

                if (y < 0)
                {
                    return defaultId;
                }

                int ySection = y / SectionData.HEIGHT;
                if (ySection >= _MAX_SECTION_COUNT)
                {
                    return defaultId;
                }

                int yPrime = y - (ySection * SectionData.HEIGHT);
                System.Diagnostics.Debug.Assert(yPrime >= 0 && yPrime < SectionData.HEIGHT);

                SectionData? section = _sections[ySection];
                if (section == null)
                {
                    return defaultId;
                }

                return section.GetId(x, yPrime, z);
            }

            public void Dispose()
            {
                // Assertion.
                System.Diagnostics.Debug.Assert(!_disposed);

                // Release resources.
                for (int i = 0; i < _sections.Length; ++i)
                {
                    SectionData? data = _sections[i];
                    if (data == null)
                    {
                        continue;
                    }

                    data.Dispose();
                }
                _sections = null;

                // Finish.
                System.GC.SuppressFinalize(this);
                _disposed = true;
            }

        }

        private bool _disposed = false;

        public static readonly Blocks _DEFAULT_BLOCK = Blocks.Air;

        private readonly Table<ChunkLocation, ChunkData> _CHUNKS = new();  // Disposable

        public BlockContext() { }

        public void SetBlock(BlockLocation loc, Blocks block)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            ChunkData chunk;

            ChunkLocation locChunk = ChunkLocation.Generate(loc);
            if (!_CHUNKS.Contains(locChunk))
            {
                if (block == _DEFAULT_BLOCK)
                {
                    return;
                }

                chunk = new ChunkData();
                _CHUNKS.Insert(locChunk, chunk);
            }
            else
            {
                chunk = _CHUNKS.Lookup(locChunk);
            }

            int x = loc.X - (locChunk.X * ChunkLocation.WIDTH),
                y = loc.Y,
                z = loc.Z - (locChunk.Z * ChunkLocation.WIDTH);
            chunk.SetId(_DEFAULT_BLOCK.GetId(), x, y, z, block.GetId());
        }

        public Blocks GetBlock(BlockLocation loc)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            ChunkLocation locChunk = ChunkLocation.Generate(loc);

            if (!_CHUNKS.Contains(locChunk))
            {
                return _DEFAULT_BLOCK;
            }

            ChunkData chunk = _CHUNKS.Lookup(locChunk);
            int x = loc.X - (locChunk.X * ChunkLocation.WIDTH),
                y = loc.Y,
                z = loc.Z - (locChunk.Z * ChunkLocation.WIDTH);
            int id = chunk.GetId(_DEFAULT_BLOCK.GetId(), x, y, z);
            return BlockExtensions.ToBlock(id);
        }

        public void Dispose()
        {
            // Assertion.
            System.Diagnostics.Debug.Assert(!_disposed);

            // Release resources.
            (ChunkLocation, ChunkData)[]_chunks = _CHUNKS.Flush();
            for (int i = 0; i < _chunks.Length; ++i)
            {
                (var _, ChunkData data) = _chunks[i];
                data.Dispose();
            }
            _CHUNKS.Dispose();

            // Finish.
            System.GC.SuppressFinalize(this);
            _disposed = true;
        }

    }
}



using Common;
using Containers;

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

                private int[]? _palette;

                private const int _BITS_PER_DATA_UNIT = sizeof(ulong) * 8; // TODO: Change to appropriate name.
                private ulong[] _data;

                private byte[] _blockLights, _skyLights;

                private static int GetDataLength(int _bitsPerBlock)
                {
                    int a = _TOTAL_BLOCK_COUNT * _bitsPerBlock;
                    System.Diagnostics.Debug.Assert(a % _BITS_PER_DATA_UNIT == 0);
                    int length = a / _BITS_PER_DATA_UNIT;
                    return length;
                }

                public SectionData(int defaultBlockId)
                {
                    _bitsPerBlock = 4;

                    _palette = [defaultBlockId];

                    {
                        int length = GetDataLength(_bitsPerBlock);
                        _data = new ulong[length];
                        System.Array.Fill<ulong>(_data, defaultBlockId);
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


                public int GetBlockId(int x, int y, int z)
                {
                    System.Diagnostics.Debug.Assert(!_disposed);

                    System.Diagnostics.Debug.Assert(x >= 0 && x <= WIDTH);
                    System.Diagnostics.Debug.Assert(z >= 0 && z <= WIDTH);
                    System.Diagnostics.Debug.Assert(y >= 0 && y <= HEIGHT);

                    ulong mask = (1UL << _bitsPerBlock) - 1UL;

                    int i;
                    ulong value;

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
                        return _palette[value];
                    }
                }

                private void ExpandData(int bitsPerBlock)
                {
                    System.Diagnostics.Debug.Assert(!_disposed);

                    System.Diagnostics.Debug.Assert(_palette != null);
                    System.Diagnostics.Debug.Assert(bitsPerBlock > _bitsPerBlock);

                    int length = GetDataLength(bitsPerBlock);
                    var data = new ulong[length];

                    ulong mask = ((Conversions.ToUlong(1) << bitsPerBlock) - 1);

                    int i;
                    ulong value;

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
                                            value = (_data[start] >> offset | _data[end] << (_BITS_PER_DATA_UNIT - offset));
                                        }

                                        value &= mask;
                                        id = _palette[value];
                                    }

                                    {
                                        start = (i * bitsPerBlock) / _BITS_PER_DATA_UNIT;
                                        offset = (i * bitsPerBlock) % _BITS_PER_DATA_UNIT;
                                        end = (((i + 1) * bitsPerBlock) - 1) / _BITS_PER_DATA_UNIT;

                                        value = Conversions.ToUlong(id);

                                        System.Diagnostics.Debug.Assert((value & ~((1UL << bitsPerBlock) - 1UL)) == 0);
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
                        System.Diagnostics.Debug.Assert(bitsPerBlock > 4 && bitsPerBlock <= 8);

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
                                            value = (_data[start] >> offset | _data[end] << (_BITS_PER_DATA_UNIT - offset));
                                        }

                                        value &= mask;
                                    }

                                    {
                                        start = (i * bitsPerBlock) / _BITS_PER_DATA_UNIT;
                                        offset = (i * bitsPerBlock) % _BITS_PER_DATA_UNIT;
                                        end = (((i + 1) * bitsPerBlock) - 1) / _BITS_PER_DATA_UNIT;

                                        System.Diagnostics.Debug.Assert(
                                            (value & ~((Conversions.ToUlong(1) << bitsPerBlock) - 1)) == 0);
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

                    _bitsPerBlock = bitsPerBlock;
                }

                private ulong AcquireValue(int id)
                {
                    System.Diagnostics.Debug.Assert(!_disposed);

                    ulong value;

                    if (_bitsPerBlock == 13)
                    {
                        System.Diagnostics.Debug.Assert(_palette == null);

                        value = Conversions.ToUlong(id);
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
                            int idPalette = _palette[i];
                            if (id == idPalette)
                            {
                                indexPalette = i;
                            }
                        }

                        if (indexPalette >= 0)
                        {
                            value = Conversions.ToUlong(indexPalette);
                        }
                        else
                        {
                            System.Diagnostics.Debug.Assert(indexPalette == -1);

                            int lengthNew = _palette.Length + 1;

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

                            if (bitsPerBlock == 13)
                            {
                                value = Conversions.ToUlong(id);

                                _palette = null;
                            }
                            else
                            {
                                System.Diagnostics.Debug.Assert(bitsPerBlock >= 4 && bitsPerBlock <= 8);

                                var paletteNew = new int[lengthNew];

                                int lastIndex = _palette.Length;
                                System.Array.Copy(_palette, paletteNew, lastIndex);
                                paletteNew[lastIndex] = id;

                                value = Conversions.ToUlong(lastIndex);

                                _palette = paletteNew;
                            }

                        }
                    }

                    return value;
                }

                public bool SetBlockId(int x, int y, int z, int id)
                {
                    System.Diagnostics.Debug.Assert(!_disposed);

                    System.Diagnostics.Debug.Assert(x >= 0 && x <= WIDTH);
                    System.Diagnostics.Debug.Assert(y >= 0 && y <= WIDTH);
                    System.Diagnostics.Debug.Assert(z >= 0 && z <= HEIGHT);

                    ulong value = AcquireValue(id);

                    int i = (((y * HEIGHT) + z) * WIDTH) + x;
                    int start = (i * _bitsPerBlock) / _BITS_PER_DATA_UNIT;
                    int offset = (i * _bitsPerBlock) % _BITS_PER_DATA_UNIT;
                    int end = ((i + 1) * _bitsPerBlock - 1) / _BITS_PER_DATA_UNIT;

                    System.Diagnostics.Debug.Assert(
                        (value & ~((Conversions.ToUlong(1) << _bitsPerBlock) - 1)) == 0);
                    _data[start] |= (value << offset);

                    if (start != end)
                    {
                        _data[end] = (value >> (_BITS_PER_DATA_UNIT - offset));
                    }

                    throw new System.NotImplementedException();
                }

                internal static void Write(Buffer buffer, SectionData section)
                {
                    int bitCount = section._bitsPerBlock;
                    buffer.WriteByte(Conversions.ToByte(bitCount));

                    // Write pallete.
                    Block[]? palette = section._palette;
                    if (bitCount == 13)
                    {
                        System.Diagnostics.Debug.Assert(palette == null);

                        buffer.WriteInt(0, true);
                    }
                    else
                    {
                        System.Diagnostics.Debug.Assert(palette != null);
                        System.Diagnostics.Debug.Assert(bitCount >= 4 && bitCount <= 8);

                        int length = palette.Length;
                        buffer.WriteInt(length, true);
                        for (int i = 0; i < length; ++i)
                        {
                            Block b = palette[i];
                            buffer.WriteInt(Conversions.ToInt(b.GlobalPaletteId), true);
                        }
                    }

                    // TODO: Use memory copying;
                    System.Diagnostics.Debug.Assert(unchecked((long)ulong.MaxValue) == -1);
                    buffer.WriteInt(section._data.Length, true);
                    for (int i = 0; i < section._data.Length; ++i)
                    {
                        buffer.WriteLong(Conversions.ToLong(section._data[i]));
                    }

                    buffer.WriteData(section._blockLights);
                    buffer.WriteData(section._skyLights);

                }

                public void Dispose()
                {
                    System.Diagnostics.Debug.Assert(!_disposed);

                    _palette = null;

                    _data = null;

                    _blockLights = null; _skyLights = null;

                    _disposed = true;
                }


            }

            private bool _disposed = false;

            private readonly int _DEFAULT_BLOCK_ID;

            private int _count = 0;
            private SectionData?[] _sections = new SectionData?[_MAX_SECTION_COUNT];  // from bottom to top
            /*public int Count => _count;*/

            internal static (int, byte[]) Write(ChunkData chunk)
            {
                using Buffer buffer = new();

                int mask = 0;
                for (int i = 0; i < _MAX_SECTION_COUNT; ++i)
                {
                    SectionData? section = chunk._sections[i];
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
                System.Diagnostics.Debug.Assert(_TOTAL_SECTION_COUNT == 16);

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

            public ChunkData(int defaultBlockId)
            {
                _DEFAULT_BLOCK_ID = defaultBlockId;
            }

            public bool SetBlockId(int x, int y, int z, int id)
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
                    if (id == _DEFAULT_BLOCK_ID)
                    {
                        return;
                    }

                    section = new SectionData(_DEFAULT_BLOCK_ID);
                    _sections[ySection] = section;
                }

                section.SetBlockId(x, yPrime, z, id);

                throw new System.NotImplementedException();
            }

            public int GetBlockId(int x, int y, int z)
            {
                System.Diagnostics.Debug.Assert(!_disposed);

                System.Diagnostics.Debug.Assert(x >= 0 && x < SectionData.WIDTH);
                System.Diagnostics.Debug.Assert(z >= 0 && z < SectionData.WIDTH);

                if (y < 0)
                {
                    return _DEFAULT_BLOCK_ID;
                }

                int ySection = y / SectionData.HEIGHT;

                if (ySection >= _MAX_SECTION_COUNT)
                {
                    return _DEFAULT_BLOCK_ID;
                }

                int yPrime = y - (ySection * SectionData.HEIGHT);
                System.Diagnostics.Debug.Assert(yPrime >= 0 && yPrime < SectionData.HEIGHT);

                SectionData? section = _sections[ySection];
                if (section == null)
                {
                    return _DEFAULT_BLOCK_ID;
                }

                return section.GetBlockId(x, yPrime, z);
            }

            public void Dispose()
            {
                System.Diagnostics.Debug.Assert(!_disposed);

                // Assertion.
                System.Diagnostics.Debug.Assert(_count == 0);


                // Release resources.

                // Finish.
                System.GC.SuppressFinalize(this);
                _disposed = true;
            }

        }

        private bool _disposed = false;

        private readonly int _DEFAULT_BLOCK_ID;

        private readonly Table<ChunkLocation, ChunkData> _CHUNKS = new();  // Disposable

        public BlockContext(int defaultBlockId)
        {
            _DEFAULT_BLOCK_ID = defaultBlockId;
        }

        public void SetBlockId(BlockLocation loc, int id)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            ChunkData chunk;

            ChunkLocation locChunk = ChunkLocation.Generate(loc);
            if (!_CHUNKS.Contains(locChunk))
            {
                if (id == _DEFAULT_BLOCK_ID)
                {
                    return;
                }

                chunk = new ChunkData(_DEFAULT_BLOCK_ID);
                _CHUNKS.Insert(locChunk, chunk);
            }
            else
            {
                chunk = _CHUNKS.Lookup(locChunk);
            }


            int x = loc.X - (locChunk.X * ChunkLocation.WIDTH),
                y = loc.Y,
                z = loc.Z - (locChunk.Z * ChunkLocation.WIDTH);

            chunk.SetBlockId(x, y, z, id);

        }

        public int GetBlockId(BlockLocation loc)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            ChunkLocation locChunk = ChunkLocation.Generate(loc);

            if (!_CHUNKS.Contains(locChunk))
            {
                return _DEFAULT_BLOCK_ID;
            }

            ChunkData chunk = _CHUNKS.Lookup(locChunk);

            int x = loc.X - (locChunk.X * ChunkLocation.WIDTH),
                y = loc.Y,
                z = loc.Z - (locChunk.Z * ChunkLocation.WIDTH);
            

            return chunk.GetBlockId(x, y, z);
        }

    }
}

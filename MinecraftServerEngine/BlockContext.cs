

using Common;
using Containers;
using MinecraftServerEngine.PhysicsEngine;

namespace MinecraftServerEngine
{
    
    internal sealed class BlockContext : Terrain
    {
        private enum Directions : int
        {
            DOWN,
            UP,
            NORTH,
            SOUTH,
            WEST,
            EAST,
        }

        private static Directions ToCCWDirection(Directions d)
        {
            switch (d)
            {
                default:
                    throw new System.NotImplementedException();
                case Directions.NORTH:
                    return Directions.WEST;
                case Directions.EAST:
                    return Directions.NORTH;
                case Directions.SOUTH:
                    return Directions.EAST;
                case Directions.WEST:
                    return Directions.SOUTH;
            }
        }

        private static Directions ToCWDirection(Directions d)
        {
            switch (d)
            {
                default:
                    throw new System.NotImplementedException();
                case Directions.NORTH:
                    return Directions.EAST;
                case Directions.EAST:
                    return Directions.SOUTH;
                case Directions.SOUTH:
                    return Directions.WEST;
                case Directions.WEST:
                    return Directions.NORTH;
            }
        }

        private static Directions ToOppositeDirection(Directions d)
        {
            switch (d)
            {
                default:
                    throw new System.NotImplementedException();
                case Directions.UP:
                    return Directions.DOWN;
                case Directions.DOWN:
                    return Directions.UP;
                case Directions.NORTH:
                    return Directions.SOUTH;
                case Directions.EAST:
                    return Directions.WEST;
                case Directions.SOUTH:
                    return Directions.NORTH;
                case Directions.WEST:
                    return Directions.EAST;
            }
        }

        private static bool IsStairsBlock(Blocks block)
        {
            switch (block)
            {
                default:
                    return false;
                case Blocks.EastBottomOakWoodStairs:
                    return true;
                case Blocks.WestBottomOakWoodStairs:
                    return true;
                case Blocks.SouthBottomOakWoodStairs:
                    return true;
                case Blocks.NorthBottomOakWoodStairs:
                    return true;
                case Blocks.EastTopOakWoodStairs:
                    return true;
                case Blocks.WestTopOakWoodStairs:
                    return true;
                case Blocks.SouthTopOakWoodStairs:
                    return true;
                case Blocks.NorthTopOakWoodStairs:
                    return true;

            }
        }

        private static Directions GetStairsDirection(Blocks block)
        {
            System.Diagnostics.Debug.Assert(IsStairsBlock(block));

            int id = block.GetId();
            int metadata = id & 0b_1111;
            switch (metadata % 4)
            {
                default:
                    throw new System.NotImplementedException();
                case 0:
                    return Directions.EAST;
                case 1:
                    return Directions.WEST;
                case 2:
                    return Directions.SOUTH;
                case 3:
                    return Directions.NORTH;
            }
        }

        private static bool IsBottomStairsBlock(Blocks block)
        {
            System.Diagnostics.Debug.Assert(IsStairsBlock(block));

            int id = block.GetId();
            int metadata = id & 0b_1111;
            return metadata < 4;
        }

        private static bool IsVerticalStairsBlock(Blocks block)
        {
            System.Diagnostics.Debug.Assert(IsStairsBlock(block));

            int id = block.GetId();
            int metadata = id & 0b_1111;
            return metadata == 0 || metadata == 1 || metadata == 4 || metadata == 5;
        }

        private sealed class ChunkData : System.IDisposable
        {
            private sealed class SectionData : System.IDisposable
            {
                public static void Write(Buffer buffer, SectionData sectionData)
                {
                    byte bitCount = sectionData._bitsPerBlock;
                    buffer.WriteByte(bitCount);

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

                private bool _disposed = false;

                public const int BlocksPerWidth = ChunkData.BlocksPerWidth;
                public const int BlocksPerHeight = ChunkData.BlocksPerHeight / SectionCount;
                public const int TotalBlockCount = BlocksPerWidth * BlocksPerWidth * BlocksPerHeight;

                private byte _bitsPerBlock;

                private (int, int)[] _palette;

                private const int _BITS_PER_DATA_UNIT = sizeof(long) * 8; // TODO: Change to appropriate name.
                private long[] _data;

                private byte[] _blockLights, _skyLights;

                private static int GetDataLength(int _bitsPerBlock)
                {
                    int a = TotalBlockCount * _bitsPerBlock;
                    System.Diagnostics.Debug.Assert(a % _BITS_PER_DATA_UNIT == 0);
                    int length = a / _BITS_PER_DATA_UNIT;
                    return length;
                }

                public SectionData(int defaultId)
                {
                    _bitsPerBlock = 4;

                    _palette = [(defaultId, TotalBlockCount)];

                    {
                        int length = GetDataLength(_bitsPerBlock);
                        _data = new long[length];

                        int i;
                        long value = (long)defaultId;

                        int start, offset, end;

                        for (int y = 0; y < BlocksPerHeight; ++y)
                        {
                            for (int z = 0; z < BlocksPerWidth; ++z)
                            {
                                for (int x = 0; x < BlocksPerWidth; ++x)
                                {
                                    i = (((y * BlocksPerHeight) + z) * BlocksPerWidth) + x;

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
                        int length = TotalBlockCount / 2;
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

                    System.Diagnostics.Debug.Assert(x >= 0 && x <= BlocksPerWidth);
                    System.Diagnostics.Debug.Assert(z >= 0 && z <= BlocksPerWidth);
                    System.Diagnostics.Debug.Assert(y >= 0 && y <= BlocksPerHeight);

                    long mask = (1L << _bitsPerBlock) - 1L;

                    int i;
                    long value;

                    int start, offset, end;

                    i = (((y * BlocksPerHeight) + z) * BlocksPerWidth) + x;
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
                        System.Diagnostics.Debug.Assert(value <= int.MaxValue);
                        return (int)value;
                    }
                    else
                    {
                        System.Diagnostics.Debug.Assert(_palette != null);

                        (int id, var _) = _palette[value];
                        return id;
                    }
                }

                private void ExpandData(byte bitsPerBlock)
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

                        for (int y = 0; y < BlocksPerHeight; ++y)
                        {
                            for (int z = 0; z < BlocksPerWidth; ++z)
                            {
                                for (int x = 0; x < BlocksPerWidth; ++x)
                                {
                                    i = (((y * BlocksPerHeight) + z) * BlocksPerWidth) + x;

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

                                        value = (long)id;

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
                        System.Diagnostics.Debug.Assert(bitsPerBlock > 4 && bitsPerBlock <= 8);

                        for (int y = 0; y < BlocksPerHeight; ++y)
                        {
                            for (int z = 0; z < BlocksPerWidth; ++z)
                            {
                                for (int x = 0; x < BlocksPerWidth; ++x)
                                {
                                    i = (((y * BlocksPerHeight) + z) * BlocksPerWidth) + x;

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

                        value = (long)id;
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
                            value = (long)indexPalette;
                        }
                        else
                        {
                            System.Diagnostics.Debug.Assert(indexPalette == -1);

                            int length = _palette.Length;
                            int lengthNew = length + 1;

                            byte bitsPerBlock;
                            if (lengthNew <= 0b_00001111U)
                            {
                                bitsPerBlock = 4;
                            }
                            else if (lengthNew <= 0b_00011111U)
                            {
                                bitsPerBlock = 5;
                            }
                            else if (lengthNew <= 0b_00111111U)
                            {
                                bitsPerBlock = 6;
                            }
                            else if (lengthNew <= 0b_01111111U)
                            {
                                bitsPerBlock = 7;
                            }
                            else if (lengthNew <= 0b_11111111U)
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
                                value = (long)id;
                            }
                            else
                            {
                                System.Diagnostics.Debug.Assert(bitsPerBlock >= 4 && bitsPerBlock <= 8);

                                value = (long)length;
                            }

                        }
                    }

                    return value;
                }

                public void SetId(int x, int y, int z, int id)
                {
                    System.Diagnostics.Debug.Assert(!_disposed);

                    System.Diagnostics.Debug.Assert(x >= 0 && x <= BlocksPerWidth);
                    System.Diagnostics.Debug.Assert(y >= 0 && y <= BlocksPerHeight);
                    System.Diagnostics.Debug.Assert(z >= 0 && z <= BlocksPerWidth);

                    long value = GetValue(id);

                    int i = (((y * BlocksPerHeight) + z) * BlocksPerWidth) + x;
                    int start = (i * _bitsPerBlock) / _BITS_PER_DATA_UNIT;
                    int offset = (i * _bitsPerBlock) % _BITS_PER_DATA_UNIT;
                    int end = ((i + 1) * _bitsPerBlock - 1) / _BITS_PER_DATA_UNIT;

                    System.Diagnostics.Debug.Assert(
                        (value & ~((1L << _bitsPerBlock) - 1L)) == 0L);
                    _data[start] |= (value << offset);

                    if (start != end)
                    {
                        _data[end] = (value >> (_BITS_PER_DATA_UNIT - offset));
                    }
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

            public const int BlocksPerWidth = ChunkLocation.BlocksPerWidth;

            public const int BlocksPerHeight = ChunkLocation.BlocksPerHeight;
            public const int SectionCount = 16;

            public static (int, byte[]) Write(ChunkData chunkData)
            {
                using Buffer buffer = new();

                int mask = 0;
                for (int i = 0; i < SectionCount; ++i)
                {
                    SectionData section = chunkData._sections[i];
                    if (section == null) continue;

                    mask |= (1 << i);  // TODO;
                    SectionData.Write(buffer, section);
                }

                // TODO
                for (int z = 0; z < BlocksPerWidth; ++z)
                {
                    for (int x = 0; x < BlocksPerWidth; ++x)
                    {
                        buffer.WriteByte(127);  // Void Biome
                    }
                }

                return (mask, buffer.ReadData());
            }

            public static (int, byte[]) Write()
            {
                using Buffer buffer = new();

                int mask = 0;
                System.Diagnostics.Debug.Assert(SectionCount == 16);

                // TODO: biomes
                for (int z = 0; z < BlocksPerWidth; ++z)
                {
                    for (int x = 0; x < BlocksPerWidth; ++x)
                    {
                        buffer.WriteByte(127);  // Void Biome
                    }
                }

                return (mask, buffer.ReadData());
            }

            private bool _disposed = false;

            private int _count = 0;
            private SectionData[] _sections = new SectionData[SectionCount];  // from bottom to top

            public ChunkData() { }

            public void SetId(int defaultId, int x, int y, int z, int id)
            {
                System.Diagnostics.Debug.Assert(!_disposed);

                System.Diagnostics.Debug.Assert(x >= 0 && x < BlocksPerWidth);
                System.Diagnostics.Debug.Assert(z >= 0 && z < BlocksPerWidth);

                System.Diagnostics.Debug.Assert(y >= 0);

                int ySection = y / SectionData.BlocksPerHeight;
                System.Diagnostics.Debug.Assert(ySection < SectionCount);

                int yPrime = y - (ySection * SectionData.BlocksPerHeight);
                System.Diagnostics.Debug.Assert(yPrime >= 0 && yPrime < SectionData.BlocksPerHeight);

                SectionData section = _sections[ySection];
                if (section == null)
                {
                    if (id == defaultId)
                    {
                        return;
                    }

                    section = new SectionData(defaultId);
                    _sections[ySection] = section;

                    _count++;
                }

                section.SetId(x, yPrime, z, id);
            }

            public int GetId(int defaultId, int x, int y, int z)
            {
                System.Diagnostics.Debug.Assert(!_disposed);

                System.Diagnostics.Debug.Assert(x >= 0 && x < BlocksPerWidth);
                System.Diagnostics.Debug.Assert(z >= 0 && z < BlocksPerWidth);

                if (y < 0)
                {
                    return defaultId;
                }

                int ySection = y / SectionData.BlocksPerHeight;
                if (ySection >= SectionCount)
                {
                    return defaultId;
                }

                int yPrime = y - (ySection * SectionData.BlocksPerHeight);
                System.Diagnostics.Debug.Assert(yPrime >= 0 && yPrime < SectionData.BlocksPerHeight);

                SectionData section = _sections[ySection];
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
                    SectionData data = _sections[i];
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

        public static readonly Blocks DefaultBlock = Blocks.Air;

        private readonly Table<ChunkLocation, ChunkData> Chunks = new();  // Disposable

        public BlockContext() 
        {
            // Dummy code.
            for (int z = -10; z <= 10; ++z)
            {
                for (int x = -10; x <= 10; ++x)
                {
                    BlockLocation loc = new(x, 100, z);

                    SetBlock(loc, Blocks.Stone);
                }
            }
        }

        private ChunkLocation BlockToChunk(BlockLocation loc)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            int x = loc.X / ChunkLocation.BlocksPerWidth,
                z = loc.Z / ChunkLocation.BlocksPerWidth;

            double r1 = (double)loc.X % (double)ChunkLocation.BlocksPerWidth,
                   r2 = (double)loc.Z % (double)ChunkLocation.BlocksPerWidth;
            if (r1 < 0.0D)
            {
                --x;
            }
            if (r2 < 0.0D)
            {
                --z;
            }

            return new ChunkLocation(x, z);
        }

        private void SetBlock(BlockLocation loc, Blocks block)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            ChunkData chunk;

            ChunkLocation locChunk = BlockToChunk(loc);
            if (!Chunks.Contains(locChunk))
            {
                if (block == DefaultBlock)
                {
                    return;
                }

                chunk = new ChunkData();
                Chunks.Insert(locChunk, chunk);
            }
            else
            {
                chunk = Chunks.Lookup(locChunk);
            }

            int x = loc.X - (locChunk.X * ChunkLocation.BlocksPerWidth),
                y = loc.Y,
                z = loc.Z - (locChunk.Z * ChunkLocation.BlocksPerWidth);
            chunk.SetId(DefaultBlock.GetId(), x, y, z, block.GetId());
        }

        private Blocks GetBlock(BlockLocation loc)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            ChunkLocation locChunk = BlockToChunk(loc);
            if (!Chunks.Contains(locChunk))
            {
                return DefaultBlock;
            }

            ChunkData chunk = Chunks.Lookup(locChunk);
            int x = loc.X - (locChunk.X * ChunkLocation.BlocksPerWidth),
                y = loc.Y,
                z = loc.Z - (locChunk.Z * ChunkLocation.BlocksPerWidth);
            int id = chunk.GetId(DefaultBlock.GetId(), x, y, z);
            return BlockExtensions.ToBlock(id);
        }

        private Blocks GetBlock(BlockLocation loc, Directions d, int s)
        {
            BlockLocation locPrime;
            switch (d)
            {
                default:
                    throw new System.NotImplementedException();
                case Directions.EAST:
                    locPrime = new BlockLocation(loc.X + s, loc.Y, loc.Z);
                    break;
                case Directions.WEST:
                    locPrime = new BlockLocation(loc.X - s, loc.Y, loc.Z);
                    break;
                case Directions.SOUTH:
                    locPrime = new BlockLocation(loc.X, loc.Y, loc.Z + s);
                    break;
                case Directions.NORTH:
                    locPrime = new BlockLocation(loc.X, loc.Y, loc.Z - s);
                    break;
                case Directions.UP:
                    locPrime = new BlockLocation(loc.X, loc.Y + s, loc.Z);
                    break;
                case Directions.DOWN:
                    locPrime = new BlockLocation(loc.X, loc.Y - s, loc.Z);
                    break;
            }

            return GetBlock(locPrime);
        }

        private void GenerateBoundingBoxForCubeBlock(
            Queue<AxisAlignedBoundingBox> queue, BlockLocation loc)
        {
            System.Diagnostics.Debug.Assert(queue != null);

            System.Diagnostics.Debug.Assert(!_disposed);

            Vector min = loc.GetMinVector(),
                   max = loc.GetMaxVector();
            AxisAlignedBoundingBox aabb = new(max, min);

            queue.Enqueue(aabb);
            return;
        }

        private (Directions, bool, int) DetermineStairsBlockShape(
            BlockLocation loc, Blocks block)
        {
            System.Diagnostics.Debug.Assert(IsStairsBlock(block));

            Directions d = GetStairsDirection(block);
            bool bottom = IsBottomStairsBlock(block);

            Blocks block2 = GetBlock(loc, d, 1);
            if (IsStairsBlock(block2) &&
                bottom == IsBottomStairsBlock(block2))
            {
                if (IsVerticalStairsBlock(block2) != IsVerticalStairsBlock(block))
                {
                    Directions d2 = GetStairsDirection(block2);
                    Blocks block3 = GetBlock(loc, ToOppositeDirection(d2), 1);
                    if (!IsStairsBlock(block3) ||
                        GetStairsDirection(block3) != d ||
                        IsBottomStairsBlock(block3) != bottom)
                    {
                        if (d2 == ToCCWDirection(d))
                        {
                            // outer left
                            return (d, bottom, 1);
                        }
                        else
                        {
                            System.Diagnostics.Debug.Assert(d2 == ToCWDirection(d));
                            // outer right
                            return (d, bottom, 2);
                        }
                    }
                }

            }

            Blocks block4 = GetBlock(loc, ToOppositeDirection(d), 1);
            if (IsStairsBlock(block4) &&
                bottom == IsBottomStairsBlock(block4))
            {
                if (IsVerticalStairsBlock(block4) != IsVerticalStairsBlock(block))
                {
                    Directions d4 = GetStairsDirection(block4);
                    Blocks block5 = GetBlock(loc, d4, 1);
                    if (!IsStairsBlock(block5) ||
                        GetStairsDirection(block5) != d ||
                        IsBottomStairsBlock(block5) != bottom)
                    {
                        if (d4 == ToCCWDirection(d))
                        {
                            // inner left
                            return (d, bottom, 3);
                        }
                        else
                        {
                            System.Diagnostics.Debug.Assert(d4 == ToCWDirection(d));
                            // inner right
                            return (d, bottom, 4);
                        }
                    }
                }
            }


            // straight
            return (d, bottom, 0);
        }

        private void GenerateBoundingBoxForStairsBlock(
            Queue<AxisAlignedBoundingBox> queue, BlockLocation loc, Blocks block)
        {
            System.Diagnostics.Debug.Assert(queue != null);

            System.Diagnostics.Debug.Assert(!_disposed);

            (Directions d, bool bottom, int b) = DetermineStairsBlockShape(loc, block);

            throw new System.NotImplementedException();
        }

        protected override void GenerateBoundingBoxForBlock(
            Queue<AxisAlignedBoundingBox> queue, BlockLocation loc)
        {
            System.Diagnostics.Debug.Assert(queue != null);

            System.Diagnostics.Debug.Assert(!_disposed);

            Blocks block = GetBlock(loc);

            /**
             * 0: None (Air)
             * 1: Cube
             * 2: Slab
             * 3: Stairs
             */
            int a = 0;

            switch (block)
            {
                default:
                    throw new System.NotImplementedException();
                case Blocks.Air:
                    a = 0;
                    break;
                case Blocks.Stone:
                    a = 1;
                    break;
                case Blocks.Granite:
                    a = 1;
                    break;
                case Blocks.PolishedGranite:
                    a = 1;
                    break;
                case Blocks.EastBottomOakWoodStairs:
                    a = 3;
                    break;
            }

            switch (a)
            {
                default:
                    throw new System.NotImplementedException();
                case 0:
                    break;
                case 1:
                    GenerateBoundingBoxForCubeBlock(queue, loc);
                    break;
                case 3:
                    GenerateBoundingBoxForStairsBlock(queue, loc, block);
                    break;
            }
        }

        internal (int, byte[]) GetChunkData(ChunkLocation loc)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            if (Chunks.Contains(loc))
            {
                ChunkData data = Chunks.Lookup(loc);
                return ChunkData.Write(data);
            }
            else
            {
                return ChunkData.Write();
            }
        }

        public override void Dispose()
        {
            // Assertion.
            System.Diagnostics.Debug.Assert(!_disposed);

            // Release resources.
            (ChunkLocation, ChunkData)[]_chunks = Chunks.Flush();
            for (int i = 0; i < _chunks.Length; ++i)
            {
                (var _, ChunkData data) = _chunks[i];
                data.Dispose();
            }
            Chunks.Dispose();

            // Finish.
            base.Dispose();
            _disposed = true;
        }

    }
}

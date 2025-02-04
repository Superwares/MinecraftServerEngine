

using Common;
using Containers;

using MinecraftPrimitives;
using MinecraftServerEngine.PhysicsEngine;

namespace MinecraftServerEngine.Blocks
{

    public sealed class BlockContext : Terrain
    {

        private sealed class ChunkData : System.IDisposable
        {
            private sealed class SectionData : System.IDisposable
            {
                private bool _disposed = false;

                public const int BlocksPerWidth = ChunkData.BlocksPerWidth;
                public const int BlocksPerHeight = ChunkData.BlocksPerHeight / SectionCount;
                public const int TotalBlockCount = BlocksPerWidth * BlocksPerWidth * BlocksPerHeight;

                public const byte MaxBitsPerBlock = 13;
                public const int MaxMetadataBits = 4;

                private byte _bitsPerBlock;

                private (int, int)[] _palette;

                private const int _BITS_PER_DATA_UNIT = sizeof(ulong) * 8; // TODO: Change to appropriate name.
                private ulong[] _data;

                private byte[] _blockLights, _skyLights;

                public static SectionData Load(NBTTagCompound section)
                {
                    byte[] blocks = section.GetNBTTag<NBTTagByteArray>("Blocks").Data;
                    byte[] _data = section.GetNBTTag<NBTTagByteArray>("Data").Data;

                    byte[] skyLights = section.GetNBTTag<NBTTagByteArray>("SkyLight").Data;
                    byte[] blockLights = section.GetNBTTag<NBTTagByteArray>("BlockLight").Data;

                    if (blocks.Length != TotalBlockCount)
                    {
                        return null;
                    }

                    byte bitsPerBlock = MaxBitsPerBlock;
                    (int, int)[] palette = [];

                    int dataLength = GetDataLength(bitsPerBlock);
                    ulong[] data = new ulong[dataLength];

                    {
                        int i;
                        ulong metadata;
                        ulong id;

                        int start, offset, end;

                        for (int y = 0; y < BlocksPerHeight; ++y)
                        {
                            for (int z = 0; z < BlocksPerWidth; ++z)
                            {
                                for (int x = 0; x < BlocksPerWidth; ++x)
                                {
                                    i = (y * BlocksPerHeight + z) * BlocksPerWidth + x;

                                    metadata = _data[i / 2];

                                    if (i % 2 == 0)
                                    {
                                        metadata &= 0b00001111;
                                    }
                                    else
                                    {
                                        metadata = metadata >> 4;
                                    }

                                    byte block = blocks[i];

                                    id = (ulong)block << 4 | metadata;

                                    //if (id != 0)
                                    //{
                                    //    Block _block = BlockExtensions.ToBlock((int)id, 0);
                                    //    if (_block == 0)
                                    //    {
                                    //        //                                            MyConsole.Warn(
                                    //        //$"It is not exists block id in this engine. \n" +
                                    //        //$"\tblock: {block} (0x{block:X}, 0b{System.Convert.ToString(block, 2).PadLeft(MaxBitsPerBlock - MaxMetadataBits, '0')}), \n" +
                                    //        //$"\tmetadata: {metadata} (0x{metadata:X}, 0b{System.Convert.ToString((long)metadata, 2).PadLeft(MaxMetadataBits, '0')}), \n" +
                                    //        //$"\tid: {id} 0b{System.Convert.ToString((long)id, 2).PadLeft(MaxBitsPerBlock, '0')})"
                                    //        //                                                );

                                    //        id = 0;
                                    //    }
                                    //}

                                    start = i * bitsPerBlock / _BITS_PER_DATA_UNIT;
                                    offset = i * bitsPerBlock % _BITS_PER_DATA_UNIT;
                                    end = ((i + 1) * bitsPerBlock - 1) / _BITS_PER_DATA_UNIT;

                                    System.Diagnostics.Debug.Assert(
                                        (id & ~((1UL << bitsPerBlock) - 1UL)) == 0);
                                    data[start] |= id << offset;

                                    if (start != end)
                                    {
                                        data[end] = id >> _BITS_PER_DATA_UNIT - offset;
                                    }

                                }
                            }
                        }
                    }


                    if (skyLights.Length != TotalBlockCount / 2)
                    {
                        skyLights = new byte[TotalBlockCount / 2];
                    }

                    if (blockLights.Length != TotalBlockCount / 2)
                    {
                        blockLights = new byte[TotalBlockCount / 2];
                    }

                    return new SectionData(bitsPerBlock, palette, data, blockLights, skyLights);
                }

                public static void Write(MinecraftProtocolDataStream buffer, SectionData sectionData)
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

                    ulong[] data = sectionData._data;
                    buffer.WriteInt(data.Length, true);
                    for (int i = 0; i < data.Length; ++i)
                    {
                        buffer.WriteLong((long)data[i]);
                    }

                    buffer.WriteData(sectionData._blockLights);
                    buffer.WriteData(sectionData._skyLights);

                }

                private static int GetDataLength(int _bitsPerBlock)
                {
                    int a = TotalBlockCount * _bitsPerBlock;
                    System.Diagnostics.Debug.Assert(a % _BITS_PER_DATA_UNIT == 0);
                    int length = a / _BITS_PER_DATA_UNIT;
                    return length;
                }

                private SectionData(
                    byte bitsPerBlock,
                    (int, int)[] palette,
                    ulong[] data,
                    byte[] blockLights,
                    byte[] skyLights)
                {
                    _bitsPerBlock = bitsPerBlock;
                    _palette = palette;
                    _data = data;
                    _blockLights = blockLights;
                    _skyLights = skyLights;
                }

                public SectionData(int defaultId)
                {
                    _bitsPerBlock = 4;

                    _palette = [(defaultId, TotalBlockCount)];

                    {
                        int length = GetDataLength(_bitsPerBlock);
                        _data = new ulong[length];

                        int i;
                        ulong value = (ulong)defaultId;

                        int start, offset, end;

                        for (int y = 0; y < BlocksPerHeight; ++y)
                        {
                            for (int z = 0; z < BlocksPerWidth; ++z)
                            {
                                for (int x = 0; x < BlocksPerWidth; ++x)
                                {
                                    i = (y * BlocksPerHeight + z) * BlocksPerWidth + x;

                                    start = i * _bitsPerBlock / _BITS_PER_DATA_UNIT;
                                    offset = i * _bitsPerBlock % _BITS_PER_DATA_UNIT;
                                    end = ((i + 1) * _bitsPerBlock - 1) / _BITS_PER_DATA_UNIT;

                                    System.Diagnostics.Debug.Assert(
                                        (value & ~((1UL << _bitsPerBlock) - 1UL)) == 0);
                                    _data[start] |= value << offset;

                                    if (start != end)
                                    {
                                        _data[end] = value >> _BITS_PER_DATA_UNIT - offset;
                                    }

                                }
                            }
                        }

                    }

                    {
                        int length = TotalBlockCount / 2;
                        _blockLights = new byte[length];
                        System.Array.Fill(_blockLights, byte.MaxValue);
                        _skyLights = new byte[length];
                        System.Array.Fill(_skyLights, byte.MaxValue);
                    }

                }

                ~SectionData() => System.Diagnostics.Debug.Assert(false);

                public int GetId(int x, int y, int z)
                {
                    System.Diagnostics.Debug.Assert(!_disposed);

                    System.Diagnostics.Debug.Assert(x >= 0 && x <= BlocksPerWidth);
                    System.Diagnostics.Debug.Assert(z >= 0 && z <= BlocksPerWidth);
                    System.Diagnostics.Debug.Assert(y >= 0 && y <= BlocksPerHeight);

                    ulong mask = (1UL << _bitsPerBlock) - 1UL;

                    int i;
                    ulong value;

                    int start, offset, end;

                    i = (y * BlocksPerHeight + z) * BlocksPerWidth + x;
                    start = i * _bitsPerBlock / _BITS_PER_DATA_UNIT;
                    offset = i * _bitsPerBlock % _BITS_PER_DATA_UNIT;
                    end = ((i + 1) * _bitsPerBlock - 1) / _BITS_PER_DATA_UNIT;

                    if (start == end)
                    {
                        value = _data[start] >> offset;
                    }
                    else
                    {
                        value = _data[start] >> offset | _data[end] << _BITS_PER_DATA_UNIT - offset;
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
                    var data = new ulong[length];

                    ulong mask = (1UL << bitsPerBlock) - 1UL;

                    int i;
                    ulong value;

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
                                    i = (y * BlocksPerHeight + z) * BlocksPerWidth + x;

                                    {
                                        start = i * _bitsPerBlock / _BITS_PER_DATA_UNIT;
                                        offset = i * _bitsPerBlock % _BITS_PER_DATA_UNIT;
                                        end = ((i + 1) * _bitsPerBlock - 1) / _BITS_PER_DATA_UNIT;

                                        if (start == end)
                                        {
                                            value = _data[start] >> offset;
                                        }
                                        else
                                        {
                                            value =
                                                _data[start] >> offset |
                                                _data[end] << _BITS_PER_DATA_UNIT - offset;
                                        }

                                        value &= mask;
                                        (id, int count) = _palette[value];
                                        System.Diagnostics.Debug.Assert(count > 0);
                                    }

                                    {
                                        start = i * bitsPerBlock / _BITS_PER_DATA_UNIT;
                                        offset = i * bitsPerBlock % _BITS_PER_DATA_UNIT;
                                        end = ((i + 1) * bitsPerBlock - 1) / _BITS_PER_DATA_UNIT;

                                        value = (ulong)id;

                                        System.Diagnostics.Debug.Assert(
                                            (value & ~((1UL << bitsPerBlock) - 1UL)) == 0);
                                        data[start] |= value << offset;

                                        if (start != end)
                                        {
                                            data[end] = value >> _BITS_PER_DATA_UNIT - offset;
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
                                    i = (y * BlocksPerHeight + z) * BlocksPerWidth + x;

                                    {
                                        start = i * _bitsPerBlock / _BITS_PER_DATA_UNIT;
                                        offset = i * _bitsPerBlock % _BITS_PER_DATA_UNIT;
                                        end = ((i + 1) * _bitsPerBlock - 1) / _BITS_PER_DATA_UNIT;

                                        if (start == end)
                                        {
                                            value = _data[start] >> offset;
                                        }
                                        else
                                        {
                                            value =
                                                _data[start] >> offset |
                                                _data[end] << _BITS_PER_DATA_UNIT - offset;
                                        }

                                        value &= mask;
                                    }

                                    {
                                        start = i * bitsPerBlock / _BITS_PER_DATA_UNIT;
                                        offset = i * bitsPerBlock % _BITS_PER_DATA_UNIT;
                                        end = ((i + 1) * bitsPerBlock - 1) / _BITS_PER_DATA_UNIT;

                                        System.Diagnostics.Debug.Assert(
                                            (value & ~((1UL << bitsPerBlock) - 1UL)) == 0);
                                        data[start] |= value << offset;

                                        if (start != end)
                                        {
                                            data[end] = value >> _BITS_PER_DATA_UNIT - offset;
                                        }
                                    }

                                }
                            }
                        }
                    }

                    _bitsPerBlock = bitsPerBlock;
                    _data = data;
                }

                private ulong MakeValue(int id)
                {
                    System.Diagnostics.Debug.Assert(!_disposed);

                    ulong value;

                    if (_bitsPerBlock == 13)
                    {
                        System.Diagnostics.Debug.Assert(_palette == null);

                        value = (ulong)id;
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
                            value = (ulong)indexPalette;
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
                                value = (ulong)id;
                            }
                            else
                            {
                                System.Diagnostics.Debug.Assert(
                                    bitsPerBlock >= 4 && bitsPerBlock <= 8);

                                value = (ulong)length;
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

                    ulong value = MakeValue(id);

                    int i = (y * BlocksPerHeight + z) * BlocksPerWidth + x;
                    int start = i * _bitsPerBlock / _BITS_PER_DATA_UNIT;
                    int offset = i * _bitsPerBlock % _BITS_PER_DATA_UNIT;
                    int end = ((i + 1) * _bitsPerBlock - 1) / _BITS_PER_DATA_UNIT;

                    System.Diagnostics.Debug.Assert(
                        (value & ~((1UL << _bitsPerBlock) - 1UL)) == 0L);
                    _data[start] |= value << offset;

                    if (start != end)
                    {
                        _data[end] = value >> _BITS_PER_DATA_UNIT - offset;
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

            private bool _disposed = false;

            private int _count = 0;
            private SectionData[] _sections;  // from bottom to top

            public static ChunkData Load(NBTTagList<NBTTagCompound> sectionList)
            {
                System.Diagnostics.Debug.Assert(sectionList != null);

                SectionData[] sections = new SectionData[SectionCount];

                bool[] prevChecks = new bool[SectionCount];

                foreach (NBTTagCompound section in sectionList.Data)
                {
                    int y = section.GetNBTTag<NBTTagByte>("Y").Value;

                    SectionData prev = sections[y];

                    if (prev != null)
                    {
                        prev.Dispose();
                    }

                    sections[y] = SectionData.Load(section);
                }

                return new ChunkData(sections);
            }

            public static (int, byte[]) Write(ChunkData chunkData)
            {
                using MinecraftProtocolDataStream buffer = new();

                int mask = 0;
                for (int i = 0; i < SectionCount; ++i)
                {
                    SectionData section = chunkData._sections[i];
                    if (section == null) continue;

                    mask |= 1 << i;  // TODO;
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
                using MinecraftProtocolDataStream buffer = new();

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

            private ChunkData(SectionData[] sections)
            {
                System.Diagnostics.Debug.Assert(sections != null);
                System.Diagnostics.Debug.Assert(sections.Length == SectionCount);

                _sections = sections;
            }

            public ChunkData()
            {
                _sections = new SectionData[SectionCount];
            }

            ~ChunkData()
            {
                System.Diagnostics.Debug.Assert(false);
            }

            public void SetId(int defaultId, int x, int y, int z, int id)
            {
                System.Diagnostics.Debug.Assert(!_disposed);

                System.Diagnostics.Debug.Assert(x >= 0 && x < BlocksPerWidth);
                System.Diagnostics.Debug.Assert(z >= 0 && z < BlocksPerWidth);

                System.Diagnostics.Debug.Assert(y >= 0);

                int ySection = y / SectionData.BlocksPerHeight;
                System.Diagnostics.Debug.Assert(ySection < SectionCount);

                int yPrime = y - ySection * SectionData.BlocksPerHeight;
                System.Diagnostics.Debug.Assert(yPrime >= 0 && yPrime < SectionData.BlocksPerHeight);

                SectionData section = _sections[ySection];
                if (section == null)
                {
                    section = new SectionData(defaultId);
                    _sections[ySection] = section;

                    _count++;
                }

                if (id != defaultId)
                {
                    section.SetId(x, yPrime, z, id);
                }
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

                int yPrime = y - ySection * SectionData.BlocksPerHeight;
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

        public static readonly Block DefaultBlock = Block.Air;

        private readonly Table<ChunkLocation, ChunkData> Chunks;  // Disposable

        public static BlockContext LoadWithRegionFiles(string folderPath)
        {
            string regionFilePattern = @"r\.(-?\d+)\.(-?\d+)\.mca$";

            Table<ChunkLocation, ChunkData> chunks = new();

            try
            {


                string[] regionFiles = System.IO.Directory.GetFiles(
                    folderPath,
                    "*.mca",
                    System.IO.SearchOption.TopDirectoryOnly);

                foreach (string filename in regionFiles)
                {
                    System.IO.FileInfo fileInfo = new(filename);

                    if (fileInfo.Exists == false)
                    {
                        MyConsole.Warn($"File not found: {filename}");
                        continue;
                    }

                    string name = fileInfo.Name;

                    System.Text.RegularExpressions.Match match =
                        System.Text.RegularExpressions.Regex.Match(name, regionFilePattern);

                    if (match.Success == false)
                    {
                        MyConsole.Warn($"Invalid filename format, skipping file: {fileInfo.FullName}");
                        continue;
                    }

                    MyConsole.Info($"Loading region file: {name}");

                    int regionX = int.Parse(match.Groups[1].Value);
                    int regionZ = int.Parse(match.Groups[2].Value);

                    //MyConsole.Debug($"Valid filename: {name}");
                    //MyConsole.Debug($"regionX = {regionX}, regionZ = {regionZ}");

                    int chunkX_min = regionX * MinecraftConstants.ChunksPerRegion;
                    int chunkZ_min = regionZ * MinecraftConstants.ChunksPerRegion;
                    int chunkX_max = chunkX_min + MinecraftConstants.ChunksPerRegion - 1;
                    int chunkZ_max = chunkZ_min + MinecraftConstants.ChunksPerRegion - 1;

                    for (int chunkX = chunkX_min; chunkX <= chunkX_max; ++chunkX)
                    {
                        for (int chunkZ = chunkZ_min; chunkZ <= chunkZ_max; ++chunkZ)
                        {
                            using NBTTagCompound tag = NBTTagRootCompoundLoader.Load(fileInfo, chunkX, chunkZ);

                            if (tag == null)
                            {
                                continue;
                            }

                            NBTTagCompound level = tag.GetNBTTag<NBTTagCompound>("Level");

                            if (level == null)
                            {
                                MyConsole.Warn($"Level tag not found: ({chunkX},{chunkZ})");
                                continue;
                            }

                            NBTTagList<NBTTagCompound> sectionList = level.GetNBTTag<NBTTagList<NBTTagCompound>>("Sections");

                            if (sectionList == null)
                            {
                                //MyConsole.Warn($"Sections tag not found: ({chunkX},{chunkZ})");
                                continue;
                            }

                            ChunkData chunkData = ChunkData.Load(sectionList);

                            if (chunkData != null)
                            {
                                chunks.Insert(new ChunkLocation(chunkX, chunkZ), chunkData);
                            }
                        }
                    }

                }

                return new BlockContext(chunks);
            }
            catch (System.IO.DirectoryNotFoundException)
            {
                return new BlockContext(chunks);
            }

        }

        private BlockContext(Table<ChunkLocation, ChunkData> chunks)
        {
            Chunks = chunks;
        }

        private BlockContext()
        {
            Chunks = new Table<ChunkLocation, ChunkData>();

            BlockLocation loc = new(0, 100, 0);

            SetBlock(loc, Block.Stone);
        }

        ~BlockContext() => System.Diagnostics.Debug.Assert(false);

        //public BlockContext() 
        //{
        //    // Dummy code.
        //    for (int z = -10; z <= 10; ++z)
        //    {
        //        for (int x = -10; x <= 10; ++x)
        //        {
        //            BlockLocation loc = new(x, 100, z);

        //            SetBlock(loc, Blocks.Stone);
        //        }
        //    }
        //}

        private ChunkLocation BlockToChunk(BlockLocation loc)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            int x = loc.X / ChunkLocation.BlocksPerWidth,
                z = loc.Z / ChunkLocation.BlocksPerWidth;

            double r1 = loc.X % (double)ChunkLocation.BlocksPerWidth,
                   r2 = loc.Z % (double)ChunkLocation.BlocksPerWidth;
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

        private void SetBlock(BlockLocation loc, Block block)
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

            int x = loc.X - locChunk.X * ChunkLocation.BlocksPerWidth,
                y = loc.Y,
                z = loc.Z - locChunk.Z * ChunkLocation.BlocksPerWidth;
            chunk.SetId(DefaultBlock.GetId(), x, y, z, block.GetId());
        }

        public Block GetBlock(BlockLocation loc)
        {
            if (_disposed == true)
            {
                throw new System.ObjectDisposedException(GetType().Name);
            }

            ChunkLocation locChunk = BlockToChunk(loc);
            if (!Chunks.Contains(locChunk))
            {
                return DefaultBlock;
            }


            ChunkData chunk = Chunks.Lookup(locChunk);
            int x = loc.X - locChunk.X * ChunkLocation.BlocksPerWidth,
                y = loc.Y,
                z = loc.Z - locChunk.Z * ChunkLocation.BlocksPerWidth;
            int id = chunk.GetId(DefaultBlock.GetId(), x, y, z);

            int defaultBlockId = DefaultBlock.GetId();
            if (defaultBlockId == id)
            {
                return DefaultBlock;
            }

            Block block = BlockExtensions.ToBlock(id, DefaultBlock);

            if (block == DefaultBlock)
            {
                MyConsole.Warn(
                    $"It is not exists block id in this engine. \n" +
                    $"\tLocation: {loc} \n" +
                    //$"\tblock: {block} (0x{block:X}, 0b{System.Convert.ToString(block, 2).PadLeft(MaxBitsPerBlock - MaxMetadataBits, '0')}), \n" +
                    //$"\tmetadata: {metadata} (0x{metadata:X}, 0b{System.Convert.ToString((long)metadata, 2).PadLeft(MaxMetadataBits, '0')}), \n" +
                    $"\tid: {id} 0b{System.Convert.ToString((long)id, 2).PadLeft(13, '0')})"
                    );

            }



            return block;
        }

        private Block GetBlock(BlockLocation loc, BlockDirection d, int s)
        {
            BlockLocation locPrime;
            switch (d)
            {
                default:
                    throw new System.NotImplementedException();
                case BlockDirection.Right:
                    locPrime = new BlockLocation(loc.X + s, loc.Y, loc.Z);
                    break;
                case BlockDirection.Left:
                    locPrime = new BlockLocation(loc.X - s, loc.Y, loc.Z);
                    break;
                case BlockDirection.Back:
                    locPrime = new BlockLocation(loc.X, loc.Y, loc.Z + s);
                    break;
                case BlockDirection.Front:
                    locPrime = new BlockLocation(loc.X, loc.Y, loc.Z - s);
                    break;
                case BlockDirection.UP:
                    locPrime = new BlockLocation(loc.X, loc.Y + s, loc.Z);
                    break;
                case BlockDirection.DOWN:
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

        private void GenerateBoundingBoxForSlabBlock(
            Queue<AxisAlignedBoundingBox> queue, BlockLocation loc, Block block)
        {
            System.Diagnostics.Debug.Assert(queue != null);

            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(block.GetShape() == BlockShape.Slab);

            Vector _min = loc.GetMinVector(),
                   _max = loc.GetMaxVector();

            Vector min = block.IsBottomSlab() == true ? _min : new(_min.X, _min.Y + BlockWidth / 2.0, _min.Z),
                max = block.IsBottomSlab() == true ? new(_max.X, _max.Y - BlockWidth / 2.0, _max.Z) : _max;
            AxisAlignedBoundingBox aabb = new(max, min);

            queue.Enqueue(aabb);
            return;
        }

        private (BlockDirection, bool, int) DetermineStairsBlockShape(
            BlockLocation loc, Block block)
        {
            System.Diagnostics.Debug.Assert(block.IsStairs() == true);

            BlockDirection d = block.GetStairsDirection();
            bool bottom = block.IsBottomStairs();

            Block block2 = GetBlock(loc, d, 1);
            if (block2.IsStairs() &&
                bottom == block2.IsBottomStairs())
            {
                if (block2.IsVerticalStairs() != block.IsVerticalStairs())
                {
                    BlockDirection d2 = block2.GetStairsDirection();
                    Block block3 = GetBlock(loc, d2.GetOpposite(), 1);
                    if (!block3.IsStairs() ||
                        block3.GetStairsDirection() != d ||
                        block3.IsBottomStairs() != bottom)
                    {
                        if (d2 == d.RotateCCW())
                        {
                            // outer left
                            return (d, bottom, 1);
                        }
                        else
                        {
                            System.Diagnostics.Debug.Assert(d2 == d.RotateCW());
                            // outer right
                            return (d, bottom, 2);
                        }
                    }
                }

            }

            Block block4 = GetBlock(loc, d.GetOpposite(), 1);
            if (block4.IsStairs() &&
                bottom == block4.IsBottomStairs())
            {
                if (block4.IsVerticalStairs() != block.IsVerticalStairs())
                {
                    BlockDirection d4 = block4.GetStairsDirection();
                    Block block5 = GetBlock(loc, d4, 1);
                    if (!block5.IsStairs() ||
                        block5.GetStairsDirection() != d ||
                        block5.IsBottomStairs() != bottom)
                    {
                        if (d4 == d.RotateCCW())
                        {
                            // inner left
                            return (d, bottom, 3);
                        }
                        else
                        {
                            System.Diagnostics.Debug.Assert(d4 == d.RotateCW());
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
            Queue<AxisAlignedBoundingBox> queue, BlockLocation loc, Block block)
        {
            System.Diagnostics.Debug.Assert(queue != null);

            System.Diagnostics.Debug.Assert(!_disposed);

            (BlockDirection d, bool bottom, int b) = DetermineStairsBlockShape(loc, block);

            throw new System.NotImplementedException();
        }

        private void GenerateBoundingBoxForCarpetBlock(
            Queue<AxisAlignedBoundingBox> queue, BlockLocation loc)
        {
            System.Diagnostics.Debug.Assert(queue != null);

            System.Diagnostics.Debug.Assert(!_disposed);

            Vector min = loc.GetMinVector(),
                   _max = loc.GetMaxVector();
            Vector max = new(_max.X, min.Y + 0.0625, _max.Z);
            AxisAlignedBoundingBox aabb = new(max, min);

            queue.Enqueue(aabb);
            return;
        }

        protected override void GenerateBoundingBoxForBlock(
            Queue<AxisAlignedBoundingBox> queue, BlockLocation loc)
        {
            System.Diagnostics.Debug.Assert(queue != null);

            System.Diagnostics.Debug.Assert(!_disposed);

            Block block = GetBlock(loc);

            switch (block.GetShape())
            {
                default:
                    throw new System.NotImplementedException();
                case BlockShape.None:
                    break;
                case BlockShape.Cube:
                    GenerateBoundingBoxForCubeBlock(queue, loc);
                    break;
                case BlockShape.Slab:
                    GenerateBoundingBoxForSlabBlock(queue, loc, block);
                    break;
                case BlockShape.Stairs:
                    //GenerateBoundingBoxForStairsBlock(queue, loc, block);
                    break;
                case BlockShape.Fence:
                    // TODO
                    break;
                case BlockShape.Bars:
                    // TODO
                    break;
                case BlockShape.Wall:
                    // TODO
                    break;
                case BlockShape.Carpet:
                    GenerateBoundingBoxForCarpetBlock(queue, loc);
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

        protected override void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if (_disposed == false)
            {
                // If disposing equals true, dispose all managed
                // and unmanaged resources.
                if (disposing == true)
                {
                    // Dispose managed resources.
                    (ChunkLocation, ChunkData)[] _chunks = Chunks.Flush();
                    for (int i = 0; i < _chunks.Length; ++i)
                    {
                        (var _, ChunkData data) = _chunks[i];
                        data.Dispose();
                    }
                    Chunks.Dispose();
                }

                // Call the appropriate methods to clean up
                // unmanaged resources here.
                // If disposing is false,
                // only the following code is executed.
                //CloseHandle(handle);
                //handle = IntPtr.Zero;

                // Note disposing has been done.
                _disposed = true;
            }

            base.Dispose(disposing);
        }

    }
}

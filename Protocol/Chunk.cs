using Common;
using Containers;
using System.ComponentModel.DataAnnotations.Schema;
using System.Threading.Tasks;

namespace Protocol
{
    public sealed class Chunk : System.IDisposable
    {

        public struct Vector : System.IEquatable<Vector>
        {
            public static Vector Convert(Entity.Vector pos)
            {
                return new(
                    (pos.X >= 0) ? ((int)pos.X / _WIDTH) : (((int)pos.X / (_WIDTH + 1)) - 1),
                    (pos.Z >= 0) ? ((int)pos.Z / _WIDTH) : (((int)pos.Z / (_WIDTH + 1)) - 1));
            }

            private int _x, _z;
            public int X => _x;
            public int Z => _z;

            public Vector(int x, int z)
            {
                _x = x; _z = z;
            }

            public override readonly string? ToString()
            {
                return $"( X: {_x}, Z: {_z} )";
            }

            public readonly bool Equals(Vector other)
            {
                return (_x == other.X) && (_z == other.Z);
            }

        }

        public class Grid : System.IEquatable<Grid>
        {
            public static Grid Generate(Vector c, int d)
            {
                System.Diagnostics.Debug.Assert(d >= 0);
                if (d == 0)
                    return new(c, c);

                int xMax = c.X + d, zMax = c.Z + d,
                    xMin = c.X - d, zMin = c.Z - d;

                /*int a = (2 * d) + 1;
                int length = a * a;
                Position[] positions = new Position[length];

                int i = 0;
                for (int z = zMin; z <= zMax; ++z)
                {
                    for (int x = xMin; x <= xMax; ++x)
                    {
                        positions[i++] = new(x, z);
                    }
                }
                System.Diagnostics.Debug.Assert(i == length);*/

                System.Diagnostics.Debug.Assert(xMax > xMin);
                System.Diagnostics.Debug.Assert(zMax > zMin);
                return new(new(xMax, zMax), new(xMin, zMin));
            }

            public static Grid? Generate(Grid g1, Grid g2)
            {
                int xMax = System.Math.Min(g1._max.X, g2._max.X),
                    zMax = System.Math.Min(g1._max.Z, g2._max.Z),
                    xMin = System.Math.Max(g1._min.X, g2._min.X),
                    zMin = System.Math.Max(g1._min.Z, g2._min.Z);

                /*int temp;*/
                if (xMax < xMin)
                {
                    /*temp = xMax;
                    xMax = xMin;
                    xMin = temp;*/
                    return null;
                }
                if (zMax < zMin)
                {
                    /*temp = zMax;
                    zMax = zMin;
                    zMin = temp;*/
                    return null;
                }

                return new(new(xMax, zMax), new(xMin, zMin));
            }

            public static Grid Generate(Entity.Vector c, BoundingBox boundingBox)
            {
                double h = boundingBox.Width / 2;
                Entity.Vector 
                    pEntityMax = new(c.X + h, 0, c.Z + h),
                    pEntityMin = new(c.X - h, 0, c.Z - h);
                Vector
                    pChunkMax = Vector.Convert(pEntityMax),
                    pChunkMin = Vector.Convert(pEntityMin);

                return new(pChunkMax, pChunkMin);
            }

            private readonly Vector _max, _min;

            public Grid(Vector max, Vector min)
            {
                System.Diagnostics.Debug.Assert(max.X >= min.X);
                System.Diagnostics.Debug.Assert(max.Z >= min.Z);

                _max = max; _min = min;
            }

            public bool Contains(Vector p)
            {
                return (p.X <= _max.X && p.X >= _min.X && p.Z <= _max.Z && p.Z >= _min.Z);
            }

            public System.Collections.Generic.IEnumerable<Vector> GetVectors()
            {
                if (_max.X == _min.X && _max.Z == _min.Z)
                {
                    yield return new(_max.X, _min.Z);
                    yield break;
                }

                for (int z = _min.Z; z <= _max.Z; ++z)
                {
                    for (int x = _min.X; x <= _max.X; ++x)
                    {
                        yield return new(x, z);
                    }
                }

            }

            public override string? ToString()
            {
                return $"( Max: ({_max.X}, {_max.Z}), Min: ({_min.X}, {_min.Z}) )";
            }

            public bool Equals(Grid? other)
            {
                if (other == null)
                {
                    return false;
                }

                System.Diagnostics.Debug.Assert(other != null);
                return (other._max.Equals(_max) && other._min.Equals(_min));
            }

        }

        private sealed class Section : System.IDisposable
        {
            private bool _disposed = false;

            public const int WIDTH = 16;
            public const int HEIGHT = 16;

            private int _bitCount;

            private Block[]? _palette;

            private const int _DATA_UNIT_BITS = sizeof(ulong) * 8; // TODO: Change to appropriate name.
            private ulong[] _data;

            private byte[] _blockLights, _skyLights;

            private static int GetDataLength(int bitCount)
            {
                int a = (WIDTH * WIDTH * HEIGHT) * bitCount;
                System.Diagnostics.Debug.Assert(_DATA_UNIT_BITS == 64);
                System.Diagnostics.Debug.Assert(a % _DATA_UNIT_BITS == 0);
                int length = a / _DATA_UNIT_BITS;
                return length;
            }

            public Section()
            {
                _bitCount = 4;

                _palette = [new Block(Block.Types.Air)];

                int dataLength = GetDataLength(_bitCount);
                _data = new ulong[dataLength];
                System.Array.Fill<ulong>(_data, 0);

                int x = (WIDTH * WIDTH * HEIGHT) / 2;
                _blockLights = new byte[x];
                System.Array.Fill<byte>(_blockLights, byte.MaxValue);
                _skyLights = new byte[x];
                System.Array.Fill<byte>(_skyLights, byte.MaxValue);


                {
                    // Dummy Code
                    PlaceBlock(0, 10, 0, new Block(Block.Types.Stone));
                }
            }

            ~Section() => System.Diagnostics.Debug.Assert(false);

            public bool ContainsBlock(int x, int y, int z)
            {
                System.Diagnostics.Debug.Assert(!_disposed);

                System.Diagnostics.Debug.Assert(x >= 0 && x <= WIDTH);
                System.Diagnostics.Debug.Assert(z >= 0 && z <= WIDTH);
                System.Diagnostics.Debug.Assert(y >= 0 && y <= HEIGHT);

                ulong mask = ((Conversions.ToUlong(1) << _bitCount) - 1);

                int i;
                ulong value;

                int start, offset, end;

                i = (((y * HEIGHT) + z) * WIDTH) + x;
                start = (i * _bitCount) / 64;
                offset = (i * _bitCount) % 64;
                end = ((i + 1) * _bitCount - 1) / 64;

                if (start == end)
                {
                    value = (_data[start] >> offset);
                }
                else
                {
                    value = (_data[start] >> offset | _data[end] << (_DATA_UNIT_BITS - offset));
                }

                value &= mask;

                if (_bitCount == 13)
                {
                    System.Diagnostics.Debug.Assert(_palette == null);
                    return value > 0;  // value == 0 is air.
                }
                else
                {
                    System.Diagnostics.Debug.Assert(_palette != null);
                    Block b = _palette[value];
                    return b.Type != Block.Types.Air;
                }
            }

            private void ExpandData(int bitCount)
            {
                System.Diagnostics.Debug.Assert(!_disposed);

                System.Diagnostics.Debug.Assert(_palette != null);
                System.Diagnostics.Debug.Assert(bitCount > _bitCount);

                int length = GetDataLength(bitCount);
                var data = new ulong[length];

                ulong mask = ((Conversions.ToUlong(1) << bitCount) - 1);

                int i;
                ulong value;

                int start, offset, end;

                if (bitCount == 13)
                {
                    Block b;

                    for (int y = 0; y < HEIGHT; ++y)
                    {
                        for (int z = 0; z < WIDTH; ++z)
                        {
                            for (int x = 0; x < WIDTH; ++x)
                            {
                                i = (((y * HEIGHT) + z) * WIDTH) + x;

                                {
                                    start = (i * _bitCount) / _DATA_UNIT_BITS;
                                    offset = (i * _bitCount) % _DATA_UNIT_BITS;
                                    end = (((i + 1) * _bitCount) - 1) / _DATA_UNIT_BITS;

                                    if (start == end)
                                    {
                                        value = (_data[start] >> offset);
                                    }
                                    else
                                    {
                                        value = (_data[start] >> offset | _data[end] << (_DATA_UNIT_BITS - offset));
                                    }

                                    value &= mask;
                                    b = _palette[value];
                                }

                                {
                                    start = (i * bitCount) / _DATA_UNIT_BITS;
                                    offset = (i * bitCount) % _DATA_UNIT_BITS;
                                    end = (((i + 1) * bitCount) - 1) / _DATA_UNIT_BITS;

                                    value = b.GlobalPaletteId;

                                    System.Diagnostics.Debug.Assert(
                                        (value & ~((Conversions.ToUlong(1) << bitCount) - 1)) == 0);
                                    data[start] |= (value << offset);

                                    if (start != end)
                                    {
                                        data[end] = (value >> (_DATA_UNIT_BITS - offset));
                                    }
                                }

                            }
                        }
                    }
                    
                }
                else
                {
                    System.Diagnostics.Debug.Assert(bitCount > 4 && bitCount <= 8);

                    for (int y = 0; y < HEIGHT; ++y)
                    {
                        for (int z = 0; z < WIDTH; ++z)
                        {
                            for (int x = 0; x < WIDTH; ++x)
                            {
                                i = (((y * HEIGHT) + z) * WIDTH) + x;

                                {
                                    start = (i * _bitCount) / _DATA_UNIT_BITS;
                                    offset = (i * _bitCount) % _DATA_UNIT_BITS;
                                    end = (((i + 1) * _bitCount) - 1) / _DATA_UNIT_BITS;

                                    if (start == end)
                                    {
                                        value = (_data[start] >> offset);
                                    }
                                    else
                                    {
                                        value = (_data[start] >> offset | _data[end] << (_DATA_UNIT_BITS - offset));
                                    }

                                    value &= mask;
                                }

                                {
                                    start = (i * bitCount) / _DATA_UNIT_BITS;
                                    offset = (i * bitCount) % _DATA_UNIT_BITS;
                                    end = (((i + 1) * bitCount) - 1) / _DATA_UNIT_BITS;

                                    System.Diagnostics.Debug.Assert(
                                        (value & ~((Conversions.ToUlong(1) << bitCount) - 1)) == 0);
                                    data[start] |= (value << offset);

                                    if (start != end)
                                    {
                                        data[end] = (value >> (_DATA_UNIT_BITS - offset));
                                    }
                                }

                            }
                        }
                    }
                }

                _bitCount = bitCount;
            }

            private ulong GetValueInPalette(Block block)
            {
                System.Diagnostics.Debug.Assert(!_disposed);

                ulong value;

                if (_bitCount == 13)
                {
                    System.Diagnostics.Debug.Assert(_palette == null);

                    value = block.GlobalPaletteId;
                }
                else
                {
                    System.Diagnostics.Debug.Assert(_palette != null);
                    System.Diagnostics.Debug.Assert(_bitCount >= 4 && _bitCount <= 8);
                    System.Diagnostics.Debug.Assert(_palette.Length > 0);

                    int indexInPalette = -1;

                    System.Diagnostics.Debug.Assert(_palette != null);
                    for (int i = 0; i < _palette.Length; ++i)
                    {
                        Block blockInPalette = _palette[i];
                        if (blockInPalette.Equals(block))
                        {
                            indexInPalette = i;
                        }
                    }

                    if (indexInPalette >= 0)
                    {
                        value = Conversions.ToUlong(indexInPalette);
                    }
                    else
                    {
                        System.Diagnostics.Debug.Assert(indexInPalette == -1);

                        int newLength = _palette.Length + 1;

                        int bitCount;
                        if (newLength <= 0b1111)
                        {
                            bitCount = 4;
                        }
                        else if (newLength <= 0b1_1111)
                        {
                            bitCount = 5;
                        }
                        else if (newLength <= 0b11_1111)
                        {
                            bitCount = 6;
                        }
                        else if (newLength <= 0b111_1111)
                        {
                            bitCount = 7;
                        }
                        else if (newLength <= 0b1111_1111)
                        {
                            bitCount = 8;
                        }
                        else
                        {
                            bitCount = 13;
                        }

                        if (bitCount > _bitCount)
                        {
                            ExpandData(bitCount);
                        }

                        if (bitCount == 13)
                        {
                            value = block.GlobalPaletteId;

                            _palette = null;
                        }
                        else
                        {
                            System.Diagnostics.Debug.Assert(bitCount >= 4 && bitCount <= 8);

                            var newPalette = new Block[newLength];

                            int lastIndex = _palette.Length;
                            System.Array.Copy(_palette, newPalette, lastIndex);
                            newPalette[lastIndex] = block;

                            value = Conversions.ToUlong(lastIndex);

                            _palette = newPalette;
                        }

                    }
                }

                return value;
            }

            private void PlaceBlock(int x, int y, int z, Block block)
            {
                System.Diagnostics.Debug.Assert(!_disposed);

                System.Diagnostics.Debug.Assert(x >= 0 && x <= WIDTH);
                System.Diagnostics.Debug.Assert(z >= 0 && z <= WIDTH);
                System.Diagnostics.Debug.Assert(y >= 0 && y <= HEIGHT);

                System.Diagnostics.Debug.Assert(block.Type != Block.Types.Air);
                System.Diagnostics.Debug.Assert(!ContainsBlock(x, y, z));

                ulong value = GetValueInPalette(block);

                int i = (((y * HEIGHT) + z) * WIDTH) + x;
                int start = (i * _bitCount) / 64;
                int offset = (i * _bitCount) % 64;
                int end = ((i + 1) * _bitCount - 1) / 64;

                System.Diagnostics.Debug.Assert(
                    (value & ~((Conversions.ToUlong(1) << _bitCount) - 1)) == 0);
                _data[start] |= (value << offset);

                if (start != end)
                {
                    _data[end] = (value >> (64 - offset));
                }
            }

            private Block BreakBlock(int x, int y, int z)
            {
                System.Diagnostics.Debug.Assert(!_disposed);

                throw new System.NotImplementedException();
            }

            internal static void Write(Buffer buffer, Section section)
            {
                int bitCount = section._bitCount;
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

        private const int _WIDTH = Section.WIDTH;

        private const int _TOTAL_SECTION_COUNT = 16;

        private int _count = 0;
        private Section?[] _sections = new Section?[_TOTAL_SECTION_COUNT];  // from bottom to top
        /*public int Count => _count;*/

        internal static (int, byte[]) Write(Chunk chunk)
        {
            using Buffer buffer = new();

            int mask = 0;
            for (int i = 0; i < _TOTAL_SECTION_COUNT; ++i)
            {
                Section? section = chunk._sections[i];
                if (section == null) continue;

                mask |= (1 << i);  // TODO;
                Section.Write(buffer, section);
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
        
        private readonly Table<int, Entity> _ENTITIES = new();

        private readonly Vector _p;
        public Vector Position => _p;

        public Chunk(Vector p)
        {
            _p = p;

            {
                // Dummy code.
                _sections[3] = new Section();
            }
        }

        public void Dispose()
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            // Assertion.
            System.Diagnostics.Debug.Assert(_count == 0);

            System.Diagnostics.Debug.Assert(_ENTITIES.Empty);

            // Release resources.
            _ENTITIES.Dispose();

            // Finish.
            System.GC.SuppressFinalize(this);
            _disposed = true;
        }

    }
}

using Common;
using Containers;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace Protocol
{
    public sealed class Chunk : System.IDisposable
    {

        public struct Vector : System.IEquatable<Vector>
        {
            public static Vector Convert(Entity.Vector pos)
            {
                int x = (int)pos.X / _WIDTH,
                    z = (int)pos.Z / _WIDTH;

                double r1 = pos.X % _WIDTH,
                       r2 = pos.Z % _WIDTH;
                if (Comparing.IsLessThan(r1, 0))
                {
                    --x;
                }

                if (Comparing.IsLessThan(r2, 0))
                {
                    --z;
                }

                return new(x, z);
            }

            public static Vector Convert(Block.Vector pos)
            {
                int x = pos.X / _WIDTH,
                    z = pos.Z / _WIDTH;

                double r1 = pos.X % _WIDTH,
                       r2 = pos.Z % _WIDTH;
                if (Comparing.IsLessThan(r1, 0))
                {
                    --x;
                }
                if (Comparing.IsLessThan(r2, 0))
                {
                    --z;
                }

                return new(x, z);
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
                /*int temp;*/

                int xMax = System.Math.Min(g1._MAX.X, g2._MAX.X),
                    xMin = System.Math.Max(g1._MIN.X, g2._MIN.X);
                
                if (xMax < xMin)
                {
                    /*temp = xMax;
                    xMax = xMin;
                    xMin = temp;*/
                    return null;
                }

                int zMin = System.Math.Max(g1._MIN.Z, g2._MIN.Z),
                    zMax = System.Math.Min(g1._MAX.Z, g2._MAX.Z);

                if (zMax < zMin)
                {
                    /*temp = zMax;
                    zMax = zMin;
                    zMin = temp;*/
                    return null;
                }

                return new(new(xMax, zMax), new(xMin, zMin));
            }

            public static Grid Generate(Entity.Vector p, Entity.BoundingBox bb)
            {
                System.Diagnostics.Debug.Assert(bb.Width > 0);
                System.Diagnostics.Debug.Assert(bb.Height > 0);

                double h = bb.Width / 2;
                System.Diagnostics.Debug.Assert(h > 0);

                double xEntityMax = p.X + h, zEntityMax = p.Z + h,
                       xEntityMin = p.X - h, zEntityMin = p.Z - h;
                Entity.Vector 
                    maxEntity = new(xEntityMax, 0, zEntityMax),
                    minEntity = new(xEntityMin, 0, zEntityMin);
                Vector max = Vector.Convert(maxEntity),
                       min = Vector.Convert(minEntity);

                double r1 = xEntityMin % _WIDTH,
                       r2 = zEntityMin % _WIDTH;
                int xMin = min.X, zMin = min.Z;
                if (Comparing.IsEqualTo(r1, 0))
                {
                    --xMin;
                }
                if (Comparing.IsEqualTo(r2, 0))
                {
                    --zMin;
                }

                return new(max, new(xMin, zMin));
            }

            private readonly Vector _MAX, _MIN;

            public Grid(Vector max, Vector min)
            {
                System.Diagnostics.Debug.Assert(max.X >= min.X);
                System.Diagnostics.Debug.Assert(max.Z >= min.Z);

                _MAX = max; _MIN = min;
            }

            public bool Contains(Vector p)
            {
                return (
                    p.X <= _MAX.X && p.X >= _MIN.X && 
                    p.Z <= _MAX.Z && p.Z >= _MIN.Z);
            }

            public System.Collections.Generic.IEnumerable<Vector> GetVectors()
            {
                if (_MAX.X == _MIN.X && _MAX.Z == _MIN.Z)
                {
                    yield return new(_MAX.X, _MIN.Z);
                }
                else
                {
                    for (int z = _MIN.Z; z <= _MAX.Z; ++z)
                    {
                        for (int x = _MIN.X; x <= _MAX.X; ++x)
                        {
                            yield return new(x, z);
                        }
                    }
                }

            }

            public override string? ToString()
            {
                return $"( Max: ({_MAX.X}, {_MAX.Z}), Min: ({_MIN.X}, {_MIN.Z}) )";
            }

            public bool Equals(Grid? other)
            {
                if (other == null)
                {
                    return false;
                }

                System.Diagnostics.Debug.Assert(other != null);
                return (other._MAX.Equals(_MAX) && other._MIN.Equals(_MIN));
            }

        }

        private sealed class Section : System.IDisposable
        {
            private bool _disposed = false;

            public const int WIDTH = 16;
            public const int HEIGHT = WIDTH;

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

                {
                    int length = GetDataLength(_bitCount);
                    _data = new ulong[length];
                    System.Array.Fill<ulong>(_data, 0);
                }

                {
                    int length = (WIDTH * WIDTH * HEIGHT) / 2;
                    _blockLights = new byte[length];
                    System.Array.Fill<byte>(_blockLights, byte.MaxValue);
                    _skyLights = new byte[length];
                    System.Array.Fill<byte>(_skyLights, byte.MaxValue);
                }

                {
                    // Dummy Code
                    for (int z = 0; z < _WIDTH; ++z)
                    {
                        for (int x = 0; x < _WIDTH; ++x)
                        {
                            PlaceBlock(new(x, 10, z), new Block(Block.Types.Stone));
                        }
                    }
                }
            }

            ~Section() => System.Diagnostics.Debug.Assert(false);

            public Block GetBlock(Block.Vector p)
            {
                System.Diagnostics.Debug.Assert(!_disposed);

                System.Diagnostics.Debug.Assert(p.X >= 0 && p.X <= WIDTH);
                System.Diagnostics.Debug.Assert(p.Z >= 0 && p.Z <= WIDTH);
                System.Diagnostics.Debug.Assert(p.Y >= 0 && p.Y <= HEIGHT);

                ulong mask = (((ulong)1 << _bitCount) - 1);

                int i;
                ulong value;

                int start, offset, end;

                i = (((p.Y * HEIGHT) + p.Z) * WIDTH) + p.X;
                start = (i * _bitCount) / _DATA_UNIT_BITS;
                offset = (i * _bitCount) % _DATA_UNIT_BITS;
                end = ((i + 1) * _bitCount - 1) / _DATA_UNIT_BITS;

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

                    return Block.GetBlockByGlobalPaletteId(value);
                }
                else
                {
                    System.Diagnostics.Debug.Assert(_palette != null);
                    Block b = _palette[value];
                    return b;
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

            private void PlaceBlock(Block.Vector p, Block block)
            {
                System.Diagnostics.Debug.Assert(!_disposed);

                System.Diagnostics.Debug.Assert(p.X >= 0 && p.X <= WIDTH);
                System.Diagnostics.Debug.Assert(p.Z >= 0 && p.Z <= WIDTH);
                System.Diagnostics.Debug.Assert(p.Y >= 0 && p.Y <= HEIGHT);

                System.Diagnostics.Debug.Assert(block.Type != Block.Types.Air);
                System.Diagnostics.Debug.Assert(GetBlock(p).Type == Block.Types.Air);

                ulong value = GetValueInPalette(block);

                int i = (((p.Y * HEIGHT) + p.Z) * WIDTH) + p.X;
                int start = (i * _bitCount) / _DATA_UNIT_BITS;
                int offset = (i * _bitCount) % _DATA_UNIT_BITS;
                int end = ((i + 1) * _bitCount - 1) / _DATA_UNIT_BITS;

                System.Diagnostics.Debug.Assert(
                    (value & ~((Conversions.ToUlong(1) << _bitCount) - 1)) == 0);
                _data[start] |= (value << offset);

                if (start != end)
                {
                    _data[end] = (value >> (_DATA_UNIT_BITS - offset));
                }
            }

            private Block BreakBlock(Block.Vector p)
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

        private const int _TOTAL_SECTION_COUNT = 16; 

        private const int _WIDTH = Section.WIDTH;
        private const int _HEIGHT = Section.HEIGHT * _TOTAL_SECTION_COUNT;

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

        public Block GetBlock(Block.Vector p)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            int y = p.Y / Section.HEIGHT;
            if (Comparing.IsLessThan(p.Y % Section.HEIGHT, 0))
            {
                return new Block(Block.Types.Air);
            }

            /*if (y < 0)
            {
                return false;
            }*/

            if (y > _HEIGHT)
            {
                return new Block(Block.Types.Air);
            }

            System.Diagnostics.Debug.Assert(y >= 0);
            Block.Vector pPrime = new(
                    p.X - (_p.X * Section.WIDTH),
                    p.Y - (y * Section.HEIGHT), 
                    p.Z - (_p.Z * Section.WIDTH));
            System.Diagnostics.Debug.Assert(pPrime.X >= 0 && pPrime.X <= Section.WIDTH);
            System.Diagnostics.Debug.Assert(pPrime.Y >= 0 && pPrime.Y <= Section.HEIGHT);
            System.Diagnostics.Debug.Assert(pPrime.Z >= 0 && pPrime.Z <= Section.WIDTH);

            Section? section = _sections[y];
            if (section == null)
            {
                return new Block(Block.Types.Air);
            }

            return section.GetBlock(pPrime);
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

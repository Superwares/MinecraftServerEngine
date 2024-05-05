using Containers;

namespace Protocol
{
    public class Chunk
    {
        public const int Width = 16;
        public const int Height = 16 * 16;

        public struct Vector : System.IEquatable<Vector>
        {
            public static Vector Convert(Entity.Vector pos)
            {
                return new(
                    (pos.X >= 0) ? ((int)pos.X / Width) : (((int)pos.X / (Width + 1)) - 1),
                    (pos.Z >= 0) ? ((int)pos.Z / Width) : (((int)pos.Z / (Width + 1)) - 1));
            }

            private int _x, _z;
            public int X => _x;
            public int Z => _z;

            public Vector(int x, int z)
            {
                _x = x; _z = z;
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

            public static Grid Generate(Grid g1, Grid g2)
            {
                int xMax = System.Math.Min(g1._max.X, g2._max.X),
                    zMax = System.Math.Min(g1._max.Z, g2._max.Z),
                    xMin = System.Math.Max(g1._min.X, g2._min.X),
                    zMin = System.Math.Max(g1._min.Z, g2._min.Z);

                int temp;
                if (xMax < xMin)
                {
                    temp = xMax;
                    xMax = xMin;
                    xMin = temp;
                }
                if (zMax < zMin)
                {
                    temp = zMax;
                    zMax = zMin;
                    zMin = temp;
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

            Grid(Vector max, Vector min)
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

            public System.Collections.Generic.IEnumerable<Vector> GetVectorsInSpiral()
            {
                int n = _max.X - _min.X;
                System.Diagnostics.Debug.Assert(n == (_max.Z - _min.Z));  // Chest it's square.
                System.Diagnostics.Debug.Assert(n % 2 == 0);  // Check it has center.

                int k = n / 2;

                int xCenter = _max.X - k, zCenter = _max.Z - k;
                yield return new(xCenter, zCenter);

                for (int i = 0; i < k; ++i)
                {
                    int j = i + 1;
                    int x = xCenter + j, z = zCenter + j;
                    int w = j * 2;

                    for (int l = 0; l < w; ++l)
                    {
                        yield return new(--x, z);
                    }

                    for (int l = 0; l < w; ++l)
                    {
                        yield return new(x, --z);
                    }

                    for (int l = 0; l < w; ++l)
                    {
                        yield return new(++x, z);
                    }

                    for (int l = 0; l < w; ++l)
                    {
                        yield return new(x, ++z);
                    }

                    System.Diagnostics.Debug.Assert(x == xCenter + j);
                    System.Diagnostics.Debug.Assert(z == zCenter + j);
                }

            }

            public bool Equals(Grid? other)
            {
                if (other == null)
                    return false;

                System.Diagnostics.Debug.Assert(other != null);
                return (other._max.Equals(_max) && other._min.Equals(_min));
            }

        }

        private class Section
        {
            public static readonly int Width = Chunk.Width;
            public static readonly int Height = Chunk.Height / Width;

            public static readonly int BlockTotalCount = Width * Width * Height;
            // (0, 0, 0) to (16, 16, 16)
            private Block?[] _blocks = new Block?[BlockTotalCount];

            internal static void Write(Buffer buffer, Section section)
            {
                int blockBitCount = 13;
                buffer.WriteByte((byte)blockBitCount);
                buffer.WriteInt(0, true);  // Write pallete as globally

                int blockBitTotalCount = (BlockTotalCount) * blockBitCount,
                    ulongBitCount = (sizeof(ulong) * 8);  // TODO: Make as constants
                int dataLength = blockBitTotalCount / ulongBitCount;
                System.Diagnostics.Debug.Assert(blockBitTotalCount % ulongBitCount == 0);
                ulong[] data = new ulong[dataLength];

                for (int y = 0; y < Height; ++y)
                {
                    for (int z = 0; z < Width; ++z)
                    {
                        for (int x = 0; x < Width; ++x)
                        {
                            int i = (((y * Height) + z) * Width) + x;

                            int start = (i * blockBitCount) / ulongBitCount,
                                offset = (i * blockBitCount) % ulongBitCount,
                                end = (((i + 1) * blockBitCount) - 1) / ulongBitCount;

                            Block? block = section._blocks[i];
                            if (block == null)
                                block = new Air();

                            if (y == 10)
                                block = new Stone();

                            ulong id = block.GetGlobalPaletteID();
                            System.Diagnostics.Debug.Assert((id >> blockBitCount) == 0);

                            data[start] |= (id << offset);

                            if (start != end)
                            {
                                data[end] = (id >> (ulongBitCount - offset));
                            }

                        }
                    }
                }

                System.Diagnostics.Debug.Assert(unchecked((long)ulong.MaxValue) == -1);
                buffer.WriteInt(dataLength, true);
                for (int i = 0; i < dataLength; ++i)
                {
                    buffer.WriteLong((long)data[i]);  // TODO
                }

                // TODO
                for (int y = 0; y < Height; ++y)
                {
                    for (int z = 0; z < Width; ++z)
                    {
                        for (int x = 0; x < Width; x += 2)
                        {
                            buffer.WriteByte(byte.MaxValue);

                        }
                    }
                }

                // TODO
                for (int y = 0; y < Height; ++y)
                {
                    for (int z = 0; z < Width; ++z)
                    {
                        for (int x = 0; x < Width; x += 2)
                        {
                            buffer.WriteByte(byte.MaxValue);

                        }
                    }
                }


            }

        }

        public static readonly int SectionTotalCount = Height / Section.Height;
        // bottom to top
        private Section?[] _sections = new Section?[SectionTotalCount];

        internal static (int, byte[]) Write(Chunk chunk)
        {
            Buffer buffer = new();

            int mask = 0;
            System.Diagnostics.Debug.Assert(SectionTotalCount == 16);
            for (int i = 0; i < SectionTotalCount; ++i)
            {
                Section? section = chunk._sections[i];
                if (section == null) continue;

                mask |= (1 << i);  // TODO;
                Section.Write(buffer, section);
            }

            // TODO
            for (int z = 0; z < Width; ++z)
            {
                for (int x = 0; x < Width; ++x)
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
            System.Diagnostics.Debug.Assert(SectionTotalCount == 16);

            // TODO: biomes
            for (int z = 0; z < Width; ++z)
            {
                for (int x = 0; x < Width; ++x)
                {
                    buffer.WriteByte(127);  // Void Biome
                }
            }

            return (mask, buffer.ReadData());
        }

        internal static (int, byte[]) Write2()
        {
            Buffer buffer = new();

            int mask = 0;
            System.Diagnostics.Debug.Assert(SectionTotalCount == 16);

            Section section = new();

            mask |= (1 << 3);  // TODO;
            Section.Write(buffer, section);

            // TODO: biomes
            for (int z = 0; z < Width; ++z)
            {
                for (int x = 0; x < Width; ++x)
                {
                    buffer.WriteByte(127);  // Void Biome
                }
            }

            return (mask, buffer.ReadData());
        }

        public readonly Vector p;

        public Chunk(Vector p)
        {
            this.p = p;
        }

    }
}

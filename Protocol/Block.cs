
using Common;

namespace Protocol
{
    public struct Block : System.IEquatable<Block>
    {
        public struct Vector : System.IEquatable<Vector>
        {
            public static Vector Convert(Entity.Vector pos)
            {
                int x = (int)pos.X,
                    y = (int)pos.Y,
                    z = (int)pos.Z;

                double r1 = pos.X % 1,
                       r2 = pos.Y % 1,
                       r3 = pos.Z % 1;
                if (Comparing.IsLessThan(r1, 0))
                {
                    --x;
                }

                if (Comparing.IsLessThan(r2, 0))
                {
                    --y;
                }

                if (Comparing.IsLessThan(r3, 0))
                {
                    --z;
                }

                return new(x, y, z);
            }

            public int X, Y, Z;

            public Vector(int x, int y, int z)
            {
                X = x; Y = y; Z = z;
            }

            public override readonly string? ToString()
            {
                return $"( X: {X}, Y: {Y}, Z: {Z} )";
            }

            public readonly bool Equals(Vector other)
            {
                return (X == other.X) && (Y == other.Y) && (Z == other.Z);
            }
        }

        public class Grid : System.IEquatable<Grid>
        {
            private readonly Vector _max, _min;

            public static Grid Generate(Entity.Vector p, BoundingBox boundingBox)
            {
                System.Diagnostics.Debug.Assert(boundingBox.Width > 0);
                System.Diagnostics.Debug.Assert(boundingBox.Height > 0);

                double h = boundingBox.Width / 2;
                System.Diagnostics.Debug.Assert(h > 0);

                Entity.Vector 
                    pEntityMax = new(p.X + h, p.Y + boundingBox.Height, p.Z + h),
                    pEntityMin = new(p.X - h, p.Y, p.Z - h);

                return new(Vector.Convert(pEntityMax), Vector.Convert(pEntityMin));
            }

            public Grid(Vector max, Vector min)
            {
                System.Diagnostics.Debug.Assert(max.X >= min.X);
                System.Diagnostics.Debug.Assert(max.Y >= min.Y);
                System.Diagnostics.Debug.Assert(max.Z >= min.Z);

                _max = max; _min = min;
            }

            public bool Contains(Vector p)
            {
                return (
                    p.X <= _max.X && p.X >= _min.X &&
                    p.Y <= _max.Y && p.Y >= _min.Y &&
                    p.Z <= _max.Z && p.Z >= _min.Z);
            }
            public System.Collections.Generic.IEnumerable<Vector> GetVectors()
            {
                if (_max.X == _min.X && _max.Y == _min.Y && _max.Z == _min.Z)
                {
                    yield return new(_max.X, _max.Y, _max.Z);
                }
                else
                {
                    for (int y = _min.Y; y <= _max.Y; ++y)
                    {
                        for (int z = _min.Z; z <= _max.Z; ++z)
                        {
                            for (int x = _min.X; x <= _max.X; ++x)
                            {
                                yield return new(x, y, z);
                            }
                        }
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

        public enum Types : uint
        {
            Air              = 0,
            Stone            = 1,
            Grass            = 2,
            Dirt             = 3,
        }

        private readonly Types _type;
        public Types Type => _type;

        private readonly uint _metadata;
        public uint Metadata => _metadata;

        public ulong GlobalPaletteId
        {
            get
            {
                byte metadata = (byte)_metadata;
                System.Diagnostics.Debug.Assert((metadata & 0b_11110000) == 0);  // metadata is 4 bits

                ushort id = (ushort)_type;
                System.Diagnostics.Debug.Assert((id & 0b_11111110_00000000) == 0);  // id is 9 bits
                return (ulong)(id << 4 | metadata);  // 13 bits
            }
        }

        public Block(Types type, uint metadata)
        {
            _type = type;
            _metadata = metadata;
        }

        public Block(Types type)
        {
            _type = type;
            _metadata = 0;
        }

        public readonly bool Equals(Block other)
        {
            return (_type == other._type) && (_metadata == other._metadata);
        }
    }

}

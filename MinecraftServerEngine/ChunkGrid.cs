
using Common;
using MinecraftPhysicsEngine;

namespace MinecraftServerEngine
{
    internal readonly struct ChunkGrid : System.IEquatable<ChunkGrid>
    {
        public static ChunkGrid Generate(ChunkLocation c, int d)
        {
            System.Diagnostics.Debug.Assert(d >= 0);
            if (d == 0)
            {
                return new(c, c);
            }

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

        public static ChunkGrid Generate(ChunkGrid g1, ChunkGrid g2)
        {
            int temp;

            int xMax = System.Math.Min(g1._MAX.X, g2._MAX.X),
                xMin = System.Math.Max(g1._MIN.X, g2._MIN.X);

            if (xMax < xMin)
            {
                temp = xMax;
                xMax = --xMin;
                xMin = ++temp;
            }

            int zMax = System.Math.Min(g1._MAX.Z, g2._MAX.Z),
                zMin = System.Math.Max(g1._MIN.Z, g2._MIN.Z);
                
            if (zMax < zMin)
            {
                temp = zMax;
                zMax = --zMin;
                zMin = ++temp;
            }

            return new(new(xMax, zMax), new(xMin, zMin));
        }

        /*public static ChunkGrid Generate(BoundingBox bb)
        {
            ChunkLocation
                max = ChunkLocation.Generate(bb.Max),
                min = ChunkLocation.Generate(bb.Min);
            double r1 = bb.Min.X % (double)ChunkLocation.WIDTH,
                   r2 = bb.Min.Z % (double)ChunkLocation.WIDTH;
            int xMin = min.X, zMin = min.Z;
            if (r1 == 0.0D)
            {
                --xMin;
            }
            if (r2 == 0.0D)
            {
                --zMin;
            }

            return new(max, new(xMin, zMin));
        }*/

        private readonly ChunkLocation _MAX, _MIN;

        public ChunkGrid(ChunkLocation max, ChunkLocation min)
        {
            System.Diagnostics.Debug.Assert(max.X >= min.X);
            System.Diagnostics.Debug.Assert(max.Z >= min.Z);

            _MAX = max; _MIN = min;
        }

        public readonly bool Contains(ChunkLocation p)
        {
            return (
                p.X <= _MAX.X && p.X >= _MIN.X &&
                p.Z <= _MAX.Z && p.Z >= _MIN.Z);
        }

        public readonly BoundingVolume GetMinBoundingVolume()
        {
            throw new System.NotImplementedException();
        }

        public readonly System.Collections.Generic.IEnumerable<ChunkLocation> GetLocations()
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

        public readonly override string ToString()
        {
            return $"( Max: ({_MAX.X}, {_MAX.Z}), Min: ({_MIN.X}, {_MIN.Z}) )";
        }

        public readonly bool Equals(ChunkGrid other)
        {
            return (_MAX.Equals(other._MAX) && _MIN.Equals(other._MIN));
        }

    }
}

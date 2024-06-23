
using Common;
using MinecraftServerEngine.PhysicsEngine;

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

        /*public static ChunkGrid Generate(ChunkGrid g1, ChunkGrid g2)
        {

            int xMax = System.Math.Min(g1._MAX.X, g2._MAX.X),
                xMin = System.Math.Max(g1._MIN.X, g2._MIN.X);

            if (xMax < xMin)
            {
                return null;
            }

            int zMax = System.Math.Min(g1._MAX.Z, g2._MAX.Z),
                zMin = System.Math.Max(g1._MIN.Z, g2._MIN.Z);
                
            if (zMax < zMin)
            {
                return null
            }

            return new(new(xMax, zMax), new(xMin, zMin));
        }*/

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

        private readonly ChunkLocation Max, Min;

        public ChunkGrid(ChunkLocation max, ChunkLocation min)
        {
            System.Diagnostics.Debug.Assert(max.X >= min.X);
            System.Diagnostics.Debug.Assert(max.Z >= min.Z);

            Max = max; Min = min;
        }

        public readonly bool Contains(ChunkLocation p)
        {
            return (
                p.X <= Max.X && p.X >= Min.X &&
                p.Z <= Max.Z && p.Z >= Min.Z);
        }

        public readonly AxisAlignedBoundingBox GetMinBoundingBox()
        {
            Vector max = Max.GetMaxVector(),
                min = Min.GetMinVector();

            return new AxisAlignedBoundingBox(max, min);
        }

        public readonly System.Collections.Generic.IEnumerable<ChunkLocation> GetLocations()
        {
            if (Max.X == Min.X && Max.Z == Min.Z)
            {
                yield return new(Max.X, Min.Z);
            }
            else
            {
                for (int z = Min.Z; z <= Max.Z; ++z)
                {
                    for (int x = Min.X; x <= Max.X; ++x)
                    {
                        yield return new(x, z);
                    }
                }
            }

        }

        public readonly override string ToString()
        {
            return $"( Max: ({Max.X}, {Max.Z}), Min: ({Min.X}, {Min.Z}) )";
        }

        public readonly bool Equals(ChunkGrid other)
        {
            return (Max.Equals(other.Max) && Min.Equals(other.Min));
        }

    }
}

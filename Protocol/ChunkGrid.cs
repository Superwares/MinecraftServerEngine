
using Common;

namespace Protocol
{
    internal sealed class ChunkGrid : System.IEquatable<ChunkGrid>
    {
        public static ChunkGrid Generate(ChunkLocation c, int d)
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

        public static ChunkGrid? Generate(ChunkGrid g1, ChunkGrid g2)
        {
            /*int temp;*/

            int xMax = System.Math.Min(g1.MAX.X, g2.MAX.X),
                xMin = System.Math.Max(g1.MIN.X, g2.MIN.X);

            if (xMax < xMin)
            {
                /*temp = xMax;
                xMax = xMin;
                xMin = temp;*/
                return null;
            }

            int zMin = System.Math.Max(g1.MIN.Z, g2.MIN.Z),
                zMax = System.Math.Min(g1.MAX.Z, g2.MAX.Z);

            if (zMax < zMin)
            {
                /*temp = zMax;
                zMax = zMin;
                zMin = temp;*/
                return null;
            }

            return new(new(xMax, zMax), new(xMin, zMin));
        }

        public static ChunkGrid Generate(BoundingBox bb)
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
            ChunkLocation max = ChunkLocation.Convert(maxEntity),
                   min = ChunkLocation.Convert(minEntity);

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

        public readonly ChunkLocation MAX, MIN;

        public ChunkGrid(ChunkLocation max, ChunkLocation min)
        {
            System.Diagnostics.Debug.Assert(max.X >= min.X);
            System.Diagnostics.Debug.Assert(max.Z >= min.Z);

            MAX = max; MIN = min;
        }

        public bool Contains(ChunkLocation p)
        {
            return (
                p.X <= MAX.X && p.X >= MIN.X &&
                p.Z <= MAX.Z && p.Z >= MIN.Z);
        }

        public System.Collections.Generic.IEnumerable<ChunkLocation> GetLocations()
        {
            if (MAX.X == MIN.X && MAX.Z == MIN.Z)
            {
                yield return new(MAX.X, MIN.Z);
            }
            else
            {
                for (int z = MIN.Z; z <= MAX.Z; ++z)
                {
                    for (int x = MIN.X; x <= MAX.X; ++x)
                    {
                        yield return new(x, z);
                    }
                }
            }

        }

        public override string? ToString()
        {
            return $"( Max: ({MAX.X}, {MAX.Z}), Min: ({MIN.X}, {MIN.Z}) )";
        }

        public bool Equals(ChunkGrid? other)
        {
            if (other == null)
            {
                return false;
            }

            System.Diagnostics.Debug.Assert(other != null);
            return (other.MAX.Equals(MAX) && other.MIN.Equals(MIN));
        }

    }
}

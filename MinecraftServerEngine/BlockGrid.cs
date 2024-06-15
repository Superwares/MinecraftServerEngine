
using Common;
using PhysicsEngine;

namespace MinecraftServerEngine
{
    internal readonly struct BlockGrid : System.IEquatable<BlockGrid>
    {
        public readonly BlockLocation Max, Min;

        public static BlockGrid Generate(Vector max, Vector min)
        {
            BlockLocation maxBlock = BlockLocation.Generate(max),
                          minBlock = BlockLocation.Generate(min);

            int xMinBlock = minBlock.X, yMinBlock = minBlock.Y, zMinBlock = minBlock.Z;

            double r1 = min.X % BlockLocation.WIDTH,
                   r2 = min.Y % BlockLocation.HEIGHT,
                   r3 = min.Z % BlockLocation.WIDTH;
            if (r1 == 0.0D)
            {
                --xMinBlock;
            }
            if (r2 == 0.0D)
            {
                --yMinBlock;
            }
            if (r3 == 0.0D)
            {
                --zMinBlock;
            }

            return new(maxBlock, new(xMinBlock, yMinBlock, zMinBlock));
        }

        /*public static BlockGrid Generate(Vector p, BoundingBox bb)
        {
            System.Diagnostics.Debug.Assert(Comparing.IsGreaterThan(bb.Width, 0));
            System.Diagnostics.Debug.Assert(Comparing.IsGreaterThan(bb.Height, 0));

            BoundingBox bb = BoundingBox.GetBlockBB();

            double h = bb.Width / 2.0D;
            System.Diagnostics.Debug.Assert(h > 0);

            double xEntityMax = p.X + h, yEntityMax = p.Y + bb.Height, zEntityMax = p.Z + h,
                   xEntityMin = p.X - h, yEntityMin = p.Y, zEntityMin = p.Z - h;
            Vector
                maxEntity = new(xEntityMax, yEntityMax, zEntityMax),
                minEntity = new(xEntityMin, yEntityMin, zEntityMin);
            BlockLocation max = BlockLocation.Convert(maxEntity),
                   min = BlockLocation.Convert(minEntity);

            double r1 = xEntityMin % bb.Width,
                   r2 = yEntityMin % bb.Height,
                   r3 = zEntityMin % bb.Width;
            int xMin = min.X, yMin = min.Y, zMin = min.Z;
            if (Comparing.IsEqualTo(r1, 0.0D))
            {
                --xMin;
            }
            if (Comparing.IsEqualTo(r2, 0.0D))
            {
                --yMin;
            }
            if (Comparing.IsEqualTo(r3, 0.0D))
            {
                --zMin;
            }

            return new(max, new(xMin, yMin, zMin));
        }*/

        public BlockGrid(BlockLocation max, BlockLocation min)
        {
            System.Diagnostics.Debug.Assert(max.X >= min.X);
            System.Diagnostics.Debug.Assert(max.Y >= min.Y);
            System.Diagnostics.Debug.Assert(max.Z >= min.Z);

            Max = max; Min = min;
        }

        public readonly bool Contains(BlockLocation p)
        {
            return (
                p.X <= Max.X && p.X >= Min.X &&
                p.Y <= Max.Y && p.Y >= Min.Y &&
                p.Z <= Max.Z && p.Z >= Min.Z);
        }

        public readonly int GetCount()
        {
            System.Diagnostics.Debug.Assert(Max.X >= Min.X);
            System.Diagnostics.Debug.Assert(Max.Y >= Min.Y);
            System.Diagnostics.Debug.Assert(Max.Z >= Min.Z);

            int l1 = (Max.X - Min.X) + 1,
                l2 = (Max.Y - Min.Y) + 1,
                l3 = (Max.Z - Min.Z) + 1;
            return l1 * l2 * l3;
        }

        public readonly System.Collections.Generic.IEnumerable<BlockLocation> GetLocations()
        {
            if (Max.X == Min.X && Max.Y == Min.Y && Max.Z == Min.Z)
            {
                yield return new(Max.X, Max.Y, Max.Z);
            }
            else
            {
                for (int y = Min.Y; y <= Max.Y; ++y)
                {
                    for (int z = Min.Z; z <= Max.Z; ++z)
                    {
                        for (int x = Min.X; x <= Max.X; ++x)
                        {
                            yield return new(x, y, z);
                        }
                    }
                }
            }

        }

        public readonly override string ToString()
        {
            return $"( Max: ({Max.X}, {Max.Z}), Min: ({Min.X}, {Min.Z}) )";
        }

        public readonly bool Equals(BlockGrid other)
        {
            return (other.Max.Equals(Max) && other.Min.Equals(Min));
        }

    }
}

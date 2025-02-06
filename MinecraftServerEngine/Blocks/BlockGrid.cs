using MinecraftServerEngine.Physics;
using MinecraftServerEngine.Physics.BoundingVolumes;

namespace MinecraftServerEngine.Blocks
{
    public readonly struct BlockGrid : System.IEquatable<BlockGrid>
    {
        private static BlockGrid Generate(Vector max, Vector min)
        {
            BlockLocation maxBlock = BlockLocation.Generate(max),
                          minBlock = BlockLocation.Generate(min);

            int xMinBlock = minBlock.X,
                yMinBlock = minBlock.Y,
                zMinBlock = minBlock.Z;

            double r1 = min.X % MinecraftUnits.BlockWidth,
                   r2 = min.Y % MinecraftUnits.BlockHeight,
                   r3 = min.Z % MinecraftUnits.BlockWidth;
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

            return new BlockGrid(
                maxBlock,
                new BlockLocation(xMinBlock, yMinBlock, zMinBlock));
        }

        internal static BlockGrid Generate(BoundingVolume volume)
        {
            if (volume is AxisAlignedBoundingBox aabb)
            {
                return BlockGrid.Generate(aabb.MaxVector, aabb.MinVector);
            }
            else
            {
                throw new System.NotImplementedException();
            }
        }

        public readonly BlockLocation Max, Min;

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

        public readonly System.Collections.Generic.IEnumerable<BlockLocation> GetBlockLocations()
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

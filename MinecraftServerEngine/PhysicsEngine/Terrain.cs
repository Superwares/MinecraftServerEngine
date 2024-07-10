
using Common;
using Containers;

namespace MinecraftServerEngine.PhysicsEngine
{
    public abstract class Terrain : System.IDisposable
    {
        public const double BlockWidth = 1.0D;
        public const double BlockHeight = 1.0D;

        protected readonly struct BlockLocation : System.IEquatable<BlockLocation>
        {
            public static BlockLocation Generate(Vector p)
            {
                int x = (int)p.X,
                    y = (int)p.Y,
                    z = (int)p.Z;

                double r1 = p.X % BlockWidth,
                       r2 = p.Y % BlockHeight,
                       r3 = p.Z % BlockWidth;
                if (r1 < 0.0D)
                {
                    --x;
                }

                if (r2 < 0.0D)
                {
                    --y;
                }

                if (r3 < 0.0D)
                {
                    --z;
                }

                return new(x, y, z);
            }

            public readonly int X, Y, Z;

            public BlockLocation(int x, int y, int z)
            {
                X = x; Y = y; Z = z;
            }

            public readonly Vector GetMinVector()
            {
                double x = (double)X,
                    y = (double)Y,
                    z = (double)Z;
                return new(x, y, z);
            }

            public readonly Vector GetMaxVector()
            {
                double x = (double)X + BlockWidth,
                    y = (double)Y + BlockHeight,
                    z = (double)Z + BlockWidth;
                return new(x, y, z);
            }

            public override readonly string ToString()
            {
                return $"( X: {X}, Y: {Y}, Z: {Z} )";
            }

            public readonly bool Equals(BlockLocation other)
            {
                return (X == other.X) && (Y == other.Y) && (Z == other.Z);
            }

        }

        private readonly struct Grid : System.IEquatable<Grid>
        {
            private static Grid Generate(Vector max, Vector min)
            {
                BlockLocation maxBlock = BlockLocation.Generate(max),
                              minBlock = BlockLocation.Generate(min);

                int xMinBlock = minBlock.X, 
                    yMinBlock = minBlock.Y, 
                    zMinBlock = minBlock.Z;

                double r1 = min.X % BlockWidth,
                       r2 = min.Y % BlockHeight,
                       r3 = min.Z % BlockWidth;
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

                return new Grid(
                    maxBlock, 
                    new BlockLocation(xMinBlock, yMinBlock, zMinBlock));
            }

            internal static Grid Generate(BoundingVolume volume)
            {
                if (volume is AxisAlignedBoundingBox aabb)
                {
                    return Grid.Generate(aabb.Max, aabb.Min);
                }
                else
                {
                    throw new System.NotImplementedException();
                }
            }

            public readonly BlockLocation Max, Min;

            public Grid(BlockLocation max, BlockLocation min)
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

            public readonly bool Equals(Grid other)
            {
                return (other.Max.Equals(Max) && other.Min.Equals(Min));
            }

        }

        private bool _disposed = false;

        public Terrain()
        {
            
        }

        protected abstract void GenerateBoundingBoxForBlock(
            Queue<AxisAlignedBoundingBox> queue, BlockLocation loc);

        private void ResolveCollisionsWithOnGround(
            Queue<AxisAlignedBoundingBox> queue,
            BoundingVolume volume, double maxStepHeight, Vector v)
        {
            System.Diagnostics.Debug.Assert(queue != null);
            System.Diagnostics.Debug.Assert(volume != null);
            System.Diagnostics.Debug.Assert(maxStepHeight >= 0.0D);

            System.Diagnostics.Debug.Assert(!_disposed);
            
            int axis;
            double t;

            int a;
            double b;

            if (maxStepHeight > 0.0D)
            {
                Vector vUp = new(0.0D, maxStepHeight, 0.0D);

                axis = -1;
                t = double.PositiveInfinity;

                System.Diagnostics.Debug.Assert(vUp.Y > 0.0D);
                foreach (AxisAlignedBoundingBox aabb in queue.GetValues())
                {
                    (a, b) = aabb.ResolveCollision(volume, vUp);

                    System.Diagnostics.Debug.Assert(t >= 0.0D);
                    if (a > -1 && b < t)
                    {
                        System.Diagnostics.Debug.Assert(b <= 1.0D);
                        System.Diagnostics.Debug.Assert(b >= 0.0D);
                        System.Diagnostics.Debug.Assert(a == 0);

                        axis = a;
                        t = b;
                    }
                }

                if (axis ==  -1)
                {
                    volume.Move(vUp);
                }
                else if (axis == 0)
                {
                    volume.Move(vUp * t);
                }
                else
                {
                    System.Diagnostics.Debug.Assert(false);
                }
            }

            {
                axis = -1;
                t = double.PositiveInfinity;

                System.Diagnostics.Debug.Assert(v.Y == 0.0D);
                foreach (AxisAlignedBoundingBox aabb in queue.GetValues())
                {
                    (a, b) = aabb.ResolveCollision(volume, v);

                    System.Diagnostics.Debug.Assert(t >= 0.0D);
                    if (a > -1 && b < t)
                    {
                        System.Diagnostics.Debug.Assert(b <= 1.0D);
                        System.Diagnostics.Debug.Assert(b >= 0.0D);
                        System.Diagnostics.Debug.Assert(a == 1 || a == 2);

                        axis = a;
                        t = b;
                    }
                }

                if (axis == -1)
                {
                    volume.Move(v);
                }
                else if (axis == 1 || axis == 2)
                {
                    volume.Move(v * t);
                }
                else
                {
                    System.Diagnostics.Debug.Assert(false);
                }
            }

            if (maxStepHeight > 0.0D)
            {
                Vector vDown = new(0.0D, -maxStepHeight, 0.0D);

                axis = -1;
                t = double.PositiveInfinity;

                System.Diagnostics.Debug.Assert(vDown.Y < 0.0D);
                foreach (AxisAlignedBoundingBox aabb in queue.GetValues())
                {
                    (a, b) = aabb.ResolveCollision(volume, vDown);

                    System.Diagnostics.Debug.Assert(t >= 0.0D);
                    if (a > -1 && b < t)
                    {
                        System.Diagnostics.Debug.Assert(b <= 1.0D);
                        System.Diagnostics.Debug.Assert(b >= 0.0D);
                        System.Diagnostics.Debug.Assert(a == 0);

                        axis = a;
                        t = b;
                    }
                }

                if (axis == -1)
                {
                    volume.Move(vDown);
                }
                else if (axis == 0)
                {
                    volume.Move(vDown * t);
                }
                else
                {
                    System.Diagnostics.Debug.Assert(false);
                }
                
            }

        }

        private (Vector, bool) ResolveCollisions(
            Queue<AxisAlignedBoundingBox> queue,
            BoundingVolume volume, double maxStepHeight, Vector v, bool onGround)
        {
            System.Diagnostics.Debug.Assert(queue != null);
            System.Diagnostics.Debug.Assert(volume != null);
            System.Diagnostics.Debug.Assert(maxStepHeight >= 0.0D);

            System.Diagnostics.Debug.Assert(!_disposed);

            if (onGround)
            {
                ResolveCollisionsWithOnGround(queue, volume, maxStepHeight, v);
                return (Vector.Zero, true);
            }

            int axis = -1;
            double t = double.PositiveInfinity;

            int a;
            double b;
            foreach (AxisAlignedBoundingBox aabb in queue.GetValues())
            {
                (a, b) = aabb.ResolveCollision(volume, v);

                System.Diagnostics.Debug.Assert(t >= 0.0D);
                if (a > -1 && b < t)
                {
                    System.Diagnostics.Debug.Assert(b <= 1.0D);
                    System.Diagnostics.Debug.Assert(b >= 0.0D);
                    System.Diagnostics.Debug.Assert(a < 3);

                    axis = a;
                    t = b;
                }
            }

            if (axis == -1)
            {
                volume.Move(v);
                return (v, onGround);
            }

            System.Diagnostics.Debug.Assert(axis < 3);
            System.Diagnostics.Debug.Assert(t <= 1.0D);
            System.Diagnostics.Debug.Assert(t >= 0.0D);
            Vector vPrime = v * (1 - t);

            v = v * t;
            volume.Move(v);

            if (axis == 0)  // Y
            {
                if (v.Y < 0.0D)
                {
                    onGround = true;
                }

                vPrime = new Vector(vPrime.X, 0.0D, vPrime.Z);
            }
            else if (axis == 1)  // X
            {
                vPrime = new Vector(0.0D, vPrime.Y, vPrime.Z);
            }
            else if (axis == 2)  // Z
            {
                vPrime = new Vector(vPrime.X, vPrime.Y, 0.0D);
            }
            else
            {
                System.Diagnostics.Debug.Assert(false);
            }

            return ResolveCollisions(queue, volume, maxStepHeight, vPrime, onGround);
        }

        internal (Vector, bool) ResolveCollisions(
            BoundingVolume volumeObj, double maxStepHeight, Vector v)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(volumeObj != null);

            AxisAlignedBoundingBox volumeTotal = volumeObj.GetMinBoundingBox();
            volumeTotal.Extend(v);

            using Queue<AxisAlignedBoundingBox> queue = new();

            Grid grid = Grid.Generate(volumeTotal);

            foreach (BlockLocation loc in grid.GetBlockLocations())
            {
                GenerateBoundingBoxForBlock(queue, loc);
            }

            return ResolveCollisions(queue, volumeObj, maxStepHeight, v, false);
        }

        public virtual void Dispose()
        {
            // Assertions.
            System.Diagnostics.Debug.Assert(!_disposed);

            // Release resources.
            

            // Finish.
            System.GC.SuppressFinalize(this);
            _disposed = true;
        }

    }
}

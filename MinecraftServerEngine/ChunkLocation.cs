

using Common;
using PhysicsEngine;

namespace MinecraftServerEngine
{
    internal readonly struct ChunkLocation : System.IEquatable<ChunkLocation>
    {
        public const int BLOCKS_PER_WIDTH = 16;
        public const double WIDTH = BlockLocation.WIDTH * BLOCKS_PER_WIDTH;

        public static ChunkLocation Generate(Vector p)
        {
            int x = (int)(p.X / WIDTH),
                z = (int)(p.Z / WIDTH);

            double r1 = p.X % WIDTH,
                   r2 = p.Z % WIDTH;
            if (r1 < 0.0D)
            {
                --x;
            }

            if (r2 < 0.0D)
            {
                --z;
            }

            return new(x, z);
        }

        public static ChunkLocation Generate(BlockLocation loc)
        {
            int x = loc.X / BLOCKS_PER_WIDTH,
                z = loc.Z / BLOCKS_PER_WIDTH;

            double r1 = (double)loc.X % (double)BLOCKS_PER_WIDTH,
                   r2 = (double)loc.Z % (double)BLOCKS_PER_WIDTH;
            if (r1 < 0.0D)
            {
                --x;
            }
            if (r2 < 0.0D)
            {
                --z;
            }

            return new(x, z);
        }

        public readonly int X, Z;

        public ChunkLocation(int x, int z)
        {
            X = x; Z = z;
        }

        public override readonly string ToString()
        {
            return $"( X: {X}, Z: {Z} )";
        }

        public readonly bool Equals(ChunkLocation other)
        {
            return (X == other.X) && (Z == other.Z);
        }

    }
}

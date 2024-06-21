

using Common;
using MinecraftPhysicsEngine;

namespace MinecraftServerEngine
{
    internal readonly struct ChunkLocation : System.IEquatable<ChunkLocation>
    {
        public const int BlocksPerWidth = 16;
        public const double Width = Terrain.BlockWidth * BlocksPerWidth;

        public static ChunkLocation Generate(Vector p)
        {
            int x = (int)(p.X / Width),
                z = (int)(p.Z / Width);

            double r1 = p.X % Width,
                   r2 = p.Z % Width;
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

        /*public static ChunkLocation Generate(BlockLocation loc)
        {
            int x = loc.X / BlocksPerWidth,
                z = loc.Z / BlocksPerWidth;

            double r1 = (double)loc.X % (double)BlocksPerWidth,
                   r2 = (double)loc.Z % (double)BlocksPerWidth;
            if (r1 < 0.0D)
            {
                --x;
            }
            if (r2 < 0.0D)
            {
                --z;
            }

            return new(x, z);
        }*/

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



using Common;
using MinecraftPhysicsEngine;

namespace MinecraftServerEngine
{
    internal readonly struct BlockLocation : System.IEquatable<BlockLocation>
    {
        public const double WIDTH = 1.0D;
        public const double HEIGHT = 1.0D;

        public static BlockLocation Generate(Vector p)
        {
            int x = (int)p.X,
                y = (int)p.Y,
                z = (int)p.Z;

            double r1 = p.X % WIDTH,
                   r2 = p.Y % HEIGHT,
                   r3 = p.Z % WIDTH;
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

        /*public static BlockLocation Genrate(ChunkLocation loc)
        {
            throw new System.NotImplementedException();
        }*/

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
            double x = (double)X + WIDTH,
                y = (double)Y + HEIGHT,
                z = (double)Z + WIDTH;
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
}



using Common;


using MinecraftServerEngine.Physics;

namespace MinecraftServerEngine
{
    internal readonly struct ChunkLocation : System.IEquatable<ChunkLocation>
    {
        public const int BlocksPerWidth = MinecraftConstants.BlocksPerChunk;

        // TODO: Integrate with physics world's cell width. They must be same.
        public const double Width = Terrain.BlockWidth * BlocksPerWidth;

        public const int BlocksPerHeight = MinecraftConstants.BlocksPerChunk * MinecraftConstants.BlocksPerChunk;
        public const double Height = Terrain.BlockHeight * BlocksPerHeight;

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

        public readonly Vector GetMinVector()
        {
            double x = (double)X * Width,
                y = 0.0D,
                z = (double)Z * Width;
            return new(x, y, z);
        }

        public readonly Vector GetMaxVector()
        {
            double x = ((double)X * Width) + Width,
                y = (double)Height,
                z = ((double)Z * Width) + Width;
            return new(x, y, z);
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

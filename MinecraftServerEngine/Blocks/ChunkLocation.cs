

using Common;

using MinecraftServerEngine.Physics;

namespace MinecraftServerEngine.Blocks
{
    internal readonly struct ChunkLocation : System.IEquatable<ChunkLocation>
    {
        //public const int BlocksPerWidth = MinecraftConstants.BlocksInChunkWidth;

        // TODO: Integrate with physics world's cell width. They must be same.
        //public const double Width = Terrain.BlockWidth * BlocksPerWidth;

        //public const int BlocksPerHeight = MinecraftConstants.BlocksInChunkWidth * MinecraftConstants.BlocksInChunkWidth;
        //public const double Height = Terrain.BlockHeight * BlocksPerHeight;

        public static ChunkLocation Generate(Vector p)
        {
            int x = (int)(p.X / MinecraftUnits.ChunkWidth),
                z = (int)(p.Z / MinecraftUnits.ChunkWidth);

            double r1 = p.X % MinecraftUnits.ChunkWidth,
                   r2 = p.Z % MinecraftUnits.ChunkWidth;
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
            double x = X * MinecraftUnits.ChunkWidth,
                y = 0.0D,
                z = Z * MinecraftUnits.ChunkWidth;
            return new(x, y, z);
        }

        public readonly Vector GetMaxVector()
        {
            double x = X * MinecraftUnits.ChunkWidth + MinecraftUnits.ChunkWidth,
                y = MinecraftUnits.ChunkHeight,
                z = Z * MinecraftUnits.ChunkWidth + MinecraftUnits.ChunkWidth;
            return new(x, y, z);
        }

        public override readonly string ToString()
        {
            return $"( X: {X}, Z: {Z} )";
        }

        public readonly bool Equals(ChunkLocation other)
        {
            return X == other.X && Z == other.Z;
        }

    }
}

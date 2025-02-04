

namespace MinecraftServerEngine.Blocks
{
    using PhysicsEngine;

    public readonly struct BlockLocation : System.IEquatable<BlockLocation>
    {
        public static BlockLocation Generate(Vector p)
        {
            int x = (int)p.X,
                y = (int)p.Y,
                z = (int)p.Z;

            double r1 = p.X % Terrain.BlockWidth,
                   r2 = p.Y % Terrain.BlockHeight,
                   r3 = p.Z % Terrain.BlockWidth;
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
            double x = X,
                y = Y,
                z = Z;
            return new(x, y, z);
        }

        public readonly Vector GetMaxVector()
        {
            double x = X + Terrain.BlockWidth,
                y = Y + Terrain.BlockHeight,
                z = Z + Terrain.BlockWidth;
            return new(x, y, z);
        }

        public override readonly string ToString()
        {
            return $"( X: {X}, Y: {Y}, Z: {Z} )";
        }

        public readonly bool Equals(BlockLocation other)
        {
            return X == other.X && Y == other.Y && Z == other.Z;
        }

    }

}

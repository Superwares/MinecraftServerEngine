using MinecraftServerEngine.Physics;

namespace MinecraftServerEngine.Blocks
{

    public readonly struct BlockLocation : System.IEquatable<BlockLocation>
    {
        public static BlockLocation Generate(Vector p)
        {
            int x = (int)p.X,
                y = (int)p.Y,
                z = (int)p.Z;

            double r1 = p.X % MinecraftUnits.BlockWidth,
                   r2 = p.Y % MinecraftUnits.BlockHeight,
                   r3 = p.Z % MinecraftUnits.BlockWidth;
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
            double x = X + MinecraftUnits.BlockWidth,
                y = Y + MinecraftUnits.BlockHeight,
                z = Z + MinecraftUnits.BlockWidth;
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

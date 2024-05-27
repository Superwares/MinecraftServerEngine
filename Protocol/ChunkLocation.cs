

using Common;

namespace Protocol
{
    internal readonly struct ChunkLocation : System.IEquatable<ChunkLocation>
    {
        public const int WIDTH = BlockLocation.Width * 16;

        public static ChunkLocation Generate(Vector p)
        {
            int x = Conversions.ToInt(p.X / WIDTH),
                z = Conversions.ToInt(p.Z / WIDTH);

            double r1 = p.X % WIDTH,
                   r2 = p.Z % WIDTH;
            if (Comparing.IsLessThan(r1, 0))
            {
                --x;
            }

            if (Comparing.IsLessThan(r2, 0))
            {
                --z;
            }

            return new(x, z);
        }

        public static ChunkLocation Generate(BlockLocation loc)
        {
            int x = loc.X / Conversions.ToInt(WIDTH),
                z = loc.Z / Conversions.ToInt(WIDTH);

            double r1 = loc.X % Conversions.ToInt(WIDTH),
                   r2 = loc.Z % Conversions.ToInt(WIDTH);
            if (Comparing.IsLessThan(r1, 0))
            {
                --x;
            }
            if (Comparing.IsLessThan(r2, 0))
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

        public override readonly string? ToString()
        {
            return $"( X: {X}, Z: {Z} )";
        }

        public readonly bool Equals(ChunkLocation other)
        {
            return (X == other.X) && (Z == other.Z);
        }

    }
}

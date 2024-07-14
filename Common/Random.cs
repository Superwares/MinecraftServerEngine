
namespace Common
{
    public static class Random
    {
        // TODO: Static destructor
        private readonly static System.Random SystemRandom = new();

        public static int NextInt()
        {
            return SystemRandom.Next();
        }

        public static uint NextUint()
        {
            return (uint)SystemRandom.Next();
        }

        public static long NextLong()
        {
            return SystemRandom.NextInt64();
        }

        public static ulong NextUlong()
        {
            return (ulong)SystemRandom.NextInt64();
        }

        public static double NextDouble()
        {
            double v = SystemRandom.NextDouble();
            System.Diagnostics.Debug.Assert(v >= 0.0D);
            System.Diagnostics.Debug.Assert(v < 1.0D);
            return v;
        }
    }
}

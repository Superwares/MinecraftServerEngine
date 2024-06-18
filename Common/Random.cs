
namespace Common
{
    public static class Random
    {
        // TODO: Static destructor
        private readonly static System.Random Random = new();

        public static int NextInt()
        {
            return Random.Next();
        }

        public static uint NextUint()
        {
            return (uint)Random.Next();
        }

        public static long NextLong()
        {
            return Random.NextInt64();
        }

        public static ulong NextUlong()
        {
            return (ulong)Random.NextInt64();
        }
    }
}

using Common;

namespace MinecraftPrimitives
{
    public static class MinecraftConstants
    {
        public readonly static Time TimePerTick = Time.FromMilliseconds(50);

        public const int BlocksPerChunk = 16;
        public const int ChunksPerRegion = 32;
        public const int BlocksPerRegion = BlocksPerChunk * ChunksPerRegion;

        public const double MetersPerBlock = 1;
        public const double MetersPerChunk = (double)MetersPerBlock * (double)BlocksPerChunk;
        public const double MetersPerRegion = (double)MetersPerBlock * (double)BlocksPerRegion;
    }
}

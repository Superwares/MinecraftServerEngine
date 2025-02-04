

namespace MinecraftServerEngine
{
    public static class MinecraftPhysics
    {
        public const int VelocityScaleFactorForClient = 8000;

        public const double MinVelocity = short.MinValue / VelocityScaleFactorForClient;
        public const double MaxVelocity = short.MaxValue / VelocityScaleFactorForClient;
    }
}

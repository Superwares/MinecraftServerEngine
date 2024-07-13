using MinecraftServerEngine.PhysicsEngine;

namespace MinecraftServerEngine
{
    internal static class ParticleExtensions
    {

        internal static BoundingVolume GenerateBoundingVolume(
            this Particles particle, Vector p)
        {
            switch (particle)
            {
                default:
                    throw new System.NotImplementedException();
                case Particles.Bubble:
                    return AxisAlignedBoundingBox.Generate(p, 0.2D);
                case Particles.Flame:
                    return AxisAlignedBoundingBox.Generate(p, 0.2D);
            }
        }
    }
}

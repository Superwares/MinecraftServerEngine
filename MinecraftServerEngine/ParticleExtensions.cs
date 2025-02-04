namespace MinecraftServerEngine
{
    using PhysicsEngine;

    internal static class ParticleExtensions
    {

        internal static BoundingVolume GenerateBoundingVolume(
            this Particle particle, Vector p)
        {
            switch (particle)
            {
                default:
                    throw new System.NotImplementedException();
                case Particle.Bubble:
                    return AxisAlignedBoundingBox.Generate(p, 0.2D);
                case Particle.Flame:
                    return AxisAlignedBoundingBox.Generate(p, 0.2D);
            }
        }
    }
}

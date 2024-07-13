using MinecraftServerEngine;
using MinecraftServerEngine.PhysicsEngine;

namespace TestMinecraftServerApplication
{
    internal sealed class Flame : SmoothParticle
    {
        public Flame(Vector p) : 
            base(p, 0.1D, byte.MaxValue, 0, 0)
        {

        }

        public override void StartRoutine(long serverTicks, PhysicsWorld world)
        {
            System.Diagnostics.Debug.Assert(world != null);
        }

    }
}

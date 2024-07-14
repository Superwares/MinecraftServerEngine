using Common;
using MinecraftServerEngine;
using MinecraftServerEngine.PhysicsEngine;

namespace TestMinecraftServerApplication
{
    internal sealed class Flame : SmoothParticle
    {
        private int ticks = 0;

        public Flame(Vector p) : 
            base(p, 0.1D, byte.MaxValue, 0, 0)
        {

        }

        public override bool IsDead()
        {
            return (ticks >= 100);
        }

        public override void StartRoutine(long serverTicks, PhysicsWorld world)
        {
            System.Diagnostics.Debug.Assert(world != null);

            ++ticks;

            if (ticks == 1)
            {
                ApplyForce(new Vector(0.5D, 0.5D, 0.5D));
            }
        }

    }
}

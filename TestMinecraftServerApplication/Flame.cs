using Common;
using MinecraftServerEngine;
using MinecraftServerEngine.PhysicsEngine;

namespace TestMinecraftServerApplication
{
    internal sealed class Flame : SmoothParticle
    {
        private int ticks = 0;

        public Flame(Vector p) : 
            base(p, 20.0D, byte.MaxValue, 0, 0)
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
                ApplyForce(new Vector(12.0D, 10.0D, 12.0D));
            }
        }

    }
}

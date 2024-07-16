using Common;
using MinecraftServerEngine;
using MinecraftServerEngine.PhysicsEngine;

namespace TestMinecraftServerApplication
{
    internal sealed class Flame : SmoothParticle
    {
        private bool _disposed = false;

        private int ticks = 0;

        public Flame(Vector p) : 
            base(p, 1.0D, 20.0D, byte.MaxValue, 0, 0)
        {

        }

        protected override bool IsDead()
        {
            return (ticks >= (20 * 5));
        }

        protected override void OnDeath(PhysicsWorld world)
        {
            System.Diagnostics.Debug.Assert(world != null);

            System.Diagnostics.Debug.Assert(!_disposed);
        }

        public override void StartRoutine(PhysicsWorld world)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(world != null);

            ++ticks;

            if (ticks == 1)
            {
                ApplyForce(new Vector(5.0D, 10.0D, 5.0D));
            }
        }

        public override void Dispose()
        {
            // Assertions.
            System.Diagnostics.Debug.Assert(!_disposed);

            // Release resources.

            // Finish.
            base.Dispose();
            _disposed = true;
        }

    }
}

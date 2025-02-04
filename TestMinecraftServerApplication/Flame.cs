using Common;
using Containers;
using MinecraftServerEngine.Entities;
using MinecraftServerEngine.PhysicsEngine;

namespace TestMinecraftServerApplication
{
    internal sealed class Flame : SmoothParticle
    {
        private bool _disposed = false;

        private int ticks = 0;

        private readonly Vector _D;
        private readonly AbstractPlayer _Owner;

        public Flame(Vector p, Vector d, AbstractPlayer owner) :
            base(p, 1.0D, 20.0D, byte.MaxValue, 0, 0)
        {
            _D = d;
            _Owner = owner;
        }

        ~Flame()
        {
            System.Diagnostics.Debug.Assert(false);

            Dispose(false);
        }

        protected override bool HandleDespawning()
        {
            return (ticks >= (20 * 1));
        }

        protected override void OnDespawn(PhysicsWorld world)
        {
            System.Diagnostics.Debug.Assert(world != null);

            System.Diagnostics.Debug.Assert(!_disposed);
        }

        public override void StartRoutine(PhysicsWorld world)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(world != null);

            if (++ticks == 1)
            {
                ApplyForce(_D * 5);
            }

            AxisAlignedBoundingBox aabb = BoundingVolume.GetMinBoundingBox();

            using Tree<PhysicsObject> objs = new();
            world.SearchObjects(objs, aabb, true);

            foreach (PhysicsObject obj in objs.GetKeys())
            {
                if (obj is LivingEntity livingEntity && ReferenceEquals(_Owner, obj) == false)
                {
                    livingEntity.Damage(0.1F, null);
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if (_disposed == false)
            {

                // If disposing equals true, dispose all managed
                // and unmanaged resources.
                if (disposing == true)
                {
                    // Dispose managed resources.

                }

                // Call the appropriate methods to clean up
                // unmanaged resources here.
                // If disposing is false,
                // only the following code is executed.
                //CloseHandle(handle);
                //handle = IntPtr.Zero;

                // Note disposing has been done.
                _disposed = true;
            }

            base.Dispose(disposing);
        }


    }
}

using MinecraftServerEngine;
using MinecraftServerEngine.PhysicsEngine;

namespace TestMinecraftServerApplication
{
    public sealed class Guest : AbstractPlayer
    {
        private bool _disposed = false;

        public Guest(UserId id, Vector p, Look look) 
            : base(id, p, look) { }

        ~Guest() => System.Diagnostics.Debug.Assert(false);

        public override void StartRoutine(long serverTicks, PhysicsWorld world)
        {
            System.Diagnostics.Debug.Assert(world != null);

            System.Diagnostics.Debug.Assert(!_disposed);

        }

        protected override void OnSneak(World world, bool f)
        {
            System.Diagnostics.Debug.Assert(world != null);

            System.Diagnostics.Debug.Assert(!_disposed);

        }

        protected override void OnSprint(World world, bool f)
        {
            System.Diagnostics.Debug.Assert(world != null);

            System.Diagnostics.Debug.Assert(!_disposed);

        }

        protected override void OnAttack(World world)
        {
            System.Diagnostics.Debug.Assert(world != null);

            System.Diagnostics.Debug.Assert(!_disposed);

        }

        protected override void OnAttack(World world, ItemStack stack)
        {
            System.Diagnostics.Debug.Assert(world != null);
            System.Diagnostics.Debug.Assert(stack != null);

            System.Diagnostics.Debug.Assert(!_disposed);

        }

        protected override void OnUseItem(World world, ItemStack stack)
        {
            System.Diagnostics.Debug.Assert(world != null);
            System.Diagnostics.Debug.Assert(stack != null);

            System.Diagnostics.Debug.Assert(!_disposed);

        }

        protected override void OnUseEntity(World world, Entity entity)
        {
            System.Diagnostics.Debug.Assert(world != null);
            System.Diagnostics.Debug.Assert(entity != null);

            System.Diagnostics.Debug.Assert(!_disposed);

            entity.ApplyForce(new Vector(0.1D, 0.1D, 0.1D));
        }

        protected override void OnDeath(PhysicsWorld world)
        {
            System.Diagnostics.Debug.Assert(world != null);

            System.Diagnostics.Debug.Assert(!_disposed);

        }

        public override void Dispose()
        {
            // Assertions.
            System.Diagnostics.Debug.Assert(!_disposed);

            // Release reousrces.

            // Finish.
            base.Dispose();
            _disposed = true;
        }
    }
}

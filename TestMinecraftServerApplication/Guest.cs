using Common;
using MinecraftServerEngine;
using MinecraftServerEngine.PhysicsEngine;

namespace TestMinecraftServerApplication
{
    public sealed class Guest : AbstractPlayer
    {
        private bool _disposed = false;

        public Guest(UserId id, Vector p, Look look) 
            : base(id, p, look, Gamemode.Adventure) { }

        ~Guest() => System.Diagnostics.Debug.Assert(false);

        public override void StartRoutine(PhysicsWorld world)
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

            Console.Printl("Attack!");

            Damage(5.0F);
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

            /*entity.Teleport(new Vector(1.0D, 103.0D, 3.0D), new Look(30.0F, 90.0F));*/

            /*entity.ApplyForce(new Vector(
                (Random.NextDouble() - 0.5D) / 10.0D, 
                (Random.NextDouble() - 0.5D) / 10.0D, 
                (Random.NextDouble() - 0.5D) / 10.0D));*/
        }

        protected override void OnDeath(PhysicsWorld world)
        {
            System.Diagnostics.Debug.Assert(world != null);

            System.Diagnostics.Debug.Assert(!_disposed);

            Console.Printl("Death!");

            Teleport(new Vector(0.0D, 110.0D, 0.0D), new Look(30.0F, 20.0F));
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

using Common;
using Containers;
using MinecraftServerEngine;
using MinecraftServerEngine.PhysicsEngine;

namespace TestMinecraftServerApplication
{
    public sealed class Guest : AbstractPlayer
    {
        private bool _disposed = false;

        public Guest(UserId id, Vector p, Angles look)
            : base(id, p, look, Gamemode.Adventure) { }

        ~Guest()
        {
            System.Diagnostics.Debug.Assert(false);

            Dispose(false);
        }

        public override void StartRoutine(PhysicsWorld world)
        {
            System.Diagnostics.Debug.Assert(world != null);

            System.Diagnostics.Debug.Assert(!_disposed);

        }

        protected override void OnSneak(World world, bool f)
        {
            System.Diagnostics.Debug.Assert(world != null);

            System.Diagnostics.Debug.Assert(!_disposed);

            if (f == true)
            {
                ApplyBlockAppearance(Block.Dirt);
            }
            else
            {
                ResetBlockAppearance();

            }
        }

        protected override void OnSprint(World world, bool f)
        {
            System.Diagnostics.Debug.Assert(world != null);

            System.Diagnostics.Debug.Assert(!_disposed);

            MyConsole.Printl("Sprint!");
            //Switch(Gamemode.Adventure);
        }

        protected override void OnAttack(World world)
        {
            System.Diagnostics.Debug.Assert(world != null);

            System.Diagnostics.Debug.Assert(!_disposed);

            MyConsole.Printl("Attack!");

            //Damage(5.0F);

            //double eyeHeight = GetEyeHeight();

            Vector eyeOrigin = GetEyeOrigin();
            Vector scaledDirection = Look.GetUnitVector() * 3;

            MyConsole.Debug($"Eye Origin: {eyeOrigin}, Look's unit vector: {scaledDirection}");

            using Tree<PhysicsObject> objs = new();

            world.SearchObjects(objs, eyeOrigin, scaledDirection);

            foreach (PhysicsObject obj in objs.GetKeys())
            {
                MyConsole.Debug($"obj: {obj}");

                if (ReferenceEquals(obj, this) == true)
                {
                    continue;
                }

                if (obj is AbstractPlayer player)
                {
                    player.Damage(5.0F);
                }
            }
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

            MyConsole.Printl("Death!");

            /*Teleport(new Vector(0.0D, 110.0D, 0.0D), new Look(30.0F, 20.0F));*/

            if (Gamemode == Gamemode.Adventure)
            {
                Switch(Gamemode.Spectator);
            }
            else
            {
                System.Diagnostics.Debug.Assert(Gamemode == Gamemode.Spectator);
                Switch(Gamemode.Adventure);
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

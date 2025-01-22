using Common;
using Containers;

using MinecraftPrimitives;

using MinecraftServerEngine;
using MinecraftServerEngine.PhysicsEngine;

namespace TestMinecraftServerApplication
{
    public sealed class Guest : AbstractPlayer
    {
        private bool _disposed = false;

        private static ChestInventory chestInventory = new();
        private static ShopInventory shopInventory = new();


        public Guest(
            UserId userId, string username,
            Vector p, Angles look)
            : base(userId, username, p, look, Gamemode.Adventure)
        {
            System.Diagnostics.Debug.Assert(userId != UserId.Null);
            System.Diagnostics.Debug.Assert(username != null && string.IsNullOrEmpty(username) == false);

            //ApplyBlockAppearance(Block.Dirt);
        }

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
                //ApplyBlockAppearance(Block.Dirt);
                //OpenInventory(chestInventory);
                //OpenInventory(shopInventory);s

                //SetExperience(0.6F, 123456789);

                AddEffect(1, 1, 1800, 2);
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
            Vector d = Look.GetUnitVector();
            Vector scaled_d = d * 3;

            //MyConsole.Debug($"Eye origin: {eyeOrigin}, Scaled direction vector: {scaled_d}");

            PhysicsObject obj = world.SearchClosestObject(eyeOrigin, scaled_d, this);

            if (obj != null && obj is LivingEntity entity)
            {
                entity.Damage(1.0F);
                entity.ApplyForce(d);
            }

            world.PlaySound("entity.player.attack.strong", 7, Position, 1.0F, 2.0F);
        }

        protected override void OnAttack(World world, ItemStack stack)
        {
            System.Diagnostics.Debug.Assert(world != null);
            System.Diagnostics.Debug.Assert(stack != null);

            System.Diagnostics.Debug.Assert(!_disposed);

            MyConsole.Debug($"stack: {stack}");

            Vector d = Look.GetUnitVector();
            Vector eyeOrigin = GetEyeOrigin();

            world.SpawnObject(new Flame(eyeOrigin, d, this));
        }

        protected override void OnUseItem(World world, ItemStack stack)
        {
            System.Diagnostics.Debug.Assert(world != null);
            System.Diagnostics.Debug.Assert(stack != null);

            System.Diagnostics.Debug.Assert(!_disposed);

            MyConsole.Debug("Use item!");

            GiveItem(new ItemStack(ItemType.DiamondSword, "Good Stick!"));
        }

        protected override void OnUseEntity(World world, Entity entity)
        {
            System.Diagnostics.Debug.Assert(world != null);
            System.Diagnostics.Debug.Assert(entity != null);

            System.Diagnostics.Debug.Assert(!_disposed);

            MyConsole.Debug("Use entity!");

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
                SwitchGamemode(Gamemode.Spectator);
            }
            else
            {
                System.Diagnostics.Debug.Assert(Gamemode == Gamemode.Spectator);
                SwitchGamemode(Gamemode.Adventure);
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

using Common;
using Containers;

using MinecraftPrimitives;
using MinecraftServerEngine;
using MinecraftServerEngine.PhysicsEngine;
using TestMinecraftServerApplication.Items;

namespace TestMinecraftServerApplication
{
    public sealed class SuperPlayer : AbstractPlayer
    {

        public const double DefaultAttackDamage = 1.0;


        private bool _disposed = false;

        //private static ChestInventory chestInventory = new();
        private static readonly ShopInventory ShopInventory = new();



        public SuperPlayer(
            UserId userId, string username,
            Vector p, Angles look)
            : base(userId, username, p, look, Gamemode.Adventure)
        {
            System.Diagnostics.Debug.Assert(userId != UserId.Null);
            System.Diagnostics.Debug.Assert(username != null && string.IsNullOrEmpty(username) == false);

            //ApplyBlockAppearance(Block.Dirt);
        }

        ~SuperPlayer()
        {
            System.Diagnostics.Debug.Assert(false);

            Dispose(false);
        }

        private double GenerateRandomValueBetween(double min, double max)
        {
            System.Random random = new System.Random();
            return min + (random.NextDouble() * (max - min));
        }

        public override void StartRoutine(PhysicsWorld world)
        {
            System.Diagnostics.Debug.Assert(world != null);

            System.Diagnostics.Debug.Assert(_disposed == false);



        }

        protected override void OnSneak(World world, bool f)
        {
            System.Diagnostics.Debug.Assert(world != null);

            System.Diagnostics.Debug.Assert(_disposed == false);

            if (f == true)
            {
                //ApplyBlockAppearance(Block.Dirt);
                //OpenInventory(chestInventory);
                //OpenInventory(ShopInventory);
                OpenInventory(SuperWorld.GameContextInventory);

                //SetExperience(0.6F, 123456789);

                //EmitParticles(Particle.Cloud, 1.0F, 100);
                //EmitParticles(Particle.Largeexplode, 1.0F, 1);

                //AddEffect(1, 1, 1800, 2);

                world.DisplayTitle(
                    Time.FromSeconds(0), Time.FromSeconds(1), Time.FromSeconds(0),
                    new TextComponent("good", TextColor.Blue));
            }
            else
            {
                ResetBlockAppearance();

            }
        }

        protected override void OnSprint(World world, bool f)
        {
            System.Diagnostics.Debug.Assert(world != null);

            System.Diagnostics.Debug.Assert(_disposed == false);

            //MyConsole.Printl("Sprint!");
            //Switch(Gamemode.Adventure);
        }

        protected override void OnItemBreak(World world, ItemStack stack)
        {
            world.PlaySound("entity.item.break", 7, Position, 1.0F, 2.0F);
        }

        private void HandleDefaultAttack(SuperWorld world, double attackCharge)
        {
            System.Diagnostics.Debug.Assert(world != null);
            System.Diagnostics.Debug.Assert(attackCharge >= 0.0);
            System.Diagnostics.Debug.Assert(attackCharge <= 1.0);

            System.Diagnostics.Debug.Assert(_disposed == false);

            if (world.CanCombat == false)
            {
                return;
            }

            double damage = DefaultAttackDamage;
            damage *= (attackCharge * attackCharge);
            damage *= GenerateRandomValueBetween(0.98, 1.01);

            System.Diagnostics.Debug.Assert(damage >= 0.0F);

            double directionScale = 3.0;
            double knockbackScale = 0.3;

            System.Diagnostics.Debug.Assert(directionScale > 0.0F);
            System.Diagnostics.Debug.Assert(knockbackScale > 0.0F);

            if (damage == 0.0F)
            {
                return;
            }

            Vector o = GetEyeOrigin();
            Vector d = Look.GetUnitVector();
            Vector d_prime = d * directionScale;

            //MyConsole.Debug($"Eye origin: {eyeOrigin}, Scaled direction vector: {scaled_d}");

            PhysicsObject obj = world.SearchClosestObject(o, d_prime, this);

            if (obj != null && obj is LivingEntity livingEntity)
            {
                (bool damaged, double health) = livingEntity.Damage(damage);

                System.Diagnostics.Debug.Assert(health >= 0.0);
                if (damaged == true && health == 0.0)
                {

                }

                livingEntity.ApplyForce(d * knockbackScale);

                Vector v = new(
                    livingEntity.Position.X,
                    livingEntity.Position.Y + livingEntity.GetEyeHeight(),
                    livingEntity.Position.Z);

                world.PlaySound("entity.player.attack.strong", 7, v, 1.0F, 2.0F);
            }
        }

        private void HandleBalloonBasherAttack(SuperWorld world, double attackCharge)
        {
            System.Diagnostics.Debug.Assert(world != null);
            System.Diagnostics.Debug.Assert(attackCharge >= 0.0);
            System.Diagnostics.Debug.Assert(attackCharge <= 1.0);

            System.Diagnostics.Debug.Assert(_disposed == false);

            if (world.CanCombat == false)
            {
                return;
            }

            double damage = BalloonBasher.Damage;
            damage *= (attackCharge * attackCharge);
            damage *= GenerateRandomValueBetween(0.98, 1.01);

            System.Diagnostics.Debug.Assert(damage >= 0.0F);

            double directionScale = 3.0;
            double knockbackScale = GenerateRandomValueBetween(0.1, 0.3123);

            System.Diagnostics.Debug.Assert(directionScale > 0.0F);
            System.Diagnostics.Debug.Assert(knockbackScale > 0.0F);

            //MyConsole.Debug($"Damage: {damage:F2}");
            if (damage == 0.0F)
            {
                return;
            }


            Vector o = GetEyeOrigin();
            Vector d = Look.GetUnitVector();
            Vector d_prime = d * directionScale;

            Vector k = new(0.0, 1.0, 0.0);

            //MyConsole.Debug($"Eye origin: {eyeOrigin}, Scaled direction vector: {scaled_d}");

            PhysicsObject obj = world.SearchClosestObject(o, d_prime, this);

            if (obj != null && obj is LivingEntity livingEntity)
            {
                (bool damaged, double health) = livingEntity.Damage(damage);

                System.Diagnostics.Debug.Assert(health >= 0.0);
                if (damaged == true && health == 0.0)
                {

                }

                livingEntity.ApplyForce(k * knockbackScale);

                world.PlaySound("entity.generic.explode", 7, livingEntity.Position, 0.2, 0.5);

                livingEntity.EmitParticles(Particle.LargeExplode, 1.0, 1);
            }
        }

        protected override void OnAttack(World _world, double attackCharge)
        {
            System.Diagnostics.Debug.Assert(_world != null);
            System.Diagnostics.Debug.Assert(attackCharge >= 0.0);
            System.Diagnostics.Debug.Assert(attackCharge <= 1.0);

            System.Diagnostics.Debug.Assert(_disposed == false);


            if (_world is SuperWorld world)
            {
                HandleDefaultAttack(world, attackCharge);
            }
        }

        protected override void OnAttack(World _world, ItemStack stack, double attackCharge)
        {
            System.Diagnostics.Debug.Assert(_world != null);
            System.Diagnostics.Debug.Assert(stack != null);
            System.Diagnostics.Debug.Assert(attackCharge >= 0.0);
            System.Diagnostics.Debug.Assert(attackCharge <= 1.0);

            System.Diagnostics.Debug.Assert(_disposed == false);

            if (_world is SuperWorld world)
            {
                switch (stack.Type)
                {
                    default:
                        HandleDefaultAttack(world, attackCharge);
                        break;
                    case BalloonBasher.Type:
                        HandleBalloonBasherAttack(world, attackCharge);
                        break;
                }

                stack.Damage(1);
            }

        }

        protected override void OnUseItem(World world, ItemStack stack)
        {
            System.Diagnostics.Debug.Assert(world != null);
            System.Diagnostics.Debug.Assert(stack != null);

            System.Diagnostics.Debug.Assert(_disposed == false);

            MyConsole.Debug("Use item!");

            GiveItem(new ItemStack(ItemType.DiamondSword, "Good Stick!", ""));
        }

        protected override void OnUseEntity(World world, Entity entity)
        {
            System.Diagnostics.Debug.Assert(world != null);
            System.Diagnostics.Debug.Assert(entity != null);

            System.Diagnostics.Debug.Assert(_disposed == false);

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

            System.Diagnostics.Debug.Assert(_disposed == false);

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

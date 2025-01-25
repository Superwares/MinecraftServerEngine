﻿using Common;
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

            System.Diagnostics.Debug.Assert(!_disposed);



        }

        protected override void OnSneak(World world, bool f)
        {
            System.Diagnostics.Debug.Assert(world != null);

            System.Diagnostics.Debug.Assert(!_disposed);

            if (f == true)
            {
                ApplyBlockAppearance(Block.Dirt);
                //OpenInventory(chestInventory);
                OpenInventory(ShopInventory);

                //SetExperience(0.6F, 123456789);

                //EmitParticles(Particle.Cloud, 1.0F, 100);
                //EmitParticles(Particle.Largeexplode, 1.0F, 1);

                //AddEffect(1, 1, 1800, 2);
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

        protected override void OnItemBreak(World world, ItemStack stack)
        {
            world.PlaySound("entity.item.break", 7, Position, 1.0F, 2.0F);
        }

        private void HandleAttack(
            World world,
            double damage,
            double directionScale, double knockbackScale)
        {
            System.Diagnostics.Debug.Assert(world != null);

            System.Diagnostics.Debug.Assert(_disposed == false);

            System.Diagnostics.Debug.Assert(damage >= 0.0F);

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
                livingEntity.Damage(damage);
                livingEntity.ApplyForce(d * knockbackScale);

                Vector v = new(
                    livingEntity.Position.X,
                    livingEntity.Position.Y + livingEntity.GetEyeHeight(),
                    livingEntity.Position.Z);

                world.PlaySound("entity.player.attack.strong", 7, v, 1.0F, 2.0F);
            }
        }

        protected override void OnAttack(World world, double attackCharge)
        {
            System.Diagnostics.Debug.Assert(world != null);

            System.Diagnostics.Debug.Assert(_disposed == false);

            double damage = DefaultAttackDamage;
            damage *= (attackCharge * attackCharge);
            damage *= GenerateRandomValueBetween(0.98, 1.01);

            //MyConsole.Debug($"Attack charge: {attackCharge:F2}");
            //MyConsole.Debug($"Damage: {damage:F2}");

            HandleAttack(
                world,
                damage,
                3, 0.3F);
        }

        protected override void OnAttack(World world, ItemStack stack, double attackCharge)
        {
            System.Diagnostics.Debug.Assert(world != null);
            System.Diagnostics.Debug.Assert(stack != null);

            System.Diagnostics.Debug.Assert(!_disposed);

            //MyConsole.Debug($"stack: {stack}");

            //Vector d = Look.GetUnitVector();
            //Vector eyeOrigin = GetEyeOrigin();

            double damage;

            switch (stack.Type)
            {
                default:
                    {
                        damage = DefaultAttackDamage;
                    }
                    break;
                case BalloonBasher.Type:
                    {
                        damage = BalloonBasher.Damage;
                    }
                    break;
            }

            damage *= (attackCharge * attackCharge);
            damage *= GenerateRandomValueBetween(0.98, 1.01);

            MyConsole.Debug($"Attack charge: {attackCharge:F2}");
            MyConsole.Debug($"Damage: {damage:F2}");

            HandleAttack(
              world,
              damage,
              3, 0.3F);

            stack.Damage(1);

        }

        protected override void OnUseItem(World world, ItemStack stack)
        {
            System.Diagnostics.Debug.Assert(world != null);
            System.Diagnostics.Debug.Assert(stack != null);

            System.Diagnostics.Debug.Assert(!_disposed);

            MyConsole.Debug("Use item!");

            GiveItem(new ItemStack(ItemType.DiamondSword, "Good Stick!", ""));
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

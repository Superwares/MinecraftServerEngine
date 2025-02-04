using Common;
using Containers;

using MinecraftPrimitives;
using MinecraftServerEngine;
using MinecraftServerEngine.PhysicsEngine;

namespace TestMinecraftServerApplication
{
    using Items;

    public sealed class SuperPlayer : AbstractPlayer
    {

        public const double DefaultAttackDamage = 1.0;

        public readonly static Time WorldBorderOutsideDamageInterval = Time.FromSeconds(1);


        private bool _disposed = false;

        private readonly static ChestInventory ChestInventory = new(GlobalChestItem.InventoryLines);
        private readonly static ShopInventory ShopInventory = new();

        private bool _speedup_running = false;
        private Time _speedup_duration = Time.Zero;
        private Time _speedup_startTime = Time.Zero;
        private int _speedup_x = 0;

        private bool _running_EmergencyEscape = false;
        private Time _startTime_EmergencyEscape = Time.Zero;


        private Time _worldBorderOutsideDamage_startTime = Time.Now();


        public SuperPlayer(
            UserId userId, string username,
            Vector p, Angles look)
            : base(userId, username, p, look, Gamemode.Adventure)
        {
            System.Diagnostics.Debug.Assert(userId != UserId.Null);
            System.Diagnostics.Debug.Assert(username != null && string.IsNullOrEmpty(username) == false);

            //ApplyBlockAppearance(Block.Dirt);

            GiveItemStacks(GamePanel.Item, GamePanel.DefaultCount);
            GiveItemStacks(ShopItem.Item, ShopItem.DefaultCount);
        }

        public void Reset()
        {
            System.Diagnostics.Debug.Assert(_disposed == false);

            FlushItems();

            SwitchGamemode(Gamemode.Adventure);

            GiveItemStacks(GamePanel.Item, GamePanel.DefaultCount);
            GiveItemStacks(ShopItem.Item, ShopItem.DefaultCount);
        }

        ~SuperPlayer()
        {
            System.Diagnostics.Debug.Assert(false);

            Dispose(false);
        }

        protected override void OnDespawn(PhysicsWorld _world)
        {
            System.Diagnostics.Debug.Assert(_world != null);

            System.Diagnostics.Debug.Assert(_disposed == false);

            if (_world is SuperWorld world)
            {
                System.Diagnostics.Debug.Assert(SuperWorld.GameContext != null);
                System.Diagnostics.Debug.Assert(UserId != UserId.Null);
                SuperWorld.GameContext.RemovePlayer(UserId);
            }

        }

        private double GenerateRandomValueBetween(double min, double max)
        {
            System.Random random = new System.Random();
            return min + (random.NextDouble() * (max - min));
        }

        public override void StartRoutine(PhysicsWorld _world)
        {
            System.Diagnostics.Debug.Assert(_world != null);

            System.Diagnostics.Debug.Assert(_disposed == false);

            if (_world is SuperWorld world)
            {
                if (_speedup_running == true && _speedup_x++ % 10 == 0)
                {
                    EmitParticles(Particle.Spell, 1.0, 1);

                    Time elapsedTime = Time.Now() - _speedup_startTime;
                    if (elapsedTime > _speedup_duration)
                    {
                        SetMovementSpeed(DefaultMovementSpeed);

                        _speedup_running = false;
                        _speedup_startTime = Time.Zero;
                    }
                }

                if (_running_EmergencyEscape == true)
                {
                    EmitParticles(EmergencyEscape.LaunchParticle, 1.0, 1);

                    Time elapsedTime = Time.Now() - _startTime_EmergencyEscape;
                    if (elapsedTime > EmergencyEscape.ParticleDuration)
                    {
                        _running_EmergencyEscape = false;
                        _startTime_EmergencyEscape = Time.Zero;
                    }
                }

                System.Diagnostics.Debug.Assert(world != null);
                if (
                    world.IsOutsideOfWorldBorder(Position) == true &&
                    Time.Now() - _worldBorderOutsideDamage_startTime > WorldBorderOutsideDamageInterval
                    )
                {
                    Damage(3.2390842905234, null);
                    world.PlaySound("entity.player.hurt", 7, Position, 0.5, 1.0);
                    _worldBorderOutsideDamage_startTime = Time.Now();
                }

            }


        }

        private void UnhideBlock(SuperWorld world)
        {
            System.Diagnostics.Debug.Assert(world != null);

            SetHelmet(null);

            ResetBlockAppearance();

        }

        private void HideBlock(SuperWorld world)
        {
            System.Diagnostics.Debug.Assert(world != null);

            BlockLocation blockLoc = BlockLocation.Generate(Position);
            BlockLocation belowBlockLoc = new(blockLoc.X, blockLoc.Y - 1, blockLoc.Z);

            System.Diagnostics.Debug.Assert(world.BlockContext != null);
            //Block block = world.BlockContext.GetBlock(blockLoc);
            Block belowBlock = world.BlockContext.GetBlock(belowBlockLoc);

            if (belowBlock != Block.Air && belowBlock.IsItemable() == true)
            {
                ItemType blockItemType = belowBlock.GetItemType();

                SetHelmet(new ItemStack(blockItemType, "Below Block!"));

                ApplyBlockAppearance(belowBlock);
            }
            else
            {
                UnhideBlock(world);
            }

        }

        protected override void OnMove(PhysicsWorld _world)
        {
            System.Diagnostics.Debug.Assert(_world != null);

            System.Diagnostics.Debug.Assert(_disposed == false);

            //ResetBlockAppearance();
            if (_world is SuperWorld world)
            {
                if (Sneaking == true)
                {
                    HideBlock(world);
                }
            }


        }

        protected override void OnSneak(World _world, bool f)
        {
            System.Diagnostics.Debug.Assert(_world != null);

            System.Diagnostics.Debug.Assert(_disposed == false);

            if (_world is SuperWorld world)
            {
                if (f == true)
                {


                    HideBlock(world);

                    //OpenInventory(chestInventory);
                    //OpenInventory(ShopInventory);
                    //OpenInventory(GameContext.Inventory);

                    //SetExperience(0.6F, 123456789);

                    //EmitParticles(Particle.Cloud, 1.0F, 100);
                    //EmitParticles(Particle.Largeexplode, 1.0F, 1);

                    //AddEffect(1, 1, 1800, 2);

                    //world.DisplayTitle(
                    //    Time.FromSeconds(0), Time.FromSeconds(1), Time.FromSeconds(0),
                    //    new TextComponent("good", TextColor.Blue));

                    //ApplyBilndness(true);

                    //world.ChangeWorldBorderSize(5.0, Time.FromSeconds(1));
                }
                else
                {
                    UnhideBlock(world);
                }
            }
        }

        protected override void OnSprint(World world, bool f)
        {
            System.Diagnostics.Debug.Assert(world != null);

            System.Diagnostics.Debug.Assert(_disposed == false);

            //MyConsole.Printl("Sprint!");
            //Switch(Gamemode.Adventure);
        }

        protected override void OnItemBreak(World world, ItemStack itemStack)
        {
            world.PlaySound("entity.item.break", 7, Position, 1.0F, 2.0F);

            switch (itemStack.Type)
            {
                case BalloonBasher.Type:
                    {
                        //System.Diagnostics.Debug.Assert(BalloonBasher.CanPurchase == false);
                        //BalloonBasher.CanPurchase = true;

                        //ShopInventory.ResetBalloonBasherSlot(null);
                    }
                    break;

                case BlastCore.Type:
                    {
                        //System.Diagnostics.Debug.Assert(BlastCore.CanPurchase == false);
                        BlastCore.CanPurchase = true;

                        ShopInventory.ResetBlastCoreSlot(null);
                    }
                    break;
                case EclipseCrystal.Type:
                    {
                        //System.Diagnostics.Debug.Assert(EclipseCrystal.CanPurchase == false);
                        EclipseCrystal.CanPurchase = true;

                        ShopInventory.ResetEclipseCrystalSlot(null);
                    }
                    break;
                case Doombringer.Type:
                    {
                        //System.Diagnostics.Debug.Assert(Doombringer.CanPurchase == false);
                        Doombringer.CanPurchase = true;

                        ShopInventory.ResetDoombringerSlot(null);
                    }
                    break;
            }
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
                livingEntity.Damage(damage, this);

                livingEntity.ApplyForce(d * knockbackScale);

                Vector v = new(
                    livingEntity.Position.X,
                    livingEntity.Position.Y + livingEntity.GetEyeHeight(),
                    livingEntity.Position.Z);

                world.PlaySound("entity.player.hurt", 7, v, 1.0F, 2.0F);
                world.PlaySound("entity.player.attack.strong", 7, Position, 1.0F, 2.0F);
            }
        }

        private bool HandleWoodenSwordAttack(SuperWorld world, double attackCharge)
        {
            System.Diagnostics.Debug.Assert(world != null);
            System.Diagnostics.Debug.Assert(attackCharge >= 0.0);
            System.Diagnostics.Debug.Assert(attackCharge <= 1.0);

            System.Diagnostics.Debug.Assert(_disposed == false);

            if (world.CanCombat == false)
            {
                return true;
            }

            double damage = WoodenSword.Damage;
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
                return true;
            }


            Vector o = GetEyeOrigin();
            Vector d = Look.GetUnitVector();
            Vector d_prime = d * directionScale;

            //MyConsole.Debug($"Eye origin: {eyeOrigin}, Scaled direction vector: {scaled_d}");

            PhysicsObject obj = world.SearchClosestObject(o, d_prime, this);

            if (obj != null && obj is LivingEntity livingEntity)
            {
                livingEntity.Damage(damage, this);

                livingEntity.ApplyForce(d * knockbackScale);

                Vector v = new(
                    livingEntity.Position.X,
                    livingEntity.Position.Y + livingEntity.GetEyeHeight(),
                    livingEntity.Position.Z);

                world.PlaySound("entity.player.attack.strong", 7, v, 1.0F, 2.0F);
            }

            return true;
        }

        private bool HandleBalloonBasherAttack(SuperWorld world, double attackCharge)
        {
            System.Diagnostics.Debug.Assert(world != null);
            System.Diagnostics.Debug.Assert(attackCharge >= 0.0);
            System.Diagnostics.Debug.Assert(attackCharge <= 1.0);

            System.Diagnostics.Debug.Assert(_disposed == false);

            if (world.CanCombat == false)
            {
                return true;
            }

            double damage = BalloonBasher.Damage;
            damage *= (attackCharge * attackCharge);
            damage *= GenerateRandomValueBetween(0.98, 1.01);

            System.Diagnostics.Debug.Assert(damage >= 0.0F);

            double directionScale = 3.0;
            double knockbackScale = GenerateRandomValueBetween(0.1, 0.3123) * (attackCharge * attackCharge);

            System.Diagnostics.Debug.Assert(directionScale > 0.0F);
            System.Diagnostics.Debug.Assert(knockbackScale > 0.0F);

            //MyConsole.Debug($"Damage: {damage:F2}");
            if (damage == 0.0F)
            {
                return true;
            }


            Vector o = GetEyeOrigin();
            Vector d = Look.GetUnitVector();
            Vector d_prime = d * directionScale;

            Vector k = new(0.0, 1.0, 0.0);

            //MyConsole.Debug($"Eye origin: {eyeOrigin}, Scaled direction vector: {scaled_d}");

            PhysicsObject obj = world.SearchClosestObject(o, d_prime, this);

            if (obj != null && obj is LivingEntity livingEntity)
            {
                livingEntity.Damage(damage, this);

                livingEntity.ApplyForce(k * knockbackScale);

                world.PlaySound("entity.generic.explode", 7, livingEntity.Position, 0.2, 0.5);

                livingEntity.EmitParticles(Particle.LargeExplode, 1.0, 1);
            }

            return true;
        }

        private void UseBlastCore(SuperWorld world)
        {
            System.Diagnostics.Debug.Assert(world != null);

            Vector v = new Vector(Position.X, Position.Y + GetEyeHeight(), Position.Z);
            Vector d;

            AxisAlignedBoundingBox aabb = AxisAlignedBoundingBox.Generate(v, BlastCore.Radius);

            using Tree<PhysicsObject> objs = new();

            System.Diagnostics.Debug.Assert(world != null);
            world.SearchObjects(objs, aabb, true, this);

            foreach (PhysicsObject obj in objs.GetKeys())
            {
                if (obj is LivingEntity livingEntity)
                {
                    livingEntity.Damage(BlastCore.Damage, this);

                    d = livingEntity.Position - v;
                    d *= BlastCore.Power;
                    d += new Vector(0.0, 0.3, 0.0);
                    d = d.Clamp(MinecraftPhysics.MinVelocity, MinecraftPhysics.MaxVelocity);

                    livingEntity.ApplyForce(d);
                }
            }

            world.PlaySound("block.end_gateway.spawn", 0, v, 1.0, 1.5);

            EmitParticles(BlastCore.EffectParticle, 1.0, 10);

        }

        private void UseDash(SuperWorld world)
        {
            System.Diagnostics.Debug.Assert(world != null);

            ItemStack[] takedItemStacks = TakeItemStacks(Dash.Item, Dash.DefaultCount);
            if (takedItemStacks == null)
            {
                return;
            }

            System.Diagnostics.Debug.Assert(takedItemStacks.Length > 0);

            Vector d = Look.GetUnitVector() + new Vector(0.0, 0.1, 0.0);

            ApplyForce(d * Dash.Power);

            EmitParticles(Particle.InstantSpell, 0.1, 150);

            world.PlaySound("entity.llama.swag", 0, Position, 0.5, 1.0);
        }

        private void UseHint(SuperWorld world)
        {
            System.Diagnostics.Debug.Assert(world != null);

            ItemStack[] takedItemStacks = TakeItemStacks(Hint.Item, Hint.DefaultCount);
            if (takedItemStacks == null)
            {
                return;
            }

            System.Diagnostics.Debug.Assert(takedItemStacks.Length > 0);

            foreach (AbstractPlayer player in world.Players)
            {
                System.Diagnostics.Debug.Assert(player != null);

                if (object.ReferenceEquals(player, this) == true)
                {
                    continue;
                }
                Vector offset = new Vector(0.0, player.GetEyeHeight(), 0.0);
                Vector v = player.Position + offset;

                world.PlaySound("entity.firework.large_blast", 0, v, 1.0, 2.0);

                player.EmitParticles(Particle.FireworksSpark, offset, 0.1, 100);
            }

        }


        private void UseChaosSwap(SuperWorld world)
        {
            System.Diagnostics.Debug.Assert(world != null);

            ItemStack[] takedItemStacks = TakeItemStacks(ChaosSwap.Item, ChaosSwap.DefaultCount);
            if (takedItemStacks == null)
            {
                return;
            }

            int i = 0;
            AbstractPlayer[] targetPlayers = new AbstractPlayer[world.AllPlayers];

            foreach (AbstractPlayer player in world.Players)
            {
                if (player.Gamemode != Gamemode.Adventure)
                {
                    continue;
                }

                targetPlayers[i++] = player;
            }

            if (i > 0)
            {
                System.Random random = new System.Random();
                AbstractPlayer randomPlayer = targetPlayers[random.Next(i)];

                randomPlayer.Teleport(Position, Look);
                Teleport(randomPlayer.Position, randomPlayer.Look);

                EmitParticles(Particle.InstantSpell, 0.1, 150);

                world.PlaySound("entity.llama.swag", 0, Position, 0.5, 1.0);
            }


        }


        private bool HandleDoombringerAttack(SuperWorld world, double attackCharge)
        {
            System.Diagnostics.Debug.Assert(world != null);
            System.Diagnostics.Debug.Assert(attackCharge >= 0.0);
            System.Diagnostics.Debug.Assert(attackCharge <= 1.0);

            System.Diagnostics.Debug.Assert(_disposed == false);

            if (world.CanCombat == false)
            {
                return false;
            }

            double damage = Doombringer.Damage;
            damage *= (attackCharge * attackCharge);
            damage *= GenerateRandomValueBetween(0.98, 1.01);

            System.Diagnostics.Debug.Assert(damage >= 0.0F);

            double directionScale = 1.0;
            double knockbackScale = GenerateRandomValueBetween(0.1, 0.3123);

            System.Diagnostics.Debug.Assert(directionScale > 0.0F);
            System.Diagnostics.Debug.Assert(knockbackScale > 0.0F);

            //MyConsole.Debug($"Damage: {damage:F2}");
            if (damage == 0.0F)
            {
                return true;
            }


            Vector o = GetEyeOrigin();
            Vector d = Look.GetUnitVector();
            Vector d_prime = d * directionScale;

            //MyConsole.Debug($"Eye origin: {eyeOrigin}, Scaled direction vector: {scaled_d}");

            PhysicsObject obj = world.SearchClosestObject(o, d_prime, this);

            if (obj != null && obj is LivingEntity livingEntity)
            {
                livingEntity.Damage(damage, this);

                //System.Diagnostics.Debug.Assert(health >= 0.0);
                //if (
                //    livingEntity is SuperPlayer &&
                //    damaged == true &&
                //    health == 0.0 &&
                //    SuperWorld.GameContext.IsStarted == true
                //    )
                //{
                //    System.Diagnostics.Debug.Assert(SuperWorld.GameContext != null);
                //    System.Diagnostics.Debug.Assert(UserId != UserId.Null);
                //    SuperWorld.GameContext.HandleKillEvent(this);
                //}

                livingEntity.ApplyForce(d * knockbackScale);

                Vector v = new(
                    livingEntity.Position.X,
                    livingEntity.Position.Y + livingEntity.GetEyeHeight(),
                    livingEntity.Position.Z);

                world.PlaySound("entity.irongolem.hurt", 0, v, 1.0, 2.0);
                world.PlaySound("entity.lightning.impact", 0, v, 1.0, 2.0);

                livingEntity.EmitParticles(Particle.Smoke, 0.23, 999);

                return true;
            }

            return false;
        }


        protected override void OnAttack(World _world, double attackCharge)
        {
            System.Diagnostics.Debug.Assert(_world != null);
            System.Diagnostics.Debug.Assert(attackCharge >= 0.0);
            System.Diagnostics.Debug.Assert(attackCharge <= 1.0);

            System.Diagnostics.Debug.Assert(_disposed == false);


            if (_world is SuperWorld world)
            {
                Vector eyeOrigin = GetEyeOrigin();
                Vector d = Look.GetUnitVector();
                Vector scaled_d = d * GetEyeHeight();

                PhysicsObject obj = world.SearchClosestObject(eyeOrigin, scaled_d, this);

                if (obj is ItemEntity itemEntity)
                {
                    itemEntity.PickUp(this);
                }
                else
                {
                    HandleDefaultAttack(world, attackCharge);
                }

            }
        }

        protected override void OnAttack(World _world, ItemStack itemStack, double attackCharge)
        {
            System.Diagnostics.Debug.Assert(_world != null);
            System.Diagnostics.Debug.Assert(itemStack != null);
            System.Diagnostics.Debug.Assert(attackCharge >= 0.0);
            System.Diagnostics.Debug.Assert(attackCharge <= 1.0);

            System.Diagnostics.Debug.Assert(_disposed == false);

            if (_world is SuperWorld world)
            {

                Vector eyeOrigin = GetEyeOrigin();
                Vector d = Look.GetUnitVector();
                Vector scaled_d = d * GetEyeHeight();

                PhysicsObject obj = world.SearchClosestObject(eyeOrigin, scaled_d, this);

                if (obj is ItemEntity itemEntity)
                {
                    itemEntity.PickUp(this);
                }
                else
                {
                    bool breaked = false;

                    switch (itemStack.Type)
                    {
                        default:
                            HandleDefaultAttack(world, attackCharge);
                            breaked = true;
                            break;
                        case WoodenSword.Type:
                            breaked = HandleWoodenSwordAttack(world, attackCharge);
                            break;
                        case BalloonBasher.Type:
                            breaked = HandleBalloonBasherAttack(world, attackCharge);
                            break;

                        case BlastCore.Type:
                            UseBlastCore(world);
                            break;
                        case Doombringer.Type:
                            breaked = HandleDoombringerAttack(world, attackCharge);
                            break;

                        case Dash.Type:
                            UseDash(world);
                            break;
                        case Hint.Type:
                            UseHint(world);
                            break;

                        case ChaosSwap.Type:
                            UseChaosSwap(world);
                            break;
                    }

                    if (breaked == true)
                    {
                        itemStack.Damage(1);
                    }
                }
                
            }

        }

        private void UseEclipseCrystal(SuperWorld world)
        {
            System.Diagnostics.Debug.Assert(world != null);

            ItemStack[] takedItemStacks = TakeItemStacks(EclipseCrystal.Item, EclipseCrystal.DefaultCount);
            if (takedItemStacks == null)
            {
                return;
            }

            System.Diagnostics.Debug.Assert(takedItemStacks.Length > 0);

            world.ChangeWorldBorderSize(
                world.DefaultWorldBorderRadiusInMeters / 2.0,
                Time.FromMilliseconds(400)
                );

            world.ChangeWorldTimeOfDay(
                MinecraftTimes.NighttimeMid,
                Time.FromSeconds(1)
                );

            foreach (AbstractPlayer player in world.Players)
            {
                Vector v = new(player.Position.X, player.Position.Y + player.GetEyeHeight(), player.Position.Z);
                world.PlaySound("block.end_portal.spawn", 6, v, 1.0, 2.0);

                if (object.ReferenceEquals(this, player) == false)
                {
                    player.Damage(EclipseCrystal.Damage, this);
                }
            }

            //System.Diagnostics.Debug.Assert(EclipseCrystal.CanPurchase == false);
            EclipseCrystal.CanPurchase = true;

            ShopInventory.ResetEclipseCrystalSlot(null);
        }

        private void UseStoneOfSwiftness(SuperWorld world)
        {
            System.Diagnostics.Debug.Assert(world != null);

            ItemStack[] takedItemStacks = TakeItemStacks(StoneOfSwiftness.Item, StoneOfSwiftness.DefaultCount);
            if (takedItemStacks == null)
            {
                return;
            }

            System.Diagnostics.Debug.Assert(takedItemStacks.Length > 0);

            System.Diagnostics.Debug.Assert(takedItemStacks != null);
            System.Diagnostics.Debug.Assert(takedItemStacks.Length == 1);
            System.Diagnostics.Debug.Assert(takedItemStacks[0].Count == StoneOfSwiftness.DefaultCount);

            _speedup_running = true;
            _speedup_duration = StoneOfSwiftness.Duration;
            _speedup_startTime = Time.Now();

            SetMovementSpeed(StoneOfSwiftness.MovementSpeed);

            world.PlaySound("block.anvil.land", 4, Position, 0.8, 1.5);
        }

        private void UseEmergencyEscape(SuperWorld world)
        {
            System.Diagnostics.Debug.Assert(world != null);

            ItemStack[] takedItemStacks = TakeItemStacks(EmergencyEscape.Item, EmergencyEscape.DefaultCount);
            if (takedItemStacks == null)
            {
                return;
            }

            System.Diagnostics.Debug.Assert(takedItemStacks.Length > 0);

            ApplyForce(new Vector(0.0, EmergencyEscape.Power, 0.0));

            _running_EmergencyEscape = true;
            _startTime_EmergencyEscape = Time.Now();

            world.PlaySound(EmergencyEscape.LaunchSoundName, 0, Position, 0.8, 1.5);
        }

        protected override void OnUseItem(World _world, ItemStack itemStack)
        {
            System.Diagnostics.Debug.Assert(_world != null);
            System.Diagnostics.Debug.Assert(itemStack != null);

            System.Diagnostics.Debug.Assert(_disposed == false);

            if (_world is SuperWorld world)
            {
                switch (itemStack.Type)
                {
                    case ShopItem.Type:
                        OpenInventory(ShopInventory);
                        break;
                    case GamePanel.Type:
                        OpenInventory(GameContext.Inventory);
                        break;

                    case GlobalChestItem.Type:
                        OpenInventory(ChestInventory);
                        break;

                    case BlastCore.Type:
                        UseBlastCore(world);
                        itemStack.Damage(1);
                        break;
                    case EclipseCrystal.Type:
                        UseEclipseCrystal(world);
                        break;

                    case StoneOfSwiftness.Type:
                        UseStoneOfSwiftness(world);
                        break;

                    case EmergencyEscape.Type:
                        UseEmergencyEscape(world);
                        break;

                    case Dash.Type:
                        UseDash(world);
                        break;
                    case Hint.Type:
                        UseHint(world);
                        break;

                    case ChaosSwap.Type:
                        UseChaosSwap(world);
                        break;
                }
            }
        }

        protected override void OnUseEntity(World world, Entity entity)
        {
            System.Diagnostics.Debug.Assert(world != null);
            System.Diagnostics.Debug.Assert(entity != null);

            System.Diagnostics.Debug.Assert(_disposed == false);

            //MyConsole.Debug("Use entity!");

            /*entity.Teleport(new Vector(1.0D, 103.0D, 3.0D), new Look(30.0F, 90.0F));*/

            /*entity.ApplyForce(new Vector(
                (Random.NextDouble() - 0.5D) / 10.0D, 
                (Random.NextDouble() - 0.5D) / 10.0D, 
                (Random.NextDouble() - 0.5D) / 10.0D));*/

        }

        private bool UsePhoenixFeather(World world)
        {
            System.Diagnostics.Debug.Assert(world != null);

            System.Diagnostics.Debug.Assert(_disposed == false);

            ItemStack[] takedItemStacks = TakeItemStacks(PhoenixFeather.Item, PhoenixFeather.DefaultCount);
            if (takedItemStacks == null)
            {
                return false;
            }

            AddAdditionalHealth(PhoenixFeather.AdditionalHearts);
            SetMovementSpeed(PhoenixFeather.MovementSpeed);

            EmitParticles(Particle.Lava, 0.8, 1_000);

            _speedup_running = true;
            _speedup_duration = PhoenixFeather.MovementSpeedDuration;
            _speedup_startTime = Time.Now();

            world.PlaySound("block.anvil.land", 4, Position, 1.0, 2.0);

            PhoenixFeather.CanPurchase = true;
            ShopInventory.ResetPhoenixFeatherSlot(null);

            return true;
        }

        protected override void OnDeath(World world)
        {
            System.Diagnostics.Debug.Assert(world != null);

            System.Diagnostics.Debug.Assert(_disposed == false);

            HealFully();

            if (
                UsePhoenixFeather(world) == false &&
                SuperWorld.GameContext.IsStarted == true &&
                Gamemode != Gamemode.Spectator
                )
            {
                SwitchGamemode(Gamemode.Spectator);

                System.Diagnostics.Debug.Assert(SuperWorld.GameContext != null);
                System.Diagnostics.Debug.Assert(UserId != UserId.Null);
                SuperWorld.GameContext.HandleKillEventForSeeker();

                System.Diagnostics.Debug.Assert(SuperWorld.GameContext != null);
                System.Diagnostics.Debug.Assert(UserId != UserId.Null);
                SuperWorld.GameContext.HandleDeathEvent(UserId);

            }


        }


        protected override void OnDeath(World world, LivingEntity attacker)
        {
            System.Diagnostics.Debug.Assert(world != null);
            System.Diagnostics.Debug.Assert(attacker != null);

            System.Diagnostics.Debug.Assert(_disposed == false);

            HealFully();

            if (
                UsePhoenixFeather(world) == false &&
                SuperWorld.GameContext.IsStarted == true &&
                Gamemode != Gamemode.Spectator
                )
            {
                SwitchGamemode(Gamemode.Spectator);

                System.Diagnostics.Debug.Assert(attacker != null);
                if (attacker is SuperPlayer attackPlayer)
                {
                    System.Diagnostics.Debug.Assert(SuperWorld.GameContext != null);
                    System.Diagnostics.Debug.Assert(UserId != UserId.Null);
                    SuperWorld.GameContext.HandleKillEvent(attackPlayer);
                }

                System.Diagnostics.Debug.Assert(SuperWorld.GameContext != null);
                System.Diagnostics.Debug.Assert(UserId != UserId.Null);
                SuperWorld.GameContext.HandleDeathEvent(UserId);

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

using Sync;
using MinecraftServerEngine.Protocols;
using MinecraftServerEngine.Text;

using MinecraftServerEngine.Inventories;
using MinecraftServerEngine.Items;
using MinecraftServerEngine.Renderers;
using MinecraftServerEngine.Physics;

namespace MinecraftServerEngine.Entities
{

    public abstract class AbstractPlayer : LivingEntity
    {
        public const double HitboxWidth = 0.6D;

        private static Hitbox GetAdventureHitbox(bool sneaking)
        {
            double w = HitboxWidth, h;
            if (sneaking == true)
            {
                h = 1.65D;
            }
            else
            {
                h = 1.8D;
            }

            return new Hitbox(w, h);
        }

        private static Hitbox GetSpectatorHitbox()
        {
            return Hitbox.Empty;
        }

        public const double DefaultMass = 1.0D;
        public const double DefaultMaxStepLevel = 0.6;
        public const double DefaultEyeHeight = 1.62D;


        private bool _disposed = false;

        private readonly Locker InventoryLocker = new();
        internal readonly PlayerInventory Inventory = new();


        public readonly UserId UserId;
        public readonly string Username;

        private Connection Conn;
        public bool Disconnected => Conn == null;
        public bool Connected => !Disconnected;


        private Vector _pControl;


        private Locker LockerGamemode = new();
        private Gamemode _newGamemode, _gamemode;
        public Gamemode Gamemode => _newGamemode;


        private float _experienceBarRatio = 0.0F;
        private int _experienceLevel = 0;
        public float ExperienceBarRatio => _experienceBarRatio;
        public int ExperienceLevel => _experienceLevel;


        private bool _blindness = false;

        public bool Blindness => _blindness;

        protected AbstractPlayer(
            UserId userId, string username,
            Vector p, EntityAngles look, Gamemode gamemode)
            : base(
                  userId.Value,
                  p, look,
                  false,  // noGravity
                  gamemode == Gamemode.Spectator ? GetSpectatorHitbox() : GetAdventureHitbox(false),
                  DefaultMass, DefaultMaxStepLevel)
        {
            System.Diagnostics.Debug.Assert(userId != UserId.Null);
            System.Diagnostics.Debug.Assert(username != null && string.IsNullOrEmpty(username) == false);
            UserId = userId;
            Username = username;

            System.Diagnostics.Debug.Assert(Sneaking == false);
            System.Diagnostics.Debug.Assert(Sprinting == false);

            _newGamemode = gamemode;
            _gamemode = gamemode;
        }

        ~AbstractPlayer()
        {
            System.Diagnostics.Debug.Assert(false);

            Dispose(false);
        }

        public void PlaySound(string name, int category, double volume, double pitch)
        {
            if (_disposed == true)
            {
                throw new System.ObjectDisposedException(GetType().Name);
            }

            if (volume < 0.0F || volume > 1.0F)
            {
                throw new System.ArgumentOutOfRangeException(nameof(volume));
            }

            if (volume < 0.5F || volume > 2.0F)
            {
                throw new System.ArgumentOutOfRangeException(nameof(volume));
            }

            Vector p = new(Position.X, Position.Y + GetEyeHeight(), Position.Z);

            if (Connected == true)
            {
                System.Diagnostics.Debug.Assert(Conn != null);
                Conn.PlaySound(name, category, p, volume, pitch);
            }
        }


        public virtual void OnRespawn()
        {

        }

        //internal void Respawn()
        //{
        //    System.Diagnostics.Debug.Assert(_disposed == false);

        //    System.Diagnostics.Debug.Assert(Gamemode != Gamemode.Spectator);

        //    if (Connected == true)
        //    {
        //        Conn.UpdateHealth(MaxHealth);
        //    }

        //    _nextGamemode = Gamemode.Spectator;
        //    _health = MaxHealth;

        //    OnRespawn();
        //}

        //internal override void _Attack(World world, ItemStack stack)
        //{
        //    System.Diagnostics.Debug.Assert(world != null);
        //    System.Diagnostics.Debug.Assert(stack != null);


        //    if (stack.IsBreaked == true)
        //    {
        //        throw new System.NotImplementedException();

        //        _Attack(world);
        //    }
        //    else
        //    {
        //        base._Attack(world, stack);
        //    }

        //    if (stack.IsBreaked == true)
        //    {
        //        throw new System.NotImplementedException();
        //    }



        //}

        protected override void HandleDamageEvent(World world, double amount, LivingEntity attacker)
        {
            System.Diagnostics.Debug.Assert(world != null);
            System.Diagnostics.Debug.Assert(amount >= 0.0D);

            System.Diagnostics.Debug.Assert(_disposed == false);

            // Anyway, thread safety, so no need for locker
            //System.Diagnostics.Debug.Assert(LockerHealths != null);
            //LockerHealths.Hold();

            try
            {
                base.HandleDamageEvent(world, amount, attacker);

                if (Connected == true)
                {
                    System.Diagnostics.Debug.Assert(AdditionalHealth >= 0.0);
                    System.Diagnostics.Debug.Assert(Health >= 0.0);
                    Conn.UpdateAdditionalHealth(Id, AdditionalHealth);
                    Conn.UpdateHealth(Health);
                }

            }
            finally
            {
                // Anyway, thread safety, so no need for locker
                //System.Diagnostics.Debug.Assert(LockerHealths != null);
                //LockerHealths.Release();
            }

        }

        protected override double _Heal(double amount)
        {
            System.Diagnostics.Debug.Assert(amount >= 0.0D);

            System.Diagnostics.Debug.Assert(_disposed == false);

            System.Diagnostics.Debug.Assert(LockerHealths != null);
            LockerHealths.Hold();

            try
            {
                double health = base._Heal(amount);

                if (Connected == true)
                {
                    System.Diagnostics.Debug.Assert(Health >= 0.0);
                    Conn.UpdateHealth(Health);
                }

                System.Diagnostics.Debug.Assert(health >= 0.0);
                return health;
            }
            finally
            {
                System.Diagnostics.Debug.Assert(LockerHealths != null);
                LockerHealths.Release();
            }

        }

        protected override void _SetMaxHealth(double amount)
        {
            System.Diagnostics.Debug.Assert(amount > 0.0D);

            System.Diagnostics.Debug.Assert(_disposed == false);

            System.Diagnostics.Debug.Assert(LockerHealths != null);
            LockerHealths.Hold();

            try
            {
                base._SetMaxHealth(amount);

                if (Connected == true)
                {
                    System.Diagnostics.Debug.Assert(MaxHealth >= 0.0);
                    System.Diagnostics.Debug.Assert(Health >= 0.0);
                    Conn.UpdateMaxHealth(Id, MaxHealth);
                    Conn.UpdateHealth(Health);
                }

            }
            finally
            {
                System.Diagnostics.Debug.Assert(LockerHealths != null);
                LockerHealths.Release();
            }
        }

        protected override void _SetAdditionalHealth(double amount)
        {
            System.Diagnostics.Debug.Assert(amount >= 0.0D);

            System.Diagnostics.Debug.Assert(_disposed == false);

            System.Diagnostics.Debug.Assert(LockerHealths != null);
            LockerHealths.Hold();

            try
            {
                base._SetAdditionalHealth(amount);

                if (Connected == true)
                {
                    System.Diagnostics.Debug.Assert(AdditionalHealth >= 0.0);
                    Conn.UpdateAdditionalHealth(Id, AdditionalHealth);
                }

            }
            finally
            {
                System.Diagnostics.Debug.Assert(LockerHealths != null);
                LockerHealths.Release();
            }
        }

        protected override void _AddAdditionalHealth(double amount)
        {
            System.Diagnostics.Debug.Assert(amount >= 0.0D);

            System.Diagnostics.Debug.Assert(_disposed == false);

            System.Diagnostics.Debug.Assert(LockerHealths != null);
            LockerHealths.Hold();

            try
            {
                base._AddAdditionalHealth(amount);

                if (Connected == true)
                {
                    System.Diagnostics.Debug.Assert(AdditionalHealth >= 0.0);
                    Conn.UpdateAdditionalHealth(Id, AdditionalHealth);
                }

            }
            finally
            {
                System.Diagnostics.Debug.Assert(LockerHealths != null);
                LockerHealths.Release();
            }
        }

        protected override void _SetMovementSpeed(double amount)
        {
            System.Diagnostics.Debug.Assert(amount >= 0.0);
            System.Diagnostics.Debug.Assert(amount <= 1024.0);

            System.Diagnostics.Debug.Assert(_disposed == false);

            System.Diagnostics.Debug.Assert(LockerMovementSpeed != null);
            LockerMovementSpeed.Hold();

            try
            {
                base._SetMovementSpeed(amount);

                if (Connected == true)
                {
                    System.Diagnostics.Debug.Assert(MovementSpeed >= 0.0);
                    System.Diagnostics.Debug.Assert(MovementSpeed <= 1024.0);
                    Conn.UpdateMovementSpeed(Id, MovementSpeed);
                }
            }
            finally
            {
                System.Diagnostics.Debug.Assert(LockerMovementSpeed != null);
                LockerMovementSpeed.Release();
            }
        }

        internal override void _AddEffect(
            byte effectId,
            byte amplifier, int duration, byte flags)
        {
            System.Diagnostics.Debug.Assert(_disposed == false);

            base._AddEffect(effectId, amplifier, duration, flags);

            if (Connected)
            {
                Conn.AddEffect(Id, effectId, amplifier, duration, flags);
            }
        }

        internal override void _EmitParticles(
            Particle particle, Vector v,
            double speed, int count,
            double r, double g, double b)
        {
            System.Diagnostics.Debug.Assert(r >= 0.0);
            System.Diagnostics.Debug.Assert(r <= 1.0);
            System.Diagnostics.Debug.Assert(g >= 0.0);
            System.Diagnostics.Debug.Assert(g <= 1.0);
            System.Diagnostics.Debug.Assert(b >= 0.0);
            System.Diagnostics.Debug.Assert(b <= 1.0);
            System.Diagnostics.Debug.Assert(speed >= 0.0);
            System.Diagnostics.Debug.Assert(speed <= 1.0);
            System.Diagnostics.Debug.Assert(count >= 0);

            System.Diagnostics.Debug.Assert(_disposed == false);

            base._EmitParticles(
                particle, v,
                speed, count,
                r, g, b);

            if (Connected == true)
            {
                Conn.EmitParticles(particle, v, (float)speed, count, (float)r, (float)g, (float)b);
            }
        }

        public void AddEffect(byte effectId, byte amplifier, int duration, byte flags)
        {
            if (_disposed == true)
            {
                throw new System.ObjectDisposedException(GetType().FullName);
            }

            _AddEffect(effectId, amplifier, duration, flags);
        }

        public void SetExperience(float ratio, int level)
        {
            if (ratio < 0 || ratio > 1)
            {
                throw new System.ArgumentOutOfRangeException(nameof(ratio));
            }

            if (level < 0)
            {
                throw new System.ArgumentOutOfRangeException(nameof(level));
            }

            _experienceBarRatio = ratio;
            _experienceLevel = level;

            Conn.SetExperience(ratio, level);
        }

        private protected override Hitbox GetHitbox()
        {
            System.Diagnostics.Debug.Assert(_disposed == false);

            return _newGamemode == Gamemode.Spectator ?
                GetSpectatorHitbox() : GetAdventureHitbox(false);
        }

        public override double GetEyeHeight()
        {
            double value = DefaultEyeHeight;

            if (Sneaking == true)
            {
                value += -0.08D;
            }

            return value;
        }

        private protected override void RenderSpawning(EntityRenderer renderer)
        {
            System.Diagnostics.Debug.Assert(renderer != null);

            System.Diagnostics.Debug.Assert(_disposed == false);

            System.Diagnostics.Debug.Assert(_gamemode == Gamemode.Adventure);
            renderer.SpawnPlayer(
                Id, UniqueId,
                Position, Look,
                Sneaking, Sprinting);

            {
                using MinecraftProtocolDataStream stream = new();

                Inventory.WriteMainHandData(stream);

                byte[] mainHand = stream.ReadData();

                Inventory.WriteOffHandData(stream);

                byte[] offHand = stream.ReadData();

                Inventory.WriteHelmetData(stream);

                byte[] helmet = stream.ReadData();

                System.Diagnostics.Debug.Assert(renderer != null);
                renderer.SetEquipmentsData(Id, mainHand, offHand, helmet);
            }
        }

        internal void Connect(MinecraftClient client, World world, UserId id)
        {
            System.Diagnostics.Debug.Assert(_disposed == false);

            System.Diagnostics.Debug.Assert(client != null);
            System.Diagnostics.Debug.Assert(world != null);
            System.Diagnostics.Debug.Assert(id != UserId.Null);

            _pControl = Position;

            Conn = new Connection(
                UserId, Username,
                client,
                world,
                Id,
                AdditionalHealth, MaxHealth, Health,
                MovementSpeed,
                Position, Look,
                Blindness,
                Inventory,
                _gamemode);

            SetExperience(ExperienceBarRatio, ExperienceLevel);
        }

        public void SwitchGamemode(Gamemode gamemode)
        {
            System.Diagnostics.Debug.Assert(_disposed == false);

            LockerGamemode.Hold();

            if (_newGamemode != gamemode)
            {
                _newGamemode = gamemode;
            }

            LockerGamemode.Release();
        }

        public void ApplyBilndness(bool f)
        {
            System.Diagnostics.Debug.Assert(_disposed == false);

            _blindness = f;

            if (Connected == true)
            {
                System.Diagnostics.Debug.Assert(Conn != null);
                Conn.ApplyBilndness(_blindness);
            }
        }

        public override void ApplyForce(Vector force)
        {
            if (force.X < MinecraftPhysics.MinVelocity || force.X > MinecraftPhysics.MaxVelocity)
            {
                throw new System.ArgumentOutOfRangeException(
                    nameof(force),
                    "The force vector's X component is out of the allowed range.");
            }
            if (force.Y < MinecraftPhysics.MinVelocity || force.Y > MinecraftPhysics.MaxVelocity)
            {
                throw new System.ArgumentOutOfRangeException(
                    nameof(force),
                    "The force vector's Y component is out of the allowed range.");
            }
            if (force.Z < MinecraftPhysics.MinVelocity || force.Z > MinecraftPhysics.MaxVelocity)
            {
                throw new System.ArgumentOutOfRangeException(
                    nameof(force),
                    "The force vector's Z component is out of the allowed range.");
            }

            if (_disposed == true)
            {
                throw new System.ObjectDisposedException(GetType().Name);
            }

            System.Diagnostics.Debug.Assert(_disposed == false);

            if (Connected == true)
            {
                System.Diagnostics.Debug.Assert(Conn != null);
                Vector v = force / Mass;
                Conn.ApplyVelocity(Id, v);
            }

            base.ApplyForce(force);
        }

        internal override void Move(PhysicsWorld world, BoundingVolume volume, Vector v)
        {
            System.Diagnostics.Debug.Assert(world != null);
            System.Diagnostics.Debug.Assert(_disposed == false);

            System.Diagnostics.Debug.Assert(volume != null);

            if (Connected == true)
            {
                // TODO: Check the difference between _p and p. and predict movement....
                /*Console.Printl($"p: {p}, _p: {_p}, ");
                Console.Printl($"Length: {Vector.GetLength(p, _p)}");
                if (Vector.GetLength(p, _p) > k)
                {
                }
                */

                /*Vector v1 = bb.GetBottomCenter(), v2 = _p;
                double length = Vector.GetLength(v1, v2);
                Console.Printl($"length: {length}");*/

                volume = GetHitbox().Convert(_pControl);

                if (_gamemode != _newGamemode)
                {
                    Conn.SetGamemode(Id, _newGamemode);
                }
            }

            _gamemode = _newGamemode;

            base.Move(world, volume, v);
        }

        public override void Teleport(Vector p, EntityAngles look)
        {
            System.Diagnostics.Debug.Assert(_disposed == false);

            System.Diagnostics.Debug.Assert(LockerTeleport != null);
            LockerTeleport.Hold();

            try
            {
                if (Connected == true)
                {
                    _pControl = p;

                    System.Diagnostics.Debug.Assert(Conn != null);
                    Conn.Teleport(p, look);
                }

                base.Teleport(p, look);


            }
            finally
            {
                System.Diagnostics.Debug.Assert(LockerTeleport != null);
                LockerTeleport.Release();
            }
        }

        internal void ControlMovement(Vector p)
        {
            System.Diagnostics.Debug.Assert(_disposed == false);

            _pControl = p;
        }

        internal void Control(World world)
        {
            System.Diagnostics.Debug.Assert(_disposed == false);

            if (Connected == true)
            {
                System.Diagnostics.Debug.Assert(Conn != null);

                Conn.Control(world, this, Inventory);
            }

        }

        // deprecated...  Replace to the OnDespawn when player is disconnected and world can despawn the player on disconnect.
        //protected internal virtual void OnDisconnected()
        //{
        //    if (_disposed == true)
        //    {
        //        throw new System.ObjectDisposedException(GetType().Name);
        //    }
        //}

        public bool HandleDisconnection(out UserId userId, World world)
        {
            System.Diagnostics.Debug.Assert(_disposed == false);

            System.Diagnostics.Debug.Assert(world != null);

            if (Conn != null && Conn.Disconnected == true)
            {
                Conn.Flush(out userId, world, Inventory);
                Conn.Dispose();

                Conn = null;

                return true;
            }

            userId = UserId.Null;
            return false;
        }


        public bool OpenInventory(SharedInventory sharedInventory)
        {
            System.Diagnostics.Debug.Assert(_disposed == false);

            System.Diagnostics.Debug.Assert(sharedInventory != null);

            if (Disconnected == true)
            {
                return false;
            }

            System.Diagnostics.Debug.Assert(Conn != null);
            return Conn.Open(Inventory, sharedInventory);
        }

        //public void SetItemDamage(ItemStack stack)
        //{
        //    System
        //}

        public void SetHelmet(ItemStack itemStack)
        {
            if (_disposed == true)
            {
                throw new System.ObjectDisposedException(GetType().Name);
            }

            System.Diagnostics.Debug.Assert(_disposed == false);

            System.Diagnostics.Debug.Assert(InventoryLocker != null);
            InventoryLocker.Hold();

            try
            {
                if (Conn != null)
                {
                    System.Diagnostics.Debug.Assert(Inventory != null);
                    System.Diagnostics.Debug.Assert(Conn.Window != null);
                    Conn.Window.SetHelmet(Inventory, itemStack);
                }
                else
                {
                    System.Diagnostics.Debug.Assert(Inventory != null);
                    Inventory.SetHelmet(itemStack);
                }

                System.Diagnostics.Debug.Assert(Inventory != null);
                UpdateEquipmentsData();
            }
            finally
            {
                System.Diagnostics.Debug.Assert(InventoryLocker != null);
                InventoryLocker.Release();
            }
        }

        public bool GiveItemStacks(IReadOnlyItem item, int count)
        {
            if (item == null)
            {
                throw new System.ArgumentNullException(nameof(item));
            }

            if (_disposed == true)
            {
                throw new System.ObjectDisposedException(GetType().Name);
            }

            System.Diagnostics.Debug.Assert(_disposed == false);

            if (count == 0)
            {
                return true;
            }

            System.Diagnostics.Debug.Assert(InventoryLocker != null);
            InventoryLocker.Hold();

            try
            {
                if (Conn != null)
                {
                    System.Diagnostics.Debug.Assert(Inventory != null);
                    System.Diagnostics.Debug.Assert(Conn.Window != null);
                    return Conn.Window.GiveItemStacks(Inventory, item, count);
                }
                else
                {
                    System.Diagnostics.Debug.Assert(Inventory != null);
                    return Inventory.GiveItemStacks(item, count);
                }
            }
            finally
            {
                System.Diagnostics.Debug.Assert(InventoryLocker != null);
                InventoryLocker.Release();
            }

        }

        public bool GiveItemStack(ref ItemStack itemStack)
        {
            if (_disposed == true)
            {
                throw new System.ObjectDisposedException(GetType().Name);
            }

            System.Diagnostics.Debug.Assert(_disposed == false);

            if (itemStack == null)
            {
                return true;
            }

            System.Diagnostics.Debug.Assert(InventoryLocker != null);
            InventoryLocker.Hold();

            try
            {
                if (Conn != null)
                {
                    System.Diagnostics.Debug.Assert(Inventory != null);
                    System.Diagnostics.Debug.Assert(Conn.Window != null);
                    return Conn.Window.GiveItemStack(Inventory, ref itemStack);
                }
                else
                {
                    System.Diagnostics.Debug.Assert(Inventory != null);
                    return Inventory.GiveItemStack(ref itemStack);
                }
            }
            finally
            {
                System.Diagnostics.Debug.Assert(InventoryLocker != null);
                InventoryLocker.Release();
            }

        }

        public ItemStack[] TakeItemStacks(IReadOnlyItem item, int count)
        {
            if (item == null)
            {
                throw new System.ArgumentNullException(nameof(item));
            }

            if (count < 0)
            {
                throw new System.ArgumentOutOfRangeException(nameof(count));
            }

            if (_disposed == true)
            {
                throw new System.ObjectDisposedException(GetType().Name);
            }

            System.Diagnostics.Debug.Assert(item != null);
            System.Diagnostics.Debug.Assert(count >= 0);
            System.Diagnostics.Debug.Assert(_disposed == false);

            InventoryLocker.Hold();

            try
            {
                if (count == 0)
                {
                    return [];
                }

                if (Conn != null)
                {
                    return Conn.Window.TakeItemStacks(Inventory, item, count);
                }
                else
                {
                    return Inventory.TakeItemStacks(item, count);
                }
            }
            finally
            {
                InventoryLocker.Release();
            }

        }

        public ItemStack[] GiveAndTakeItemStacks(
            IReadOnlyItem giveItem, int giveCount,
            IReadOnlyItem takeItem, int takeCount)
        {
            if (giveItem == null)
            {
                throw new System.ArgumentNullException(nameof(giveItem));
            }

            if (giveCount < 0)
            {
                throw new System.ArgumentOutOfRangeException(nameof(giveCount));
            }

            if (takeItem == null)
            {
                throw new System.ArgumentNullException(nameof(takeItem));
            }

            if (takeCount < 0)
            {
                throw new System.ArgumentOutOfRangeException(nameof(takeCount));
            }

            if (_disposed == true)
            {
                throw new System.ObjectDisposedException(GetType().Name);
            }

            System.Diagnostics.Debug.Assert(_disposed == false);

            InventoryLocker.Hold();

            try
            {


                if (Conn != null)
                {
                    return Conn.Window.GiveAndTakeItemStacks(Inventory,
                        giveItem, giveCount, takeItem, takeCount);
                }
                else
                {
                    return Inventory.GiveAndTakeItemStacks(
                        giveItem, giveCount, takeItem, takeCount);
                }
            }
            finally
            {
                InventoryLocker.Release();
            }

        }

        public void FlushItems()
        {
            System.Diagnostics.Debug.Assert(_disposed == false);

            InventoryLocker.Hold();

            try
            {
                if (Conn != null)
                {
                    Conn.Window.FlushItems(Inventory);
                }
                else
                {
                    Inventory.FlushItems();
                }
            }
            finally
            {
                InventoryLocker.Release();
            }

        }

        public void WriteMessageInChatBox(params TextComponent[] components)
        {
            if (_disposed == true)
            {
                throw new System.ObjectDisposedException(GetType().Name);
            }

            if (Connected == false)
            {
                return;
            }

            if (components == null)
            {
                components = [];
            }

            var extra = new object[components.Length];

            for (int i = 0; i < components.Length; ++i)
            {
                TextComponent component = components[i];

                extra[i] = new
                {
                    text = component.Text,
                    color = component.Color.GetName(),
                };
            }

            var chat = new
            {
                text = "",
                extra,
            };

            string data = System.Text.Json.JsonSerializer.Serialize(chat);

            Conn.OutPackets.Enqueue(new ClientboundChatmessagePacket(data, 0x00));
        }

        internal override void _Animate(EntityAnimation animation)
        {
            System.Diagnostics.Debug.Assert(_disposed == false);

            base._Animate(animation);

            if (Connected == true)
            {
                Conn.Animate(Id, animation);
            }
        }

        internal void UpdateEquipmentsData()
        {
            //System.Diagnostics.Debug.Assert(equipmentsData.helmet != null);
            //System.Diagnostics.Debug.Assert(equipmentsData.mainHand != null);
            //System.Diagnostics.Debug.Assert(equipmentsData.offHand != null);

            System.Diagnostics.Debug.Assert(_disposed == false);

            System.Diagnostics.Debug.Assert(Renderers != null);
            if (Renderers.Empty == true)
            {
                return;
            }

            using MinecraftProtocolDataStream stream = new();

            Inventory.WriteMainHandData(stream);

            byte[] mainHand = stream.ReadData();

            Inventory.WriteOffHandData(stream);

            byte[] offHand = stream.ReadData();

            Inventory.WriteHelmetData(stream);

            byte[] helmet = stream.ReadData();

            System.Diagnostics.Debug.Assert(Renderers != null);
            foreach (EntityRenderer renderer in Renderers.GetKeys())
            {
                System.Diagnostics.Debug.Assert(renderer != null);
                renderer.SetEquipmentsData(Id, mainHand, offHand, helmet);
            }

        }

        public void LoadWorld(World world)
        {
            System.Diagnostics.Debug.Assert(_disposed == false);

            if (Disconnected == true)
            {
                return;
            }

            System.Diagnostics.Debug.Assert(Conn != null);
            Conn.LoadWorld(
                Id,
                world,
                Position,
                Blindness);
        }

        public void SendData()
        {
            System.Diagnostics.Debug.Assert(_disposed == false);

            if (Disconnected == true)
            {
                return;
            }

            System.Diagnostics.Debug.Assert(Conn != null);
            Conn.SendData();
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
                    if (Disconnected == false)
                    {
                        Conn.Dispose();
                    }

                    Inventory.Dispose();

                    LockerGamemode.Dispose();
                    InventoryLocker.Dispose();
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

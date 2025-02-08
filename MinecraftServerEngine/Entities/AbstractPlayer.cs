using Sync;

using MinecraftServerEngine.Protocols;
using MinecraftServerEngine.Text;
using MinecraftServerEngine.Inventories;
using MinecraftServerEngine.Items;
using MinecraftServerEngine.Renderers;
using MinecraftServerEngine.Physics;
using MinecraftServerEngine.Physics.BoundingVolumes;
using MinecraftServerEngine.Particles;

namespace MinecraftServerEngine.Entities
{

    public abstract class AbstractPlayer : LivingEntity
    {


        public const double HitboxWidth = 0.6D;

        public const double DefaultMass = 1.0D;
        public const double DefaultMaxStepLevel = 0.6;
        public const double DefaultEyeHeight = 1.62D;

       

        private bool _disposed = false;

        private readonly Locker _LockerInventory = new();
        internal readonly PlayerInventory Inventory = new();


        public readonly UserId UserId;
        public readonly string Username;

        private Connection _Connection;
        public bool IsDisconnected => _Connection == null;
        public bool IsConnected => _Connection != null;


        private Vector _pControl;


        private Locker _LockerGamemode = new();
        private Gamemode _newGamemode, _gamemode;
        public Gamemode Gamemode => _newGamemode;


        private float _experienceBarRatio = 0.0F;
        private int _experienceLevel = 0;
        public float ExperienceBarRatio => _experienceBarRatio;
        public int ExperienceLevel => _experienceLevel;


        private bool _blindness = false;

        public bool IsBlindness => _blindness;

        private static EntityHitbox GetAdventureHitbox(bool sneaking)
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

            return new EntityHitbox(w, h);
        }

        private static EntityHitbox GetSpectatorHitbox()
        {
            return EntityHitbox.Empty;
        }


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

            System.Diagnostics.Debug.Assert(IsSneaking == false);
            System.Diagnostics.Debug.Assert(IsSprinting == false);

            _newGamemode = gamemode;
            _gamemode = gamemode;
        }

        ~AbstractPlayer()
        {
            System.Diagnostics.Debug.Assert(false);

            Dispose(false);
        }


        private protected override EntityHitbox GetHitbox()
        {
            System.Diagnostics.Debug.Assert(_disposed == false);

            return _newGamemode == Gamemode.Spectator ?
                GetSpectatorHitbox() : GetAdventureHitbox(false);
        }

        public override double GetEyeHeight()
        {
            double value = DefaultEyeHeight;

            if (IsSneaking == true)
            {
                value += -0.08D;
            }

            return value;
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

            if (IsConnected == true)
            {
                System.Diagnostics.Debug.Assert(_Connection != null);
                _Connection.PlaySound(name, category, p, volume, pitch);
            }
        }

        public virtual void OnRespawn()
        {

        }

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

                if (IsConnected == true)
                {
                    System.Diagnostics.Debug.Assert(AdditionalHealth >= 0.0);
                    System.Diagnostics.Debug.Assert(Health >= 0.0);
                    _Connection.UpdateAdditionalHealth(Id, AdditionalHealth);
                    _Connection.UpdateHealth(Health);
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

                if (IsConnected == true)
                {
                    System.Diagnostics.Debug.Assert(Health >= 0.0);
                    _Connection.UpdateHealth(Health);
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

                if (IsConnected == true)
                {
                    System.Diagnostics.Debug.Assert(MaxHealth >= 0.0);
                    System.Diagnostics.Debug.Assert(Health >= 0.0);
                    _Connection.UpdateMaxHealth(Id, MaxHealth);
                    _Connection.UpdateHealth(Health);
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

                if (IsConnected == true)
                {
                    System.Diagnostics.Debug.Assert(AdditionalHealth >= 0.0);
                    _Connection.UpdateAdditionalHealth(Id, AdditionalHealth);
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

            System.Diagnostics.Debug.Assert(_LockerMovementSpeed != null);
            _LockerMovementSpeed.Hold();

            try
            {
                base._SetMovementSpeed(amount);

                if (IsConnected == true)
                {
                    System.Diagnostics.Debug.Assert(MovementSpeed >= 0.0);
                    System.Diagnostics.Debug.Assert(MovementSpeed <= 1024.0);
                    _Connection.UpdateMovementSpeed(Id, MovementSpeed);
                }
            }
            finally
            {
                System.Diagnostics.Debug.Assert(_LockerMovementSpeed != null);
                _LockerMovementSpeed.Release();
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

            if (IsConnected == true)
            {
                _Connection.EmitParticles(particle, v, (float)speed, count, (float)r, (float)g, (float)b);
            }
        }

        internal override void _AddEffect(
            byte effectId,
            byte amplifier, int duration, byte flags)
        {
            System.Diagnostics.Debug.Assert(_disposed == false);

            base._AddEffect(effectId, amplifier, duration, flags);

            if (IsConnected)
            {
                _Connection.AddEffect(Id, effectId, amplifier, duration, flags);
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

            _Connection.SetExperience(ratio, level);
        }

        private protected override void RenderSpawning(EntityRenderer renderer)
        {
            System.Diagnostics.Debug.Assert(renderer != null);

            System.Diagnostics.Debug.Assert(_disposed == false);

            System.Diagnostics.Debug.Assert(_gamemode == Gamemode.Adventure);
            renderer.SpawnPlayer(
                Id, UniqueId,
                Position, Look,
                IsSneaking, IsSprinting);

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

        public void SwitchGamemode(Gamemode gamemode)
        {
            System.Diagnostics.Debug.Assert(_disposed == false);

            _LockerGamemode.Hold();

            if (_newGamemode != gamemode)
            {
                _newGamemode = gamemode;
            }

            _LockerGamemode.Release();
        }

        public void ApplyBilndness(bool f)
        {
            System.Diagnostics.Debug.Assert(_disposed == false);

            _blindness = f;

            if (IsConnected == true)
            {
                System.Diagnostics.Debug.Assert(_Connection != null);
                _Connection.ApplyBilndness(_blindness);
            }
        }

        private protected override void _ApplyForce(Vector force)
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

            if (IsConnected == true)
            {
                System.Diagnostics.Debug.Assert(_Connection != null);
                Vector v = force / Mass;
                _Connection.ApplyVelocity(Id, v);
            }

            base._ApplyForce(force);
        }

        internal override void Move(PhysicsWorld world, BoundingVolume volume, Vector v)
        {
            System.Diagnostics.Debug.Assert(world != null);
            System.Diagnostics.Debug.Assert(_disposed == false);

            System.Diagnostics.Debug.Assert(volume != null);

            if (IsConnected == true)
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
                    _Connection.SetGamemode(Id, _newGamemode);
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
                if (IsConnected == true)
                {
                    _pControl = p;

                    System.Diagnostics.Debug.Assert(_Connection != null);
                    _Connection.Teleport(p, look);
                }

                base.Teleport(p, look);


            }
            finally
            {
                System.Diagnostics.Debug.Assert(LockerTeleport != null);
                LockerTeleport.Release();
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

        internal bool HandleDisconnection(out UserId userId, World world)
        {
            System.Diagnostics.Debug.Assert(_disposed == false);

            System.Diagnostics.Debug.Assert(world != null);

            if (IsConnected == true && _Connection.IsDisconnected == true)
            {
                _Connection.Flush(out userId, world, Inventory);
                _Connection.Dispose();

                _Connection = null;

                return true;
            }

            userId = UserId.Null;
            return false;
        }

        public bool OpenInventory(SharedInventory sharedInventory)
        {
            if (sharedInventory == null)
            {
                throw new System.ArgumentNullException(nameof(sharedInventory));
            }

            if (_disposed == true)
            {
                throw new System.ObjectDisposedException(GetType().Name);
            }


            System.Diagnostics.Debug.Assert(_disposed == false);

            System.Diagnostics.Debug.Assert(sharedInventory != null);

            System.Diagnostics.Debug.Assert(_LockerInventory != null);
            _LockerInventory.Hold();

            try
            {

                if (IsDisconnected == true)
                {
                    return false;
                }

                System.Diagnostics.Debug.Assert(_Connection != null);
                return _Connection.Open(Inventory, sharedInventory);

            }
            finally
            {
                System.Diagnostics.Debug.Assert(_LockerInventory != null);
                _LockerInventory.Release();
            }
        }

        public void SetHelmet(ItemStack itemStack)
        {
            if (_disposed == true)
            {
                throw new System.ObjectDisposedException(GetType().Name);
            }

            System.Diagnostics.Debug.Assert(_disposed == false);

            System.Diagnostics.Debug.Assert(_LockerInventory != null);
            _LockerInventory.Hold();

            try
            {
                if (IsConnected == true)
                {
                    System.Diagnostics.Debug.Assert(Inventory != null);
                    System.Diagnostics.Debug.Assert(_Connection.Window != null);
                    _Connection.Window.SetHelmet(Inventory, itemStack);
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
                System.Diagnostics.Debug.Assert(_LockerInventory != null);
                _LockerInventory.Release();
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

            System.Diagnostics.Debug.Assert(_LockerInventory != null);
            _LockerInventory.Hold();

            try
            {
                if (IsConnected == true)
                {
                    System.Diagnostics.Debug.Assert(Inventory != null);
                    System.Diagnostics.Debug.Assert(_Connection.Window != null);
                    return _Connection.Window.GiveItemStacks(Inventory, item, count);
                }
                else
                {
                    System.Diagnostics.Debug.Assert(Inventory != null);
                    return Inventory.GiveItemStacks(item, count);
                }
            }
            finally
            {
                System.Diagnostics.Debug.Assert(_LockerInventory != null);
                _LockerInventory.Release();
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

            System.Diagnostics.Debug.Assert(_LockerInventory != null);
            _LockerInventory.Hold();

            try
            {
                if (IsConnected == true)
                {
                    System.Diagnostics.Debug.Assert(Inventory != null);
                    System.Diagnostics.Debug.Assert(_Connection.Window != null);
                    return _Connection.Window.GiveItemStack(Inventory, ref itemStack);
                }
                else
                {
                    System.Diagnostics.Debug.Assert(Inventory != null);
                    return Inventory.GiveItemStack(ref itemStack);
                }
            }
            finally
            {
                System.Diagnostics.Debug.Assert(_LockerInventory != null);
                _LockerInventory.Release();
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

            _LockerInventory.Hold();

            try
            {
                if (count == 0)
                {
                    return [];
                }

                if (IsConnected == true)
                {
                    return _Connection.Window.TakeItemStacks(Inventory, item, count);
                }
                else
                {
                    return Inventory.TakeItemStacks(item, count);
                }
            }
            finally
            {
                _LockerInventory.Release();
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

            _LockerInventory.Hold();

            try
            {


                if (IsConnected == true)
                {
                    return _Connection.Window.GiveAndTakeItemStacks(Inventory,
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
                _LockerInventory.Release();
            }

        }

        public ItemStack CloseInventory()
        {
            if (_disposed == true)
            {
                throw new System.ObjectDisposedException(GetType().Name);
            }

            System.Diagnostics.Debug.Assert(_LockerInventory != null);
            _LockerInventory.Hold();

            try
            {
                if (_Connection == null)
                {
                    return null;
                }

                System.Diagnostics.Debug.Assert(Inventory != null);
                System.Diagnostics.Debug.Assert(_Connection.Window != null);
                return _Connection.Window.Close(Inventory);
            }
            finally
            {
                System.Diagnostics.Debug.Assert(_LockerInventory != null);
                _LockerInventory.Release();
            }
        }

        public void FlushItems()
        {
            System.Diagnostics.Debug.Assert(_disposed == false);

            _LockerInventory.Hold();

            try
            {
                if (IsConnected == true)
                {
                    _Connection.Window.FlushItems(Inventory);
                }
                else
                {
                    Inventory.FlushItems();
                }
            }
            finally
            {
                _LockerInventory.Release();
            }

        }

        public void WriteMessageInChatBox(params TextComponent[] components)
        {
            if (_disposed == true)
            {
                throw new System.ObjectDisposedException(GetType().Name);
            }

            if (IsConnected == false)
            {
                return;
            }

            if (components == null)
            {
                components = [];
            }

            string data = TextComponent.GenerateJsonString(components);

            _Connection.OutPackets.Enqueue(new ClientboundChatmessagePacket(data, 0x00));
        }

        internal override void _Animate(EntityAnimation animation)
        {
            System.Diagnostics.Debug.Assert(_disposed == false);

            base._Animate(animation);

            if (IsConnected == true)
            {
                _Connection.Animate(Id, animation);
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

        internal void Connect(MinecraftClient client, World world, UserId id)
        {
            System.Diagnostics.Debug.Assert(_disposed == false);

            System.Diagnostics.Debug.Assert(client != null);
            System.Diagnostics.Debug.Assert(world != null);
            System.Diagnostics.Debug.Assert(id != UserId.Null);

            _pControl = Position;

            _Connection = new Connection(
                UserId, Username,
                client,
                world,
                Id,
                AdditionalHealth, MaxHealth, Health,
                MovementSpeed,
                Position, Look,
                IsBlindness,
                Inventory,
                _gamemode);

            SetExperience(ExperienceBarRatio, ExperienceLevel);
        }

        internal void ControlMovement(Vector p)
        {
            System.Diagnostics.Debug.Assert(_disposed == false);

            _pControl = p;
        }

        internal void Control(World world)
        {
            System.Diagnostics.Debug.Assert(_disposed == false);

            if (IsConnected == true)
            {
                System.Diagnostics.Debug.Assert(_Connection != null);
                _Connection.Control(world, this);
            }

        }

        public void LoadWorld(World world)
        {
            System.Diagnostics.Debug.Assert(_disposed == false);

            if (IsDisconnected == true)
            {
                return;
            }

            System.Diagnostics.Debug.Assert(_Connection != null);
            _Connection.LoadWorld(
                Id,
                world,
                Position,
                IsBlindness);
        }

        public void SendData()
        {
            System.Diagnostics.Debug.Assert(_disposed == false);

            if (IsDisconnected == true)
            {
                return;
            }

            System.Diagnostics.Debug.Assert(_Connection != null);
            _Connection.SendData();
        }

        internal override void Flush(PhysicsWorld _world)
        {
            System.Diagnostics.Debug.Assert(_world != null);

            System.Diagnostics.Debug.Assert(_disposed == false);

            base.Flush(_world);
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
                    if (IsDisconnected == false)
                    {
                        _Connection.Dispose();
                    }

                    _LockerInventory.Dispose();
                    Inventory.Dispose();

                    _LockerGamemode.Dispose();
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

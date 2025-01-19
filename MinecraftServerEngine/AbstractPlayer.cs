using Sync;

namespace MinecraftServerEngine
{
    using PhysicsEngine;

    public abstract class AbstractPlayer : LivingEntity
    {
        private static Hitbox GetAdventureHitbox(bool sneaking)
        {
            double w = 0.6D, h;
            if (sneaking)
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


        protected readonly PlayerInventory Inventory = new();


        private Connection Conn;
        public bool Disconnected => (Conn == null);
        public bool Connected => !Disconnected;


        private Vector _pControl;


        private Locker LockerGamemode = new();
        private Gamemode _nextGamemode, _gamemode;
        public Gamemode Gamemode => _gamemode;


        protected AbstractPlayer(UserId id, Vector p, Angles look, Gamemode gamemode)
            : base(
                  id.Data,
                  p, look,
                  false,  // noGravity
                  gamemode == Gamemode.Spectator ? GetSpectatorHitbox() : GetAdventureHitbox(false),
                  DefaultMass, DefaultMaxStepLevel)
        {
            System.Diagnostics.Debug.Assert(id != UserId.Null);

            System.Diagnostics.Debug.Assert(!Sneaking);
            System.Diagnostics.Debug.Assert(!Sprinting);

            _nextGamemode = gamemode;
            _gamemode = gamemode;
        }

        ~AbstractPlayer()
        {
            System.Diagnostics.Debug.Assert(false);

            Dispose(false);
        }

        internal void Respawn()
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(Gamemode != Gamemode.Spectator);

            if (Connected)
            {
                Conn.UpdateHealth(MaxHealth);
            }

            _nextGamemode = Gamemode.Spectator;
            _health = MaxHealth;
        }

        public override void Damage(float amount)
        {
            System.Diagnostics.Debug.Assert(amount >= 0.0D);

            if (amount == 0.0D)
            {
                return;
            }

            if (Gamemode == Gamemode.Spectator)
            {
                return;
            }

            LockerHealth.Hold();

            base.Damage(amount);

            if (Connected && Health > 0.0F)
            {
                Conn.UpdateHealth(_health);
            }

            LockerHealth.Release();
        }

        private protected override Hitbox GetHitbox()
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            return (_nextGamemode == Gamemode.Spectator) ?
                GetSpectatorHitbox() : GetAdventureHitbox(false);
        }

        public override double GetEyeHeight()
        {
            double value = DefaultEyeHeight;

            if (Sneaking == true)
            {
                value = -0.08D;
            }

            return value;
        }

        private protected override void RenderSpawning(EntityRenderer renderer)
        {
            System.Diagnostics.Debug.Assert(renderer != null);

            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(_gamemode == Gamemode.Adventure);
            renderer.SpawnPlayer(
                Id, UniqueId,
                Position, Look,
                Sneaking, Sprinting,
                Inventory.GetEquipmentsData());
        }

        internal void Connect(Client client, World world, UserId id)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(client != null);
            System.Diagnostics.Debug.Assert(world != null);
            System.Diagnostics.Debug.Assert(id != UserId.Null);

            _pControl = Position;

            Conn = new Connection(
                id, client,
                world,
                Id,
                Health,
                Position, Look,
                Inventory,
                _gamemode);
        }

        public void SwitchGamemode(Gamemode gamemode)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            LockerGamemode.Hold();

            if (_nextGamemode != gamemode)
            {
                _nextGamemode = gamemode;
            }

            LockerGamemode.Release();
        }

        public override void ApplyForce(Vector force)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            if (Connected)
            {
                System.Diagnostics.Debug.Assert(Conn != null);
                Vector v = force / Mass;
                Conn.ApplyVelocity(Id, v);
            }

            base.ApplyForce(force);
        }

        internal override void Move(BoundingVolume volume, Vector v)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(volume != null);

            if (Connected)
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

                if (_gamemode != _nextGamemode)
                {
                    Conn.Set(Id, _nextGamemode);
                }
            }

            _gamemode = _nextGamemode;

            base.Move(volume, v);
        }

        public override void Teleport(Vector p, Angles look)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            LockerTeleport.Hold();

            if (Connected)
            {
                _pControl = p;

                Conn.Teleport(p, look);
            }

            base.Teleport(p, look);

            LockerTeleport.Release();
        }

        internal void ControlMovement(Vector p)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            _pControl = p;
        }

        internal void Control(World world)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            if (Connected)
            {
                System.Diagnostics.Debug.Assert(Conn != null);

                Conn.Control(world, this, Inventory);
            }

        }

        public bool HandleDisconnection(out UserId id, World world)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(world != null);

            if (Disconnected)
            {
                id = UserId.Null;
                return false;
            }

            System.Diagnostics.Debug.Assert(Conn != null);
            if (Conn.Disconnected)
            {
                Conn.Flush(out id, world, Inventory);
                Conn.Dispose();

                Conn = null;

                return true;
            }

            id = UserId.Null;
            return false;
        }

        public void LoadAndSendData(World world)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            if (Disconnected)
            {
                return;
            }

            System.Diagnostics.Debug.Assert(Conn != null);
            Conn.LoadAndSendData(
                world,
                Id,
                Position, Look);
        }

        public bool OpenInventory(SharedInventory sharedInventory)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(sharedInventory != null);

            if (Disconnected == true)
            {
                return false;
            }

            System.Diagnostics.Debug.Assert(Conn != null);
            return Conn.Open(Inventory, sharedInventory);
        }

        public void GiveItem(ItemStack stack)
        {
            System.Diagnostics.Debug.Assert(_disposed == false);

            if (stack == null)
            {
                return;
            }

            if (Conn != null)
            {
                Conn.Window.GiveItem(Inventory, stack);
            } else
            {
                Inventory.GiveItem(stack);
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
                    if (Disconnected == false)
                    {
                        Conn.Dispose();
                    }

                    Inventory.Dispose();

                    LockerGamemode.Dispose();
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

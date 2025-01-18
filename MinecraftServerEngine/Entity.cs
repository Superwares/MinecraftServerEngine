
using Common;
using Sync;
using Containers;


namespace MinecraftServerEngine
{
    using PhysicsEngine;

    public abstract class Entity : PhysicsObject
    {
        private protected readonly struct Hitbox : System.IEquatable<Hitbox>
        {
            internal static Hitbox Empty = new(0.0D, 0.0D);

            private readonly double Width, Height;
            /*public readonly double EyeHeight, MaxStepHeight;*/

            internal Hitbox(double w, double h)
            {
                System.Diagnostics.Debug.Assert(w >= 0.0D);
                System.Diagnostics.Debug.Assert(h >= 0.0D);

                Width = w;
                Height = h;
            }

            internal readonly BoundingVolume Convert(Vector p)
            {
                if (Width == 0.0D || Height == 0.0D)
                {
                    System.Diagnostics.Debug.Assert(Width == 0.0D);
                    System.Diagnostics.Debug.Assert(Height == 0.0D);

                    return new EmptyBoundingVolume(p);
                }

                System.Diagnostics.Debug.Assert(Width > 0.0D);
                System.Diagnostics.Debug.Assert(Height > 0.0D);

                double w = Width / 2.0D;

                Vector max = new(p.X + w, p.Y + Height, p.Z + w),
                       min = new(p.X - w, p.Y, p.Z - w);
                return new AxisAlignedBoundingBox(max, min);
            }

            public readonly override string ToString()
            {
                throw new System.NotImplementedException();
            }

            public readonly bool Equals(Hitbox other)
            {
                throw new System.NotImplementedException();
            }
        }

        private bool _disposed = false;


        internal readonly int Id;
        internal readonly System.Guid UniqueId;

        private bool _hasMovement = false;
        private bool _noRendering = false;

        private readonly ConcurrentTree<EntityRenderer> Renderers = new();  // Disposable


        private Vector _p;
        internal Vector Position => _p;


        private readonly Locker LockerRotate = new();
        private bool _rotated = false;
        private Look _look;
        internal Look Look => _look;


        protected bool _sneaking, _sprinting = false;
        public bool Sneaking => _sneaking;
        public bool Sprinting => _sprinting;


        protected readonly Locker LockerTeleport = new();
        private bool _teleported = false;
        private Vector _pTeleport;

        //private bool _fakeBlockApplied = false;
        //private Block _fakeBlock;

        // ApplyBlockAppearance
        // TransformAppearance

        private readonly bool NoGravity;




        private protected Entity(
            System.Guid uniqueId,
            Vector p, Look look,
            bool noGravity,
            Hitbox hitbox,
            double m, double maxStepHeight)
            : base(m, hitbox.Convert(p), new StepableMovement(maxStepHeight))
        {
            Id = EntityIdAllocator.Alloc();
            UniqueId = uniqueId;

            _p = p;

            System.Diagnostics.Debug.Assert(!_rotated);
            _look = look;

            System.Diagnostics.Debug.Assert(!_sneaking);
            System.Diagnostics.Debug.Assert(!_sprinting);

            NoGravity = noGravity;
        }

        ~Entity()
        {
            System.Diagnostics.Debug.Assert(false);
            //Dispose(false);
        }

        private protected abstract Hitbox GetHitbox();

        private protected abstract void RenderSpawning(EntityRenderer renderer);

        protected abstract void OnSneak(World world, bool f);
        protected abstract void OnSprint(World world, bool f);

        protected internal abstract void OnAttack(World world);
        protected internal abstract void OnAttack(World world, ItemStack stack);
        protected internal abstract void OnUseItem(World world, ItemStack stack);
        protected internal abstract void OnUseEntity(World world, Entity entity);

        protected (Vector, Vector) GetRay()
        {
            throw new System.NotImplementedException();
            /*return (origin, dir);*/
        }

        internal void ApplyRenderer(EntityRenderer renderer)
        {
            System.Diagnostics.Debug.Assert(renderer != null);

            System.Diagnostics.Debug.Assert(!_disposed);

            if (_noRendering)
            {
                System.Diagnostics.Debug.Assert(Renderers.Empty);
                return;
            }

            if (Renderers.Contains(renderer))
            {
                return;
            }

            System.Diagnostics.Debug.Assert(Renderers != null);
            Renderers.Insert(renderer);

            RenderSpawning(renderer);
        }

        protected override (BoundingVolume, bool noGravity) GetCurrentStatus()
        {
            Hitbox hitbox = GetHitbox();

            Forces.Enqueue(
                -1.0D *
                new Vector(1.0D - 0.91D, 1.0D - 0.9800000190734863D, 1.0D - 0.91D) *
                Velocity);  // Damping Force

            BoundingVolume volume = _teleported ? hitbox.Convert(_pTeleport) : hitbox.Convert(_p);
            return (volume, NoGravity || volume is EmptyBoundingVolume);
        }

        private bool HandleRendering(
                BoundingVolume volume, out Vector p)
        {
            System.Diagnostics.Debug.Assert(!_disposed);


            p = volume.GetBottomCenter();

            if (volume is not EmptyBoundingVolume)
            {
                _noRendering = false;

                System.Diagnostics.Debug.Assert(!_hasMovement);

                using Queue<EntityRenderer> queue = new();

                System.Diagnostics.Debug.Assert(Renderers != null);
                foreach (EntityRenderer renderer in Renderers.GetKeys())
                {
                    System.Diagnostics.Debug.Assert(renderer != null);

                    if (renderer.CanRender(p))
                    {
                        continue;
                    }

                    queue.Enqueue(renderer);
                }

                while (!queue.Empty)
                {
                    EntityRenderer renderer = queue.Dequeue();

                    if (!renderer.Disconnected)
                    {
                        renderer.DestroyEntity(Id);
                    }
                    Renderers.Extract(renderer);
                }

                System.Diagnostics.Debug.Assert(_hasMovement == false);
                _hasMovement = true;
                return true;
            }
            else
            {
                _noRendering = true;

                EntityRenderer[] renderers = Renderers.Flush();
                foreach (var renderer in renderers)
                {
                    if (renderer.Disconnected)
                    {
                        continue;
                    }

                    renderer.DestroyEntity(Id);
                }

                System.Diagnostics.Debug.Assert(_hasMovement == false);
                _hasMovement = false;
                return false;
            }

        }

        internal override void Move(BoundingVolume volume, Vector v)
        {
            System.Diagnostics.Debug.Assert(volume != null);

            System.Diagnostics.Debug.Assert(_disposed == false);


            bool hasMovementRendering = HandleRendering(
                volume, out Vector p);

            if (hasMovementRendering == true)
            {
                if (_teleported)
                {
                    System.Diagnostics.Debug.Assert(_noRendering == false || (_noRendering == true && Renderers.Empty == true));
                    if (_noRendering == false)
                    {
                        System.Diagnostics.Debug.Assert(Renderers != null);
                        foreach (EntityRenderer renderer in Renderers.GetKeys())
                        {
                            System.Diagnostics.Debug.Assert(renderer != null);
                            renderer.Teleport(Id, _pTeleport, _look, false);
                        }
                    }

                    _p = _pTeleport;

                    _rotated = false;
                    _teleported = false;
                }

                System.Diagnostics.Debug.Assert(_hasMovement == true);
                System.Diagnostics.Debug.Assert(_noRendering == false);

                bool moved = !p.Equals(_p);  // TODO: Compare with machine epsilon.
                if (moved && _rotated)
                {
                    System.Diagnostics.Debug.Assert(_noRendering == false || (_noRendering == true && Renderers.Empty == true));
                    if (_noRendering == false)
                    {
                        System.Diagnostics.Debug.Assert(Renderers != null);
                        foreach (EntityRenderer renderer in Renderers.GetKeys())
                        {
                            System.Diagnostics.Debug.Assert(renderer != null);
                            renderer.RelMoveAndRotate(Id, p, _p, _look);
                        }
                    }
                }
                else if (moved)
                {
                    System.Diagnostics.Debug.Assert(!_rotated);

                    System.Diagnostics.Debug.Assert(_noRendering == false || (_noRendering == true && Renderers.Empty == true));
                    if (_noRendering == false)
                    {
                        System.Diagnostics.Debug.Assert(Renderers != null);
                        foreach (EntityRenderer renderer in Renderers.GetKeys())
                        {
                            System.Diagnostics.Debug.Assert(renderer != null);
                            renderer.RelMove(Id, p, _p);
                        }
                    }
                }
                else if (_rotated)
                {
                    System.Diagnostics.Debug.Assert(!moved);

                    System.Diagnostics.Debug.Assert(_noRendering == false || (_noRendering == true && Renderers.Empty == true));
                    if (_noRendering == false)
                    {
                        System.Diagnostics.Debug.Assert(Renderers != null);
                        foreach (EntityRenderer renderer in Renderers.GetKeys())
                        {
                            System.Diagnostics.Debug.Assert(renderer != null);
                            renderer.Rotate(Id, _look);
                        }
                    }

                }
                else
                {
                    System.Diagnostics.Debug.Assert(!moved);
                    System.Diagnostics.Debug.Assert(!_rotated);

                    System.Diagnostics.Debug.Assert(_noRendering == false || (_noRendering == true && Renderers.Empty == true));
                    if (_noRendering == false)
                    {
                        System.Diagnostics.Debug.Assert(Renderers != null);
                        foreach (EntityRenderer renderer in Renderers.GetKeys())
                        {
                            System.Diagnostics.Debug.Assert(renderer != null);
                            renderer.Stand(Id);
                        }
                    }
                }

                _hasMovement = false;

            }
            else if (_teleported == true)
            {
                _p = _pTeleport;

                _rotated = false;
                _teleported = false;
            }

            _p = p;
            _rotated = false;

            base.Move(volume, v);
        }

        internal void SetEntityStatus(byte v)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            if (_noRendering)
            {
                System.Diagnostics.Debug.Assert(Renderers.Empty);
                return;
            }

            System.Diagnostics.Debug.Assert(Renderers != null);
            foreach (EntityRenderer renderer in Renderers.GetKeys())
            {
                System.Diagnostics.Debug.Assert(renderer != null);
                renderer.SetEntityStatus(Id, v);
            }
        }

        public virtual void Teleport(Vector p, Look look)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            LockerTeleport.Hold();

            _teleported = true;
            _pTeleport = p;

            Rotate(look);

            LockerTeleport.Release();
        }

        internal void Rotate(Look look)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            LockerRotate.Hold();

            _rotated = true;
            _look = look;

            LockerRotate.Release();
        }

        private void ChangeForms(bool sneaking, bool sprinting)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(_noRendering == false || (_noRendering == true && Renderers.Empty == true));
            if (_noRendering == false)
            {
                System.Diagnostics.Debug.Assert(Renderers != null);
                foreach (EntityRenderer renderer in Renderers.GetKeys())
                {
                    System.Diagnostics.Debug.Assert(renderer != null);
                    renderer.ChangeForms(Id, sneaking, sprinting);
                }
            }
        }

        internal void Sneak(World world)
        {
            System.Diagnostics.Debug.Assert(world != null);

            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(!Sneaking);

            _sneaking = true;

            OnSneak(world, _sneaking);

            ChangeForms(_sneaking, _sprinting);
        }

        internal void Unsneak(World world)
        {
            System.Diagnostics.Debug.Assert(world != null);

            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(Sneaking);

            _sneaking = false;

            OnSneak(world, _sneaking);

            ChangeForms(_sneaking, _sprinting);
        }

        internal void Sprint(World world)
        {
            System.Diagnostics.Debug.Assert(world != null);

            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(!Sprinting);

            _sprinting = true;

            OnSprint(world, _sprinting);

            ChangeForms(_sneaking, _sprinting);
        }

        internal void Unsprint(World world)
        {
            System.Diagnostics.Debug.Assert(world != null);

            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(Sprinting);

            _sprinting = false;

            OnSprint(world, _sprinting);

            ChangeForms(_sneaking, _sprinting);
        }

        internal void UpdateEntityEquipmentsData((
            byte[] mainHand, byte[] offHand) equipmentsData)
        {
            System.Diagnostics.Debug.Assert(equipmentsData.mainHand != null);
            System.Diagnostics.Debug.Assert(equipmentsData.offHand != null);

            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(_noRendering == false || (_noRendering == true && Renderers.Empty == true));
            if (_noRendering == false)
            {
                System.Diagnostics.Debug.Assert(Renderers != null);
                foreach (EntityRenderer renderer in Renderers.GetKeys())
                {
                    System.Diagnostics.Debug.Assert(renderer != null);
                    renderer.SetEquipmentsData(Id, equipmentsData);
                }
            }

        }

        protected void EmitParticles(Particles particle, int count)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            throw new System.NotImplementedException();
        }

        //public void ApplyBlockAppearance(Block block)
        //{
        //    System.Diagnostics.Debug.Assert(_disposed == false);

        //    _fakeBlockApplied = true;
        //    _fakeBlock = block;

        //    _fakeBlockChanged = true;
        //}

        internal override void Flush()
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(_noRendering == false || (_noRendering == true && Renderers.Empty == true));
            if (_noRendering == false)
            {
                System.Diagnostics.Debug.Assert(Renderers != null);
                EntityRenderer[] renderers = Renderers.Flush();
                foreach (EntityRenderer renderer in renderers)
                {
                    System.Diagnostics.Debug.Assert(renderer != null);

                    if (renderer.Disconnected)
                    {
                        continue;
                    }

                    renderer.DestroyEntity(Id);
                }
            }



            base.Flush();
        }

        public override void Dispose()
        {
            // Assertion
            System.Diagnostics.Debug.Assert(!_disposed);
            System.Diagnostics.Debug.Assert(!_hasMovement);

            // Release resources.
            EntityIdAllocator.Dealloc(Id);
            Renderers.Dispose();

            LockerRotate.Dispose();
            LockerTeleport.Dispose();

            // Finish.
            base.Dispose();
            _disposed = true;
        }

    }

    public abstract class BlockEntity : Entity
    {
        private static readonly Hitbox DefaultHitbox = new(1.0D, 1.0D);
        public const double DefaultMass = 1.0D;
        public const double DefaultMaxStepLevel = 0.0D;


        private bool _disposed = false;


        internal BlockEntity(Vector p, Look look, bool noGravity) :
            base(System.Guid.NewGuid(), p, look, noGravity,
                DefaultHitbox,
                DefaultMass, DefaultMaxStepLevel)
        {
        }

        public override void Dispose()
        {
            // Assertions.
            System.Diagnostics.Debug.Assert(!_disposed);

            // Release resources.

            // Finish.
            base.Dispose();
            _disposed = true;
        }
    }

    public abstract class FixedBlockEntity : BlockEntity
    {
        private bool _disposed = false;



        public FixedBlockEntity(Vector p) : base(p, new Look(), true)
        {

        }

        private protected override void RenderSpawning(
            EntityRenderer renderer)
        {
            System.Diagnostics.Debug.Assert(renderer != null);

            throw new System.NotImplementedException();
        }

        public override void Dispose()
        {
            // Assertions.
            System.Diagnostics.Debug.Assert(!_disposed);

            // Release resources.

            // Finish.
            base.Dispose();
            _disposed = true;
        }

    }

    /*public sealed class MovableBlockEntity : BlockEntity
    {

    }*/

    public sealed class ItemEntity : Entity
    {
        private static readonly Hitbox DefaultHitbox = new(0.25D, 0.25D);
        public const double DefaultMass = 1.0D;
        public const double DefaultMaxStepLevel = 0.0D;


        private bool _disposed = false;


        private readonly ItemStack Stack;


        public ItemEntity(ItemStack stack, Vector p)
            : base(System.Guid.NewGuid(), p, new Look(0.0F, 0.0F), false,
                  DefaultHitbox,
                  DefaultMass, DefaultMaxStepLevel)
        {
            System.Diagnostics.Debug.Assert(stack != null);

            Stack = stack;
        }

        ~ItemEntity() => System.Diagnostics.Debug.Assert(false);

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

        protected internal override void OnAttack(World world)
        {
            System.Diagnostics.Debug.Assert(world != null);

            System.Diagnostics.Debug.Assert(!_disposed);

        }

        protected internal override void OnAttack(World world, ItemStack stack)
        {
            System.Diagnostics.Debug.Assert(world != null);
            System.Diagnostics.Debug.Assert(stack != null);

            System.Diagnostics.Debug.Assert(!_disposed);

        }

        protected internal override void OnUseItem(World world, ItemStack stack)
        {
            System.Diagnostics.Debug.Assert(world != null);
            System.Diagnostics.Debug.Assert(stack != null);

            System.Diagnostics.Debug.Assert(!_disposed);

        }

        protected internal override void OnUseEntity(World world, Entity entity)
        {
            System.Diagnostics.Debug.Assert(world != null);
            System.Diagnostics.Debug.Assert(entity != null);

            System.Diagnostics.Debug.Assert(!_disposed);

        }

        protected internal override void OnDeath(PhysicsWorld world)
        {
            System.Diagnostics.Debug.Assert(world != null);

            System.Diagnostics.Debug.Assert(!_disposed);

        }

        private protected override Hitbox GetHitbox()
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            return DefaultHitbox;
        }

        private protected override void RenderSpawning(EntityRenderer renderer)
        {
            System.Diagnostics.Debug.Assert(renderer != null);

            System.Diagnostics.Debug.Assert(!_disposed);

            renderer.SpawnItemEntity(
                Id, UniqueId,
                Position, Look,
                Stack);
        }

        public override void StartRoutine(PhysicsWorld world)
        {
            System.Diagnostics.Debug.Assert(world != null);
        }

        public void PickUp()
        {
            throw new System.NotImplementedException();
        }

        public override void Dispose()
        {
            // Assertions.
            System.Diagnostics.Debug.Assert(!_disposed);
            System.Diagnostics.Debug.Assert(Stack != null);

            // Release resources.

            // Finish.
            base.Dispose();
            _disposed = true;
        }

    }

    public abstract class LivingEntity : Entity
    {
        private bool _disposed = false;

        protected readonly Locker LockerHealth = new();
        protected float _maxHealth = 20.0F;
        protected float _health;
        public float MaxHealth => _maxHealth;
        public float Health => _health;

        private protected LivingEntity(
            System.Guid uniqueId,
            Vector p, Look look,
            bool noGravity,
            Hitbox hitbox,
            double m, double maxStepLevel)
            : base(uniqueId, p, look, noGravity, hitbox, m, maxStepLevel)
        {
            _health = _maxHealth;
        }

        ~LivingEntity()
        {
            System.Diagnostics.Debug.Assert(false);

            //Dispose(false);
        }

        protected internal override bool IsDead()
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            return _health <= 0.0D;
        }

        public virtual void Damage(float amount)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(amount >= 0.0D);
            System.Diagnostics.Debug.Assert(_health > 0.0D);
            System.Diagnostics.Debug.Assert(_health <= _maxHealth);

            LockerHealth.Hold();

            _health -= amount;

            SetEntityStatus(2);

            if (_health <= 0.0D)
            {
                SetEntityStatus(3);
            }

            LockerHealth.Release();
        }

        public override void Dispose()
        {
            // Assertions.
            System.Diagnostics.Debug.Assert(!_disposed);

            // Release resources.
            LockerHealth.Dispose();

            // Finish.
            base.Dispose();
            _disposed = true;
        }

    }

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


        private bool _disposed = false;


        protected readonly PlayerInventory Inventory = new();


        private Connection Conn;
        public bool Disconnected => (Conn == null);
        public bool Connected => !Disconnected;


        private Vector _pControl;


        private Locker LockerGamemode = new();
        private Gamemode _nextGamemode, _gamemode;
        public Gamemode Gamemode => _gamemode;


        protected AbstractPlayer(UserId id, Vector p, Look look, Gamemode gamemode)
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

        ~AbstractPlayer() => System.Diagnostics.Debug.Assert(false);

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

        public void Switch(Gamemode gamemode)
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

        public override void Teleport(Vector p, Look look)
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

        public bool Open(PublicInventory invPublic)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(invPublic != null);

            if (Disconnected)
            {
                return false;
            }

            System.Diagnostics.Debug.Assert(Conn != null);
            return Conn.Open(Inventory, invPublic);
        }

        public override void Dispose()
        {
            // Assertion.
            System.Diagnostics.Debug.Assert(!_disposed);

            // Release resources.
            if (!Disconnected)
            {
                Conn.Dispose();
            }

            Inventory.Dispose();

            LockerGamemode.Dispose();

            // Finish.
            base.Dispose();
            _disposed = true;
        }

    }

}

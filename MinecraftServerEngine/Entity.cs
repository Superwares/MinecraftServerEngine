
using Common;
using Containers;


namespace MinecraftServerEngine
{
    using PhysicsEngine;
    using Sync;

    public abstract class Entity : PhysicsObject
    {
        private sealed class RendererManager : System.IDisposable
        {
            private bool _disposed = false;

            private bool _movement = false;

            private readonly int _id;
            public int Id => _id;

            private readonly ConcurrentTree<EntityRenderer> Renderers = new();  // Disposable

            public RendererManager(int id)
            {
                _id = id;
            }

            ~RendererManager() => System.Diagnostics.Debug.Assert(false);

            public bool Apply(EntityRenderer renderer)
            {
                System.Diagnostics.Debug.Assert(renderer != null);

                System.Diagnostics.Debug.Assert(!_disposed);

                if (Renderers.Contains(renderer))
                {
                    return false;
                }

                System.Diagnostics.Debug.Assert(Renderers != null);
                Renderers.Insert(renderer);

                return true;
            }

            public void HandleRendering(Vector p)
            {
                System.Diagnostics.Debug.Assert(!_disposed);

                System.Diagnostics.Debug.Assert(!_movement);

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

            }

            public void MoveAndRotate(Vector p, Vector pPrev, Look look)
            {
                System.Diagnostics.Debug.Assert(!_disposed);

                System.Diagnostics.Debug.Assert(!_movement);

                System.Diagnostics.Debug.Assert(Renderers != null);
                foreach (EntityRenderer renderer in Renderers.GetKeys())
                {
                    System.Diagnostics.Debug.Assert(renderer != null);
                    renderer.RelMoveAndRotate(Id, p, pPrev, look);
                }

                _movement = true;
            }

            public void Move(Vector p, Vector pPrev)
            {
                System.Diagnostics.Debug.Assert(!_disposed);

                System.Diagnostics.Debug.Assert(!_movement);

                System.Diagnostics.Debug.Assert(Renderers != null);
                foreach (EntityRenderer renderer in Renderers.GetKeys())
                {
                    System.Diagnostics.Debug.Assert(renderer != null);
                    renderer.RelMove(Id, p, pPrev);
                }

                _movement = true;
            }

            public void Rotate(Look look)
            {
                System.Diagnostics.Debug.Assert(!_disposed);

                System.Diagnostics.Debug.Assert(!_movement);

                System.Diagnostics.Debug.Assert(Renderers != null);
                foreach (EntityRenderer renderer in Renderers.GetKeys())
                {
                    System.Diagnostics.Debug.Assert(renderer != null);
                    renderer.Rotate(Id, look);
                }

                _movement = true;
            }

            public void Stand()
            {
                System.Diagnostics.Debug.Assert(!_disposed);

                System.Diagnostics.Debug.Assert(!_movement);

                System.Diagnostics.Debug.Assert(Renderers != null);
                foreach (EntityRenderer renderer in Renderers.GetKeys())
                {
                    System.Diagnostics.Debug.Assert(renderer != null);
                    renderer.Stand(Id);
                }

                _movement = true;
            }

            public void FinishMovementRenderring()
            {
                System.Diagnostics.Debug.Assert(!_disposed);

                System.Diagnostics.Debug.Assert(_movement);

                _movement = false;
            }

            public void ChangeForms(bool sneaking, bool sprinting)
            {
                System.Diagnostics.Debug.Assert(!_disposed);

                System.Diagnostics.Debug.Assert(Renderers != null);
                foreach (EntityRenderer renderer in Renderers.GetKeys())
                {
                    System.Diagnostics.Debug.Assert(renderer != null);
                    renderer.ChangeForms(Id, sneaking, sprinting);
                }
            }

            public void Teleport(Vector p, Look look, bool onGround)
            {
                System.Diagnostics.Debug.Assert(!_disposed);

                System.Diagnostics.Debug.Assert(Renderers != null);
                foreach (EntityRenderer renderer in Renderers.GetKeys())
                {
                    System.Diagnostics.Debug.Assert(renderer != null);
                    renderer.Teleport(Id, p, look, onGround);
                }
            }

            public void SetEquipmentsData(
                (byte[] mainHand, byte[] offHand) equipmentsData)
            {
                System.Diagnostics.Debug.Assert(!_disposed);

                System.Diagnostics.Debug.Assert(equipmentsData.mainHand != null);
                System.Diagnostics.Debug.Assert(equipmentsData.offHand != null);

                System.Diagnostics.Debug.Assert(Renderers != null);
                foreach (EntityRenderer renderer in Renderers.GetKeys())
                {
                    System.Diagnostics.Debug.Assert(renderer != null);
                    renderer.SetEquipmentsData(Id, equipmentsData);
                }
            }

            public void Flush()
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

            public void Dispose()
            {
                // Assertions.
                System.Diagnostics.Debug.Assert(!_disposed);
                System.Diagnostics.Debug.Assert(!_movement);

                // Release  resources.
                Renderers.Dispose();

                // Finish
                System.GC.SuppressFinalize(this);
                _disposed = true;
            }

        }

        private protected readonly struct Hitbox : System.IEquatable<Hitbox>
        {
            private readonly double Width, Height;
            /*public readonly double EyeHeight, MaxStepHeight;*/

            public Hitbox(double w, double h)
            {
                System.Diagnostics.Debug.Assert(w > 0.0D);
                System.Diagnostics.Debug.Assert(h > 0.0D);

                Width = w;
                Height = h;
            }

            public readonly BoundingVolume Convert(Vector p)
            {
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


        private RendererManager Manager;  // Disposable


        private protected Entity(
            System.Guid uniqueId,
            Vector p, Look look,
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

            Manager = new(Id);
        }

        ~Entity() => System.Diagnostics.Debug.Assert(false);

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

            System.Diagnostics.Debug.Assert(Manager != null);
            if (Manager.Apply(renderer))
            {
                RenderSpawning(renderer);
            }
        }

        protected override BoundingVolume GenerateBoundingVolume()
        {
            Hitbox hitbox = GetHitbox();

            // TODO: Apply force based on hitbox.
            Forces.Enqueue(
                -1.0D *
                new Vector(1.0D - 0.91D, 1.0D - 0.9800000190734863D, 1.0D - 0.91D) *
                Velocity);  // Damping Force

            return _teleported ? hitbox.Convert(_pTeleport) : hitbox.Convert(_p);
        }

        internal override void Move(BoundingVolume volume, Vector v)
        {
            System.Diagnostics.Debug.Assert(volume != null);

            System.Diagnostics.Debug.Assert(!_disposed);

            if (_teleported)
            {
                Manager.Teleport(_pTeleport, _look, false);

                _p = _pTeleport;

                _rotated = false;
                _teleported = false;
            }

            Vector p = volume.GetBottomCenter();

            System.Diagnostics.Debug.Assert(Manager != null);
            Manager.HandleRendering(p);

            bool moved = !p.Equals(_p);  // TODO: Compare with machine epsilon.
            if (moved && _rotated)
            {
                Manager.MoveAndRotate(p, _p, _look);
            }
            else if (moved)
            {
                System.Diagnostics.Debug.Assert(!_rotated);

                Manager.Move(p, _p);
            }
            else if (_rotated)
            {
                System.Diagnostics.Debug.Assert(!moved);

                Manager.Rotate(_look);
            }
            else
            {
                System.Diagnostics.Debug.Assert(!moved);
                System.Diagnostics.Debug.Assert(!_rotated);

                Manager.Stand();
            }

            Manager.FinishMovementRenderring();

            base.Move(volume, v);

            _p = p;
            _rotated = false;
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

        private void UpdateFormChangingRendering()
        {
            System.Diagnostics.Debug.Assert(!_disposed);
            
            Manager.ChangeForms(_sneaking, _sprinting);
        }

        internal void Sneak(World world)
        {
            System.Diagnostics.Debug.Assert(world != null);

            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(!Sneaking);

            _sneaking = true;

            OnSneak(world, _sneaking);

            UpdateFormChangingRendering();
        }

        internal void Unsneak(World world)
        {
            System.Diagnostics.Debug.Assert(world != null);

            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(Sneaking);

            _sneaking = false;

            OnSneak(world, _sneaking);

            UpdateFormChangingRendering();
        }

        internal void Sprint(World world)
        {
            System.Diagnostics.Debug.Assert(world != null);

            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(!Sprinting);

            _sprinting = true;

            OnSprint(world, _sprinting);

            UpdateFormChangingRendering();
        }

        internal void Unsprint(World world)
        {
            System.Diagnostics.Debug.Assert(world != null);

            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(Sprinting);

            _sprinting = false;

            OnSprint(world, _sprinting);

            UpdateFormChangingRendering();
        }

        internal void UpdateEntityEquipmentsData((
            byte[] mainHand, byte[] offHand) equipmentsData)
        {
            System.Diagnostics.Debug.Assert(equipmentsData.mainHand != null);
            System.Diagnostics.Debug.Assert(equipmentsData.offHand != null);

            System.Diagnostics.Debug.Assert(!_disposed);

            Manager.SetEquipmentsData(equipmentsData);
        }

        protected void EmitParticles(Particles particle, int count)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            throw new System.NotImplementedException();
        }

        internal override void Flush()
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            Manager.Flush();

            base.Flush();
        }

        public override void Dispose()
        {
            // Assertion
            System.Diagnostics.Debug.Assert(!_disposed);

            // Release resources.
            EntityIdAllocator.Dealloc(Id);
            Manager.Dispose();

            LockerRotate.Dispose();
            LockerTeleport.Dispose();

            // Finish.
            base.Dispose();
            _disposed = true;
        }

    }

    public sealed class ItemEntity : Entity
    {
        private static readonly Hitbox DefaultHitbox = new(0.25D, 0.25D);

        public const double DefaultMass = 1.0D;

        public const double DefaultMaxStepLevel = 0.0D;


        private bool _disposed = false;


        private readonly ItemStack Stack;


        public ItemEntity(ItemStack stack, Vector p)
            : base(System.Guid.NewGuid(), p, new Look(0.0F, 0.0F), DefaultHitbox,
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
        protected float _health = 0.0F;

        private protected LivingEntity(
            System.Guid uniqueId,
            Vector p, Look look,
            Hitbox hitbox,
            double m, double maxStepLevel) 
            : base(uniqueId, p, look, hitbox, m, maxStepLevel) 
        { }

        ~LivingEntity() => System.Diagnostics.Debug.Assert(false);

        protected internal override bool IsDead()
        {
            return _health <= 0.0D;
        }

        public virtual void Damage(float amount)
        {
            System.Diagnostics.Debug.Assert(amount >= 0.0D);

            LockerHealth.Hold();

            _health -= amount;

            LockerHealth.Release();
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

    public abstract class AbstractPlayer : LivingEntity
    {
        private static Hitbox GetHitbox(bool sneaking)
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

        private static Hitbox DefaultHitbox = GetHitbox(false);

        public const double DefaultMass = 1.0D;

        public const double DefaultMaxStepLevel = 0.6;


        private bool _disposed = false;


        protected readonly PlayerInventory Inventory = new();


        private Connection Conn;
        public bool Disconnected => (Conn == null);
        public bool Connected => !Disconnected;


        private Vector _pControl;


        public AbstractPlayer(UserId id, Vector p, Look look) 
            : base(id.Data, p, look, DefaultHitbox, DefaultMass, DefaultMaxStepLevel) 
        {
            System.Diagnostics.Debug.Assert(id != UserId.Null);

            System.Diagnostics.Debug.Assert(!Sneaking);
            System.Diagnostics.Debug.Assert(!Sprinting);
        }

        ~AbstractPlayer() => System.Diagnostics.Debug.Assert(false);

        public override void Damage(float amount)
        {
            System.Diagnostics.Debug.Assert(amount >= 0.0D);

            LockerHealth.Hold();

            base.Damage(amount);

            if (!Disconnected)
            {
                Conn.UpdateHealth(_health);

                if (_health <= 0.0D)
                {
                    Conn.Respawn();
                }
            }

            LockerHealth.Release();
        }

        private protected override Hitbox GetHitbox()
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            return GetHitbox(Sneaking);
        }

        private protected override void RenderSpawning(EntityRenderer renderer)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(renderer != null);

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

            Conn = new Connection(id, client, world, Id, _pControl, Inventory);
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
            }
            
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
            Inventory.Dispose();
            if (!Disconnected)
            {
                Conn.Dispose();
            }

            // Finish.
            base.Dispose();
            _disposed = true;
        }

    }

}

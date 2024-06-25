
using Common;
using Containers;
using MinecraftServerEngine.PhysicsEngine;

namespace MinecraftServerEngine
{
    public abstract class Entity : PhysicsObject
    {
        private sealed class RendererManager : System.IDisposable
        {
            private bool _disposed = false;

            private bool _movement = false;

            private readonly int _id;
            public int Id => _id;

            private readonly ConcurrentSet<int> Ids = new();  // Disposable
            private readonly ConcurrentQueue<EntityRenderer> Renderers = new();  // Disposable

            public RendererManager(int id)
            {
                _id = id;
            }

            ~RendererManager() => System.Diagnostics.Debug.Assert(false);

            public bool Apply(EntityRenderer renderer)
            {
                System.Diagnostics.Debug.Assert(renderer != null);

                System.Diagnostics.Debug.Assert(!_disposed);

                if (renderer.Id == Id)
                {
                    return false;
                }
                if (Ids.Contains(renderer.Id))
                {
                    return false;
                }

                Ids.Insert(renderer.Id);
                Renderers.Enqueue(renderer);

                return true;
            }

            public void HandleRendering(Vector p)
            {
                System.Diagnostics.Debug.Assert(!_disposed);

                System.Diagnostics.Debug.Assert(!_movement);

                for (int i = 0; i < Renderers.Count; ++i)
                {
                    EntityRenderer renderer = Renderers.Dequeue();

                    if (renderer.Disconnected)
                    {
                        Ids.Extract(renderer.Id);

                        continue;
                    }
                    else if (!renderer.CanRender(p))
                    {
                        Ids.Extract(renderer.Id);
                        renderer.DestroyEntity(Id);

                        continue;
                    }

                    Renderers.Enqueue(renderer);
                }

            }

            public void MoveAndRotate(
                Vector p, Vector pPrev, Look look, bool onGround)
            {
                System.Diagnostics.Debug.Assert(!_disposed);

                System.Diagnostics.Debug.Assert(!_movement);

                foreach (EntityRenderer renderer in Renderers.GetValues())
                {
                    renderer.MoveAndRotate(Id, p, pPrev, look, onGround);
                }

                _movement = true;
            }

            public void Move(Vector p, Vector pPrev, bool onGround)
            {
                System.Diagnostics.Debug.Assert(!_disposed);

                System.Diagnostics.Debug.Assert(!_movement);

                foreach (EntityRenderer renderer in Renderers.GetValues())
                {
                    renderer.Move(Id, p, pPrev, onGround);
                }

                _movement = true;
            }

            public void Rotate(Look look, bool onGround)
            {
                System.Diagnostics.Debug.Assert(!_disposed);

                System.Diagnostics.Debug.Assert(!_movement);

                foreach (EntityRenderer renderer in Renderers.GetValues())
                {
                    renderer.Rotate(Id, look, onGround);
                }

                _movement = true;
            }

            public void Stand()
            {
                System.Diagnostics.Debug.Assert(!_disposed);

                System.Diagnostics.Debug.Assert(!_movement);

                foreach (EntityRenderer renderer in Renderers.GetValues())
                {
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

                foreach (EntityRenderer renderer in Renderers.GetValues())
                {
                    renderer.ChangeForms(Id, sneaking, sprinting);
                }
            }

            public void Teleport(Vector p, Look look, bool onGround)
            {
                System.Diagnostics.Debug.Assert(!_disposed);

                foreach (EntityRenderer renderer in Renderers.GetValues())
                {
                    renderer.Teleport(Id, p, look, onGround);
                }
            }

            public void Flush()
            {
                System.Diagnostics.Debug.Assert(!_disposed);

                System.Diagnostics.Debug.Assert(!Ids.Contains(Id));

                while (!Renderers.Empty)
                {
                    EntityRenderer renderer = Renderers.Dequeue();
                    if (renderer.Disconnected)
                    {
                        continue;
                    }

                    renderer.DestroyEntity(Id);

                    System.Diagnostics.Debug.Assert(Ids.Contains(renderer.Id));
                }
            }

            public void Dispose()
            {
                System.Diagnostics.Debug.Assert(!_disposed);

                // Assertion
                System.Diagnostics.Debug.Assert(!_movement);
                System.Diagnostics.Debug.Assert(Renderers.Empty);

                // Release  resources.
                Ids.Dispose();
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

        private bool _rotated = false;
        private Look _look;
        internal Look Look => _look;

        protected bool _sneaking, _sprinting = false;
        public bool Sneaking => _sneaking;
        public bool Sprinting => _sprinting;


        private protected bool _teleported = false;
        private protected Vector _pTeleport;
        private protected Look _lookTeleport;


        private RendererManager Manager;  // Disposable


        private protected Entity(
            System.Guid uniqueId,
            Vector p, Look look,
            Hitbox hitbox,
            double m)
            : base(hitbox.Convert(p), m)
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

        public virtual bool IsDead()
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            return false;
        }

        public virtual void StartRoutine(long serverTicks, World world) { }

        protected override BoundingVolume GenerateBoundingVolume()
        {
            Hitbox hitbox = GetHitbox();

            return _teleported ? hitbox.Convert(_pTeleport) : hitbox.Convert(_p);
        }

        internal override void Move(BoundingVolume volume, Vector v, bool onGround)
        {
            System.Diagnostics.Debug.Assert(volume != null);

            System.Diagnostics.Debug.Assert(!_disposed);

            if (_teleported)
            {
                Manager.Teleport(_pTeleport, _lookTeleport, false);

                _p = _pTeleport;

                if (!_rotated)
                {
                    _look = _lookTeleport;
                }

                _teleported = false;
            }

            Vector p = volume.GetBottomCenter();

            System.Diagnostics.Debug.Assert(Manager != null);
            Manager.HandleRendering(p);

            bool moved = !p.Equals(_p);  // TODO: Compare with machine epsilon.
            if (moved && _rotated)
            {
                Manager.MoveAndRotate(p, _p, _look, onGround);
            }
            else if (moved)
            {
                System.Diagnostics.Debug.Assert(!_rotated);

                Manager.Move(p, _p, onGround);
            }
            else if (_rotated)
            {
                System.Diagnostics.Debug.Assert(!moved);

                Manager.Rotate(_look, onGround);
            }
            else
            {
                System.Diagnostics.Debug.Assert(!moved);
                System.Diagnostics.Debug.Assert(!_rotated);

                Manager.Stand();
            }

            Manager.FinishMovementRenderring();

            base.Move(volume, v, onGround);

            _p = p;
            _rotated = false;
        }

        public virtual void Teleport(Vector p, Look look)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            _rotated = false;

            _teleported = true;
            _pTeleport = p;
            _lookTeleport = look;
        }

        public void Rotate(Look look)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            _rotated = true;
            _look = look;
        }

        private void RanderFormChanging()
        {
            System.Diagnostics.Debug.Assert(!_disposed);
            
            Manager.ChangeForms(_sneaking, _sprinting);
        }

        private protected void Sneak()
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(!Sneaking);

            _sneaking = true;

            RanderFormChanging();
        }

        private protected void Unsneak()
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(Sneaking);

            _sneaking = false;

            RanderFormChanging();
        }

        private protected void Sprint()
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(!Sprinting);

            _sprinting = true;

            RanderFormChanging();
        }

        private protected void Unsprint()
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(Sprinting);

            _sprinting = false;

            RanderFormChanging();
        }
        
        public virtual void Flush()
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(Manager != null);
            Manager.Flush();
        }

        public override void Dispose()
        {
            // Assertion
            System.Diagnostics.Debug.Assert(!_disposed);

            // Release resources.
            EntityIdAllocator.Dealloc(Id);
            Manager.Dispose();

            // Finish.
            base.Dispose();
            _disposed = true;
        }

    }

    public abstract class ItemEntity : Entity
    {
        private static readonly Hitbox DefaultHitbox = new(0.25D, 0.25D);

        public const double DefaultMass = 1.0D;

        private bool _disposed = false;

        public ItemEntity(Vector p, Look look)
            : base(System.Guid.NewGuid(), p, look, DefaultHitbox, DefaultMass) 
        { }

        ~ItemEntity() => System.Diagnostics.Debug.Assert(false);

        private protected override void RenderSpawning(EntityRenderer renderer)
        {
            /*(byte x, byte y) = Look.ConvertToPacketFormat();
            outPackets.Enqueue(new SpawnObjectPacket(
                Id, UniqueId, 2,
                Position.X, Position.Y, Position.Z,
                x, y,
                1, 0, 0, 0));

            using EntityMetadata metadata = new();
            metadata.AddBool(5, true);
            metadata.AddSlotData(6, new SlotData(280, 1));
            outPackets.Enqueue(new EntityMetadataPacket(
                Id, metadata.WriteData()));*/

            renderer.SpawnItemEntity(Id);
        }

        public override void Dispose()
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            // Assertion.

            // Release resources.

            // Finish.
            base.Dispose();
            _disposed = true;
        }

    }

    public abstract class LivingEntity : Entity
    {
        private bool _disposed = false;

        private protected LivingEntity(
            System.Guid uniqueId,
            Vector p, Look look,
            Hitbox hitbox,
            double m) : base(uniqueId, p, look, hitbox, m) { }

        ~LivingEntity() => System.Diagnostics.Debug.Assert(false);

        public override void Dispose()
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            // Assertion.

            // Release resources.

            // Finish.
            base.Dispose();
            _disposed = true;
        }

    }

    public abstract class Player : LivingEntity
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

        private bool _disposed = false;

        internal readonly PlayerInventory SelfInventory = new();

        private Connection Conn;
        public bool Disconnected => (Conn == null);
        public bool Connected => !Disconnected;

        private Vector _pControl;
        private bool _onGroundControl;

        public Player(System.Guid userId, Vector p, Look look) 
            : base(userId, p, look, DefaultHitbox, DefaultMass) 
        {
            System.Diagnostics.Debug.Assert(!Sneaking);
            System.Diagnostics.Debug.Assert(!Sprinting);
        }

        ~Player() => System.Diagnostics.Debug.Assert(false);

        private protected override Hitbox GetHitbox()
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            return GetHitbox(Sneaking);
        }

        private protected override void RenderSpawning(EntityRenderer renderer)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            renderer.SpawnPlayer(
                Id, UniqueId,
                Position, Look,
                Sneaking, Sprinting);
        }

        internal void Connect(Client client, World world, System.Guid userId)
        {
            System.Diagnostics.Debug.Assert(client != null);
            System.Diagnostics.Debug.Assert(world != null);
            System.Diagnostics.Debug.Assert(userId != System.Guid.Empty);

            System.Diagnostics.Debug.Assert(!_disposed);

            _pControl = Position;
            _onGroundControl = OnGround;

            Conn = new Connection(client, world, Id, userId, _pControl, SelfInventory);
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

        internal override void Move(BoundingVolume volume, Vector v, bool onGround)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

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
                onGround = _onGroundControl;
            }
            
            base.Move(volume, v, onGround);
        }

        public override void Teleport(Vector p, Look look)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            if (Connected)
            {
                _pControl = p;
                _onGroundControl = false;

                Conn.Teleport(p, look);
            }

            base.Teleport(p, look);
        }

        private void HandleControl(Control control)
        {
            System.Diagnostics.Debug.Assert(control != null);

            System.Diagnostics.Debug.Assert(!_disposed);

            switch (control)
            {
                default:
                    throw new System.NotImplementedException();
                case MoveControl moveControl:
                    _pControl = moveControl.Position;
                    break;
                case RotControl rotControl:
                    Rotate(rotControl.Look);
                    break;
                case StandControl standControl:
                    _onGroundControl = standControl.OnGround;
                    break;
                case SneakControl:
                    Sneak();
                    break;
                case UnsneakControl:
                    Unsneak();
                    break;
                case SprintControl:
                    Sprint();
                    break;
                case UnsprintControl:
                    Unsprint();
                    break;
            }
        }

        internal void Control(long serverTicks, World world)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            if (Connected)
            {
                System.Diagnostics.Debug.Assert(Conn != null);

                using Queue<Control> controls = new();

                Conn.Control(
                    controls, 
                    world, 
                    UniqueId, Sneaking, Sprinting, SelfInventory);

                while (!controls.Empty)
                {
                    Control control = controls.Dequeue();
                    HandleControl(control);
                }
            }

        }

        public bool HandleConnection(World world)
        {
            System.Diagnostics.Debug.Assert(world != null);

            System.Diagnostics.Debug.Assert(!_disposed);

            if (Disconnected)
            {
                return false;
            }

            System.Diagnostics.Debug.Assert(Conn != null);
            if (Conn.Disconnected)
            {
                Conn.Flush(world, UniqueId);
                Conn.Dispose();

                Conn = null;

                return true;
            }

            return false;
        }

        public void Render(World world)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            if (Disconnected)
            {
                return;
            }

            System.Diagnostics.Debug.Assert(Conn != null);
            Conn.Render(
                world, 
                Id, 
                Position, Look, 
                SelfInventory);
        }
        
        public bool GiveItem(Items item, int count)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            throw new System.NotImplementedException();
        }

        public override void Dispose()
        {
            // Assertion.
            System.Diagnostics.Debug.Assert(!_disposed);

            // Release resources.

            // Finish.
            base.Dispose();
            _disposed = true;
        }


    }

}

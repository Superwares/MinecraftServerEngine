
using Common;
using Containers;
using PhysicsEngine;

namespace MinecraftServerEngine
{
    public abstract class Entity : PhysicsObject
    {

        internal readonly struct Hitbox : System.IEquatable<Hitbox>
        {
            private readonly double _WIDTH, _HEIGHT;
            /*public readonly double EyeHeight, MaxStepHeight;*/

            public Hitbox(double w, double h)
            {
                _WIDTH = w;
                _HEIGHT = h;
            }

            public readonly IBoundingVolume Convert(Vector p)
            {
                System.Diagnostics.Debug.Assert(_WIDTH > 0.0D);
                System.Diagnostics.Debug.Assert(_HEIGHT > 0.0D);

                double w = _WIDTH / 2.0D;

                Vector max = new(p.X + w, p.Y + _HEIGHT, p.Z + w),
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

        public readonly int ID;
        public readonly System.Guid UNIQUE_ID;

        private Vector _p;
        internal Vector POSITION
        {
            get
            {
                System.Diagnostics.Debug.Assert(!_disposed);
                
                return _p;
            }
        }

        private bool _rotated = false;
        private Look _look;
        public Look LOOK
        {
            get
            {
                System.Diagnostics.Debug.Assert(!_disposed);
                
                return _look;
            }
        }

        protected bool _sneaking, _sprinting = false;
        public bool SNEAKING
        {
            get
            {
                System.Diagnostics.Debug.Assert(!_disposed);
                
                return _sneaking;
            }
        }
        public bool SPRINTING
        {
            get
            {
                System.Diagnostics.Debug.Assert(!_disposed);
                
                return _sprinting;
            }
        }


        /*protected bool _teleported = false;
        protected Vector _posTeleport;
        protected Angles _lookTeleport;*/


        private EntityRendererManager _MANAGER;  // Disposable


        internal Entity(
            System.Guid uniqueId,
            Vector p, Look look,
            Hitbox hitbox,
            double m)
            : base(hitbox.Convert(p), m)
        {
            ID = EntityIdAllocator.Alloc();
            UNIQUE_ID = uniqueId;

            _p = p;

            System.Diagnostics.Debug.Assert(!_rotated);
            _look = look;

            System.Diagnostics.Debug.Assert(!_sneaking);
            System.Diagnostics.Debug.Assert(!_sprinting);

            _MANAGER = new(ID);
        }

        ~Entity() => System.Diagnostics.Debug.Assert(false);

        private protected abstract Hitbox GetHitbox();

        private protected abstract void RenderSpawning(EntityRenderer renderer);

        internal void ApplyRenderer(EntityRenderer renderer)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(_MANAGER != null);
            if (_MANAGER.Apply(renderer))
            {
                RenderSpawning(renderer);
            }
        }

        public virtual bool IsDead()
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            return false;
        }

        public virtual void StartInternalRoutine(long serverTicks, World world) { }

        public virtual void StartRoutine(long serverTicks, World world) { }

        protected override IBoundingVolume GenerateBoundingVolume()
        {
            Hitbox hitbox = GetHitbox();
            return hitbox.Convert(_p);
        }

        public override void Move(IBoundingVolume volume, Vector v, bool onGround)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            Vector p = volume.GetBottomCenter();

            System.Diagnostics.Debug.Assert(_MANAGER != null);
            _MANAGER.HandleRendering(p);

            bool moved = !p.Equals(_p);  // TODO: Compare with machine epsilon.
            if (moved && _rotated)
            {
                _MANAGER.MoveAndRotate(p, _p, _look, onGround);
            }
            else if (moved)
            {
                System.Diagnostics.Debug.Assert(!_rotated);

                _MANAGER.Move(p, _p, onGround);
            }
            else if (_rotated)
            {
                System.Diagnostics.Debug.Assert(!moved);

                _MANAGER.Rotate(_look, onGround);
            }
            else
            {
                System.Diagnostics.Debug.Assert(!moved);
                System.Diagnostics.Debug.Assert(!_rotated);

                _MANAGER.Stand();
            }

            _p = p;
            _rotated = false;

            _MANAGER.FinishMovementRenderring();

            base.Move(volume, v, onGround);
        }

        /*public virtual void Teleport(Vector pos, Angles look)
        {
            _teleported = true;
            _posTeleport = pos;
            _lookTeleport = look;
        }*/

        public void Rotate(Look look)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            _rotated = true;
            _look = look;
        }

        private void RanderFormChanging()
        {
            System.Diagnostics.Debug.Assert(!_disposed);
            
            _MANAGER.ChangeForms(_sneaking, _sprinting);
        }

        public void Sneak()
        {
            System.Diagnostics.Debug.Assert(!_disposed);
            
            System.Diagnostics.Debug.Assert(!_sneaking);
            _sneaking = true;

            RanderFormChanging();
        }

        public void Unsneak()
        {
            System.Diagnostics.Debug.Assert(!_disposed);
            
            System.Diagnostics.Debug.Assert(_sneaking);
            _sneaking = false;

            RanderFormChanging();
        }

        public void Sprint()
        {
            System.Diagnostics.Debug.Assert(!_disposed);
            
            System.Diagnostics.Debug.Assert(!_sprinting);
            _sprinting = true;

            RanderFormChanging();
        }

        public void Unsprint()
        {
            System.Diagnostics.Debug.Assert(!_disposed);
            
            System.Diagnostics.Debug.Assert(_sprinting);
            _sprinting = false;

            RanderFormChanging();
        }
        
        public virtual void Flush()
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(_MANAGER != null);
            _MANAGER.Flush();
        }

        public override void Dispose()
        {
            // Assertion
            System.Diagnostics.Debug.Assert(!_disposed);

            // Release resources.
            _FORCES.Dispose();
            _MANAGER.Dispose();

            // Finish.
            base.Dispose();
            _disposed = true;
        }

    }

    public abstract class ItemEntity : Entity
    {
        private bool _disposed = false;

        
        public const double MASS = 0.1D;


        private static readonly Hitbox HITBOX = new(0.25D, 0.25D);


        public ItemEntity(Vector p, Look look)
            : base(System.Guid.NewGuid(), p, look, HITBOX, MASS) 
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

            renderer.SpawnItemEntity(ID);
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

        internal LivingEntity() : base() { }

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
        private bool _disposed = false;


        public const double MASS = 1.0D;
        public override double GetMass() => MASS;


        public override Hitbox GetHitbox()
        {
            double w = 0.6D, h;
            if (SNEAKING)
            {
                h = 1.65D;
            }
            else
            {
                h = 1.8D;
            }

            return new(w, h);
        }



        internal readonly PlayerInventory _selfInventory = new();


        private Connection? _CONN;
        public bool Connected => (_CONN != null);


        private Vector _p;
        private bool _onGround;


        public Player() : base() { }

        ~Player() => System.Diagnostics.Debug.Assert(false);

        private protected override void RenderSpawning(EntityRenderer renderer)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            renderer.SpawnPlayer(
                ID, UNIQUE_ID,
                Position, Look,
                SNEAKING, SPRINTING);
        }

        internal void Connect(Client client, World world, System.Guid userId)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            _p = Position;

            _CONN = new Connection(client, world, ID, userId, _p, _selfInventory);
        }

        public override void ApplyForce(Vector force)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            if (Connected)
            {
                System.Diagnostics.Debug.Assert(_CONN != null);
                Vector v = force / MASS;
                _CONN.ApplyVelocity(ID, v);
            }
            else
            {
                base.ApplyForce(force);
            }
        }

        /*public override void Teleport(Vector pos, Angles look)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            base.Teleport(pos, look);

            System.Diagnostics.Debug.Assert(_selfRenderer != null);
            _selfRenderer.Teleport(pos, look);
            // TODO: Check the velocity was reset in client when teleported.
        }*/

        public override void Move(BoundingBox bb, Vector v, bool onGround)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            if (Connected)
            {
                // TODO: Check the difference between _p and p. and predict movement....
                /*System.Console.WriteLine($"p: {p}, _p: {_p}, ");
                System.Console.WriteLine($"Length: {Vector.GetLength(p, _p)}");
                if (Vector.GetLength(p, _p) > k)
                {
                }
                */

                /*Vector v1 = bb.GetBottomCenter(), v2 = _p;
                double length = Vector.GetLength(v1, v2);
                System.Console.WriteLine($"length: {length}");*/

                bb = GetHitbox().Convert(_p);
                onGround = _onGround;
            }
            
            base.Move(bb, v, onGround);
        }

        internal void Control(Vector p, bool onGround)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(Connected);

            _p = p;
            _onGround = onGround;
        }

        internal void Control(bool onGround)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(Connected);

            _onGround = onGround;
        }

        private void HandleControl(ServerboundPlayingPacket control)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            switch (control)
            {
                default:
                    throw new System.NotImplementedException();
                case PlayerPacket:
                    throw new System.NotImplementedException();
                case PlayerPositionPacket:
                    throw new System.NotImplementedException();
                case PlayerPosAndLookPacket:
                    throw new System.NotImplementedException();
                case PlayerLookPacket:
                    throw new System.NotImplementedException();
                case EntityActionPacket:
                    throw new System.NotImplementedException();
            }
        }

        public override void StartInternalRoutine(long serverTicks, World world)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            if (Connected)
            {
                System.Diagnostics.Debug.Assert(_CONN != null);

                using Queue<ServerboundPlayingPacket> controls = new();

                _CONN.Control(controls, world, UNIQUE_ID, SNEAKING, SPRINTING, _selfInventory);

                while (!controls.Empty)
                {
                    ServerboundPlayingPacket control = controls.Dequeue();
                    HandleControl(control);
                }
            }

            base.StartInternalRoutine(serverTicks, world);
        }

        public bool HandlePlayerConnection(World world)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            if (_CONN == null)
            {
                return false;
            }

            System.Diagnostics.Debug.Assert(_CONN != null);
            if (_CONN.Disconnected)
            {
                _CONN.Flush(world, UNIQUE_ID);
                _CONN.Dispose();

                _CONN = null;

                return true;
            }

            return false;
        }

        public void Render(World world)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            if (!Connected)
            {
                return;
            }

            System.Diagnostics.Debug.Assert(_CONN != null);
            _CONN.Render(
                world, 
                ID, 
                Position, Look, 
                _selfInventory);
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

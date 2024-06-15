
using Common;
using Containers;
using PhysicsEngine;

namespace MinecraftServerEngine
{
    public abstract class Entity : PhysicsObject
    {

        public readonly struct Hitbox
        {
            public readonly double _W, _H;
            public double Width => _W;
            public double Height => _H;
            /*public readonly double EyeHeight, MaxStepHeight;*/

            public Hitbox(double w, double h)
            {
                _W = w;
                _H = h;
            }

            public BoundingBox Convert(Vector p)
            {
                System.Diagnostics.Debug.Assert(_W > 0.0D);
                System.Diagnostics.Debug.Assert(_H > 0.0D);

                double w = _W / 2.0D;

                Vector max = new(p.X + w, p.Y + _H, p.Z + w),
                       min = new(p.X - w, p.Y, p.Z - w);
                return new(max, min);
            }
        }

        public readonly struct Position : System.IEquatable<Position>
        {
            public readonly double X, Y, Z;

            public Position(double x, double y, double z)
            {
                X = x; Y = y; Z = z;
            }


        }

        public readonly struct Angles : System.IEquatable<Angles>
        {
            internal const float MaxYaw = 180, MinYaw = -180;
            internal const float MaxPitch = 90, MinPitch = -90;

            public readonly float Yaw, Pitch;

            private static float Frem(float angle)
            {
                float y = 360.0f;
                return angle - (y * (float)System.Math.Floor(angle / y));
            }

            public Angles(float yaw, float pitch)
            {
                // TODO: map yaw from 180 to -180.
                System.Diagnostics.Debug.Assert(pitch >= MinPitch);
                System.Diagnostics.Debug.Assert(pitch <= MaxPitch);

                Yaw = Frem(yaw);
                Pitch = pitch;
            }

            internal (byte, byte) ConvertToPacketFormat()
            {
                System.Diagnostics.Debug.Assert(Pitch >= MinPitch);
                System.Diagnostics.Debug.Assert(Pitch <= MaxPitch);
                
                float x = Frem(Yaw);
                float y = Frem(Pitch);

                return (
                    (byte)((byte.MaxValue * x) / 360),
                    (byte)((byte.MaxValue * y) / 360));
            }

            public override readonly string ToString()
            {
                throw new System.NotImplementedException();
            }

            public bool Equals(Angles other)
            {
                return (Yaw == other.Yaw) && (Pitch == other.Pitch);
            }

        }

        private bool _disposed = false;

        private bool _created = false;


        private int _id;
        internal int Id
        {
            get
            {
                System.Diagnostics.Debug.Assert(!_disposed);
                System.Diagnostics.Debug.Assert(_created);
                return _id;
            }
        }

        private System.Guid _uniqueId;
        internal System.Guid UniqueId
        {
            get
            {
                System.Diagnostics.Debug.Assert(!_disposed);
                System.Diagnostics.Debug.Assert(_created);
                return _uniqueId;
            }
        }


        public abstract double GetMass();

        public abstract Hitbox GetHitbox();


        private readonly ConcurrentQueue<Vector> _FORCES = new();

        private Vector _v = new Vector(0, 0, 0);
        public Vector Velocity
        {
            get
            {
                System.Diagnostics.Debug.Assert(!_disposed);
                System.Diagnostics.Debug.Assert(_created);
                return _v;
            }
        }

        private BoundingBox _bb;
        public BoundingBox BB
        {
            get
            {
                System.Diagnostics.Debug.Assert(!_disposed);
                System.Diagnostics.Debug.Assert(_created);
                return _bb;
            }
        }

        private Vector _p;
        internal Vector Position
        {
            get
            {
                System.Diagnostics.Debug.Assert(!_disposed);
                System.Diagnostics.Debug.Assert(_created);
                return _p;
            }
        }


        private bool _rotated = false;
        private Angles _look;
        public Angles Look
        {
            get
            {
                System.Diagnostics.Debug.Assert(!_disposed);
                System.Diagnostics.Debug.Assert(_created);
                return _look;
            }
        }


        protected bool _sneaking = false, _sprinting = false;
        public bool Sneaking
        {
            get
            {
                System.Diagnostics.Debug.Assert(!_disposed);
                System.Diagnostics.Debug.Assert(_created);
                return _sneaking;
            }
        }
        public bool Sprinting
        {
            get
            {
                System.Diagnostics.Debug.Assert(!_disposed);
                System.Diagnostics.Debug.Assert(_created);
                return _sprinting;
            }
        }


        /*protected bool _teleported = false;
        protected Vector _posTeleport;
        protected Angles _lookTeleport;*/


        private EntityRendererManager? _MANAGER = null;  // Disposable

        internal Entity() { }

        ~Entity() => System.Diagnostics.Debug.Assert(false);

        public void Create(
            int id, System.Guid uniqueId, 
            Vector p, Angles look)
        {
            System.Diagnostics.Debug.Assert(!_created);

            _id = id;
            _uniqueId = uniqueId;

            Hitbox hitbox = GetHitbox();
            _bb = hitbox.Convert(p);

            _p = p;

            _look = look;

            _MANAGER = new(id);

            _created = true;
        }

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

        public virtual void ApplyForce(Vector force)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            _FORCES.Enqueue(force);
        }

        public virtual void ApplyGlobalForce(Vector force)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            _FORCES.Enqueue(force);
        }

        public virtual bool IsDead()
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            return false;
        }

        public virtual void StartInternalRoutine(long serverTicks, World world)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

        }

        public abstract void StartRoutine(long serverTicks, World world);

        protected override BoundingVolume GenerateBoundingVolume()
        {
            Hitbox hitbox = GetHitbox();
            BoundingBox bb = hitbox.Convert(_p);

            throw new System.NotImplementedException();
        }

        /*internal (BoundingBox, Vector) Integrate()
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            Vector v = _v;

            while (!_FORCES.Empty)
            {
                Vector force = _FORCES.Dequeue();

                v += (force / GetMass());
            }

            Hitbox hitbox = GetHitbox();
            BoundingBox bb = hitbox.Convert(_p);

            return (bb, v);
        }*/

        public override void Move(BoundingVolume volume, Vector v, bool onGround)
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

        public void Rotate(Angles look)
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
        public override double GetMass()
        {
            return MASS;
        }


        public static readonly Hitbox HITBOX = new(0.25D, 0.25D);
        public override Hitbox GetHitbox()
        {
            return HITBOX;
        }


        public ItemEntity() : base() { }

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
            if (Sneaking)
            {
                h = 1.65D;
            }
            else
            {
                h = 1.8D;
            }

            return new(w, h);
        }



        internal readonly SelfInventory _selfInventory = new();


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
                Id, UniqueId,
                Position, Look,
                Sneaking, Sprinting);
        }

        internal void Connect(Client client, World world, System.Guid userId)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            _p = Position;

            _CONN = new Connection(client, world, Id, userId, _p, _selfInventory);
        }

        public override void ApplyForce(Vector force)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            if (Connected)
            {
                System.Diagnostics.Debug.Assert(_CONN != null);
                Vector v = force / GetMass();
                _CONN.ApplyVelocity(Id, v);
            }

            base.ApplyForce(force);
        }

        public override void ApplyGlobalForce(Vector force)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            if (Connected)
            {
                
            }

            base.ApplyGlobalForce(force);
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
                    System.Diagnostics.Debug.Assert(false);
                    break;
                case PlayerPacket:
                    throw new System.NotImplementedException();
                    break;
                case PlayerPositionPacket:
                    throw new System.NotImplementedException();
                    break;
                case PlayerPosAndLookPacket:
                    throw new System.NotImplementedException();
                    break;
                case PlayerLookPacket:
                    throw new System.NotImplementedException();
                    break;
                case EntityActionPacket:
                    throw new System.NotImplementedException();
                    break;
            }
        }

        public override void StartInternalRoutine(long serverTicks, World world)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            if (Connected)
            {
                System.Diagnostics.Debug.Assert(_CONN != null);

                using Queue<ServerboundPlayingPacket> controls = new();

                _CONN.Control(controls, world, UniqueId, Sneaking, Sprinting, _selfInventory);

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
                _CONN.Flush(world, UniqueId);
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
                Id, 
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


using Common;
using Containers;

namespace Protocol
{
    public abstract class Entity : System.IDisposable
    {

        public readonly struct Vector : System.IEquatable<Vector>
        {
            public static Vector operator+ (Vector v1, Vector v2)
            {
                return new(v1.X + v2.X, v1.Y + v2.Y, v1.Z + v2.Z);
            }

            public static Vector operator* (Vector v1, Vector v2)
            {
                return new(v1.X * v2.X, v1.Y * v2.Y, v1.Z * v2.Z);
            }

            public static Vector operator *(Vector v, double s)
            {
                return new(v.X * s, v.Y * s, v.Z * s);
            }

            public static Vector operator* (double s, Vector v)
            {
                return new(v.X * s, v.Y * s, v.Z * s);
            }

            public readonly double X, Y, Z;

            public Vector(double x, double y, double z)
            {
                X = x; Y = y; Z = z;
            }

            public bool Equals(Vector other)
            {
                return Comparing.IsEqualTo(X, other.X) && 
                    Comparing.IsEqualTo(Y, other.Y) &&
                    Comparing.IsEqualTo(Z, other.Z);
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

            public bool Equals(Angles other)
            {
                return Comparing.IsEqualTo(Yaw, other.Yaw) &&
                    Comparing.IsEqualTo(Pitch, other.Pitch);
            }

        }

        private bool _disposed = false;

        internal abstract BoundingBox GetBoundingBox();

        private int _id;
        public int Id => _id;

        public System.Guid UniqueId;


        private readonly Queue<Vector> _FORCES = new();


        private Vector _v = new(0, 0, 0);
        public Vector Velocity => _v;


        private Vector _pos;
        public Vector Position => _pos;


        private bool _rotated = false;
        private Angles _look;
        public Angles Look => _look;


        protected bool _onGround;
        public bool IsOnGround => _onGround;


        protected bool _sneaking, _sprinting;
        public bool IsSneaking => _sneaking;
        public bool IsSprinting => _sprinting;


        protected bool _teleported;
        protected Vector _posTeleport;
        protected Angles _lookTeleport;


        private readonly EntityRendererManager _RENDERER_MANAGER = new();  // Disposable

        internal Entity(
            int id,
            System.Guid uniqueId,
            Vector pos, Angles look)
        {
            _id = id;

            UniqueId = uniqueId;
            _pos = pos;
            _look = look;
            _onGround = false;

            _sneaking = _sprinting = false;

            _teleported = false;
            /*_posTeleport = new(0, 0, 0);
            _lookTeleport = new(0, 0);*/
        }

        ~Entity() => System.Diagnostics.Debug.Assert(false);

        internal EntityRenderer ApplyForRenderer(
            Queue<ClientboundPlayingPacket> outPackets,
            Chunk.Vector p, int renderDistance)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            return _RENDERER_MANAGER.Apply(outPackets, p, renderDistance);
        }

        public virtual void AddForce(Vector force)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            _FORCES.Enqueue(force);
        }

        public virtual bool IsDead()
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            return false;
        }

        protected internal virtual void StartRoutine(long serverTicks, World world)
        {
            System.Diagnostics.Debug.Assert(!_disposed);
        }

        protected virtual Vector Integrate()
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            Vector pos = _pos;

            // update position with velocity, accelaration and forces(gravity, damping).
            while (!_FORCES.Empty)
            {
                Vector force = _FORCES.Dequeue();

                _v += force;
            }

            pos += _v;

            /*System.Console.WriteLine($"pos: ({pos.X}, {pos.Y}, {pos.Z})");*/

            return pos;
        }

        protected internal virtual void Move()
        {
            System.Diagnostics.Debug.Assert(!_disposed);
            
            Vector posNew = Integrate();
            System.Diagnostics.Debug.Assert(_FORCES.Empty);

            bool moved = !posNew.Equals(_pos);  // TODO: Compare with machine epsilon.
            // TODO: Check _pos - _posPrev is lower than 8 blocks.
            // If not, use EntityTeleportPacket to render.
            if (moved && _rotated)
            {
                _RENDERER_MANAGER.MoveAndRotate(Id, posNew, _pos, _look, _onGround);

                _pos = posNew;
                _rotated = false;
            }
            else if (moved)
            {
                System.Diagnostics.Debug.Assert(!_rotated);

                _RENDERER_MANAGER.Move(Id, posNew, _pos, _onGround);

                _pos = posNew;
            }
            else if (_rotated)
            {
                System.Diagnostics.Debug.Assert(!moved);

                _RENDERER_MANAGER.Rotate(Id, _look, _onGround);

                _rotated = false;
            }
            else
            {
                System.Diagnostics.Debug.Assert(!moved);
                System.Diagnostics.Debug.Assert(!_rotated);

                _RENDERER_MANAGER.Stand(Id);
            }

            _RENDERER_MANAGER.DeterminToContinueRendering(Id, _pos, GetBoundingBox());

            if (_teleported)
            {
                _pos = _posTeleport;
                _look = _lookTeleport;
                // update position data in chunk

                _RENDERER_MANAGER.Teleport(Id, _pos, _look, _onGround);

                _teleported = false;
            }
        }

        public virtual void Teleport(Vector pos, Angles look)
        {
            _teleported = true;
            _posTeleport = pos;
            _lookTeleport = look;
        }

        public void Stand(bool f)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            _onGround = f;
        }

        public void Rotate(Angles look)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            _rotated = true;
            _look = look;
        }

        private void RanderFormChanging()
        {
            _RENDERER_MANAGER.ChangeForms(Id, _sneaking, _sprinting);
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
        
        public void Flush()
        {
            _FORCES.Flush();
            _RENDERER_MANAGER.Flush(Id);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;

            // Assertion
            System.Diagnostics.Debug.Assert(_FORCES.Empty);

            if (disposing == true)
            {
                // Release managed resources.
                _FORCES.Dispose();

                _RENDERER_MANAGER.Dispose();
            }

            // Release unmanaged resources.

            _disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            System.GC.SuppressFinalize(this);
        }
    }

    public sealed class ItemEntity : Entity
    {
        private bool _disposed = false;

        internal override BoundingBox GetBoundingBox() => new(0.25f, 0.25f);

        public ItemEntity(
            int id,
            Vector pos, Angles look) : base(id, System.Guid.NewGuid(), pos, look)
        { }

        ~ItemEntity() => System.Diagnostics.Debug.Assert(false);

        protected override void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                // Assertion.

                if (disposing == true)
                {
                    // Release managed resources.
                }

                // Release unmanaged resources.

                _disposed = true;
            }

            base.Dispose(disposing);
        }

    }

    public abstract class LivingEntity : Entity
    {
        private bool _disposed = false;

        internal LivingEntity(
            int id,
            System.Guid uniqueId,
            Vector pos, Angles look) : base(id, uniqueId, pos, look)
        { }

        ~LivingEntity() => System.Diagnostics.Debug.Assert(false);

        protected override void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                // Assertion.

                if (disposing == true)
                {
                    // Release managed resources.
                }

                // Release unmanaged resources.

                _disposed = true;
            }

            base.Dispose(disposing);
        }

    }

    public sealed class Player : LivingEntity
    {
        private bool _disposed = false;

        internal override BoundingBox GetBoundingBox() => new(0.6f, 1.8f);

        public readonly string Username;

        internal readonly SelfInventory _selfInventory;

        private bool _connected = false;
        public bool IsConnected => _connected;

        private Queue<ClientboundPlayingPacket>? _selfRenderer = null;

        private bool _controled;
        private Vector _posControl;
        internal Player(
            int id,
            System.Guid uniqueId,
            Vector pos, Angles look,
            string username) : base(id, uniqueId, pos, look)
        {
            _controled = false;
            /*_posControl = new(0, 0, 0);*/

            Username = username;

            _selfInventory = new();
        }

        ~Player() => System.Diagnostics.Debug.Assert(false);

        internal void Connect(Queue<ClientboundPlayingPacket> selfRenderer)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(!IsConnected);
            _connected = true;

            {
                int payload = new System.Random().Next();
                selfRenderer.Enqueue(new TeleportSelfPlayerPacket(
                    Position.X, Position.Y, Position.Z,
                    Look.Yaw, Look.Pitch,
                    false, false, false, false, false,
                    payload));
            }

            {
                selfRenderer.Enqueue(new SetPlayerAbilitiesPacket(
                    false, false, true, false, 0.1f, 0));
            }

            System.Diagnostics.Debug.Assert(_selfRenderer == null);
            _selfRenderer = selfRenderer;
        }

        internal void Disconnect()
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(IsConnected);
            _connected = false;

            System.Diagnostics.Debug.Assert(_selfRenderer != null);
            _selfRenderer = null;
        }

        public override void AddForce(Vector force)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            if (IsConnected)
            {
                throw new System.NotImplementedException();
            }
            
            base.AddForce(force);

        }

        public override void Teleport(Vector pos, Angles look)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            base.Teleport(pos, look);

            int payload = new System.Random().Next();

            System.Diagnostics.Debug.Assert(_selfRenderer != null);
            _selfRenderer.Enqueue(new TeleportSelfPlayerPacket(
                pos.X, pos.Y, pos.Z,
                look.Yaw, look.Pitch,
                false, false, false, false, false,
                payload));
        }

        protected override Vector Integrate()
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            if (IsConnected)
            {
                if (_controled)
                {
                    _controled = false;
                    return _posControl;
                }
            }
            else
            {
                return base.Integrate();
            }

            return Position;
        }

        internal void Control(Vector pos)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            _controled = true;
            _posControl = pos;
        }

        protected internal override void StartRoutine(long serverTicks, World world)
        {
            if (IsConnected)
            {

            }
            else
            {

            }

            base.StartRoutine(serverTicks, world);
        }

        protected override void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                // Assertion.

                if (disposing == true)
                {
                    // Release managed resources.
                    _selfInventory.Dispose();
                }

                // Release unmanaged resources.

                _disposed = true;
            }

            base.Dispose(disposing);
        }


    }

}

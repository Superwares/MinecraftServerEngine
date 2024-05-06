
using Common;
using Containers;

namespace Protocol
{
    

    public abstract class Entity : System.IDisposable
    {

        public struct Vector : System.IEquatable<Vector>
        {
            private double _x, _y, _z;
            public double X => _x;
            public double Y => _y;
            public double Z => _z;

            public Vector(double x, double y, double z)
            {
                _x = x; _y = y; _z = z;
            }

            public bool Equals(Vector other)
            {
                return Comparing.IsEqualTo(_x, other._x) && 
                    Comparing.IsEqualTo(_y, other._y) &&
                    Comparing.IsEqualTo(_z, other._z);
            }

        }

        public struct Angles : System.IEquatable<Angles>
        {
            internal const float MaxYaw = 180, MinYaw = -180;
            internal const float MaxPitch = 90, MinPitch = -90;

            private float _yaw, _pitch;
            public float Yaw => _yaw;
            public float Pitch => _pitch;

            public Angles(float yaw, float pitch)
            {
                // TODO: map yaw from 180 to -180.
                System.Diagnostics.Debug.Assert(pitch >= MinPitch);
                System.Diagnostics.Debug.Assert(pitch <= MaxPitch);

                _yaw = Frem(yaw);
                _pitch = pitch;
            }

            private static float Frem(float angle)
            {
                float y = 360.0f;
                return angle - (y * (float)System.Math.Floor(angle / y));
            }

            internal (byte, byte) ConvertToPacketFormat()
            {
                System.Diagnostics.Debug.Assert(_pitch >= MinPitch);
                System.Diagnostics.Debug.Assert(_pitch <= MaxPitch);

                _yaw = Frem(_yaw);
                float x = _yaw;
                float y = Frem(_pitch);

                return (
                    (byte)((byte.MaxValue * x) / 360),
                    (byte)((byte.MaxValue * y) / 360));
            }

            public bool Equals(Angles other)
            {
                return Comparing.IsEqualTo(_yaw, other._yaw) &&
                    Comparing.IsEqualTo(_pitch, other._pitch);
            }

        }

        private bool _disposed = false;

        internal abstract BoundingBox GetBoundingBox();

        private int _id;
        public int Id => _id;

        public System.Guid UniqueId;

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

        internal readonly EntityRendererManager _RendererManager = new();  // Disposable

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

        public virtual void AddForce()
        {
            throw new System.NotImplementedException();
        }

        public virtual void Reset()
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            // reset forces
        }

        public virtual bool IsDead()
        {
            return false;
        }

        protected internal virtual void StartRoutine(long serverTicks, World world)
        {

        }

        protected virtual Vector Integrate()
        {
            // update position with velocity, accelaration and forces(gravity, damping).
            throw new System.NotImplementedException();
        }

        protected internal virtual void Move()
        {
            System.Diagnostics.Debug.Assert(!_disposed);
            
            Vector posNew = Integrate();

            bool moved = !posNew.Equals(_pos);  // TODO: Compare with machine epsilon.
            // TODO: Check _pos - _posPrev is lower than 8 blocks.
            // If not, use EntityTeleportPacket to render.
            if (moved && _rotated)
            {
                _RendererManager.MoveAndRotate(Id, posNew, _pos, _look, _onGround);

                _pos = posNew;
                _rotated = false;
            }
            else if (moved)
            {
                System.Diagnostics.Debug.Assert(!_rotated);

                _RendererManager.Move(Id, posNew, _pos, _onGround);

                _pos = posNew;
                _rotated = false;
            }
            else if (_rotated)
            {
                System.Diagnostics.Debug.Assert(!moved);

                _RendererManager.Rotate(Id, _look, _onGround);
            }
            else
            {
                System.Diagnostics.Debug.Assert(!moved);
                System.Diagnostics.Debug.Assert(!_rotated);

                _RendererManager.Stand(Id);
            }

            if (_teleported)
            {
                _pos = _posTeleport;
                _look = _lookTeleport;
                // update position data in chunk

                _RendererManager.Teleport(Id, _pos, _look, _onGround);

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
            _RendererManager.ChangeForms(Id, _sneaking, _sprinting);
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
        
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;

            if (disposing == true)
            {
                // Release managed resources.
                _RendererManager.Dispose();
            }

            // Release unmanaged resources.

            _disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            System.GC.SuppressFinalize(this);
        }

        public void Close() => Dispose();
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

        protected internal override void Move()
        {
            
        }

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
        
        public override void Reset()
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            base.Reset();

            _controled = false;
        }

        public override void Teleport(Vector pos, Angles look)
        {
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
            if (IsConnected)
            {
                return base.Integrate();
            }

            if (_controled)
            {
                return _posControl;
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

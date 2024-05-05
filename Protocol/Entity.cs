
using Containers;

namespace Protocol
{
    internal sealed class EntityRendererManager : System.IDisposable
    {
        private bool _disposed = false;

        private readonly Table<int, EntityRenderer> _Renderers = new();

        public EntityRendererManager() { }

        ~EntityRendererManager() => System.Diagnostics.Debug.Assert(false);

        internal bool ContainsRenderer(int connId)
        {
            System.Diagnostics.Debug.Assert(!_disposed);
            return _Renderers.Contains(connId);
        }

        public void AddRenderer(int connId, EntityRenderer renderer)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(!_Renderers.Contains(connId));
            _Renderers.Insert(connId, renderer);
        }

        public void RemoveRenderer(int connId)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(_Renderers.Contains(connId));
            _Renderers.Extract(connId);
        }

        public void MoveAndRotate(
            int entityId,
            Entity.Vector pos, Entity.Vector posPrev, Entity.Angles look, bool onGround)
        {
            foreach (var renderer in _Renderers.GetValues())
            {
                renderer.MoveAndRotate(entityId, pos, posPrev, look, onGround);
            }
        }

        public void Move(int entityId, Entity.Vector pos, Entity.Vector posPrev, bool onGround)
        {
            foreach (var renderer in _Renderers.GetValues())
            {
                renderer.Move(entityId, pos, posPrev, onGround);
            }
        }

        public void Rotate(int entityId, Entity.Angles look, bool onGround)
        {
            foreach (var renderer in _Renderers.GetValues())
            {
                renderer.Rotate(entityId, look, onGround);
            }
        }

        public void Stand(int entityId)
        {
            foreach (var renderer in _Renderers.GetValues())
            {
                renderer.Stand(entityId);
            }
        }

        public void ChangeForms(int entityId, bool sneaking, bool sprinting)
        {
            foreach (var renderer in _Renderers.GetValues())
            {
                renderer.ChangeForms(entityId, sneaking, sprinting);
            }
        }

        public void Teleport(
            int entityId, Entity.Vector pos, Entity.Angles look, bool onGround)
        {
            foreach (var renderer in _Renderers.GetValues())
            {
                renderer.Teleport(entityId, pos, look, onGround);
            }
        }

        private void Dispose(bool disposing)
        {
            if (_disposed) return;

            System.Diagnostics.Debug.Assert(_Renderers.Empty);

            if (disposing == true)
            {
                // Release managed resources.
                _Renderers.Dispose();
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
                return (_x == other._x) && (_y == other._y) && (_z == other._z);
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
                return (other._yaw == _yaw) && (other._pitch == _pitch);
            }

        }

        private bool _disposed = false;

        internal abstract BoundingBox GetBoundingBox();

        private int _id;
        public int Id => _id;

        public System.Guid UniqueId;

        protected Vector _pos, _posPrev;
        public Vector Position => _pos;

        private bool _rotated = false;
        protected Angles _look;
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

        public virtual void Reset()
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            _posPrev = _pos;
            _rotated = false;
            _teleported = false;

            // reset forces
        }

        public virtual bool IsDead()
        {
            throw new System.NotImplementedException();
        }

        protected internal virtual void StartRoutine(long serverTicks, World world)
        {

        }

        protected internal virtual void Move()
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            // update position with velocity, accelaration and forces(gravity, damping).

            bool moved = !_posPrev.Equals(_pos);  // TODO: Compare with machine epsilon.
            // TODO: Check _pos - _posPrev is lower than 8 blocks.
            // If not, use EntityTeleportPacket to render.
            if (moved && _rotated)
            {
                _RendererManager.MoveAndRotate(Id, _pos, _posPrev, _look, _onGround);
            }
            else if (moved)
            {
                System.Diagnostics.Debug.Assert(!_rotated);

                _RendererManager.Move(Id, _pos, _posPrev, _onGround);
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
            }
        }

        public void Teleport(Vector pos, Angles look)
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
                    false, true, true, false, 0.1f, 0));
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

        internal void Control(Vector pos)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            _controled = true;
            _posControl = pos;

            // TODO: send render data;
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

        protected internal override void Move()
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            if (_controled)
            {
                _pos = _posControl;
            }

            base.Move();

            if (!IsConnected) return;

            if (_teleported)
            {
                int payload = new System.Random().Next();

                System.Diagnostics.Debug.Assert(_selfRenderer != null);
                _selfRenderer.Enqueue(new TeleportSelfPlayerPacket(
                    _pos.X, _pos.Y, _pos.Z,
                    _look.Yaw, _look.Pitch,
                    false, false, false, false, false,
                    payload));
            }
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

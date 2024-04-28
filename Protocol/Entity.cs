
using System.Diagnostics;
using System;
using Containers;

namespace Protocol
{
    public abstract class Entity : IDisposable
    {

        public struct Vector : IEquatable<Vector>
        {
            private double _x, _y, _z;
            public double X => _x;
            public double Y => _y;
            public double Z => _z;

            public Vector(double x, double y, double z)
            {
                _x = x; _y = y; _z = z;
            }

            internal void Set(double x, double y, double z)
            {
                _x = x; _y = y; _z = z;
            }

            public bool Equals(Vector other)
            {
                return (_x == other._x) && (_y == other._y) && (_z == other._z);
            }

        }

        public struct Angles : IEquatable<Angles>
        {
            internal const float MaxYaw = 180, MinYaw = -180;
            internal const float MaxPitch = 90, MinPitch = -90;

            private static float Frem(float angle)
            {
                float y = 360.0f;
                return angle - (y * (float)Math.Floor(angle / y));
            }

            private float _yaw, _pitch;
            public float Yaw => _yaw;
            public float Pitch => _pitch;

            public Angles(float yaw, float pitch)
            {
                // TODO: map yaw from 180 to -180.
                Debug.Assert(pitch >= MinPitch);
                Debug.Assert(pitch <= MaxPitch);

                _yaw = Frem(yaw);
                _pitch = pitch;
            }

            internal (byte, byte) ConvertToProtocolFormat()
            {
                Debug.Assert(_pitch >= MinPitch);
                Debug.Assert(_pitch <= MaxPitch);

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

        private int _id;
        private readonly NumList _EntityIdList;
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

        internal readonly EntityRenderer _Renderer = new();  // Disposable

        internal Entity(
            NumList entityIdList,
            Guid uniqueId,
            Vector pos, Angles look)
        {
            _id = entityIdList.Alloc();
            _EntityIdList = entityIdList;

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
            Debug.Assert(!_disposed);

            _posPrev = _pos;
            _rotated = false;
            _teleported = false;

            // reset forces
        }

        public virtual bool IsDead()
        {
            throw new NotImplementedException();
        }

        public virtual void StartRoutine(long serverTicks, World world)
        {
        }

        public virtual void Move(World world)
        {
            Debug.Assert(!_disposed);

            // update position with velocity, accelaration and forces(gravity, damping).

            bool moved = !_posPrev.Equals(_pos);  // TODO: Compare with machine epsilon.
            // TODO: Check _pos - _posPrev is lower than 8 blocks.
            // If not, use EntityTeleportPacket to render.
            if (moved && _rotated)
            {
                (byte x, byte y) = _look.ConvertToProtocolFormat();
                _Renderer.Render(new EntityLookAndRelMovePacket(
                    Id,
                    (short)((_pos.X - _posPrev.X) * 32 * 128),
                    (short)((_pos.Y - _posPrev.Y) * 32 * 128),
                    (short)((_pos.Z - _posPrev.Z) * 32 * 128),
                    x, y, 
                    _onGround));
                _Renderer.Render(new EntityHeadLookPacket(Id, x));
            }
            else if (moved)
            {
                Debug.Assert(!_rotated);

                _Renderer.Render(new EntityRelMovePacket(
                    Id,
                    (short)((_pos.X - _posPrev.X) * 32 * 128),
                    (short)((_pos.Y - _posPrev.Y) * 32 * 128),
                    (short)((_pos.Z - _posPrev.Z) * 32 * 128),
                    _onGround));
            }
            else if (_rotated)
            {
                Debug.Assert(!moved);

                (byte x, byte y) = _look.ConvertToProtocolFormat();
                _Renderer.Render(new EntityLookPacket(Id, x, y, _onGround));
                _Renderer.Render(new EntityHeadLookPacket(Id, x));
            }
            else
            {
                Debug.Assert(!moved);
                Debug.Assert(!_rotated);

                _Renderer.Render(new EntityPacket(Id));
            }

            if (_teleported)
            {
                _pos = _posTeleport;
                _look = _lookTeleport;
                // update position data in chunk

                _Renderer.Render(new EntityTeleportPacket(
                    Id,
                    _pos.X, _pos.Y, _pos.Z,
                    _look.Yaw, _look.Pitch,
                    _onGround));
            }

            world.UpdateEntityRendering(this);
        }

        public void Teleport(Vector pos, Angles look)
        {
            _teleported = true;
            _posTeleport = pos;
            _lookTeleport = look;
        }

        public void Stand(bool f)
        {
            Debug.Assert(!_disposed);

            _onGround = f;

            _Renderer.Render(new EntityPacket(Id));
        }

        public void Rotate(Angles look)
        {
            Debug.Assert(!_disposed);

            _rotated = true;
            _look = look;

            /*(byte x, byte y) = _look.ConvertToProtocolFormat();*/
            /*Console.Write($"x: {x}, y: {y} ");*/
            /*Render(new EntityLookPacket(Id, x, y, _onGround));*/
        }

        private void RanderFormChanging()
        {
            byte flags = 0x00;

            if (_sneaking)
                flags |= 0x02;
            if (_sprinting)
                flags |= 0x08;

            using EntityMetadata metadata = new();
            metadata.AddByte(0, flags);

            _Renderer.Render(new EntityMetadataPacket(Id, metadata.WriteData()));
        }

        public void Sneak()
        {
            Debug.Assert(!_disposed);

            Debug.Assert(!_sneaking);
            _sneaking = true;

            RanderFormChanging();
        }

        public void Unsneak()
        {
            Debug.Assert(!_disposed);

            Debug.Assert(_sneaking);
            _sneaking = false;

            RanderFormChanging();
        }

        public void Sprint()
        {
            Debug.Assert(!_disposed);

            Debug.Assert(!_sprinting);
            _sprinting = true;

            RanderFormChanging();
        }

        public void Unsprint()
        {
            Debug.Assert(!_disposed);

            Debug.Assert(_sprinting);
            _sprinting = false;

            RanderFormChanging();
        }

        internal virtual void Flush()
        {
            _EntityIdList.Dealloc(Id);
            _Renderer.Flush();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;

            if (disposing == true)
            {
                // Release managed resources.
                _Renderer.Dispose();
            }

            // Release unmanaged resources.

            _disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void Close() => Dispose();
    }

    public abstract class LivingEntity : Entity
    {
        private bool _disposed = false;

        internal LivingEntity(
            NumList entityIdList,
            Guid uniqueId,
            Vector pos, Angles look) : base(entityIdList, uniqueId, pos, look)
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

        public readonly string Username;


        private Connection? _conn = null;
        public bool IsConnected => _conn != null;

        private bool _controled;
        private Vector _posControl;

        private readonly PlayerList _PlayerList;

        internal Player(
            NumList entityIdList,
            Guid uniqueId,
            Vector pos, Angles look,
            string username,
            PlayerList playerList) : base(entityIdList, uniqueId, pos, look)
        {
            _controled = false;
            /*_posControl = new(0, 0, 0);*/

            Username = username;

            _PlayerList = playerList;
        }

        ~Player() => System.Diagnostics.Debug.Assert(false);

        internal void Connect(Connection conn)
        {
            Debug.Assert(!_disposed);

            Debug.Assert(!IsConnected);
            _conn = conn;

            conn.Render(new SetPlayerAbilitiesPacket(
                true, false, true, true, 0.1f, 0));

            int payload = new System.Random().Next();
            conn.Render(new TeleportSelfPlayerPacket(
                _pos.X, _pos.Y, _pos.Z,
                _look.Yaw, _look.Pitch,
                false, false, false, false, false,
                payload));

            _PlayerList.Connect(UniqueId, conn.Renderer);
        }

        internal void Disconnect()
        {
            Debug.Assert(!_disposed);

            Debug.Assert(IsConnected);

            Debug.Assert(_conn != null);
            _conn.Flush();
            _conn.Close();
            _conn = null;

            _PlayerList.Disconnect(UniqueId);
        }
        
        public override void Reset()
        {
            Debug.Assert(!_disposed);

            base.Reset();

            _controled = false;
        }

        internal void Control(Vector pos)
        {
            Debug.Assert(!_disposed);

            _controled = true;
            _posControl = pos;

            // TODO: send render data;
        }

        public override void StartRoutine(long serverTicks, World world)
        {
            if (IsConnected)
            {
                Debug.Assert(_conn != null);
                _conn.RenderChunks(world, Position);
                _conn.RenderEntities(world, Id);

                try
                {
                    _conn.SendData(this);
                    _conn.Control(serverTicks, world, _PlayerList, this);
                }
                catch (DisconnectedClientException)
                {
                    Disconnect();

                    throw new NotImplementedException();
                }

            }
            else
            {

            }

            /*base.StartRoutine(world);*/
        }

        public override void Move(World world)
        {
            Debug.Assert(!_disposed);

            if (_controled)
            {
                _pos = _posControl;
            }

            base.Move(world);

            if (!IsConnected) return;

            if (_teleported)
            {
                int payload = new Random().Next();
                System.Diagnostics.Debug.Assert(_conn != null);
                _conn.Render(new TeleportSelfPlayerPacket(
                    _pos.X, _pos.Y, _pos.Z,
                    _look.Yaw, _look.Pitch,
                    false, false, false, false, false,
                    payload));
            }
        }

        internal override void Flush()
        {
            base.Flush();

            _PlayerList.ClosePlayer(UniqueId);
        }

        protected override void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                // Assertion.
                Debug.Assert(_conn == null);  // Must be disconnected when Dispose();

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

}

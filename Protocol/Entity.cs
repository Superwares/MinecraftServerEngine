
using System.Diagnostics;
using System;
using Containers;

namespace Protocol
{
    public abstract class Entity : IDisposable
    {
        private bool _disposed = false;

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

        internal readonly Queue<Queue<ClientboundPlayingPacket>> _renderers = new();

        private readonly int _Id;
        public int Id => _Id;

        private readonly Guid _UniqueId;
        public Guid UniqueId => _UniqueId;

        protected Vector _pos, _posPrev;
        private bool _rotated = false;
        protected Angles _look;
        public Vector Position => _pos;
        public Angles Look => _look;

        protected bool _onGround;

        protected bool _sneaking, _sprinting;
        public bool IsSneaking => _sneaking;
        public bool IsSprinting => _sprinting;

        protected bool _teleported;
        protected Vector _posTeleport;
        protected Angles _lookTeleport;

        internal Entity(
            IUpdateOnlyEntityIdList entityIdList,
            IUpdateOnlyEntityRenderingTable entitySearchTable,
            Guid uniqueId,
            Vector pos, Angles look)
            : this(
                  entityIdList, entitySearchTable,
                  entityIdList.Alloc(),
                  uniqueId,
                  pos, look)
        { }

        internal Entity(
            IUpdateOnlyEntityIdList entityIdList,
            IUpdateOnlyEntityRenderingTable entitySearchTable,
            int id,
            Guid uniqueId,
            Vector pos, Angles look)
        {
            _EntityIdList = entityIdList;
            _EntitySearchTable = entitySearchTable;

            _Id = id;

            _UniqueId = uniqueId;
            _pos = pos;
            _look = look;
            _onGround = false;

            _sneaking = _sprinting = false;

            _teleported = false;
            _posTeleport = new(0, 0, 0);
            _lookTeleport = new(0, 0);

            _EntitySearchTable.Init(this);
        }

        ~Entity() => Debug.Assert(false);

        internal void AddRenderer(int id, Queue<ClientboundPlayingPacket> outPackets)
        {
            // TODO spawn
            _renderers.Enqueue(outPackets);
        }

        internal void RemoveRenderer(int id)
        {
            // extract renderer and despawn to that renderer
        }

        private protected void Render(ClientboundPlayingPacket packet)
        {
            foreach (var outPackets in _renderers.GetValues())
                outPackets.Enqueue(packet);
        }

        private protected abstract void Spawn(Queue<ClientboundPlayingPacket> outPackets);

        void IRenderOnlyEntity.Spawn(Queue<ClientboundPlayingPacket> outPackets)
        {
            Spawn(outPackets);
        }

        public virtual void Reset()
        {
            Debug.Assert(!_disposed);

            _posPrev = _pos;
            _rotated = false;
            _teleported = false;

            // reset forces
        }

        public virtual void IsDead()
        {
            throw new NotImplementedException();
        }

        public virtual void StartRoutine(World world)
        {
            throw new System.NotImplementedException();

            // must entity is alive when call this method.
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
                Render(new EntityLookAndRelMovePacket(
                    Id,
                    (short)((_pos.X - _posPrev.X) * 32 * 128),
                    (short)((_pos.Y - _posPrev.Y) * 32 * 128),
                    (short)((_pos.Z - _posPrev.Z) * 32 * 128),
                    x, y, 
                    _onGround));
                Render(new EntityHeadLookPacket(Id, x));
            }
            else if (moved)
            {
                Debug.Assert(!_rotated);

                Render(new EntityRelMovePacket(
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
                Render(new EntityLookPacket(Id, x, y, _onGround));
                Render(new EntityHeadLookPacket(Id, x));
            }
            else
            {
                Debug.Assert(!moved);
                Debug.Assert(!_rotated);

                Render(new EntityPacket(Id));
            }

            if (_teleported)
            {
                _pos = _posTeleport;
                _look = _lookTeleport;
                // update position data in chunk

                Render(new EntityTeleportPacket(
                    Id,
                    _pos.X, _pos.Y, _pos.Z,
                    _look.Yaw, _look.Pitch,
                    _onGround));
            }

            world.Update(_Id, _pos);
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

            Render(new EntityPacket(Id));
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

            Render(new EntityMetadataPacket(Id, metadata.WriteData()));
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

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;

            Debug.Assert(_renderers.Empty);

            if (disposing == true)
            {
                // Release managed resources.
                _renderers.Dispose();
            }

            // Release unmanaged resources.

            _disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public virtual void Close(EntityIdList idList, World world)
        {
            _EntityIdList.Dealloc(Id);
            _EntitySearchTable.Close(Id);
            _renderers.Flush();  // TODO: Release resources for no garbage.
            Dispose();
        }
    }

    public abstract class LivingEntity : Entity
    {
        private bool _disposed = false;

        internal LivingEntity(
            IUpdateOnlyEntityIdList entityIdList,
            IUpdateOnlyEntityRenderingTable entitySearchTable,
            int id,
            Guid uniqueId,
            Vector pos, Angles look)
            : base(entityIdList, entitySearchTable, id, uniqueId, pos, look)
        { }

        ~LivingEntity()
        {
            Debug.Assert(false);
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

    public sealed class Player : LivingEntity
    {
        private bool _disposed = false;

        private readonly IUpdateOnlyPlayerList _playerList;

        public readonly string Username;

        private Queue<ClientboundPlayingPacket>? _selfOutPackets;
        private bool _connected;
        public bool IsConnected => _connected;

        private bool _controled;
        private Vector _posControl;

        internal Player(
            IUpdateOnlyEntityIdList entityIdList,
            IUpdateOnlyEntityRenderingTable entitySearchTable,
            int id,
            Guid uniqueId,
            Vector pos, Angles look,
            IUpdateOnlyPlayerList playerList,
            string username)
            : base(entityIdList, entitySearchTable, id, uniqueId, pos, look)
        {
            _controled = false;
            _posControl = new(0, 0, 0);

            _playerList = playerList;
            Username = username;

            playerList.Add(UniqueId, Username);

            _connected = false;
            _selfOutPackets = null;

            Teleport(pos, look);
        }

        ~Player()
        {
            Debug.Assert(false);
        }

        internal void Connect(Queue<ClientboundPlayingPacket> selfOutPackets)
        {
            Debug.Assert(!_disposed);

            Debug.Assert(!_connected);
            _connected = true;
            _selfOutPackets = selfOutPackets;
        }

        public void Disconnect()
        {
            Debug.Assert(!_disposed);

            Debug.Assert(_connected);
            _connected = false;
            _selfOutPackets = null;
        }

        private void RenderSelf(ClientboundPlayingPacket packet)
        {
            Debug.Assert(!_disposed);

            Debug.Assert(_selfOutPackets != null);
            _selfOutPackets.Enqueue(packet);
        }

        private protected override void Spawn(Queue<ClientboundPlayingPacket> outPackets)
        {
            /*Console.WriteLine("Spawn!");*/

            byte flags = 0x00;

            if (IsSneaking)
                flags |= 0x02;
            if (IsSprinting)
                flags |= 0x08;

            using EntityMetadata metadata = new();
            metadata.AddByte(0, flags);

            (byte x, byte y) = _look.ConvertToProtocolFormat();
            outPackets.Enqueue(new SpawnNamedEntityPacket(
                Id,
                UniqueId,
                Position.X, Position.Y, Position.Z,
                x, y,  // TODO: Convert yaw and pitch to angles of minecraft protocol.
                metadata.WriteData()));

        }

        public override void Reset()
        {
            Debug.Assert(!_disposed);

            base.Reset();

            _controled = false;
        }

        public void Control(Vector pos)
        {
            Debug.Assert(!_disposed);

            _controled = true;
            _posControl = pos;

            // TODO: send render data;
        }

        public override void Move()
        {
            Debug.Assert(!_disposed);

            if (_controled)
            {
                _pos = _posControl;

                /*(byte x, byte y) = _look.ConvertToProtocolFormat();
                Render(new EntityLookAndRelMovePacket(
                    Id,
                    (short)((_pos.X - posPrev.X) * 32 * 128),
                    (short)((_pos.Y - posPrev.Y) * 32 * 128),
                    (short)((_pos.Z - posPrev.Z) * 32 * 128),
                    x, y,
                    _onGround));*/
            }

            base.Move();

            if (!_connected) return;

            if (_teleported)
            {
                int payload = new Random().Next();
                RenderSelf(new TeleportPacket(
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
                Debug.Assert(!_connected);

                if (disposing == true)
                {
                    // Release managed resources.
                    _selfOutPackets = null;
                }

                // Release unmanaged resources.

                _disposed = true;
            }

            base.Dispose(disposing);
        }

        public override void Close()
        {
            _playerList.Remove(UniqueId);

            base.Close();
        }

    }

}

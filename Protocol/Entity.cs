
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

            public static Vector operator* (Vector v, double s)
            {
                return new(v.X * s, v.Y * s, v.Z * s);
            }

            public static Vector operator* (double s, Vector v)
            {
                return new(s * v.X, s * v.Y, s * v.Z);
            }

            public static Vector operator/ (Vector v, double s)
            {
                return new(v.X / s, v.Y / s, v.Z / s);
            }

            public static Vector operator/ (double s, Vector v)
            {
                return new(s / v.X, s / v.Y, s / v.Z);
            }

            public static double GetLengthSquared(Vector v1, Vector v2)
            {
                double dx = v1.X - v2.X,
                       dy = v1.Y - v2.Y,
                       dz = v1.Z - v2.Z;
                return (dx * dx) + (dy * dy) + (dz * dz);
            }

            public static double GetLength(Vector v1, Vector v2)
            {
                return System.Math.Sqrt(GetLengthSquared(v1, v2));
            }

            public readonly double X, Y, Z;

            public Vector(double x, double y, double z)
            {
                X = x; Y = y; Z = z;
            }

            public override readonly string? ToString()
            {
                return $"( X: {X}, Y: {Y}, Z: {Z} )";
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

            public override readonly string? ToString()
            {
                throw new System.NotImplementedException();
            }

            public bool Equals(Angles other)
            {
                return Comparing.IsEqualTo(Yaw, other.Yaw) &&
                    Comparing.IsEqualTo(Pitch, other.Pitch);
            }

        }

        private const double _MAX_MOVEMENT_LENGTH = 0.4;  // at one tick.

        private bool _disposed = false;

        private int _id;
        public int Id => _id;

        public System.Guid UniqueId;

        public abstract BoundingBox GetBoundingBox();

        public abstract double GetMass();

        private readonly Queue<Vector> _FORCES;

        private Vector _v;
        public Vector Velocity => _v;


        private Vector _p;
        public Vector Position => _p;


        private bool _rotated;
        private Angles _look;
        public Angles Look => _look;


        protected bool _onGround;
        public bool IsOnGround => _onGround;


        protected bool _sneaking, _sprinting;
        public bool IsSneaking => _sneaking;
        public bool IsSprinting => _sprinting;


        /*protected bool _teleported;
        protected Vector _posTeleport;
        protected Angles _lookTeleport;*/


        private readonly EntityRendererManager _RENDERER_MANAGER = new();  // Disposable

        internal Entity(
            int id, System.Guid uniqueId,
            Vector pos, Angles look)
        {
            _id = id;

            UniqueId = uniqueId;

            _FORCES = new Queue<Vector>();

            _v = new Vector(0, 0, 0);

            _p = pos;

            _rotated = false;
            _look = look;

            _onGround = false;

            _sneaking = _sprinting = false;

            /*_teleported = false;*/
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

        public void ApplyBaseForce(Vector force)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            _FORCES.Enqueue(force);
        }

        public virtual void ApplyForce(Vector force)
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

        public virtual Vector Integrate()
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            Vector pos = _p;

            while (!_FORCES.Empty)
            {
                Vector force = _FORCES.Dequeue();

                _v += (force / GetMass());
            }

            pos += _v;
            /*System.Console.WriteLine($"pos: ({pos.X}, {pos.Y}, {pos.Z})");*/

            return pos;
        }

        internal void UpdateMovement(Vector p, bool onGround)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(_FORCES.Empty);

            /*System.Console.WriteLine($"p: {p}");*/

            bool moved = !p.Equals(_p);  // TODO: Compare with machine epsilon.

            if (moved)
            {
                /*System.Diagnostics.Debug.Assert(Vector.GetLength(p, _p) < _MAX_MOVEMENT_LENGTH);*/
            }

            if (moved && _rotated)
            {
                _RENDERER_MANAGER.MoveAndRotate(Id, p, _p, _look, onGround);

                _p = p;
                _rotated = false;
            }
            else if (moved)
            {
                System.Diagnostics.Debug.Assert(!_rotated);

                _RENDERER_MANAGER.Move(Id, p, _p, onGround);

                _p = p;
            }
            else if (_rotated)
            {
                System.Diagnostics.Debug.Assert(!moved);

                _RENDERER_MANAGER.Rotate(Id, _look, onGround);

                _rotated = false;
            }
            else
            {
                System.Diagnostics.Debug.Assert(!moved);
                System.Diagnostics.Debug.Assert(!_rotated);

                _RENDERER_MANAGER.Stand(Id);
            }

            _onGround = onGround;

            _RENDERER_MANAGER.DeterminToContinueRendering(Id, _p, GetBoundingBox());

            /*if (_teleported)
            {
                _pos = _posTeleport;
                _look = _lookTeleport;
                // update position data in chunk

                _RENDERER_MANAGER.Teleport(Id, _pos, _look, _onGround);

                _teleported = false;
            }*/
        }

        /*public virtual void Teleport(Vector pos, Angles look)
        {
            _teleported = true;
            _posTeleport = pos;
            _lookTeleport = look;
        }*/

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

        public virtual void Dispose()
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            // Assertion
            System.Diagnostics.Debug.Assert(_FORCES.Empty);

            // Release resources.
            _FORCES.Dispose();

            _RENDERER_MANAGER.Dispose();

            // Finish.
            System.GC.SuppressFinalize(this);
            _disposed = true;
        }

    }

    public sealed class ItemEntity : Entity
    {
        private bool _disposed = false;


        public readonly BoundingBox BOUNDING_BOX = new(0.25f, 0.25f);
        public override BoundingBox GetBoundingBox() => BOUNDING_BOX;


        public const double MASS = 0.2;
        public override double GetMass() => MASS;


        public ItemEntity(
            int id,
            Vector pos, Angles look) : base(id, System.Guid.NewGuid(), pos, look)
        { }

        ~ItemEntity() => System.Diagnostics.Debug.Assert(false);

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


        internal LivingEntity(
            int id, System.Guid uniqueId,
            Vector pos, Angles look) : base(id, uniqueId, pos, look)
        { }

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

    public sealed class Player : LivingEntity
    {
        private bool _disposed = false;


        public readonly BoundingBox BOUNDING_BOX = new(0.6f, 1.8f);
        public override BoundingBox GetBoundingBox() => BOUNDING_BOX;


        public const double MASS = 1.0;
        public override double GetMass() => MASS;


        public readonly string Username;


        internal readonly SelfInventory _selfInventory;


        private bool _connected = false;
        public bool IsConnected => _connected;


        private SelfPlayerRenderer? _selfRenderer;

        private readonly Queue<Vector> _FORCES;
        private Vector _v;

        private bool _controled;
        private Vector _p;

        internal Player(
            int id, System.Guid uniqueId,
            Vector p, Angles look,
            string username) : base(id, uniqueId, p, look)
        {

            Username = username;

            _selfInventory = new();

            _selfRenderer = null;

            _FORCES = new();
            _v = new(0, 0, 0);

            _controled = false;
            _p = p;
        }

        ~Player() => System.Diagnostics.Debug.Assert(false);

        internal void Connect(SelfPlayerRenderer renderer)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(!IsConnected);
            _connected = true;

            System.Diagnostics.Debug.Assert(_selfRenderer == null);
            _selfRenderer = renderer;

            _p = Position;

            renderer.Init(Id, _v, _p, Look);
        }

        internal void Disconnect()
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(IsConnected);
            _connected = false;

            System.Diagnostics.Debug.Assert(_selfRenderer != null);
            _selfRenderer = null;
        }

        public override void ApplyForce(Vector force)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            if (IsConnected)
            {
                System.Diagnostics.Debug.Assert(_selfRenderer != null);
                _selfRenderer.ApplyVelocity(Id, force / GetMass());
            }

            _FORCES.Enqueue(force);

            base.ApplyForce(force);

        }

        /*public override void Teleport(Vector pos, Angles look)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            base.Teleport(pos, look);

            System.Diagnostics.Debug.Assert(_selfRenderer != null);
            _selfRenderer.Teleport(pos, look);
            // TODO: Check the velocity was reset in client when teleported.
        }*/

        public override Vector Integrate()
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            Vector p = base.Integrate();

            while (!_FORCES.Empty)
            {
                Vector force = _FORCES.Dequeue();

                _v += (force / GetMass());
            }

            if (IsConnected)
            {
                // TODO: Check the difference between _p and p. and predict movement....
                /*System.Console.WriteLine($"p: {p}, _p: {_p}, ");*/
                System.Console.WriteLine($"Length: {Vector.GetLength(p, _p)}");
                /*if (Vector.GetLength(p, _p) > k)
                {
                }*/


                if (_controled)
                {
                    System.Console.WriteLine("Hello!");
                    _controled = false;
                    return _p;
                }
            }

            return p;
        }

        internal void Control(Vector p)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(IsConnected);

            _controled = true;
            _p = p;
        }

        protected internal override void StartRoutine(long serverTicks, World world)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            if (IsConnected)
            {

            }
            else
            {

            }

            base.StartRoutine(serverTicks, world);
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

}

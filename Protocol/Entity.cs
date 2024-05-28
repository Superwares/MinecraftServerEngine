
using Common;
using Containers;
using static Protocol.Entity;

namespace Protocol
{
    public abstract class Entity : System.IDisposable
    {

        /*public readonly struct Vector : System.IEquatable<Vector>
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

        public class Grid : System.IEquatable<Grid>
        {
            public static Grid Generate(Block.Location p, BoundingBox bb)
            {
                Vector min = new(
                    Conversions.ToDouble(p.X),
                    Conversions.ToDouble(p.Y),
                    Conversions.ToDouble(p.Z));
                Vector max = new(
                    Conversions.ToDouble(p.X) + bb.Width,
                    Conversions.ToDouble(p.Y) + bb.Height,
                    Conversions.ToDouble(p.Z) + bb.Width);

                return new(max, min);
            }

            public static Grid Generate(Grid g1, Grid g2)
            {
                double xMax = System.Math.Max(g1.MAX.X, g2.MAX.X),
                       xMin = System.Math.Min(g1.MIN.X, g2.MIN.X);
                System.Diagnostics.Debug.Assert(Comparing.IsGreaterThan(xMax, xMin));

                double yMax = System.Math.Max(g1.MAX.Y, g2.MAX.Y),
                       yMin = System.Math.Min(g1.MIN.Y, g2.MIN.Y);
                System.Diagnostics.Debug.Assert(Comparing.IsGreaterThan(yMax, yMin));

                double zMax = System.Math.Max(g1.MAX.Z, g2.MAX.Z),
                       zMin = System.Math.Min(g1.MIN.Z, g2.MIN.Z);
                System.Diagnostics.Debug.Assert(Comparing.IsGreaterThan(zMax, zMin));

                return new(new(xMax, yMax, zMax), new(xMin, yMin, zMin));
            }

            public static Grid Generate(Vector p, BoundingBox bb)
            {
                double h = bb.Width / 2.0D;
                Vector min = new(
                        p.X - h, p.Y, p.Z - h),
                       max = new(
                        p.X + h, p.Y + bb.Height, p.Z + h);

                return new(max, min);
            }

            public readonly Vector MAX, MIN;

            public Grid(Vector max, Vector min)
            {
                System.Diagnostics.Debug.Assert(
                    Comparing.IsGreaterThanOrEqualTo(max.X, min.X));
                System.Diagnostics.Debug.Assert(
                    Comparing.IsGreaterThanOrEqualTo(max.Y, min.Y));
                System.Diagnostics.Debug.Assert(
                    Comparing.IsGreaterThanOrEqualTo(max.Z, min.Z));

                MAX = max; MIN = min;
            }

            public Vector GetCenter()
            {
                return new((MAX.X + MIN.X) / 2.0D, (MAX.Y + MIN.Y) / 2.0D, (MAX.Z + MIN.Z) / 2.0D);
            }

            public double GetHeight()
            {
                double h = (MAX.Y - MIN.Y);
                System.Diagnostics.Debug.Assert(h >= 0);
                return h;
            }

            public bool Contains(Vector p)
            {
                return (
                    p.X <= MAX.X && p.X >= MIN.X &&
                    p.Y <= MAX.Y && p.Y >= MIN.Y &&
                    p.Z <= MAX.Z && p.Z >= MIN.Z);
            }

            public bool IsOverlapped(Grid other)
            {
                double xMax = System.Math.Min(MAX.X, other.MAX.X),
                       xMin = System.Math.Max(MIN.X, other.MIN.X);
                if (Comparing.IsLessThan(xMax, xMin))
                {
                    return false;
                }

                double yMax = System.Math.Min(MAX.Y, other.MAX.Y),
                       yMin = System.Math.Max(MIN.Y, other.MIN.Y);
                if (Comparing.IsLessThan(yMax, yMin))
                {
                    return false;
                }

                double zMax = System.Math.Min(MAX.Z, other.MAX.Z),
                       zMin = System.Math.Max(MIN.Z, other.MIN.Z);
                if (Comparing.IsLessThan(zMax, zMin))
                {
                    return false;
                }

                return true;
            }

            public override string? ToString()
            {
                return $"( MAX: {MAX}, MIN: {MIN} )";
            }

            public bool Equals(Grid? other)
            {
                if (other == null)
                {
                    return false;
                }

                return MAX.Equals(other.MAX) && MIN.Equals(other.MIN);
            }
        }*/

        public readonly struct Hitbox
        {
            public readonly double Width, Height;
            /*public readonly double EyeHeight, MaxStepHeight;*/

            public Hitbox(double w, double h)
            {
                Width = w;
                Height = h;
            }

            public BoundingBox Convert(Vector p)
            {
                double w = Width / 2.0D;

                Vector max = new(p.X + w, p.Y + Height, p.Z + w),
                       min = new(p.X - w, p.Y, p.Z - w);
                return new(max, min);
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

        private bool _disposed = false;

        private int _id;
        public int Id => _id;

        public System.Guid UniqueId;


        public abstract double GetMass();


        private readonly Queue<Vector> _FORCES;
        public Queue<Vector> Forces => _FORCES;

        private Vector _v;
        public Vector Velocity => _v;

        private Vector _p;
        public Vector Position => _p;


        private bool _rotated;
        private Angles _look;
        public Angles Look => _look;


        protected bool _sneaking, _sprinting;
        public bool IsSneaking => _sneaking;
        public bool IsSprinting => _sprinting;


        public abstract Hitbox GetHitbox();

        private BoundingBox _bb;
        public BoundingBox BB => _bb;

        /*protected bool _teleported;
        protected Vector _posTeleport;
        protected Angles _lookTeleport;*/


        private readonly EntityRendererManager _RENDERER_MANAGER = new();  // Disposable

        internal Entity(
            int id, System.Guid uniqueId, Vector p, Angles look)
        {
            _id = id;

            UniqueId = uniqueId;

            _FORCES = new Queue<Vector>();

            _v = new Vector(0, 0, 0);

            _p = p;

            _rotated = false;
            _look = look;

            _sneaking = _sprinting = false;

            Hitbox hitbox = GetHitbox();
            _bb = hitbox.Convert(_p);

            /*_teleported = false;*/
            /*_posTeleport = new(0, 0, 0);
            _lookTeleport = new(0, 0);*/
        }

        ~Entity() => System.Diagnostics.Debug.Assert(false);

        internal abstract void Spawn(Queue<ClientboundPlayingPacket> outPackets);

        internal void ApplyRenderer(
            Queue<ClientboundPlayingPacket> outPackets, 
            int id, EntityRenderer renderer)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            bool exists = _RENDERER_MANAGER.Apply(id, renderer);
            if (!exists)
            {
                Spawn(outPackets);
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

        protected internal virtual void StartRoutine(long serverTicks, World world)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

        }

        internal (BoundingBox, Vector) Integrate()
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            Vector v = _v;
            /*System.Console.WriteLine();
            System.Console.WriteLine($"p: ({p.X}, {p.Y}, {p.Z})");*/

            while (!_FORCES.Empty)
            {
                Vector force = _FORCES.Dequeue();

                v += (force / GetMass());
            }

            /*p += v;*/

            /*System.Console.WriteLine($"v: {v}, p: {p}, ");*/

            Hitbox hitbox = GetHitbox();
            BoundingBox bb = hitbox.Convert(_p);

            return (bb, v);
        }
        
        public virtual void Move(BoundingBox bb, Vector v, bool onGround)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(_FORCES.Empty);

            Vector p = bb.GetBottomCenter();

            _RENDERER_MANAGER.HandleRendering(Id, _p);

            bool moved = !p.Equals(_p);  // TODO: Compare with machine epsilon.
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

            _v = v;
            _bb = bb;

            _RENDERER_MANAGER.FinishMovementRenderring();

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
        
        public virtual void Flush()
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            _RENDERER_MANAGER.Flush(Id);
        }

        public virtual void Dispose()
        {
            // Assertion
            System.Diagnostics.Debug.Assert(!_disposed);

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


        public ItemEntity(int id, Vector pos, Angles look) 
            : base(id, System.Guid.NewGuid(), pos, look) { }

        ~ItemEntity() => System.Diagnostics.Debug.Assert(false);

        internal override void Spawn(Queue<ClientboundPlayingPacket> outPackets)
        {
            (byte x, byte y) = Look.ConvertToPacketFormat();
            outPackets.Enqueue(new SpawnObjectPacket(
                Id, UniqueId, 2,
                Position.X, Position.Y, Position.Z,
                x, y,
                1, 0, 0, 0));

            using EntityMetadata metadata = new();
            metadata.AddBool(5, true);
            metadata.AddSlotData(6, new SlotData(280, 1));
            outPackets.Enqueue(new EntityMetadataPacket(
                Id, metadata.WriteData()));
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

        internal LivingEntity(int id, System.Guid uniqueId, Vector pos, Angles look) 
            : base(id, uniqueId, pos, look) { }

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


        public const double MASS = 1.0D;
        public override double GetMass() => MASS;


        public override Hitbox GetHitbox()
        {
            double w = 0.6D, h;
            if (IsSneaking)
            {
                h = 1.65D;
            }
            else
            {
                h = 1.8D;
            }

            return new(w, h);
        }


        public readonly string Username;


        internal readonly SelfInventory _selfInventory;


        private bool _connected = false;
        public bool IsConnected => _connected;


        private SelfPlayerRenderer? _selfRenderer;

        private Vector _p;
        private bool _onGround;

        internal Player(
            int id, System.Guid uniqueId, Vector p, Angles look, string username) 
            : base(id, uniqueId, p, look)
        {

            Username = username;

            _selfInventory = new();

            _selfRenderer = null;

        }

        ~Player() => System.Diagnostics.Debug.Assert(false);

        internal override void Spawn(Queue<ClientboundPlayingPacket> outPackets)
        {
            byte flags = 0x00;

            if (IsSneaking)
                flags |= 0x02;
            if (IsSprinting)
                flags |= 0x08;

            using EntityMetadata metadata = new();
            metadata.AddByte(0, flags);

            (byte x, byte y) = Look.ConvertToPacketFormat();
            outPackets.Enqueue(new SpawnNamedEntityPacket(
                Id, UniqueId,
                Position.X, Position.Y, Position.Z,
                x, y,
                metadata.WriteData()));
        }

        internal void Connect(SelfPlayerRenderer renderer)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(!IsConnected);
            _connected = true;

            System.Diagnostics.Debug.Assert(_selfRenderer == null);
            _selfRenderer = renderer;

            _p = Position;

            renderer.Init(Id, _p, Look);
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

            base.ApplyForce(force);
        }

        public override void ApplyGlobalForce(Vector force)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            if (IsConnected)
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

            if (IsConnected)
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

            System.Diagnostics.Debug.Assert(IsConnected);

            _p = p;
            _onGround = onGround;
        }

        internal void Control(bool onGround)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(IsConnected);

            _onGround = onGround;
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
            // Assertion.
            System.Diagnostics.Debug.Assert(!_disposed);

            // Release resources.

            // Finish.
            base.Dispose();
            _disposed = true;
        }


    }

}

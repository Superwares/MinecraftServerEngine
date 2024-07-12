
using Common;
using Containers;
using Sync;

namespace MinecraftServerEngine.PhysicsEngine
{
    public abstract class PhysicsObject : System.IDisposable
    {
        internal abstract class Movement
        {
            internal abstract Vector Resolve(Terrain terrain, BoundingVolume volume, Vector v);
        }

        internal sealed class WallPharsing : Movement
        {
            internal override Vector Resolve(Terrain terrain, BoundingVolume volume, Vector v)
            {
                System.Diagnostics.Debug.Assert(terrain != null);
                System.Diagnostics.Debug.Assert(volume != null);

                volume.Move(v);

                return v;
            }
        }

        internal abstract class NoneWallPharsing : Movement
        {
        }

        internal sealed class SimpleMovement : NoneWallPharsing
        {
            internal override Vector Resolve(Terrain terrain, BoundingVolume volume, Vector v)
            {
                System.Diagnostics.Debug.Assert(terrain != null);
                System.Diagnostics.Debug.Assert(volume != null);

                return terrain.ResolveCollisions(volume, v);
            }
        }

        internal class SmoothMovement : NoneWallPharsing
        {
            internal readonly double MaxStepHeight;

            internal SmoothMovement()
            {
                MaxStepHeight = 0.0D;
            }

            protected SmoothMovement(double maxStepHeight)
            {
                System.Diagnostics.Debug.Assert(maxStepHeight >= 0.0D);

                MaxStepHeight = maxStepHeight;
            }

            internal override Vector Resolve(Terrain terrain, BoundingVolume volume, Vector v)
            {
                System.Diagnostics.Debug.Assert(terrain != null);
                System.Diagnostics.Debug.Assert(volume != null);

                return terrain.ResolveCollisions(volume, MaxStepHeight, v);
            }
        }

        internal sealed class StepableMovement : SmoothMovement
        {

            internal StepableMovement(double maxStepHeight) : base(maxStepHeight)
            {
            }
        }

        private bool _disposed = false;

        private readonly double _m;
        public double Mass => _m;

        private readonly Queue<Vector> Forces = new();  // Disposable

        private Vector _v;
        public Vector Velocity => _v;

        private readonly bool _noGravity;
        public bool NoGravity => _noGravity;

        private BoundingVolume _volume;
        public BoundingVolume BoundingVolume => _volume;

        internal readonly Movement _Movement;

        internal PhysicsObject(
            double m, BoundingVolume volume,
            Movement movement)
        {
            System.Diagnostics.Debug.Assert(volume != null);
            System.Diagnostics.Debug.Assert(m > 0.0D);

            _m = m;

            _v = new(0.0D, 0.0D, 0.0D);

            _noGravity = false;

            _volume = volume;

            _Movement = movement;
        }

        public virtual bool IsDead()
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            return false;
        }

        public abstract void StartRoutine(long serverTicks, PhysicsWorld world);

        public virtual void ApplyForce(Vector v)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(Forces != null);
            Forces.Enqueue(v);
        }

        protected abstract BoundingVolume GenerateBoundingVolume();

        internal (BoundingVolume, Vector) Integrate(Terrain terrain)
        {
            System.Diagnostics.Debug.Assert(terrain != null);

            System.Diagnostics.Debug.Assert(!_disposed);

            Forces.Enqueue(
                -1.0D *
                new Vector(1.0D - 0.91D, 1.0D - 0.9800000190734863D, 1.0D - 0.91D) *
                Velocity);  // Damping Force

            if (!NoGravity)
            {
                Forces.Enqueue(Mass * 0.08D * new Vector(0.0D, -1.0D, 0.0D));  // Gravity
            }

            Vector v = _v;

            while (!Forces.Empty)
            {
                Vector force = Forces.Dequeue();

                System.Diagnostics.Debug.Assert(_m > 0.0D);
                v += (force / _m);
            }

            BoundingVolume volume = GenerateBoundingVolume();

            double s = v.GetLengthSquared();
            System.Diagnostics.Debug.Assert(s >= 0.0D);
            if (s == 0.0D)
            {
                return (volume, v);
            }

            v = _Movement.Resolve(terrain, volume, v);

            return (volume, v);
        }

        internal virtual void Move(BoundingVolume volume, Vector v)
        {
            System.Diagnostics.Debug.Assert(volume != null);

            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(Forces.Empty);

            _volume = volume;
            _v = v;
        }

        public virtual void Dispose()
        {
            // Assertions.
            System.Diagnostics.Debug.Assert(!_disposed);

            // Release resources.
            Forces.Dispose();

            // Finish.
            System.GC.SuppressFinalize(this);
            _disposed = true;
        }

    }
}

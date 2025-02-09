
using Common;
using Containers;
using Sync;


namespace MinecraftServerEngine.Physics
{
    using BoundingVolumes;

    public abstract class PhysicsObject : System.IDisposable
    {
        internal abstract class PhysicsObjectMovement
        {
            internal abstract Vector Resolve(Terrain terrain, BoundingVolume volume, Vector v);
        }

        internal sealed class EmptyMovement : PhysicsObjectMovement
        {
            internal override Vector Resolve(Terrain terrain, BoundingVolume volume, Vector v)
            {
                System.Diagnostics.Debug.Assert(terrain != null);
                System.Diagnostics.Debug.Assert(volume != null);

                return v;
            }
        }

        internal sealed class WallPharsingMovement : PhysicsObjectMovement
        {
            internal override Vector Resolve(Terrain terrain, BoundingVolume volume, Vector v)
            {
                System.Diagnostics.Debug.Assert(terrain != null);
                System.Diagnostics.Debug.Assert(volume != null);

                volume.Move(v);

                return v;
            }
        }

        internal abstract class NoneWallPharsingMovement : PhysicsObjectMovement
        {
        }

        internal sealed class SimpleMovement : NoneWallPharsingMovement
        {
            internal override Vector Resolve(Terrain terrain, BoundingVolume volume, Vector v)
            {
                System.Diagnostics.Debug.Assert(terrain != null);
                System.Diagnostics.Debug.Assert(volume != null);

                return terrain.ResolveCollisions(volume, v);
            }
        }

        internal class SmoothMovement : NoneWallPharsingMovement
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

        internal readonly Queue<Vector> Forces = new();  // Disposable


        private Vector _v;
        public Vector Velocity => _v;


        private BoundingVolume _bv;
        public BoundingVolume BoundingVolume => _bv;


        private readonly PhysicsObjectMovement _Movement;


        internal PhysicsObject(
            double m, BoundingVolume bv,
            PhysicsObjectMovement movement)
        {
            System.Diagnostics.Debug.Assert(bv != null);
            System.Diagnostics.Debug.Assert(m >= 0.0D);
            System.Diagnostics.Debug.Assert(movement != null);

            _m = m;

            _v = new(0.0D, 0.0D, 0.0D);

            _bv = bv;

            _Movement = movement;
        }

        ~PhysicsObject()
        {
            System.Diagnostics.Debug.Assert(false);

            Dispose(false);
        }

        protected internal virtual bool HandleDespawning()
        {
            System.Diagnostics.Debug.Assert(_disposed == false);

            return false;
        }

        protected internal virtual void OnDespawn(PhysicsWorld world) { }

        public virtual void StartRoutine(PhysicsWorld world) 
        {
            System.Diagnostics.Debug.Assert(world != null);
        }

        private protected virtual void _ApplyForce(Vector v)
        {
            System.Diagnostics.Debug.Assert(_disposed == false);

            System.Diagnostics.Debug.Assert(Forces != null);
            Forces.Enqueue(v);
        }

        public void ApplyForce(Vector v)
        {
            if (_disposed == true)
            {
                throw new System.ObjectDisposedException(GetType().Name);
            }

            _ApplyForce(v);
        }

        protected abstract (BoundingVolume, bool noGravity) GetCurrentStatus();

        internal (BoundingVolume, Vector) Integrate(Time dt, Terrain terrain)
        {
            System.Diagnostics.Debug.Assert(terrain != null);

            System.Diagnostics.Debug.Assert(_disposed == false);

            (BoundingVolume volume, bool noGravity) = GetCurrentStatus();

            if (_m == 0.0)
            {
                return (volume, Vector.Zero);
            }

            if (noGravity == false)
            {
                double x = 0.08 * ((double)dt.Amount / (double)MinecraftTimes.TimePerTick.Amount);
                Forces.Enqueue(_m * x * new Vector(0.0D, -1.0D, 0.0D));  // Gravity
            }

            Vector v = _v;

            System.Diagnostics.Debug.Assert(_m > 0.0D);
            while (Forces.Empty == false)
            {
                Vector force = Forces.Dequeue();

                System.Diagnostics.Debug.Assert(_m > 0.0D);
                v += (force / _m);
            }

            double s = v.GetLengthSquared();
            System.Diagnostics.Debug.Assert(s >= 0.0D);
            if (s == 0.0D)
            {
                return (volume, v);
            }

            v = _Movement.Resolve(terrain, volume, v);

            return (volume, v);
        }

        internal virtual void Move(PhysicsWorld world, BoundingVolume volume, Vector v)
        {
            System.Diagnostics.Debug.Assert(world != null);
            System.Diagnostics.Debug.Assert(volume != null);

            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(Forces.Empty);

            _bv = volume;
            _v = v;

            //MyConsole.Debug($"v: {_v}");
        }

        internal virtual void Flush(PhysicsWorld world)
        {
            System.Diagnostics.Debug.Assert(!_disposed);
        }

        public void Dispose()
        {
            Dispose(true);
            System.GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if (_disposed == false)
            {

                // If disposing equals true, dispose all managed
                // and unmanaged resources.
                if (disposing == true)
                {
                    // Dispose managed resources.
                    Forces.Dispose();
                }

                // Call the appropriate methods to clean up
                // unmanaged resources here.
                // If disposing is false,
                // only the following code is executed.
                //CloseHandle(handle);
                //handle = IntPtr.Zero;

                // Note disposing has been done.
                _disposed = true;
            }

        }


    }
}


using Common;
using Containers;
using Sync;

namespace MinecraftServerEngine.PhysicsEngine
{
    public abstract class PhysicsObject : System.IDisposable
    {
        private bool _disposed = false;

        private readonly double _m;
        public double Mass => _m;

        /*private readonly double MaxStepLevel;*/

        private readonly Queue<Vector> Forces = new();  // Disposable

        private Vector _v;
        public Vector Velocity => _v;

        private readonly bool _noGravity;
        public bool NoGravity => _noGravity;

        private bool _onGround;
        public bool OnGround => _onGround;

        private BoundingVolume _volume;
        public BoundingVolume BoundingVolume => _volume;


        public PhysicsObject(BoundingVolume volume, double m/*, double maxStepLevel*/)
        {
            System.Diagnostics.Debug.Assert(volume != null);
            System.Diagnostics.Debug.Assert(m > 0.0D);

            _m = m;

            _v = new(0.0D, 0.0D, 0.0D);

            _noGravity = false;

            _onGround = false;

            _volume = volume;
            /*_MAX_STEP_LEVEL = maxStepLevel;*/
        }

        public virtual void ApplyForce(Vector v)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(Forces != null);
            Forces.Enqueue(v);
        }

        protected abstract BoundingVolume GenerateBoundingVolume();

        internal (BoundingVolume, Vector) Integrate()
        {
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
            return (volume, v);
        }

        internal virtual void Move(BoundingVolume volume, Vector v, bool onGround)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(Forces.Empty);

            _volume = volume;
            _v = v;
            _onGround = onGround;

            /*Console.Printl($"Velocity: {v}, OnGround: {onGround}");*/
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


using Containers;
using Sync;

namespace PhysicsEngine
{
    public abstract class PhysicsObject : System.IDisposable
    {
        private bool _disposed = false;

        private readonly RWLock Lock = new();  // Disposable

        private readonly double _m;
        public double GetMass() => _m;

        /*private readonly double MaxStepLevel;*/

        private readonly Queue<Vector> Forces = new();  // Disposable

        private Vector _v = new(0, 0, 0);
        public Vector Velocity;

        private bool _onGround;
        public bool OnGround => _onGround;

        private IBoundingVolume _volume;
        public IBoundingVolume BoundingVolume => _volume;


        public PhysicsObject(IBoundingVolume volume, double m/*, double maxStepLevel*/)
        {
            _v = new(0.0D, 0.0D, 0.0D);

            _onGround = false;

            _m = m;

            /*_MAX_STEP_LEVEL = maxStepLevel;*/
        }

        public virtual void ApplyForce(Vector v)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            Forces.Enqueue(v);
        }

        protected abstract IBoundingVolume GenerateBoundingVolume();

        internal (IBoundingVolume, Vector) Integrate()
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            Vector v = _v;

            while (!Forces.Empty)
            {
                Vector force = Forces.Dequeue();

                v += (force / _m);
            }

            IBoundingVolume volume = GenerateBoundingVolume();
            return (volume, v);
        }

        public virtual void Move(IBoundingVolume volume, Vector v, bool onGround)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(Forces.Empty);

            _volume = volume;
            _v = v;
            _onGround = onGround;
        }

        public virtual void Dispose()
        {
            // Assertions.
            System.Diagnostics.Debug.Assert(!_disposed);

            // Release resources.
            Lock.Dispose();
            Forces.Dispose();

            // Finish.
            System.GC.SuppressFinalize(this);
            _disposed = true;
        }

    }
}


using Containers;
using Sync;

namespace PhysicsEngine
{
    public abstract class PhysicsObject : System.IDisposable
    {
        private bool _disposed = false;

        private readonly RWLock _LOCK = new();  // Disposable

        private readonly double _m;
        public double GetMass() => _m;

        private readonly double _MAX_STEP_LEVEL;

        private readonly Queue<Vector> _FORCES = new();  // Disposable

        private Vector _v = new(0, 0, 0);
        public Vector VELOCITY;

        private bool _onGround;
        public bool ON_GROUND => _onGround;

        private IBoundingVolume _volume;
        public IBoundingVolume BOUNDING_VOLUME => _volume;


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

            _FORCES.Enqueue(v);
        }

        protected abstract IBoundingVolume GenerateBoundingVolume();

        internal (IBoundingVolume, Vector) Integrate()
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            Vector v = _v;

            while (!_FORCES.Empty)
            {
                Vector force = _FORCES.Dequeue();

                v += (force / _m);
            }

            IBoundingVolume volume = GenerateBoundingVolume();
            return (volume, v);
        }

        public virtual void Move(IBoundingVolume volume, Vector v, bool onGround)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(_FORCES.Empty);

            _volume = volume;
            _v = v;
            _onGround = onGround;
        }

        public virtual void Dispose()
        {
            // Assertions.
            System.Diagnostics.Debug.Assert(!_disposed);

            // Release resources.
            _LOCK.Dispose();
            _FORCES.Dispose();

            // Finish.
            System.GC.SuppressFinalize(this);
            _disposed = true;
        }

    }
}

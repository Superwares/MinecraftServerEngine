
using Common;
using Containers;
using Sync;

namespace MinecraftPhysicsEngine
{
    public abstract class PhysicsObject : System.IDisposable
    {
        private bool _disposed = false;

        private readonly double _m;
        public double GetMass() => _m;

        /*private readonly double MaxStepLevel;*/

        private readonly Queue<Vector> Forces = new();  // Disposable

        private Vector _v;
        public Vector Velocity;

        private bool _onGround;
        public bool OnGround => _onGround;

        private BoundingVolume _volume;
        public BoundingVolume BoundingVolume => _volume;


        public PhysicsObject(BoundingVolume volume, double m/*, double maxStepLevel*/)
        {
            _m = m;

            _v = new(0.0D, 0.0D, 0.0D);

            _onGround = false;

            _volume = volume;
            /*_MAX_STEP_LEVEL = maxStepLevel;*/
        }

        public virtual void ApplyForce(Vector v)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            Forces.Enqueue(v);
        }

        protected abstract BoundingVolume GenerateBoundingVolume();

        internal (BoundingVolume, Vector) Integrate()
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            Vector v = _v;

            while (!Forces.Empty)
            {
                Vector force = Forces.Dequeue();

                v += (force / _m);
            }

            BoundingVolume volume = GenerateBoundingVolume();
            return (volume, v);
        }

        public virtual void Move(BoundingVolume volume, Vector v, bool onGround)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(Forces.Empty);

            _volume = volume;
            _v = v;
            _onGround = onGround;

            Console.Printl($"Velocity: {v}, OnGround: {onGround}");
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

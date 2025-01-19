﻿
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


        protected Vector _p;
        public Vector Position => _p;


        private readonly double _m;
        public double Mass => _m;

        internal readonly Queue<Vector> Forces = new();  // Disposable

        private Vector _v;
        public Vector Velocity => _v;

        private BoundingVolume _volume;
        public BoundingVolume BoundingVolume => _volume;

        internal readonly Movement _Movement;

        internal PhysicsObject(
            Vector p,
            double m, BoundingVolume volume,
            Movement movement)
        {
            System.Diagnostics.Debug.Assert(volume != null);
            System.Diagnostics.Debug.Assert(m > 0.0D);
            System.Diagnostics.Debug.Assert(movement != null);

            _p = p;

            _m = m;

            _v = new(0.0D, 0.0D, 0.0D);

            _volume = volume;

            _Movement = movement;
        }

        ~PhysicsObject()
        {
            System.Diagnostics.Debug.Assert(false);

            Dispose(false);
        }

        protected internal virtual bool IsDead()
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            return false;
        }

        protected internal virtual void OnDeath(PhysicsWorld world) { }

        public abstract void StartRoutine(PhysicsWorld world);

        public virtual void ApplyForce(Vector v)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(Forces != null);
            Forces.Enqueue(v);
        }

        protected abstract (BoundingVolume, bool noGravity) GetCurrentStatus();

        internal (BoundingVolume, Vector) Integrate(Terrain terrain)
        {
            System.Diagnostics.Debug.Assert(terrain != null);

            System.Diagnostics.Debug.Assert(!_disposed);

            (BoundingVolume volume, bool noGravity) = GetCurrentStatus();

            if (!noGravity)
            {
                Forces.Enqueue(Mass * 0.08D * new Vector(0.0D, -1.0D, 0.0D));  // Gravity
            }

            Vector v = _v;

            System.Diagnostics.Debug.Assert(Mass > 0.0D);
            while (!Forces.Empty)
            {
                Vector force = Forces.Dequeue();

                System.Diagnostics.Debug.Assert(_m > 0.0D);
                v += (force / Mass);
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

        internal virtual void Move(BoundingVolume volume, Vector v)
        {
            System.Diagnostics.Debug.Assert(volume != null);

            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(Forces.Empty);

            _volume = volume;
            _v = v;
        }

        internal virtual void Flush()
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

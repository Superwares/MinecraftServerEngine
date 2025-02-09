
using Common;
using Containers;

namespace MinecraftServerEngine.Physics
{
    using BoundingVolumes;

    public abstract class Terrain : System.IDisposable
    {

        private bool _disposed = false;

        // In minecraft client,
        // when online, a player cannot be passed through a wall if pushed into a wall,
        // but if an offline player is pushed into a wall and then reconnects,
        // the player will pass through the wall, so an error needs to be added.
        private static double AddError(double t)
        {
            System.Diagnostics.Debug.Assert(t <= 1.0D);
            System.Diagnostics.Debug.Assert(t >= 0.0D);

            if (t > 0.0)
            {
                t -= 0.00001;  // error
            }

            if (t < 0.0)
            {
                t = 0.0;
            }

            return t;
        }

        public Terrain()
        {

        }

        ~Terrain()
        {
            System.Diagnostics.Debug.Assert(false);

            Dispose(false);
        }

        protected abstract void GenerateBoundingBoxForBlock(
            Queue<AxisAlignedBoundingBox> queue,
            AxisAlignedBoundingBox volume);

        private Vector ResolveCollisionsOnGround(
            Queue<AxisAlignedBoundingBox> queue,
            BoundingVolume volume, double maxStepHeight, Vector v)
        {
            System.Diagnostics.Debug.Assert(queue != null);
            System.Diagnostics.Debug.Assert(!queue.Empty);
            System.Diagnostics.Debug.Assert(volume != null);
            System.Diagnostics.Debug.Assert(maxStepHeight >= 0.0D);

            System.Diagnostics.Debug.Assert(!_disposed);

            int axis;
            double t;

            int a;
            double b;

            if (maxStepHeight > 0.0D)
            {
                Vector vUp = new(0.0D, maxStepHeight, 0.0D);

                axis = -1;
                t = double.PositiveInfinity;

                System.Diagnostics.Debug.Assert(vUp.Y > 0.0D);
                foreach (AxisAlignedBoundingBox aabb in queue.GetValues())
                {
                    (a, b) = aabb.ResolveCollision(volume, vUp);

                    System.Diagnostics.Debug.Assert(t >= 0.0D);
                    if (a > -1 && b < t)
                    {
                        System.Diagnostics.Debug.Assert(b <= 1.0D);
                        System.Diagnostics.Debug.Assert(b >= 0.0D);
                        System.Diagnostics.Debug.Assert(a == 0);

                        axis = a;
                        t = b;
                    }
                }

                if (axis == -1)
                {
                    volume.Move(vUp);
                }
                else if (axis == 0)
                {
                    System.Diagnostics.Debug.Assert(t <= 1.0D);
                    System.Diagnostics.Debug.Assert(t >= 0.0D);

                    t = AddError(t);

                    volume.Move(vUp * t);
                }
                else
                {
                    System.Diagnostics.Debug.Assert(false);
                }
            }

            {
                axis = -1;
                t = double.PositiveInfinity;

                System.Diagnostics.Debug.Assert(v.Y == 0.0D);
                foreach (AxisAlignedBoundingBox aabb in queue.GetValues())
                {
                    (a, b) = aabb.ResolveCollision(volume, v);

                    System.Diagnostics.Debug.Assert(t >= 0.0D);
                    if (a > -1 && b < t)
                    {
                        System.Diagnostics.Debug.Assert(b <= 1.0D);
                        System.Diagnostics.Debug.Assert(b >= 0.0D);
                        System.Diagnostics.Debug.Assert(a == 1 || a == 2);

                        axis = a;
                        t = b;
                    }
                }

                if (axis == -1)
                {
                    volume.Move(v);
                }
                else if (axis == 1 || axis == 2)
                {
                    System.Diagnostics.Debug.Assert(t <= 1.0D);
                    System.Diagnostics.Debug.Assert(t >= 0.0D);

                    t = AddError(t);

                    volume.Move(v * t);
                }
                else
                {
                    System.Diagnostics.Debug.Assert(false);
                }
            }

            if (maxStepHeight > 0.0D)
            {
                Vector vDown = new(0.0D, -maxStepHeight, 0.0D);

                axis = -1;
                t = double.PositiveInfinity;

                System.Diagnostics.Debug.Assert(vDown.Y < 0.0D);
                foreach (AxisAlignedBoundingBox aabb in queue.GetValues())
                {
                    (a, b) = aabb.ResolveCollision(volume, vDown);

                    System.Diagnostics.Debug.Assert(t >= 0.0D);
                    if (a > -1 && b < t)
                    {
                        System.Diagnostics.Debug.Assert(b <= 1.0D);
                        System.Diagnostics.Debug.Assert(b >= 0.0D);
                        System.Diagnostics.Debug.Assert(a == 0);

                        axis = a;
                        t = b;
                    }
                }

                if (axis == -1)
                {
                    volume.Move(vDown);
                }
                else if (axis == 0)
                {
                    System.Diagnostics.Debug.Assert(t <= 1.0D);
                    System.Diagnostics.Debug.Assert(t >= 0.0D);

                    t = AddError(t);

                    volume.Move(vDown * t);
                }
                else
                {
                    System.Diagnostics.Debug.Assert(false);
                }

            }

            return Vector.Zero;
        }

        private Vector ResolveCollisions(
            Queue<AxisAlignedBoundingBox> queue,
            BoundingVolume volume, double maxStepHeight, Vector v)
        {
            System.Diagnostics.Debug.Assert(queue != null);
            System.Diagnostics.Debug.Assert(!queue.Empty);
            System.Diagnostics.Debug.Assert(volume != null);
            System.Diagnostics.Debug.Assert(maxStepHeight >= 0.0D);

            System.Diagnostics.Debug.Assert(!_disposed);

            bool onGround = false;

            int axis = -1;
            double t = double.PositiveInfinity;

            int a;
            double b;
            foreach (AxisAlignedBoundingBox aabb in queue.GetValues())
            {
                (a, b) = aabb.ResolveCollision(volume, v);

                System.Diagnostics.Debug.Assert(t >= 0.0D);
                if (a > -1 && b < t)
                {
                    System.Diagnostics.Debug.Assert(b <= 1.0D);
                    System.Diagnostics.Debug.Assert(b >= 0.0D);
                    System.Diagnostics.Debug.Assert(a < 3);

                    axis = a;
                    t = b;
                }
            }

            if (axis == -1)
            {
                volume.Move(v);
                return v;
            }

            System.Diagnostics.Debug.Assert(maxStepHeight >= 0.0D);
            System.Diagnostics.Debug.Assert(maxStepHeight <= volume.GetHeight() / 2.0D);

            System.Diagnostics.Debug.Assert(axis < 3);
            System.Diagnostics.Debug.Assert(t <= 1.0D);
            System.Diagnostics.Debug.Assert(t >= 0.0D);

            t = AddError(t);

            Vector vPrime = v * (1 - t);

            v = v * t;
            volume.Move(v);

            if (axis == 0)  // Y
            {
                if (v.Y <= 0.0)
                {
                    onGround = true;
                }

                vPrime = new Vector(vPrime.X, 0.0D, vPrime.Z);
            }
            else if (axis == 1)  // X
            {
                vPrime = new Vector(0.0D, vPrime.Y, vPrime.Z);
            }
            else if (axis == 2)  // Z
            {
                vPrime = new Vector(vPrime.X, vPrime.Y, 0.0D);
            }
            else
            {
                System.Diagnostics.Debug.Assert(false);
            }

            return onGround == true ?
                ResolveCollisionsOnGround(queue, volume, maxStepHeight, vPrime) :
                ResolveCollisions(queue, volume, maxStepHeight, vPrime);
        }

        internal Vector ResolveCollisions(
            BoundingVolume volume, double maxStepHeight, Vector v)
        {
            System.Diagnostics.Debug.Assert(volume != null);
            System.Diagnostics.Debug.Assert(v.GetLengthSquared() > 0.0D);

            System.Diagnostics.Debug.Assert(!_disposed);

            AxisAlignedBoundingBox volumeTotal = volume.GetMinBoundingBox();
            volumeTotal.Extend(v);

            using Queue<AxisAlignedBoundingBox> queue = new();

            GenerateBoundingBoxForBlock(queue, volumeTotal);

            if (queue.Empty == true)
            {
                //MyConsole.Debug($"Empty!");

                volume.Move(v);
                return v;
            }

            return ResolveCollisions(queue, volume, maxStepHeight, v);
        }

        internal Vector ResolveCollisions(BoundingVolume volume, Vector v)
        {
            System.Diagnostics.Debug.Assert(volume != null);
            System.Diagnostics.Debug.Assert(v.GetLengthSquared() > 0.0D);

            System.Diagnostics.Debug.Assert(!_disposed);

            AxisAlignedBoundingBox volumeTotal = volume.GetMinBoundingBox();
            volumeTotal.Extend(v);

            using Queue<AxisAlignedBoundingBox> queue = new();

            GenerateBoundingBoxForBlock(queue, volumeTotal);

            if (queue.Empty == true)
            {
                volume.Move(v);
                return v;
            }

            int axis = -1;
            double t = double.PositiveInfinity;

            int a;
            double b;
            foreach (AxisAlignedBoundingBox aabb in queue.GetValues())
            {
                (a, b) = aabb.ResolveCollision(volume, v);

                System.Diagnostics.Debug.Assert(t >= 0.0D);
                if (a > -1 && b < t)
                {
                    System.Diagnostics.Debug.Assert(b <= 1.0D);
                    System.Diagnostics.Debug.Assert(b >= 0.0D);
                    System.Diagnostics.Debug.Assert(a < 3);

                    axis = a;
                    t = b;
                }
            }

            if (axis == -1)
            {
                volume.Move(v);
                return v;
            }

            System.Diagnostics.Debug.Assert(axis < 3);
            System.Diagnostics.Debug.Assert(t <= 1.0);
            System.Diagnostics.Debug.Assert(t >= 0.0);

            t = AddError(t);

            v = v * t;
            volume.Move(v);

            return Vector.Zero;
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

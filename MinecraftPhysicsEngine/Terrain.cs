
using Containers;

namespace MinecraftPhysicsEngine
{
    public sealed class Terrain : System.IDisposable
    {
        private bool _disposed = false;

        private readonly AxisAlignedBoundingBox MinBoundingBox;
        public Vector Max => MinBoundingBox.Max;
        public Vector Min => MinBoundingBox.Min;


        private readonly Queue<AxisAlignedBoundingBox> BoundingBoxes = new();  // Disposable

        internal Terrain(AxisAlignedBoundingBox aabb)
        {
            MinBoundingBox = aabb;
        }

        public void AddBoundingBox(AxisAlignedBoundingBox aabb)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            BoundingBoxes.Enqueue(aabb);
        }

        private (Vector, bool) ResolveCollisions(BoundingVolume volume, Vector v, bool onGround)
        {
            int axis = -1;
            double t = double.PositiveInfinity;

            int a;
            double b;
            foreach (AxisAlignedBoundingBox aabb in BoundingBoxes.GetValues())
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
                return (v, onGround);
            }

            bool movedDown = v.Y < 0.0D;

            System.Diagnostics.Debug.Assert(axis < 3);
            System.Diagnostics.Debug.Assert(t <= 1.0D);
            System.Diagnostics.Debug.Assert(t >= 0.0D);
            Vector vPrime = v * (1 - t);

            v = v * t;
            volume.Move(v);

            if (axis == 0)  // X
            {
                vPrime = new Vector(0.0D, vPrime.Y, vPrime.Z);
            }
            else if (axis == 1)  // Y
            {
                if (movedDown)
                {
                    onGround = true;
                }

                vPrime = new Vector(vPrime.X, 0.0D, vPrime.Z);
            }
            else if (axis == 2)  // Z
            {
                vPrime = new Vector(vPrime.X, vPrime.Y, 0.0D);
            }
            else
            {
                System.Diagnostics.Debug.Assert(false);
            }

            return ResolveCollisions(volume, vPrime, onGround);
        }

        internal (Vector, bool) ResolveCollisions(BoundingVolume volume, Vector v)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            return ResolveCollisions(volume, v, false);
        }

        public void Dispose()
        {
            // Assertions.
            System.Diagnostics.Debug.Assert(!_disposed);

            // Release resources.
            BoundingBoxes.Dispose();

            // Finish.
            System.GC.SuppressFinalize(this);
            _disposed = true;
        }

    }
}

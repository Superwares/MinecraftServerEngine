
using PhysicsEngine;
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

        internal (Vector, bool) AdjustVolumeMovement(BoundingVolume volume, Vector v)
        {

            Vector vPrime1 = v;
            foreach (AxisAlignedBoundingBox aabbFixed in BoundingBoxes.GetValues())
            {
                vPrime1 = aabbFixed.AdjustMovingVolumeSideToSide(volume, vPrime1);
            }

            foreach (AxisAlignedBoundingBox aabbFixed in BoundingBoxes.GetValues())
            {
                vPrime1 = aabbFixed.AdjustMovingVolumeUpAndDown(volume, vPrime1);
            }

            Vector vPrime2 = v;
            foreach (AxisAlignedBoundingBox aabbFixed in BoundingBoxes.GetValues())
            {
                vPrime2 = aabbFixed.AdjustMovingVolumeUpAndDown(volume, vPrime2);
            }

            foreach (AxisAlignedBoundingBox aabbFixed in BoundingBoxes.GetValues())
            {
                vPrime2 = aabbFixed.AdjustMovingVolumeUpAndDown(volume, vPrime2);
            }

            Vector vPrime3 = new(vPrime1.X, 0.0D, vPrime1.Z),
                   vPrime4 = new(vPrime2.X, 0.0D, vPrime2.Z);

            double lenSquared3 = vPrime3.GetLengthSquared(), lenSquared4 = vPrime4.GetLengthSquared();
            System.Diagnostics.Debug.Assert(lenSquared3 <= lenSquared4);
            Vector vPrime5 = (lenSquared3 == lenSquared4) ? vPrime1 : vPrime2;

            bool movedUpAndDown = vPrime5.Y != v.Y;
            bool onGround = v.Y < 0.0D && movedUpAndDown;

            volume.Move(vPrime5);

            v = new(
                vPrime5.X != v.X ? 0.0D : vPrime5.X,
                movedUpAndDown ? 0.0D : vPrime5.Y,
                vPrime5.Z != v.Z ? 0.0D : vPrime5.Z);

            return (v, onGround);
        }

        public Vector AdjustMovingVolumeUpAndDown(BoundingVolume volume, Vector v)
        {

        }

        internal abstract Vector AdjustMovingVolumeSideToSide(BoundingVolume volume, Vector v);

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

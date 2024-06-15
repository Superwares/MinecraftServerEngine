using Common;

namespace PhysicsEngine
{
    public interface IBoundingVolume
    {
        IBoundingVolume Extend(Vector v);
        IBoundingVolume Move(Vector v);

        Vector GetCenter();
        Vector GetBottomCenter();

        IBoundingVolume GetMinBoundingVolume();
        IBoundingVolume GetMinBoundingVolume(Vector v);

        bool TestIntersection(IBoundingVolume volume);
        bool TestIntersection(IBoundingVolume volumeMoving, Vector v);

        Vector AdjustMovingVolumeUpAndDown(IBoundingVolume volumeMoving, Vector v);
        Vector AdjustMovingVolumeSideToSide(IBoundingVolume volumeMoving, Vector v);

    }

    public readonly struct EmptyBoundingVolume : IBoundingVolume
    {
        public EmptyBoundingVolume() { }

        public readonly IBoundingVolume Extend(Vector v)
        {
            return this;
        }

        public readonly IBoundingVolume Move(Vector v)
        {
            throw new System.NotImplementedException();
        }

        public readonly Vector GetCenter()
        {
            throw new System.NotImplementedException();
        }

        public readonly Vector GetBottomCenter()
        {
            throw new System.NotImplementedException();
        }

        public readonly IBoundingVolume GetMinBoundingVolume()
        {
            throw new System.NotImplementedException();
        }

        public readonly IBoundingVolume GetMinBoundingVolume(Vector v)
        {
            throw new System.NotImplementedException();
        }

        public readonly bool TestIntersection(IBoundingVolume volume)
        {
            return false;
        }

        public readonly bool TestIntersection(IBoundingVolume volumeMoving, Vector v)
        {
            return false;
        }

        public readonly Vector AdjustMovingVolumeSideToSide(IBoundingVolume volumeMoving, Vector v)
        {
            if (!TestIntersection(volumeMoving))
            {
                return v;
            }

            return v;
        }

        public readonly Vector AdjustMovingVolumeUpAndDown(IBoundingVolume volumeMoving, Vector v)
        {
            if (!TestIntersection(volumeMoving))
            {
                return v;
            }

            return v;
        }

    }

    public readonly struct AxisAlignedBoundingBox : IBoundingVolume
    {
        private readonly Vector _MAX, _MIN;
        public Vector Max => _MAX;
        public Vector Min => _MIN;

        public AxisAlignedBoundingBox(Vector Max, Vector Min)
        {
            System.Diagnostics.Debug.Assert(Max.X >= Min.X);
            System.Diagnostics.Debug.Assert(Max.Y >= Min.Y);
            System.Diagnostics.Debug.Assert(Max.Z >= Min.Z);

            _MAX = Max;
            _MIN = Min;
        }

        public readonly IBoundingVolume Extend(Vector v)
        {
            throw new System.NotImplementedException();
        }

        public readonly IBoundingVolume Move(Vector v)
        {
            throw new System.NotImplementedException();
        }

        public readonly Vector GetCenter()
        {
            throw new System.NotImplementedException();
        }

        public readonly Vector GetBottomCenter()
        {
            throw new System.NotImplementedException();
        }

        public readonly IBoundingVolume GetMinBoundingVolume()
        {
            throw new System.NotImplementedException();
        }

        public readonly IBoundingVolume GetMinBoundingVolume(Vector v)
        {
            throw new System.NotImplementedException();
        }

        public readonly bool TestIntersection(IBoundingVolume other)
        {
            switch (other)
            {
                default:
                    System.Diagnostics.Debug.Assert(false);
                    break;
                case AxisAlignedBoundingBox aabb:
                    throw new System.NotImplementedException();
                /*case BoundingSphere sphere:
                    throw new System.NotImplementedException();*/
                case CompoundBoundingVolume compound:
                    return compound.TestIntersection(this);
            }

            throw new System.NotImplementedException();
        }

        public readonly bool TestIntersection(IBoundingVolume volumeMoving, Vector v)
        {
            throw new System.NotImplementedException();
        }

        public readonly Vector AdjustMovingVolumeSideToSide(IBoundingVolume volumeMoving, Vector v)
        {
            if (!TestIntersection(volumeMoving))
            {
                return v;
            }

            throw new System.NotImplementedException();
        }

        public readonly Vector AdjustMovingVolumeUpAndDown(IBoundingVolume volumeMoving, Vector v)
        {
            if (!TestIntersection(volumeMoving))
            {
                return v;
            }

            throw new System.NotImplementedException();
        }

    }

    /*public readonly struct OrientedBoundingBox 
     * : BoundingVolume
    {

    }*/

    /*public readonly struct BoundingSphere 
     * : BoundingVolume
    {
        private readonly Vector _CENTER;
        private readonly double _RADIUS;

        public BoundingSphere(Vector c, double r)
        {
            _CENTER = c;
            _RADIUS = r;
        }

        public readonly BoundingVolume Extend(Vector v)
        {
            throw new System.NotImplementedException();
        }

        public readonly bool TestIntersection(BoundingVolume other)
        {
            switch (other)
            {
                default:
                    System.Diagnostics.Debug.Assert(false);
                    break;
                case AxisAlignedBoundingBox aabb:
                    throw new System.NotImplementedException();
                case BoundingSphere sphere:
                    throw new System.NotImplementedException();
                case CompoundBoundingVolume compound:
                    return compound.TestIntersection(this);
            }

            throw new System.NotImplementedException();
        }

    }*/

    public readonly struct CompoundBoundingVolume : IBoundingVolume
    {
        private readonly IBoundingVolume[] _VOLUMES;

        public CompoundBoundingVolume(params IBoundingVolume[] volumes)
        {
            // TODO: Assert all volumes are attached.
            _VOLUMES = volumes;
        }

        public readonly IBoundingVolume Extend(Vector v)
        {
            throw new System.NotImplementedException();
        }

        public readonly IBoundingVolume Move(Vector v)
        {
            throw new System.NotImplementedException();
        }

        public readonly Vector GetCenter()
        {
            throw new System.NotImplementedException();
        }

        public readonly Vector GetBottomCenter()
        {
            throw new System.NotImplementedException();
        }

        public readonly IBoundingVolume GetMinBoundingVolume()
        {
            throw new System.NotImplementedException();
        }

        public readonly IBoundingVolume GetMinBoundingVolume(Vector v)
        {
            throw new System.NotImplementedException();
        }

        public readonly bool TestIntersection(IBoundingVolume other)
        {
            for (int i = 0; i < _VOLUMES.Length; ++i)
            {
                IBoundingVolume volume = _VOLUMES[i];

                if (volume.TestIntersection(other))
                {
                    return true;
                }
            }

            return false;
        }

        public readonly bool TestIntersection(IBoundingVolume volumeMoving, Vector v)
        {
            throw new System.NotImplementedException();
        }

        public readonly Vector AdjustMovingVolumeSideToSide(IBoundingVolume volumeMoving, Vector v)
        {
            if (!TestIntersection(volumeMoving))
            {
                return v;
            }

            throw new System.NotImplementedException();
        }

        public readonly Vector AdjustMovingVolumeUpAndDown(IBoundingVolume volumeMoving, Vector v)
        {
            if (!TestIntersection(volumeMoving))
            {
                return v;
            }

            throw new System.NotImplementedException();
        }

    }

}

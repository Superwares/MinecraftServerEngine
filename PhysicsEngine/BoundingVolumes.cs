using Common;

namespace PhysicsEngine
{
    public interface BoundingVolume
    {
        BoundingVolume Extend(Vector v);
        BoundingVolume Move(Vector v);

        BoundingVolume GetMinBoundingVolume();
        BoundingVolume GetMinBoundingVolume(Vector v);

        Vector GetCenter();
        Vector GetBottomCenter();

        bool TestIntersection(BoundingVolume volume);
        bool TestIntersection(BoundingVolume volumeMoving, Vector v);

        Vector AdjustMovingVolumeUpAndDown(BoundingVolume volumeMoving, Vector v);
        Vector AdjustMovingVolumeSideToSide(BoundingVolume volumeMoving, Vector v);

    }

    public readonly struct EmptyBoundingVolume 
        : BoundingVolume, System.IEquatable<EmptyBoundingVolume>
    {
        public EmptyBoundingVolume() { }


    }

    public readonly struct AxisAlignedBoundingBox 
        : BoundingVolume, System.IEquatable<AxisAlignedBoundingBox>
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

        public readonly bool TestIntersection(BoundingVolume other)
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

        
    }

    /*public readonly struct OrientedBoundingBox 
     * : BoundingVolume, System.IEquatable<EmptyBoundingVolume>
    {

    }*/

    /*public readonly struct BoundingSphere 
     * : BoundingVolume, System.IEquatable<EmptyBoundingVolume>
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

    public readonly struct CompoundBoundingVolume 
        : BoundingVolume, System.IEquatable<EmptyBoundingVolume>
    {
        private readonly BoundingVolume[] _VOLUMES;

        public CompoundBoundingVolume(params BoundingVolume[] volumes)
        {
            // TODO: Assert all volumes are attached.
            _VOLUMES = volumes;
        }

        public readonly BoundingVolume Extend(Vector v)
        {
            throw new System.NotImplementedException();
        }

        public readonly bool TestIntersection(BoundingVolume other)
        {
            for (int i = 0; i < _VOLUMES.Length; ++i)
            {
                BoundingVolume volume = _VOLUMES[i];

                if (volume.TestIntersection(other))
                {
                    return true;
                }
            }

            return false;
        }
        

    }

}

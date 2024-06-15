using Common;

namespace PhysicsEngine
{
    public abstract class BoundingVolume
    {
        public abstract BoundingVolume Extend(Vector v);
        public abstract BoundingVolume Move(Vector v);

        public abstract BoundingVolume GetMinBoundingVolume();
        public abstract BoundingVolume GetMinBoundingVolume(Vector v);

        public abstract Vector GetCenter();
        public abstract Vector GetBottomCenter();

        public abstract bool TestIntersection(BoundingVolume volume);
        public abstract bool TestIntersection(BoundingVolume volumeMoving, Vector v);

        public abstract Vector AdjustMovingVolumeUpAndDown(BoundingVolume volumeMoving, Vector v);
        public abstract Vector AdjustMovingVolumeSideToSide(BoundingVolume volumeMoving, Vector v);

    }

    public sealed class AxisAlignedBoundingBox : BoundingVolume
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

        public override bool TestIntersection(BoundingVolume other)
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

    /*public sealed class OrientedBoundingBox : BoundingVolume
    {

    }*/

    /*public sealed class BoundingSphere : BoundingVolume
    {
        private readonly Vector _CENTER;
        private readonly double _RADIUS;

        public BoundingSphere(Vector c, double r)
        {
            _CENTER = c;
            _RADIUS = r;
        }

        public override BoundingVolume Extend(Vector v)
        {
            throw new System.NotImplementedException();
        }

        public override bool TestIntersection(BoundingVolume other)
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

    public sealed class CompoundBoundingVolume : BoundingVolume
    {
        private readonly BoundingVolume[] _VOLUMES;

        public CompoundBoundingVolume(params BoundingVolume[] volumes)
        {
            // TODO: Assert all volumes are attached.
            _VOLUMES = volumes;
        }

        public override BoundingVolume Extend(Vector v)
        {
            throw new System.NotImplementedException();
        }

        public override bool TestIntersection(BoundingVolume other)
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

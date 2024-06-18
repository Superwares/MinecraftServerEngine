using Common;

namespace PhysicsEngine
{
    

    public interface BoundingVolume
    {
        BoundingVolume Extend(Vector v);
        BoundingVolume Move(Vector v);

        Vector GetCenter();
        Vector GetBottomCenter();

        BoundingVolume GetMinBoundingVolume();
        BoundingVolume GetMinBoundingVolume(Vector v);

        bool TestIntersection(BoundingVolume volume);
        bool TestIntersection(BoundingVolume volumeMoving, Vector v);

        Vector AdjustMovingVolumeUpAndDown(BoundingVolume volume, Vector v);
        Vector AdjustMovingVolumeSideToSide(BoundingVolume volume, Vector v);

    }

    public readonly struct EmptyBoundingVolume : BoundingVolume
    {
        public EmptyBoundingVolume() { }

        public readonly BoundingVolume Extend(Vector v)
        {
            return this;
        }

        public readonly BoundingVolume Move(Vector v)
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

        public readonly BoundingVolume GetMinBoundingVolume()
        {
            throw new System.NotImplementedException();
        }

        public readonly BoundingVolume GetMinBoundingVolume(Vector v)
        {
            throw new System.NotImplementedException();
        }

        public readonly bool TestIntersection(BoundingVolume volume)
        {
            return false;
        }

        public readonly bool TestIntersection(BoundingVolume volume, Vector v)
        {
            return false;
        }

        public readonly Vector AdjustMovingVolumeSideToSide(BoundingVolume volume, Vector v)
        {
            if (!TestIntersection(volume))
            {
                return v;
            }

            return v;
        }

        public readonly Vector AdjustMovingVolumeUpAndDown(BoundingVolume volume, Vector v)
        {
            if (!TestIntersection(volume))
            {
                return v;
            }

            return v;
        }

    }

    public readonly struct AxisAlignedBoundingBox : BoundingVolume
    {

        public readonly Vector Max, Min;

        public AxisAlignedBoundingBox(Vector max, Vector min)
        {
            System.Diagnostics.Debug.Assert(max.X >= min.X);
            System.Diagnostics.Debug.Assert(max.Y >= min.Y);
            System.Diagnostics.Debug.Assert(max.Z >= min.Z);

            this.Max = max;
            this.Min = min;
        }

        public readonly BoundingVolume Extend(Vector v)
        {
            throw new System.NotImplementedException();
        }

        public readonly BoundingVolume Move(Vector v)
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

        public readonly BoundingVolume GetMinBoundingVolume()
        {
            throw new System.NotImplementedException();
        }

        public readonly BoundingVolume GetMinBoundingVolume(Vector v)
        {
            throw new System.NotImplementedException();
        }

        public readonly bool TestIntersection(BoundingVolume volume)
        {
            if (volume is AxisAlignedBoundingBox aabb)
            {
                return IntersectionTests.TestFixedAndFixed(this, aabb);
            }
            else if (volume is CompoundBoundingVolume cbv)
            {
                return IntersectionTests.TestFixedAndFixed(this, cbv);
            }

            throw new System.NotImplementedException();
        }

        public readonly bool TestIntersection(BoundingVolume volume, Vector v)
        {
            if (volume is AxisAlignedBoundingBox aabb)
            {
                return IntersectionTests.TestFixedAndMoving(this, aabb, v);
            }
            else if (volume is CompoundBoundingVolume cbv)
            {
                return IntersectionTests.TestFixedAndMoving(this, cbv, v);
            }

            throw new System.NotImplementedException();
        }

        internal readonly Vector AdjustMovingVolumeSideToSide(BoundingVolume other, Vector v)
        {
            if (!TestIntersection(other))
            {
                return v;
            }

            throw new System.NotImplementedException();
        }

        internal readonly Vector AdjustMovingVolumeUpAndDown(BoundingVolume other, Vector v)
        {
            if (!TestIntersection(other))
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

    }*/

    public readonly struct CompoundBoundingVolume : BoundingVolume
    {

        public readonly BoundingVolume[] Volumes;

        public CompoundBoundingVolume(params BoundingVolume[] volumes)
        {
            // TODO: Assert all volumes are attached.
            Volumes = volumes;
        }

        public readonly BoundingVolume Extend(Vector v)
        {
            throw new System.NotImplementedException();
        }

        public readonly BoundingVolume Move(Vector v)
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

        public readonly BoundingVolume GetMinBoundingVolume()
        {
            throw new System.NotImplementedException();
        }

        public readonly BoundingVolume GetMinBoundingVolume(Vector v)
        {
            throw new System.NotImplementedException();
        }

        public readonly bool TestIntersection(BoundingVolume volume)
        {
            if (volume is AxisAlignedBoundingBox aabb)
            {
                return IntersectionTests.TestFixedAndFixed(this, aabb);
            }
            else if (volume is CompoundBoundingVolume cbv)
            {
                return IntersectionTests.TestFixedAndFixed(this, cbv);
            }

            throw new System.NotImplementedException();     
        }

        public readonly bool TestIntersection(BoundingVolume volume, Vector v)
        {
            if (volume is AxisAlignedBoundingBox aabb)
            {
                return IntersectionTests.TestFixedAndMoving(this, aabb, v);
            }
            else if (volume is CompoundBoundingVolume cbv)
            {
                return IntersectionTests.TestFixedAndMoving(this, cbv, v);
            }

            throw new System.NotImplementedException();
        }

        internal readonly Vector AdjustMovingVolumeSideToSide(BoundingVolume volumeMoving, Vector v)
        {
            if (!TestIntersection(volumeMoving))
            {
                return v;
            }

            throw new System.NotImplementedException();
        }

        internal readonly Vector AdjustMovingVolumeUpAndDown(BoundingVolume volumeMoving, Vector v)
        {
            if (!TestIntersection(volumeMoving))
            {
                return v;
            }

            throw new System.NotImplementedException();
        }

    }

}

using Common;

namespace PhysicsEngine
{
    public abstract class BoundingVolume
    {
        internal abstract BoundingVolume Extend(Vector v);
        internal abstract BoundingVolume Move(Vector v);

        internal abstract Vector GetCenter();
        internal abstract Vector GetBottomCenter();

        internal abstract BoundingVolume GetMinBoundingVolume();
        internal abstract BoundingVolume GetMinBoundingVolume(Vector v);

        internal abstract bool TestIntersection(BoundingVolume volume);
        internal abstract bool TestIntersection(BoundingVolume volume, Vector v);

        internal abstract Vector AdjustMovingVolumeUpAndDown(BoundingVolume volume, Vector v);
        internal abstract Vector AdjustMovingVolumeSideToSide(BoundingVolume volume, Vector v);

    }

    public sealed class EmptyBoundingVolume : BoundingVolume
    {
        public EmptyBoundingVolume() { }

        internal override BoundingVolume Extend(Vector v)
        {
            return this;
        }

        internal override BoundingVolume Move(Vector v)
        {
            throw new System.NotImplementedException();
        }

        internal override Vector GetCenter()
        {
            throw new System.NotImplementedException();
        }

        internal override Vector GetBottomCenter()
        {
            throw new System.NotImplementedException();
        }

        internal override BoundingVolume GetMinBoundingVolume()
        {
            throw new System.NotImplementedException();
        }

        internal override  BoundingVolume GetMinBoundingVolume(Vector v)
        {
            throw new System.NotImplementedException();
        }

        internal override  bool TestIntersection(BoundingVolume volume)
        {
            return false;
        }

        internal override bool TestIntersection(BoundingVolume volume, Vector v)
        {
            return false;
        }

        internal override Vector AdjustMovingVolumeUpAndDown(BoundingVolume volume, Vector v)
        {
            if (!TestIntersection(volume))
            {
                return v;
            }

            return v;
        }

        internal override Vector AdjustMovingVolumeSideToSide(BoundingVolume volume, Vector v)
        {
            if (!TestIntersection(volume))
            {
                return v;
            }

            return v;
        }

    }

    public sealed class AxisAlignedBoundingBox : BoundingVolume
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

        internal override BoundingVolume Extend(Vector v)
        {
            throw new System.NotImplementedException();
        }

        internal override BoundingVolume Move(Vector v)
        {
            throw new System.NotImplementedException();
        }

        internal override Vector GetCenter()
        {
            throw new System.NotImplementedException();
        }

        internal override Vector GetBottomCenter()
        {
            throw new System.NotImplementedException();
        }

        internal override BoundingVolume GetMinBoundingVolume()
        {
            throw new System.NotImplementedException();
        }

        internal override BoundingVolume GetMinBoundingVolume(Vector v)
        {
            throw new System.NotImplementedException();
        }

        internal override bool TestIntersection(BoundingVolume volume)
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

        internal override bool TestIntersection(BoundingVolume volume, Vector v)
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

        internal override Vector AdjustMovingVolumeUpAndDown(BoundingVolume volume, Vector v)
        {
            if (!TestIntersection(volume, v))
            {
                return v;
            }

            throw new System.NotImplementedException();
        }

        internal override Vector AdjustMovingVolumeSideToSide(BoundingVolume volume, Vector v)
        {
            if (!TestIntersection(volume, v))
            {
                return v;
            }

            throw new System.NotImplementedException();
        }

    }

    /*public sealed class OrientedBoundingBox : BoundingVolume
    {

    }*/

    /*public sealed class BoundingSphere : BoundingVolume
    {

    }*/

    public sealed class CompoundBoundingVolume : BoundingVolume
    {

        public readonly BoundingVolume[] Volumes;

        public CompoundBoundingVolume(params BoundingVolume[] volumes)
        {
            // TODO: Assert all volumes are attached.
            Volumes = volumes;
        }

        internal override  BoundingVolume Extend(Vector v)
        {
            throw new System.NotImplementedException();
        }

        internal override  BoundingVolume Move(Vector v)
        {
            throw new System.NotImplementedException();
        }

        internal override  Vector GetCenter()
        {
            throw new System.NotImplementedException();
        }

        internal override  Vector GetBottomCenter()
        {
            throw new System.NotImplementedException();
        }

        internal override  BoundingVolume GetMinBoundingVolume()
        {
            throw new System.NotImplementedException();
        }

        internal override  BoundingVolume GetMinBoundingVolume(Vector v)
        {
            throw new System.NotImplementedException();
        }

        internal override  bool TestIntersection(BoundingVolume volume)
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

        internal override  bool TestIntersection(BoundingVolume volume, Vector v)
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

        internal override  Vector AdjustMovingVolumeUpAndDown(BoundingVolume volume, Vector v)
        {
            if (!TestIntersection(volume, v))
            {
                return v;
            }

            throw new System.NotImplementedException();
        }

        internal override  Vector AdjustMovingVolumeSideToSide(BoundingVolume volume, Vector v)
        {
            if (!TestIntersection(volume, v))
            {
                return v;
            }

            throw new System.NotImplementedException();
        }

    }

}

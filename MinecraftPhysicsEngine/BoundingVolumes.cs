using Common;

namespace MinecraftPhysicsEngine
{
    public abstract class BoundingVolume
    {
        public abstract void Extend(Vector v);
        public abstract void Move(Vector v);

        public abstract Vector GetCenter();
        public abstract Vector GetBottomCenter();

        public abstract AxisAlignedBoundingBox GetMinBoundingBox();

        public abstract bool TestIntersection(BoundingVolume volume);

    }

    public sealed class AxisAlignedBoundingBox : BoundingVolume
    {
        private Vector _max, _min;
        public Vector Max => _max;
        public Vector Min => _min;

        public AxisAlignedBoundingBox(Vector max, Vector min)
        {
            System.Diagnostics.Debug.Assert(max.X >= min.X);
            System.Diagnostics.Debug.Assert(max.Y >= min.Y);
            System.Diagnostics.Debug.Assert(max.Z >= min.Z);

            _max = max;
            _min = min;
        }

        public override void Extend(Vector v)
        {
            throw new System.NotImplementedException();
        }

        public override void Move(Vector v)
        {
            throw new System.NotImplementedException();
        }

        public override Vector GetCenter()
        {
            throw new System.NotImplementedException();
        }

        public override Vector GetBottomCenter()
        {
            throw new System.NotImplementedException();
        }

        public override AxisAlignedBoundingBox GetMinBoundingBox()
        {
            throw new System.NotImplementedException();
        }

        public override bool TestIntersection(BoundingVolume volume)
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

        /*public override bool TestIntersection(BoundingVolume volume, Vector v)
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
        }*/

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

        public override void Extend(Vector v)
        {
            throw new System.NotImplementedException();
        }

        public override void Move(Vector v)
        {
            throw new System.NotImplementedException();
        }

        public override  Vector GetCenter()
        {
            throw new System.NotImplementedException();
        }

        public override  Vector GetBottomCenter()
        {
            throw new System.NotImplementedException();
        }

        public override AxisAlignedBoundingBox GetMinBoundingBox()
        {
            throw new System.NotImplementedException();
        }

        public override  bool TestIntersection(BoundingVolume volume)
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

        /*public override  bool TestIntersection(BoundingVolume volume, Vector v)
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
        }*/

    }

}

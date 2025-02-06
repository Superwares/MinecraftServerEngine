

namespace MinecraftServerEngine.Physics.BoundingVolumes
{
    public sealed class CompoundBoundingVolume : BoundingVolume
    {
        public readonly BoundingVolume[] Volumes;

        public CompoundBoundingVolume(params BoundingVolume[] volumes)
        {
            // TODO: Assert all volumes are attached.
            Volumes = volumes;
        }

        internal override void Extend(Vector v)
        {
            throw new System.NotImplementedException();
        }

        internal override void Move(Vector v)
        {
            throw new System.NotImplementedException();
        }
        internal override bool Intersects(Vector o, Vector d)
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

        internal override double GetHeight()
        {
            throw new System.NotImplementedException();
        }

        public override AxisAlignedBoundingBox GetMinBoundingBox()
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

}

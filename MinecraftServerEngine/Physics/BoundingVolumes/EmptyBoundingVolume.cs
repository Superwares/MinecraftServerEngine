

namespace MinecraftServerEngine.Physics.BoundingVolumes
{
    public class EmptyBoundingVolume : BoundingVolume
    {
        private Vector _p;
        public Vector Position => _p;

        internal EmptyBoundingVolume(Vector p)
        {
            _p = p;
        }

        internal override void Extend(Vector v)
        {

        }

        internal override void Move(Vector v)
        {
            _p += v;
        }

        

        internal override Vector GetCenter()
        {
            return _p;
        }

        internal override Vector GetBottomCenter()
        {
            return _p;
        }

        internal override double GetHeight()
        {
            return 0.0D;
        }

        public override AxisAlignedBoundingBox GetMinBoundingBox()
        {
            return new(_p, _p);
        }

        internal override double TestIntersection(Vector o, Vector d)
        {
            return -1;
        }

        internal override bool TestIntersection(BoundingVolume volume)
        {
            return false;
        }

        internal override void ExtendAndMove(Vector extents, Vector v)
        {
            throw new System.NotImplementedException();
        }

        
    }

}

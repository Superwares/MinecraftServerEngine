
namespace MinecraftServerEngine.Physics.BoundingVolumes
{
    public abstract class BoundingVolume
    {

        internal abstract Vector GetCenter();
        internal abstract Vector GetBottomCenter();

        internal abstract double GetHeight();

        public abstract AxisAlignedBoundingBox GetMinBoundingBox();

        internal abstract void ExtendAndMove(Vector extents, Vector v);

        internal abstract void Extend(Vector extents);
        internal abstract void Move(Vector v);

        internal abstract double TestIntersection(Vector o, Vector d);

        // TODO: return tMin (power) like the method double TestIntersection(Vector o, Vector d).
        internal abstract bool TestIntersection(BoundingVolume volume);
        
        /*public abstract bool TestIntersection(BoundingVolume volume, Vector v);*/

        /*public override string ToString()
        {
            return base.ToString();
        }
        */

    }
}

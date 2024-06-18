

using Common;

namespace PhysicsEngine
{
    internal static class IntersectionTests
    {

        public static bool TestFixedAndFixed(
            AxisAlignedBoundingBox aabb1, AxisAlignedBoundingBox aabb2)
        {
            return !PhysicsEquations.IsNonOverlappingRanges(
                    aabb1.Max.X, aabb1.Min.X, aabb2.Max.X, aabb2.Min.X) &&
                !PhysicsEquations.IsNonOverlappingRanges(
                    aabb1.Max.Y, aabb1.Min.Y, aabb2.Max.Y, aabb2.Min.Y) &&
                !PhysicsEquations.IsNonOverlappingRanges(
                    aabb1.Max.Z, aabb1.Min.Z, aabb2.Max.Z, aabb2.Min.Z);
        }

        public static bool TestFixedAndFixed(
            AxisAlignedBoundingBox aabb, CompoundBoundingVolume cbv)
        {
            foreach (BoundingVolume volume in cbv.Volumes)
            {
                if (volume.TestIntersection(aabb))
                {
                    return true;
                }
            }

            return false;
        }

        public static bool TestFixedAndFixed(
            CompoundBoundingVolume cbv, AxisAlignedBoundingBox aabb)
        {
            return TestFixedAndFixed(aabb, cbv);
        }

        public static bool TestFixedAndFixed(
            CompoundBoundingVolume cbv1, CompoundBoundingVolume cbv2)
        {
            foreach (BoundingVolume volume in cbv1.Volumes)
            {
                if (volume.TestIntersection(cbv2))
                {
                    return true;
                }
            }

            return false;
        }

        public static bool TestFixedAndMoving(
            AxisAlignedBoundingBox aabb1,
            AxisAlignedBoundingBox aabb2, Vector v)
        {
            if (TestFixedAndFixed(aabb1, aabb2))
            {
                return true;
            }
            
            double t = 0.0D, tPrime = 1.0D;

            return PhysicsEquations.FindOverlapInterval(
                    aabb1.Max.X, aabb1.Min.X, 
                    aabb2.Max.X, aabb2.Min.X, v.X,
                    ref t, ref tPrime) &&
                PhysicsEquations.FindOverlapInterval(
                    aabb1.Max.Y, aabb1.Min.Y, 
                    aabb2.Max.Y, aabb2.Min.Y, v.Y,
                    ref t, ref tPrime) && 
                PhysicsEquations.FindOverlapInterval(
                    aabb1.Max.Z, aabb1.Min.Z, 
                    aabb2.Max.Z, aabb2.Min.Z, v.Z,
                    ref t, ref tPrime);
        }

        public static bool TestFixedAndMoving(
            AxisAlignedBoundingBox aabb,
            CompoundBoundingVolume cbv, Vector v)
        {
            foreach (BoundingVolume volume in cbv.Volumes)
            {
                if (aabb.TestIntersection(volume, v))
                {
                    return true;
                }
            }

            return false;
        }

        public static bool TestFixedAndMoving(
            CompoundBoundingVolume cbv,
            AxisAlignedBoundingBox aabb, Vector v)
        {
            foreach (BoundingVolume volume in cbv.Volumes)
            {
                if (volume.TestIntersection(aabb, v))
                {
                    return true;
                }
            }

            return false;
        }

        public static bool TestFixedAndMoving(
            CompoundBoundingVolume cbv1,
            CompoundBoundingVolume cbv2, Vector v)
        {
            foreach (BoundingVolume volume in cbv1.Volumes)
            {
                if (volume.TestIntersection(cbv2, v))
                {
                    return true;
                }
            }

            return false;
        }
    }
}

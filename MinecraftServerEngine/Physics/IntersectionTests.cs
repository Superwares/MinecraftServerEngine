

using Common;

namespace MinecraftServerEngine.Physics
{
    using BoundingVolumes;

    internal static class IntersectionTests
    {

        public static bool TestFixedAndFixed(
            AxisAlignedBoundingBox aabb0, AxisAlignedBoundingBox aabb1)
        {
            System.Diagnostics.Debug.Assert(aabb0 != null);
            System.Diagnostics.Debug.Assert(aabb1 != null);

            return Equations.IsNonOverlappingRanges(
                    aabb0.MaxVector.X, aabb0.MinVector.X, 
                    aabb1.MaxVector.X, aabb1.MinVector.X) == false
                && Equations.IsNonOverlappingRanges(
                    aabb0.MaxVector.Y, aabb0.MinVector.Y, 
                    aabb1.MaxVector.Y, aabb1.MinVector.Y) == false
                && Equations.IsNonOverlappingRanges(
                    aabb0.MaxVector.Z, aabb0.MinVector.Z, 
                    aabb1.MaxVector.Z, aabb1.MinVector.Z) == false;
        }

        public static bool TestFixedAndFixed(
            AxisAlignedBoundingBox aabb, OrientedBoundingBox obb)
        {
            System.Diagnostics.Debug.Assert(aabb != null);
            System.Diagnostics.Debug.Assert(obb != null);

            Vector[] axes = Equations.FindPotentialAxes(AxisAlignedBoundingBox.Axes, obb.Axes);

            foreach (Vector axis in axes)
            {
                (double min_aabb, double max_aabb) = Equations.FindAxisInterval(axis, aabb.Vertices);
                (double min_obb, double max_obb) = Equations.FindAxisInterval(axis, obb.Vertices);

                if (max_aabb < min_obb || min_aabb < max_obb)
                {
                    return false;
                }
            }

            return true;
        }

        public static bool TestFixedAndFixed(
            OrientedBoundingBox obb, AxisAlignedBoundingBox aabb)
        {
            System.Diagnostics.Debug.Assert(obb != null);
            System.Diagnostics.Debug.Assert(aabb != null);

            return TestFixedAndFixed(aabb, obb);
        }

        public static bool TestFixedAndFixed(
            OrientedBoundingBox obb0, OrientedBoundingBox obb1)
        {
            System.Diagnostics.Debug.Assert(obb0 != null);
            System.Diagnostics.Debug.Assert(obb1 != null);

            Vector[] axes = Equations.FindPotentialAxes(obb0.Axes, obb1.Axes);

            foreach (Vector axis in axes)
            {
                (double min0, double max0) = Equations.FindAxisInterval(axis, obb0.Vertices);
                (double min1, double max1) = Equations.FindAxisInterval(axis, obb1.Vertices);

                if (max0 < min1 || min0 < max1)
                {
                    return false;
                }
            }

            return true;
        }

        public static bool TestFixedAndFixed(
            AxisAlignedBoundingBox aabb, CompoundBoundingVolume cbv)
        {
            System.Diagnostics.Debug.Assert(aabb != null);
            System.Diagnostics.Debug.Assert(cbv != null);

            foreach (BoundingVolume volume in cbv.Volumes)
            {
                if (volume.TestIntersection(aabb) == true)
                {
                    return true;
                }
            }

            return false;
        }

        public static bool TestFixedAndFixed(
            CompoundBoundingVolume cbv, AxisAlignedBoundingBox aabb)
        {
            System.Diagnostics.Debug.Assert(cbv != null);
            System.Diagnostics.Debug.Assert(aabb != null);

            return TestFixedAndFixed(aabb, cbv);
        }

        public static bool TestFixedAndFixed(
            CompoundBoundingVolume cbv0, CompoundBoundingVolume cbv1)
        {
            System.Diagnostics.Debug.Assert(cbv0 != null);
            System.Diagnostics.Debug.Assert(cbv1 != null);

            foreach (BoundingVolume volume in cbv0.Volumes)
            {
                if (volume.TestIntersection(cbv1) == true)
                {
                    return true;
                }
            }

            return false;
        }

        /*public static bool TestFixedAndMoving(
            AxisAlignedBoundingBox aabb1,
            AxisAlignedBoundingBox aabb2, Vector v)
        {
            if (TestFixedAndFixed(aabb1, aabb2))
            {
                return true;
            }
            
            double t = 0.0D, tPrime = 1.0D;

            return Equations.F1(
                    aabb1.MaxVector.X, aabb1.MinVector.X, 
                    aabb2.MaxVector.X, aabb2.MinVector.X, v.X,
                    ref t, ref tPrime) &&
                Equations.F1(
                    aabb1.MaxVector.Y, aabb1.MinVector.Y, 
                    aabb2.MaxVector.Y, aabb2.MinVector.Y, v.Y,
                    ref t, ref tPrime) && 
                Equations.F1(
                    aabb1.MaxVector.Z, aabb1.MinVector.Z, 
                    aabb2.MaxVector.Z, aabb2.MinVector.Z, v.Z,
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
        }*/
    }
}

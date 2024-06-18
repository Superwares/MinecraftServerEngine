

using Common;
using Containers;

namespace PhysicsEngine
{
    internal static class IntersectionTests
    {
        private const double MinTime = 0.0D, MaxTime = 1.0D;

        public static bool TestFixedAndFixed(
            AxisAlignedBoundingBox aabb1, AxisAlignedBoundingBox aabb2)
        {
            if (aabb1.Max.X < aabb2.Min.X || aabb1.Min.X > aabb2.Max.X) return false;
            if (aabb1.Max.Y < aabb2.Min.Y || aabb1.Min.Y > aabb2.Max.Y) return false;
            if (aabb1.Max.Z < aabb2.Min.Z || aabb1.Min.Z > aabb2.Max.Z) return false;

            return true;
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

        private static bool CheckAxis(
            double max1, double min1, 
            double max2, double min2, double v,
            ref double t, ref double tPrime)
        {
            System.Diagnostics.Debug.Assert(max1 > min1);
            System.Diagnostics.Debug.Assert(max2 > min2);
            System.Diagnostics.Debug.Assert(t <= tPrime);

            if (v < 0.0D)
            {
                if (max2 < min1)
                {
                    return false;
                }

                if (max1 < min2)
                {
                    t = System.Math.Max((max1 - min2) / v, t);
                    System.Diagnostics.Debug.Assert(t >= MinTime);
                }
                if (min1 < max2)
                {
                    tPrime = System.Math.Min((min1 - max2) / v, tPrime);
                    System.Diagnostics.Debug.Assert(tPrime <= MaxTime);
                }
            }
            else if (v > 0.0D)
            {
                if (max1 < min2)
                {
                    return false;
                }

                if (max2 < min1)
                {
                    t = System.Math.Max((min1 - max2) / v, t);
                    System.Diagnostics.Debug.Assert(t >= MinTime);
                }
                if (min2 < max1)
                {
                    tPrime = System.Math.Min((max1 - min2) / v, tPrime);
                    System.Diagnostics.Debug.Assert(tPrime <= MaxTime);
                }
            }
            else
            {
                if (max2 < min1 || min2 > max1)
                {
                    return false;
                }
            }

            return t <= tPrime;
        }

        public static bool TestFixedAndMoving(
            AxisAlignedBoundingBox aabb1,
            AxisAlignedBoundingBox aabb2, Vector v)
        {
            if (TestFixedAndFixed(aabb1, aabb2))
            {
                return true;
            }
            
            double t = MinTime, tPrime = MaxTime;

            return CheckAxis(
                    aabb1.Max.X, aabb1.Min.X, 
                    aabb2.Max.X, aabb2.Min.X, v.X,
                    ref t, ref tPrime) &&
                CheckAxis(
                    aabb1.Max.Y, aabb1.Min.Y, 
                    aabb2.Max.Y, aabb2.Min.Y, v.Y,
                    ref t, ref tPrime) && 
                CheckAxis(
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

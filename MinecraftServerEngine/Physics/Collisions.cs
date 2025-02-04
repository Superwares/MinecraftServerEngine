

namespace MinecraftServerEngine.Physics
{
    internal static class Collisions
    {
        // returns axis, t
        public static (int, double) Resolve(
            AxisAlignedBoundingBox aabb1, 
            AxisAlignedBoundingBox aabb2, Vector v)
        {
            System.Diagnostics.Debug.Assert(aabb1 != null);
            System.Diagnostics.Debug.Assert(aabb2 != null);

            if ((aabb2.Min.X < aabb1.Max.X && aabb1.Min.X < aabb2.Max.X) &&
                (aabb2.Min.Y < aabb1.Max.Y && aabb1.Min.Y < aabb2.Max.Y) &&
                (aabb2.Min.Z < aabb1.Max.Z && aabb1.Min.Z < aabb2.Max.Z))
            {
                return (-1, 0.0D);
            }

            double t = double.NegativeInfinity, tPrime = double.PositiveInfinity;

            int axis = -1;

            bool collided, updated;

            (collided, updated) = Equations.FindCollisionInterval1(
                aabb1.Max.Y, aabb1.Min.Y,
                aabb2.Max.Y, aabb2.Min.Y, v.Y,
                ref t, ref tPrime);
            if (!collided)
            {
                return (-1, 0.0D);
            }

            System.Diagnostics.Debug.Assert(t <= tPrime);

            if (updated)
            {
                axis = 0;
            }

            (collided, updated) = Equations.FindCollisionInterval1(
                aabb1.Max.X, aabb1.Min.X,
                aabb2.Max.X, aabb2.Min.X, v.X,
                ref t, ref tPrime);
            if (!collided)
            {
                return (-1, 0.0D);
            }

            System.Diagnostics.Debug.Assert(t <= tPrime);

            if (updated)
            {
                axis = 1;
            }

            (collided, updated) = Equations.FindCollisionInterval1(
                aabb1.Max.Z, aabb1.Min.Z,
                aabb2.Max.Z, aabb2.Min.Z, v.Z,
                ref t, ref tPrime);
            if (!collided)
            {
                return (-1, 0.0D);
            }

            System.Diagnostics.Debug.Assert(t <= tPrime);

            if (updated)
            {
                axis = 2;
            }

            if (t < 0.0D || t > 1.0D)
            {
                return (-1, 0.0D);
            }

            System.Diagnostics.Debug.Assert(axis > -1);
            System.Diagnostics.Debug.Assert(axis < 3);
            System.Diagnostics.Debug.Assert(t >= 0.0D);
            System.Diagnostics.Debug.Assert(t <= 1.0D);
            return (axis, t);
        }

        public static (int, double) Resolve(
            AxisAlignedBoundingBox aabb, 
            CompoundBoundingVolume cbv, Vector v)
        {
            int axis = -1;
            double t = double.PositiveInfinity;

            int a;
            double b;
            foreach (BoundingVolume volume in cbv.Volumes)
            {
                (a, b) = aabb.ResolveCollision(volume, v);

                System.Diagnostics.Debug.Assert(t >= 0.0D);
                if (a > -1 && b < t)
                {
                    System.Diagnostics.Debug.Assert(b <= 1.0D);
                    System.Diagnostics.Debug.Assert(b >= 0.0D);
                    System.Diagnostics.Debug.Assert(a < 3);

                    axis = a;
                    t = b;
                }
            }

            System.Diagnostics.Debug.Assert(t >= 0.0D);

            return (axis, t);
        }


    }
}

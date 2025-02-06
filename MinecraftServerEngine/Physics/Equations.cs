
namespace MinecraftServerEngine.Physics
{
    internal static class Equations
    {

        internal static bool IsNonOverlappingRanges(
            double max1, double min1, double max2, double min2)
        {
            System.Diagnostics.Debug.Assert(max1 > min1);
            System.Diagnostics.Debug.Assert(max2 > min2);

            return (max1 < min2 || max2 < min1);
        }

        internal static double TestNonOverlappingRanges(
            double max1, double min1, double max2, double min2)
        {
            System.Diagnostics.Debug.Assert(max1 > min1);
            System.Diagnostics.Debug.Assert(max2 > min2);

            if (max1 < min2 || max2 < min1)
            {
                return -1;
            }

            throw new System.NotImplementedException();
        }

        // returns collided, updated.
        internal static (bool, bool) FindCollisionInterval1(
            double max1, double min1,
            double max2, double min2, double v,
            ref double t, ref double tPrime)
        {
            System.Diagnostics.Debug.Assert(max1 > min1);
            System.Diagnostics.Debug.Assert(max2 > min2);

            System.Diagnostics.Debug.Assert(t <= tPrime);

            bool updated = false;
            double x;
            if (v < 0.0D)
            {
                if (max2 <= min1)
                {
                    return (false, false);
                }

                x = (max1 - min2) / v;
                if (x > t)
                {
                    t = x;

                    updated = true;
                }

                tPrime = System.Math.Min((min1 - max2) / v, tPrime);
            }
            else if (v > 0.0D)
            {
                if (max1 <= min2)
                {
                    return (false, false);
                }

                x = (min1 - max2) / v;
                if (x > t)
                {
                    t = x;

                    updated = true;
                }

                tPrime = System.Math.Min((max1 - min2) / v, tPrime);
            }
            else
            {
                System.Diagnostics.Debug.Assert(!updated);
                if (max1 <= min2 || max2 <= min1)
                {
                    return (false, false);
                }
                else
                {
                    return (true, false);
                }
            }

            return (t <= tPrime, updated);
        }

        /*public static bool FindOverlapInterval(
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
                }
                if (min1 < max2)
                {
                    tPrime = System.Math.Min((min1 - max2) / v, tPrime);
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
                }
                if (min2 < max1)
                {
                    tPrime = System.Math.Min((max1 - min2) / v, tPrime);
                }
            }
            else
            {
                if (max2 < min1 || min2 > max1)
                {
                    return false;
                }
            }

            return (t <= tPrime);
        }*/

        internal static Vector[] FindPotentialAxes(Vector[] axes1, Vector[] axes2)
        {
            System.Diagnostics.Debug.Assert(axes1 != null);
            System.Diagnostics.Debug.Assert(axes1.Length == Vector.Dimension);
            System.Diagnostics.Debug.Assert(axes2 != null);
            System.Diagnostics.Debug.Assert(axes2.Length == Vector.Dimension);

            const int Length = Vector.Dimension + Vector.Dimension + (Vector.Dimension * Vector.Dimension);
            Vector[] potential_axes = new Vector[Length];

            potential_axes[0] = axes1[0];
            potential_axes[1] = axes1[1];
            potential_axes[2] = axes1[2];
            potential_axes[3] = axes2[0];
            potential_axes[4] = axes2[1];
            potential_axes[5] = axes2[2];

            for (int i = 0; i < Vector.Dimension; ++i)
            {
                int j = i * 3;
                potential_axes[6 + j] = axes1[i].CrossProduct(axes2[0]);
                potential_axes[7 + j] = axes1[i].CrossProduct(axes2[1]);
                potential_axes[8 + j] = axes1[i].CrossProduct(axes2[2]);
            }

            return potential_axes;
        }

        // The function to get the interval of the shapes specified by the axis.
        internal static (double, double) FindAxisInterval(Vector axis, Vector[] vectors)
        {
            System.Diagnostics.Debug.Assert(vectors.Length > 0);

            double out_min = axis.DotProduct(vectors[0]);
            double out_max = out_min;

            double projection;

            // Projection of individual vertices on the specified axes.
            foreach (Vector vector in vectors)
            {
                // Projection of the axis onto the individual vertices 
                // of the bounding box (OBB, AABB).
                projection = axis.DotProduct(vector);

                // Store the minimum projection in an interval structure.
                if (projection < out_min)
                {
                    out_min = projection;
                }

                // Store the maximum projection in an interval structure.
                if (projection > out_max)
                {
                    out_max = projection;
                }
            }
            
            return (out_min, out_max);
        }

    }
}

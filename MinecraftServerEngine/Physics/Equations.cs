
namespace MinecraftServerEngine.Physics
{
    internal static class Equations
    {
        public static bool IsNonOverlappingRanges(
            double max1, double min1, double max2, double min2)
        {
            System.Diagnostics.Debug.Assert(max1 > min1);
            System.Diagnostics.Debug.Assert(max2 > min2);

            return (max1 < min2 || max2 < min1);
        }

        public static double TestNonOverlappingRanges(
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
        public static (bool, bool) FindCollisionInterval1(
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
    }
}

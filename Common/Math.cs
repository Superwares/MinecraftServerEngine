

namespace Common
{
    public static class Math
    {
        public static double Sqrt(double s)
        {
            return System.Math.Sqrt(s);
        }

        public static double Abs(double s)
        {
            return System.Math.Abs(s);
        }

        public static bool AreDoublesEqual(double a, double b)
        {
            const double Tolerance = 1e-10;
            return System.Math.Abs(a - b) < Tolerance;
        }

        /// <summary>
        /// This method normalizes a value within the range of 0 to ModulusValue - 1.
        /// If the value is negative, it wraps around to ensure the result is positive.
        /// </summary>
        /// <param name="value">The value to be normalized.</param>
        /// <param name="ModulusValue">It is exclusive, meaning it should be greater than 0.</param>
        public static long Normalize(long value, long ModulusValue)
        {
            if (ModulusValue <= 0)
            {
                throw new System.ArgumentNullException(nameof(value));
            }

            long normalizedValue = value % ModulusValue;
            return normalizedValue < 0 ? normalizedValue + ModulusValue : normalizedValue;
        }
    }
}

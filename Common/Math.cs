

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
    }
}

namespace Common
{
    public static class Comparing
    {

        public static bool IsEqualTo(float v1, float v2)
        {
            return v1 == v2;
        }

        public static bool IsInRange(float v, float from, float to)
        {
            System.Diagnostics.Debug.Assert(from <= to);
            return v >= from && v <= to;
        }

        public static bool IsGreaterThan(float v1, float v2)
        {
            return v1 > v2;
        }

        public static bool IsGreaterThanOrEqualTo(float v1, float v2)
        {
            return v1 >= v2;
        }

        public static bool IsLessThan(float v1, float v2)
        {
            return v1 < v2;
        }

        public static bool IsLessThanOrEqualTo(float v1, float v2)
        {
            return v1 <= v2;
        }

        public static bool IsEqualTo(double v1, double v2)
        {
            return v1 == v2;
        }

        public static bool IsInRange(double v, double from, double to)
        {
            System.Diagnostics.Debug.Assert(from <= to);
            return v >= from && v <= to;
        }

        public static bool IsGreaterThan(double v1, double v2)
        {
            return v1 > v2;
        }

        public static bool IsGreaterThanOrEqualTo(double v1, double v2)
        {
            return v1 >= v2;
        }

        public static bool IsLessThan(double v1, double v2)
        {
            return v1 < v2;
        }

        public static bool IsLessThanOrEqualTo(double v1, double v2)
        {
            return v1 <= v2;
        }

    }
}

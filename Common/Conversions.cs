

namespace Common
{
    public static class Conversions
    {
        // TODO: Write test case to check it is correctely.

        public static byte ToByte(int v)
        {
            System.Diagnostics.Debug.Assert(v >= 0);
            System.Diagnostics.Debug.Assert(v <= byte.MaxValue);
            return (byte)v;
        }

        public static short ToShort(double v)
        {
            return (short)v;
        }

        public static int ToInt(ulong v)
        {
            System.Diagnostics.Debug.Assert(v <= int.MaxValue);
            return (int)v;
        }

        public static int ToInt(double v)
        {
            return (int)v;
        }

        public static uint ToUint(int v)
        {
            System.Diagnostics.Debug.Assert(v >= 0);
            return (uint)v;
        }

        public static uint ToUint(ulong v)
        {
            System.Diagnostics.Debug.Assert(v <= uint.MaxValue);
            return (uint)v;
        }
        public static long ToLong(ulong v)
        {
            System.Diagnostics.Debug.Assert(v <= long.MaxValue);
            return (long)v;
        }

        public static ulong ToUlong(int v)
        {
            System.Diagnostics.Debug.Assert(v >= 0);
            return (ulong)v;
        }

        public static double ToDouble(int v)
        {
            return (double)v;
        }

    }
}

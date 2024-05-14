

namespace Common
{
    public static class Conversions
    {
        // TODO: Write test case to check it is correctely.
        public static ulong ToUlong(int v)
        {
            System.Diagnostics.Debug.Assert(v >= 0);

            return (ulong)v;
        }

        public static byte ToByte(int v)
        {
            System.Diagnostics.Debug.Assert(v >= 0);

            return (byte)v;
        }

        public static uint ToUint(ulong v)
        {
            return (uint)v;
        }

        public static uint ToUint(int v)
        {
            return (uint)v;
        }

        public static long ToLong(ulong v)
        {
            long ret = (long)v;
            System.Diagnostics.Debug.Assert(ret >= 0);

            return ret;
        }

        public static int ToInt(ulong v)
        {
            return (int)v;
        }

        public static short ToShort(double v)
        {
            return (short)v;
        }

        public static double ToDouble(int v)
        {
            return (double)v;
        }

    }
}

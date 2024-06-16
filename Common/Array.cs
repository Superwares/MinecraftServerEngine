

namespace Common
{
    public static class Array<T>
    {
        public static void Fill(T[] array, T value)
        {
            if (array == null)
            {
                return;
            }

            System.Array.Fill<T>(array, value);
        }

        public static void Copy(T[] arraySrc, T[] arrayDest, int length)
        {
            System.Diagnostics.Debug.Assert(length >= 0);
            if (length == 0)
            {
                return;
            }

            throw new System.NotImplementedException();
        }
    }
}

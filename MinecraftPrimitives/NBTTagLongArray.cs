
namespace MinecraftPrimitives
{
    public sealed class NBTTagLongArray : NBTTagBase, IReadableNBTTag<NBTTagLongArray>
    {
        public const int TypeId = 12;

        private bool _disposed = false;

        public readonly long[] Data;

        public static NBTTagLongArray Read(System.IO.Stream s, int depth)
        {
            System.Diagnostics.Debug.Assert(s != null);
            System.Diagnostics.Debug.Assert(depth >= 0);

            int length = DataInputStreamUtils.ReadInt(s);

            long[] data = new long[length];

            for (int i = 0; i < length; ++i)
            {

                data[i] = DataInputStreamUtils.ReadLong(s);
            }

            return new NBTTagLongArray(data);
        }

        private NBTTagLongArray(long[] data)
        {
            System.Diagnostics.Debug.Assert(data != null);

            Data = data;
        }

        ~NBTTagLongArray()
        {
            System.Diagnostics.Debug.Assert(false);

            Dispose(false);
        }

        public override void Write(MinecraftDataStream s)
        {
            System.Diagnostics.Debug.Assert(s != null);

            System.Diagnostics.Debug.Assert(_disposed == false);

            s.WriteInt(Data.Length);

            foreach (long value in Data)
            {
                s.WriteLong(value);
            }
        }

        public override string ToString()
        {
            return $"{this.GetType().Name}({Data.Length})";
        }

        protected override void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if (_disposed == false)
            {

                // If disposing equals true, dispose all managed
                // and unmanaged resources.
                if (disposing == true)
                {
                    // Dispose managed resources.
                }

                // Call the appropriate methods to clean up
                // unmanaged resources here.
                // If disposing is false,
                // only the following code is executed.
                //CloseHandle(handle);
                //handle = IntPtr.Zero;

                // Note disposing has been done.
                _disposed = true;
            }

            base.Dispose(disposing);
        }
    }
}

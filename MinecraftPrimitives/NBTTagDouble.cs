
namespace MinecraftPrimitives
{
    public sealed class NBTTagDouble : NBTTagBase, IReadableNBTTag<NBTTagDouble>
    {
        public const int TypeId = 6;

        private bool _disposed = false;

        public readonly double Value;

        public static NBTTagDouble Read(System.IO.Stream s, int depth)
        {
            System.Diagnostics.Debug.Assert(s != null);
            System.Diagnostics.Debug.Assert(depth >= 0);

            double value = DataInputStreamUtils.ReadDouble(s);
            return new NBTTagDouble(value);
        }

        private NBTTagDouble(double value)
        {
            Value = value;
        }

        ~NBTTagDouble()
        {
            System.Diagnostics.Debug.Assert(false);

            Dispose(false);
        }

        public override void Write(System.IO.Stream s)
        {
            System.Diagnostics.Debug.Assert(s != null);

            System.Diagnostics.Debug.Assert(_disposed == false);

            throw new System.NotImplementedException();
        }

        public override string ToString()
        {
            return $"NBTTagDouble({Value})";
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

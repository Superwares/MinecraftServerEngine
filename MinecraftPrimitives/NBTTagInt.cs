

namespace MinecraftPrimitives
{
    public sealed class NBTTagInt : NBTTagBase, IReadableNBTTag<NBTTagInt>
    {
        public const int TypeId = 3;

        private bool _disposed = false;

        public readonly int Value;

        public static NBTTagInt Read(System.IO.Stream s, int depth)
        {
            System.Diagnostics.Debug.Assert(s != null);
            System.Diagnostics.Debug.Assert(depth >= 0);

            int value = DataInputStreamUtils.ReadInt(s);
            return new NBTTagInt(value);
        }

        private NBTTagInt(int value)
        {
            Value = value;
        }

        ~NBTTagInt()
        {
            System.Diagnostics.Debug.Assert(false);

            Dispose(false);
        }

        public override void Write(MinecraftDataStream s)
        {
            System.Diagnostics.Debug.Assert(s != null);

            System.Diagnostics.Debug.Assert(_disposed == false);

            s.WriteInt(Value);
        }

        public override string ToString()
        {
            return $"NBTTagInt({Value})";
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

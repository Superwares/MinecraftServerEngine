using MinecraftServerEngine.Protocols;

namespace MinecraftServerEngine.NBT
{
    public sealed class NBTTagLong : NBTTagBase, IReadableNBTTag<NBTTagLong>
    {
        public const int TypeId = 4;

        private bool _disposed = false;

        public readonly long Value;

        public static byte GetTypeId() => TypeId;

        public static NBTTagLong Read(System.IO.Stream s, int depth)
        {
            System.Diagnostics.Debug.Assert(s != null);
            System.Diagnostics.Debug.Assert(depth >= 0);

            long value = DataInputStreamUtils.ReadLong(s);
            return new NBTTagLong(value);
        }

        public NBTTagLong(long value)
        {
            Value = value;
        }

        ~NBTTagLong()
        {
            System.Diagnostics.Debug.Assert(false);

            Dispose(false);
        }

        public override void Write(MinecraftProtocolDataStream s)
        {
            System.Diagnostics.Debug.Assert(s != null);

            System.Diagnostics.Debug.Assert(_disposed == false);

            s.WriteLong(Value);
        }

        public override string ToString()
        {
            return $"NBTTagLong({Value})";
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

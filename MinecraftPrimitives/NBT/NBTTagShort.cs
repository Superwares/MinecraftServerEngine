using MinecraftPrimitives.Protocols;

namespace MinecraftPrimitives.NBT
{
    public sealed class NBTTagShort : NBTTagBase, IReadableNBTTag<NBTTagShort>
    {
        public const int TypeId = 2;

        private bool _disposed = false;

        public readonly short Value;

        public static byte GetTypeId() => TypeId;

        public static NBTTagShort Read(System.IO.Stream s, int depth)
        {
            System.Diagnostics.Debug.Assert(s != null);
            System.Diagnostics.Debug.Assert(depth >= 0);

            int value = DataInputStreamUtils.ReadShort(s);

            System.Diagnostics.Debug.Assert(value >= short.MinValue);
            System.Diagnostics.Debug.Assert(value <= short.MaxValue);
            return new NBTTagShort((short)value);
        }

        public NBTTagShort(short value)
        {
            Value = value;
        }

        ~NBTTagShort()
        {
            System.Diagnostics.Debug.Assert(false);

            Dispose(false);
        }

        public override void Write(MinecraftProtocolDataStream s)
        {
            System.Diagnostics.Debug.Assert(s != null);

            System.Diagnostics.Debug.Assert(_disposed == false);

            s.WriteShort(Value);
        }

        public override string ToString()
        {
            return $"NBTTagShort({Value})";
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

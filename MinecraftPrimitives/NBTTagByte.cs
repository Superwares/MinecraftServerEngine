
namespace MinecraftPrimitives
{
    public sealed class NBTTagByte : NBTTagBase, IReadableNBTTag<NBTTagByte>
    {
        public const int TypeId = 1;

        private bool _disposed = false;

        public readonly int Value;

        public static NBTTagByte Read(System.IO.Stream s, int depth)
        {
            System.Diagnostics.Debug.Assert(s != null);
            System.Diagnostics.Debug.Assert(depth >= 0);

            int value = DataInputStreamUtils.ReadByte(s);
            return new NBTTagByte(value);
        }

        private NBTTagByte(int value)
        {
            Value = value;
        }

        ~NBTTagByte()
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
            return $"NBTTagByte({Value})";
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

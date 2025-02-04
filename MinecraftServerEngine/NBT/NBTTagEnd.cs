using MinecraftServerEngine.Protocols;

namespace MinecraftServerEngine.NBT
{
    public sealed class NBTTagEnd : NBTTagBase, IReadableNBTTag<NBTTagEnd>
    {
        public const int TypeId = 0;


        private bool _disposed = false;

        public static byte GetTypeId() => TypeId;

        public static NBTTagEnd Read(System.IO.Stream s, int depth)
        {
            System.Diagnostics.Debug.Assert(s != null);
            System.Diagnostics.Debug.Assert(depth >= 0);

            return new NBTTagEnd();
        }

        private NBTTagEnd()
        {
        }

        ~NBTTagEnd()
        {
            System.Diagnostics.Debug.Assert(false);

            Dispose(false);
        }

        public override void Write(MinecraftProtocolDataStream s)
        {
            System.Diagnostics.Debug.Assert(s != null);

            System.Diagnostics.Debug.Assert(_disposed == false);
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

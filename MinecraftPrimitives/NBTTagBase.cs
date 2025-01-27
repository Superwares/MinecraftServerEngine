

namespace MinecraftPrimitives
{

    public abstract class NBTTagBase : System.IDisposable
    {
        private bool _disposed = false;

        ~NBTTagBase()
        {
            System.Diagnostics.Debug.Assert(false);

            Dispose(false);
        }

        public abstract void Write(MinecraftProtocolDataStream s);

        public override string ToString()
        {
            throw new System.NotImplementedException();
        }

        public void Dispose()
        {
            Dispose(true);
            System.GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
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

        }

    }
}

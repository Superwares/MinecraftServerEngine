namespace MinecraftServerEngine.Inventories
{
    public sealed class ChestInventory : SharedInventory
    {

        private bool _disposed = false;

        public override string Title => "Chest";

        public ChestInventory(int lines) : base(lines) { }

        ~ChestInventory()
        {
            System.Diagnostics.Debug.Assert(false);

            Dispose(false);
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

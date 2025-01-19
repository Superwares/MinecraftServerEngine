

namespace MinecraftServerEngine
{
    public abstract class ItemInterfaceInventory : SharedInventory
    {

        private const int MaxLineCount = 6;

        private bool _disposed = false;


        private readonly int totalSlotCount;

        internal override int GetTotalSlotCount()
        {
            System.Diagnostics.Debug.Assert(totalSlotCount > 0);
            return totalSlotCount;
        }

        public ItemInterfaceInventory(int line) : base()
        {
            System.Diagnostics.Debug.Assert(line > 0);
            System.Diagnostics.Debug.Assert(line <= MaxLineCount);
            totalSlotCount = line * SlotsPerLine;
        }

        ~ItemInterfaceInventory()
        {
            System.Diagnostics.Debug.Assert(false);

            Dispose(false);
        }

        protected abstract void OnLeftClickSharedItem(
            UserId userId,
            PlayerInventory playerInventory,
            int i, ItemType Item, int count);

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

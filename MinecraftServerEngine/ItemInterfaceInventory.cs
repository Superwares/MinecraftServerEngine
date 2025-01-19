

namespace MinecraftServerEngine
{
    public abstract class ItemInterfaceInventory : PublicInventory
    {
        protected abstract class ClickContext
        {
            private readonly PrivateInventory Inventory;

            internal ClickContext(PrivateInventory inv)
            {
                System.Diagnostics.Debug.Assert(inv != null);
                Inventory = inv;
            }

            public bool Give(ItemType item, int count)
            {
                System.Diagnostics.Debug.Assert(count >= 0);

                System.Diagnostics.Debug.Assert(Inventory != null);
                return Inventory.GiveFromLeftInMain(item, count);
            }
        }

        protected sealed class LeftClickContext : ClickContext
        {
            internal LeftClickContext(PrivateInventory inv) : base(inv)
            {
                System.Diagnostics.Debug.Assert(inv != null);
            }
        }

        protected sealed class RightClickContext : ClickContext
        {
            internal RightClickContext(PrivateInventory inv) : base(inv)
            {
                System.Diagnostics.Debug.Assert(inv != null);
            }
        }

        protected sealed class MiddleClickContext : ClickContext
        {
            internal MiddleClickContext(PrivateInventory inv) : base(inv)
            {
                System.Diagnostics.Debug.Assert(inv != null);
            }
        }

        private const int MaxLineCount = 6;

        private bool _disposed = false;


        private readonly int totalSlotCount;

        private protected override int GetTotalSlotCount()
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


        protected abstract void OnLeftClick(LeftClickContext ctx, int i);

        protected abstract void OnRightClick(RightClickContext ctx, int i);

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

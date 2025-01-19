using Common;
using Containers;
using Sync;

namespace MinecraftServerEngine
{

    public abstract class Inventory : System.IDisposable
    {
        internal const int SlotsPerLine = 9;


        private bool _disposed = false;

        


        internal readonly InventorySlot[] Slots;


        private protected abstract int GetTotalSlotCount();

        internal Inventory()
        {
            int totalSlots = GetTotalSlotCount();
            System.Diagnostics.Debug.Assert(totalSlots >= 0);

            Slots = new InventorySlot[totalSlots];
            for (int i = 0; i < totalSlots; ++i)
            {
                Slots[i] = new InventorySlot();
            }
        }

        ~Inventory()
        {
            System.Diagnostics.Debug.Assert(false);

            Dispose(false);
        }

        public void Print()
        {
            int totalSlots = GetTotalSlotCount();
            System.Diagnostics.Debug.Assert(totalSlots >= 0);

            MyConsole.Printl($"Inventory: ");
            for (int i = 0; i < totalSlots; ++i)
            {
                if (i % SlotsPerLine == 0)
                {
                    MyConsole.NewLine();
                    MyConsole.NewTab();
                }

                InventorySlot slot = Slots[i];
                System.Diagnostics.Debug.Assert(slot != null);

                MyConsole.Print($"{slot}");
            }
            MyConsole.NewLine();
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

using Common;
using Containers;
using Sync;

namespace MinecraftServerEngine
{

    public abstract class Inventory : System.IDisposable
    {
        public const int SlotCountPerLine = 9;



        private bool _disposed = false;

        private readonly int _totalSlotCount;
        internal readonly InventorySlot[] Slots;


        internal Inventory(int totalSlotCount)
        {
            System.Diagnostics.Debug.Assert(totalSlotCount > 0);

            Slots = new InventorySlot[totalSlotCount];
            for (int i = 0; i < totalSlotCount; ++i)
            {
                Slots[i] = new InventorySlot();
            }

            _totalSlotCount = totalSlotCount;
        }

        ~Inventory()
        {
            System.Diagnostics.Debug.Assert(false);

            Dispose(false);
        }

        public int GetTotalSlotCount()
        {
            System.Diagnostics.Debug.Assert(_totalSlotCount > 0);
            return _totalSlotCount;
        }

        public void Print()
        {
            int totalSlots = GetTotalSlotCount();
            System.Diagnostics.Debug.Assert(totalSlots >= 0);

            MyConsole.Printl($"Inventory: ");
            for (int i = 0; i < totalSlots; ++i)
            {
                if (i % SlotCountPerLine == 0)
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

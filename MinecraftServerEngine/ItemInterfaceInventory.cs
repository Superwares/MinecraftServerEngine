
using MinecraftPrimitives;

namespace MinecraftServerEngine
{
    public abstract class ItemInterfaceInventory : SharedInventory
    {

        private bool _disposed = false;


        public ItemInterfaceInventory(int totalLineCount) : base(totalLineCount)
        {
            System.Diagnostics.Debug.Assert(totalLineCount > 0);
            System.Diagnostics.Debug.Assert(totalLineCount <= MaxLineCount);

            ItemStack a = new ItemStack(ItemType.Stick, "Hello");

            // TODO: remove
            //Slots[10].Move(ref a);

        }

        ~ItemInterfaceInventory()
        {
            System.Diagnostics.Debug.Assert(false);

            Dispose(false);
        }

        public void ResetSlot(int index, ItemStack itemStack)
        {
            if (index < 0 || index >= GetTotalSlotCount())
            {
                throw new System.ArgumentOutOfRangeException(nameof(index));
            }

            if (itemStack == null)
            {
                Slots[index].Reset(null);
            } else
            {
                Slots[index].Reset(itemStack);
            }


        }

        protected abstract void OnLeftClickSharedItem(
            UserId userId, AbstractPlayer player,
            PlayerInventory playerInventory,
            int i, ItemStack itemStack);

        protected abstract void OnRightClickSharedItem(
            UserId userId, AbstractPlayer player,
            PlayerInventory playerInventory,
            int i, ItemStack itemStack);

        internal override void LeftClick(
            UserId userId, AbstractPlayer player, PlayerInventory playerInventory,
            int i, InventorySlot cursor)
        {
            System.Diagnostics.Debug.Assert(_disposed == false);

            int totalSlots = GetTotalSlotCount();
            System.Diagnostics.Debug.Assert(totalSlots > 0);

            System.Diagnostics.Debug.Assert(i >= 0);
            System.Diagnostics.Debug.Assert(i < totalSlots + PlayerInventory.PrimarySlotCount);
            System.Diagnostics.Debug.Assert(cursor != null);

            //base.LeftClick(userId, playerInventory, i, cursor);

            Locker.Hold();

            try
            {

                if (i < totalSlots)
                {
                    InventorySlot slot = GetSlot(playerInventory, i);

                    System.Diagnostics.Debug.Assert(slot != null);

                    if (slot.Empty == false)
                    {
                        OnLeftClickSharedItem(
                            userId, player, playerInventory,
                            i, slot.Stack);
                    }

                }
            }
            finally
            {
                UpdateRendering(userId, playerInventory);

                Locker.Release();
            }

        }

        internal override void RightClick(
            UserId userId, AbstractPlayer player, PlayerInventory playerInventory,
            int i, InventorySlot cursor)
        {
            System.Diagnostics.Debug.Assert(_disposed == false);

            int totalSlots = GetTotalSlotCount();
            System.Diagnostics.Debug.Assert(totalSlots > 0);

            System.Diagnostics.Debug.Assert(i >= 0);
            System.Diagnostics.Debug.Assert(i < totalSlots + PlayerInventory.PrimarySlotCount);
            System.Diagnostics.Debug.Assert(cursor != null);

            //base.RightClick(userId, playerInventory, i, cursor);

            Locker.Hold();

            try
            {

                if (i < totalSlots)
                {
                    InventorySlot slot = GetSlot(playerInventory, i);

                    System.Diagnostics.Debug.Assert(slot != null);

                    if (slot.Empty == false)
                    {
                        OnRightClickSharedItem(
                            userId, player, playerInventory,
                            i, slot.Stack);
                    }

                }
            }
            finally
            {
                UpdateRendering(userId, playerInventory);

                Locker.Release();
            }

        }

        internal override void QuickMove(UserId userId, PlayerInventory playerInventory, int i)
        {
            //base.QuickMove(userId, playerInventory, i);

            Locker.Hold();

            try
            {
            }
            finally
            {
                UpdateRendering(userId, playerInventory);

                Locker.Release();
            }
        }

        internal override void SwapItems(UserId userId, PlayerInventory playerInventory, int i, int j)
        {
            //base.SwapItems(userId, playerInventory, i, j);

            Locker.Hold();

            try
            {
            }
            finally
            {
                UpdateRendering(userId, playerInventory);

                Locker.Release();
            }
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

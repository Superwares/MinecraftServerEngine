﻿
using MinecraftServerEngine.Entities;
using MinecraftServerEngine.Items;
using MinecraftServerEngine.Protocols;

namespace MinecraftServerEngine.Inventories
{
    public abstract class ItemInterfaceInventory : SharedInventory
    {

        private bool _disposed = false;


        public ItemInterfaceInventory(int totalLineCount) : base(totalLineCount)
        {
            if (totalLineCount < 0 || totalLineCount > MaxLines)
            {
                throw new System.ArgumentOutOfRangeException(nameof(totalLineCount));
            }

            System.Diagnostics.Debug.Assert(totalLineCount > 0);
            System.Diagnostics.Debug.Assert(totalLineCount <= MaxLines);

            ItemStack a = new ItemStack(ItemType.Stick, "Hello");

            // TODO: remove
            //Slots[10].Move(ref a);

        }

        ~ItemInterfaceInventory()
        {
            System.Diagnostics.Debug.Assert(false);

            Dispose(false);
        }


        protected virtual void OnLeftClickSharedItem(
            World world,
            UserId userId, 
            AbstractPlayer player, PlayerInventory playerInventory,
            int i, ItemStack itemStack)
        { }

        protected virtual void OnRightClickSharedItem(
            World world,
            UserId userId, 
            AbstractPlayer player, PlayerInventory playerInventory,
            int i, ItemStack itemStack)
        { }

        internal override void LeftClick(
            World world,
            UserId userId,
            AbstractPlayer player,
            int i, InventorySlot cursor)
        {
            System.Diagnostics.Debug.Assert(_disposed == false);

            int totalSlots = GetTotalSlotCount();
            System.Diagnostics.Debug.Assert(totalSlots > 0);

            System.Diagnostics.Debug.Assert(i >= 0);
            System.Diagnostics.Debug.Assert(i < totalSlots + PlayerInventory.PrimarySlotCount);
            System.Diagnostics.Debug.Assert(cursor != null);

            //base.LeftClick(userId, playerInventory, i, cursor);

            HoldLocker();

            try
            {

                if (i < totalSlots)
                {
                    System.Diagnostics.Debug.Assert(player.Inventory != null);
                    InventorySlot slot = GetSlot(player.Inventory, i);

                    System.Diagnostics.Debug.Assert(slot != null);

                    if (slot.Empty == false)
                    {
                        System.Diagnostics.Debug.Assert(player.Inventory != null);
                        OnLeftClickSharedItem(
                            world,
                            userId, 
                            player, player.Inventory,
                            i, slot.Stack);
                    }

                }
            }
            finally
            {
                ReleaseLocker(userId);
            }

        }

        internal override void RightClick(
            World world,
            UserId userId,
            AbstractPlayer player,
            int i, InventorySlot cursor)
        {
            System.Diagnostics.Debug.Assert(_disposed == false);

            int totalSlots = GetTotalSlotCount();
            System.Diagnostics.Debug.Assert(totalSlots > 0);

            System.Diagnostics.Debug.Assert(i >= 0);
            System.Diagnostics.Debug.Assert(i < totalSlots + PlayerInventory.PrimarySlotCount);
            System.Diagnostics.Debug.Assert(cursor != null);

            //base.RightClick(userId, playerInventory, i, cursor);

            HoldLocker();

            try
            {

                if (i < totalSlots)
                {
                    System.Diagnostics.Debug.Assert(player.Inventory != null);
                    InventorySlot slot = GetSlot(player.Inventory, i);

                    System.Diagnostics.Debug.Assert(slot != null);

                    if (slot.Empty == false)
                    {
                        System.Diagnostics.Debug.Assert(player.Inventory != null);
                        OnRightClickSharedItem(
                            world,
                            userId, 
                            player, player.Inventory,
                            i, slot.Stack);
                    }

                }
            }
            finally
            {
                ReleaseLocker(userId);
            }

        }

        internal override void QuickMove(UserId userId, PlayerInventory playerInventory, int i)
        {
        }

        internal override void SwapItems(UserId userId, PlayerInventory playerInventory, int i, int j)
        {
        }

        internal override ItemStack DropSingle(UserId userId, PlayerInventory playerInventory, int i)
        {
            return null;
        }

        internal override ItemStack DropFull(UserId userId, PlayerInventory playerInventory, int i)
        {
            return null;
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

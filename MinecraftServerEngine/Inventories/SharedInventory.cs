

using Containers;
using Sync;

using MinecraftServerEngine.Items;
using MinecraftServerEngine.Renderers;
using MinecraftServerEngine.Entities;
using MinecraftServerEngine.Protocols;

namespace MinecraftServerEngine.Inventories
{

    public abstract class SharedInventory : Inventory
    {
        public const int MaxLines = 6;


        public abstract string Title { get; }

        private bool _disposed = false;

        internal int _lockDepth = 0;
        internal readonly Locker Locker = new();  // Disposable

        private readonly Map<PlayerInventory, PublicInventoryRenderer> Renderers = new();

        internal SharedInventory(int totalLineCount) : base(totalLineCount * SlotCountPerLine)
        {
            System.Diagnostics.Debug.Assert(totalLineCount > 0);
            System.Diagnostics.Debug.Assert(totalLineCount <= MaxLines);
        }

        ~SharedInventory()
        {
            System.Diagnostics.Debug.Assert(false);

            Dispose(false);
        }

        internal void UpdateRendering(UserId userId)
        {
            System.Diagnostics.Debug.Assert(_disposed == false);

            //Locker.Hold();

            System.Diagnostics.Debug.Assert(_lockDepth == 0);

            try
            {
                using MinecraftProtocolDataStream buffer = new();

                System.Diagnostics.Debug.Assert(Slots != null);
                foreach (InventorySlot slot in Slots)
                {
                    System.Diagnostics.Debug.Assert(slot != null);
                    slot.WriteData(buffer);
                }

                int totalSlots = GetTotalSlotCount();
                byte[] data = buffer.ReadData();

                System.Diagnostics.Debug.Assert(totalSlots > 0);
                System.Diagnostics.Debug.Assert(Renderers != null);
                foreach (PublicInventoryRenderer renderer in Renderers.GetValues())
                {
                    if (renderer.UserId == userId)
                    {
                        continue;
                    }

                    renderer.Update(totalSlots, data);
                }
            }
            finally
            {
                //Locker.Release();
            }


        }

        internal void Open(
            UserId userId,
            PlayerInventory playerInventory,
            ConcurrentQueue<ClientboundPlayingPacket> outPackets)
        {
            System.Diagnostics.Debug.Assert(_disposed == false);

            System.Diagnostics.Debug.Assert(playerInventory != null);
            System.Diagnostics.Debug.Assert(outPackets != null);

            Locker.Hold();

            try
            {
                PublicInventoryRenderer renderer = new(userId, outPackets);

                System.Diagnostics.Debug.Assert(!Renderers.Contains(playerInventory));
                Renderers.Insert(playerInventory, renderer);
            }
            finally
            {
                Locker.Release();
            }


        }

        internal void Close(PlayerInventory inv)
        {
            System.Diagnostics.Debug.Assert(_disposed == false);

            System.Diagnostics.Debug.Assert(inv != null);

            Locker.Hold();

            try
            {
                System.Diagnostics.Debug.Assert(Renderers.Contains(inv) == true);
                Renderers.Extract(inv);
            }
            finally
            {
                Locker.Release();
            }
        }

        private protected void HoldLocker()
        {
            System.Diagnostics.Debug.Assert(_lockDepth >= 0);

            Locker.Hold();
            ++_lockDepth;
        }

        private protected void ReleaseLocker(UserId userId)
        {
            System.Diagnostics.Debug.Assert(_lockDepth >= 0);

            if (--_lockDepth == 0)
            {
                UpdateRendering(userId);
            }

            System.Diagnostics.Debug.Assert(Locker != null);
            Locker.Release();
        }

        public void SetSlots((bool, ItemStack)[] slots)
        {
            if (slots == null)
            {
                throw new System.ArgumentNullException(nameof(slots));
            }

            if (slots.Length != GetTotalSlotCount())
            {
                throw new System.ArgumentException(
                    "The length of itemStacks does not match the total slot count.",
                    nameof(slots));
            }

            HoldLocker();

            try
            {
                for (int i = 0; i < GetTotalSlotCount(); ++i)
                {
                    (bool applied, ItemStack itemStack) = slots[i];

                    if (applied == true)
                    {
                        Slots[i].Reset(itemStack);
                    }

                }
            }
            finally
            {
                ReleaseLocker(UserId.Null);
            }
        }

        public void SetSlot(int index, ItemStack itemStack)
        {
            if (index < 0 || index >= GetTotalSlotCount())
            {
                throw new System.ArgumentOutOfRangeException(nameof(index));
            }

            HoldLocker();

            try
            {
                Slots[index].Reset(itemStack);
            }
            finally
            {
                ReleaseLocker(UserId.Null);
            }

        }

        internal InventorySlot GetSlot(
            PlayerInventory invPlayer,
            int i)
        {
            System.Diagnostics.Debug.Assert(i >= 0);

            int totalSlots = GetTotalSlotCount();

            System.Diagnostics.Debug.Assert(totalSlots > 0);
            System.Diagnostics.Debug.Assert(i < totalSlots + PlayerInventory.PrimarySlotCount);

            if (i < totalSlots)
            {
                return Slots[i];
            }
            else
            {
                int j = i - totalSlots;
                System.Diagnostics.Debug.Assert(j >= 0);
                return invPlayer.GetPrimarySlot(j);
            }

        }

        internal void SetSlot(
            PlayerInventory invPlayer,
            int i, InventorySlot slot)
        {
            System.Diagnostics.Debug.Assert(i >= 0);

            int totalSlots = GetTotalSlotCount();

            System.Diagnostics.Debug.Assert(totalSlots > 0);
            System.Diagnostics.Debug.Assert(i < totalSlots + PlayerInventory.PrimarySlotCount);

            if (i < totalSlots)
            {
                Slots[i] = slot;
            }
            else
            {
                int j = i - totalSlots;
                System.Diagnostics.Debug.Assert(j >= 0);
                invPlayer.SetPrimarySlot(j, slot);
            }

        }

        internal virtual void LeftClick(
            World world,
            UserId userId, 
            AbstractPlayer player, 
            int i, InventorySlot cursor)
        {
            System.Diagnostics.Debug.Assert(_disposed == false);

            System.Diagnostics.Debug.Assert(world != null);

            System.Diagnostics.Debug.Assert(userId != UserId.Null);
            System.Diagnostics.Debug.Assert(player != null);

            int totalSlots = GetTotalSlotCount();
            System.Diagnostics.Debug.Assert(totalSlots > 0);

            System.Diagnostics.Debug.Assert(i >= 0);
            System.Diagnostics.Debug.Assert(i < totalSlots + PlayerInventory.PrimarySlotCount);
            System.Diagnostics.Debug.Assert(cursor != null);

            HoldLocker();

            try
            {
                //System.Diagnostics.Debug.Assert(Renderers.Contains(player.Inventory));

                if (i < totalSlots)
                {
                    InventorySlot slot = Slots[i];
                    System.Diagnostics.Debug.Assert(slot != null);

                    slot.LeftClick(cursor);

                }
                else
                {
                    int j = i - totalSlots;

                    System.Diagnostics.Debug.Assert(player.Inventory != null);
                    player.Inventory.LeftClickInPrimary(j, cursor);
                }

            }
            finally
            {
                ReleaseLocker(userId);
            }

        }

        internal virtual void RightClick(
            World world,
            UserId userId, 
            AbstractPlayer player, 
            int i, InventorySlot cursor)
        {
            System.Diagnostics.Debug.Assert(_disposed == false);

            System.Diagnostics.Debug.Assert(world != null);

            System.Diagnostics.Debug.Assert(userId != UserId.Null);
            System.Diagnostics.Debug.Assert(player != null);

            int totalSlots = GetTotalSlotCount();
            System.Diagnostics.Debug.Assert(totalSlots > 0);

            System.Diagnostics.Debug.Assert(i >= 0);
            System.Diagnostics.Debug.Assert(i < totalSlots + PlayerInventory.PrimarySlotCount);
            System.Diagnostics.Debug.Assert(cursor != null);

            HoldLocker();

            try
            {
                //System.Diagnostics.Debug.Assert(Renderers.Contains(player.Inventory));

                if (i < totalSlots)
                {
                    InventorySlot slot = Slots[i];
                    System.Diagnostics.Debug.Assert(slot != null);

                    slot.RightClick(cursor);

                }
                else
                {
                    int j = i - totalSlots;

                    System.Diagnostics.Debug.Assert(player.Inventory != null);
                    player.Inventory.RightClickInPrimary(j, cursor);
                }
            }
            finally
            {
                ReleaseLocker(userId);
            }


        }

        private void QuickMoveFromLeft(InventorySlot slot)
        {
            System.Diagnostics.Debug.Assert(_disposed == false);

            System.Diagnostics.Debug.Assert(slot != null);

            if (slot.Empty)
            {
                return;
            }

            int totalSlots = GetTotalSlotCount();
            System.Diagnostics.Debug.Assert(totalSlots > 0);

            int j = -1;

            InventorySlot slotInside;
            for (int i = 0; i < totalSlots; ++i)
            {
                System.Diagnostics.Debug.Assert(!slot.Empty);

                slotInside = Slots[i];
                System.Diagnostics.Debug.Assert(slotInside != null);
                System.Diagnostics.Debug.Assert(!ReferenceEquals(slotInside, slot));

                if (slotInside.Empty)
                {
                    j = j < 0 ? i : j;

                    continue;
                }

                slotInside.Move(slot);

                if (slot.Empty)
                {
                    break;
                }
            }

            System.Diagnostics.Debug.Assert(j <= totalSlots);
            if (!slot.Empty && j >= 0)
            {
                slotInside = Slots[j];
                System.Diagnostics.Debug.Assert(slotInside != null);
                System.Diagnostics.Debug.Assert(slotInside.Empty);

                slotInside.Move(slot);
            }
        }

        internal virtual void QuickMove(
            UserId userId, PlayerInventory playerInventory,
            int i)
        {
            System.Diagnostics.Debug.Assert(_disposed == false);

            System.Diagnostics.Debug.Assert(userId != UserId.Null);
            System.Diagnostics.Debug.Assert(playerInventory != null);

            int totalSlots = GetTotalSlotCount();
            System.Diagnostics.Debug.Assert(totalSlots > 0);

            System.Diagnostics.Debug.Assert(i >= 0);
            System.Diagnostics.Debug.Assert(i < totalSlots + PlayerInventory.PrimarySlotCount);

            HoldLocker();

            try
            {
                if (i < totalSlots)
                {
                    InventorySlot slot = Slots[i];
                    System.Diagnostics.Debug.Assert(slot != null);

                    playerInventory.QuickMoveFromRightInPrimary(slot);
                }
                else
                {
                    int j = i - totalSlots;
                    System.Diagnostics.Debug.Assert(j >= 0);
                    InventorySlot slot = playerInventory.GetPrimarySlot(j);
                    System.Diagnostics.Debug.Assert(slot != null);

                    QuickMoveFromLeft(slot);
                }
            }
            finally
            {
                ReleaseLocker(userId);
            }


        }

        internal virtual void SwapItems(
            UserId userId, PlayerInventory playerInventory,
            int i, int j)
        {
            int totalSlots = GetTotalSlotCount();
            System.Diagnostics.Debug.Assert(totalSlots > 0);

            System.Diagnostics.Debug.Assert(i >= 0);
            System.Diagnostics.Debug.Assert(i < totalSlots + PlayerInventory.PrimarySlotCount);
            System.Diagnostics.Debug.Assert(j >= 0);
            System.Diagnostics.Debug.Assert(i < totalSlots + PlayerInventory.PrimarySlotCount);

            if (i == j)
            {
                return;
            }

            HoldLocker();

            try
            {
                InventorySlot slot_i = GetSlot(playerInventory, i), slot_j = GetSlot(playerInventory, j);

                SetSlot(playerInventory, i, slot_j);
                SetSlot(playerInventory, j, slot_i);

            }
            finally
            {
                ReleaseLocker(userId);
            }
        }

        internal void SwapItemsWithHotbarSlot(
            UserId userId, PlayerInventory playerInventory,
            int i, int j)
        {
            int totalSlots = GetTotalSlotCount();
            System.Diagnostics.Debug.Assert(totalSlots > 0);

            System.Diagnostics.Debug.Assert(i >= 0);
            System.Diagnostics.Debug.Assert(i < totalSlots + PlayerInventory.PrimarySlotCount);
            System.Diagnostics.Debug.Assert(j >= 0);
            System.Diagnostics.Debug.Assert(j < PlayerInventory.HotbarSlotCount);

            HoldLocker();

            try
            {

                int k = totalSlots + (PlayerInventory.HotbarSlotsOffset - PlayerInventory.PrimarySlotsOffset) + j;
                SwapItems(userId, playerInventory, i, k);
            }
            finally
            {
                ReleaseLocker(userId);
            }
        }


        internal virtual ItemStack DropSingle(
            UserId userId,
            PlayerInventory playerInventory, int i)
        {
            int totalSlots = GetTotalSlotCount();

            System.Diagnostics.Debug.Assert(i >= 0);
            System.Diagnostics.Debug.Assert(i < totalSlots + PlayerInventory.PrimarySlotCount);

            HoldLocker();

            try
            {

                InventorySlot slot = GetSlot(playerInventory, i);

                return slot.DropSingle();
            }
            finally
            {
                ReleaseLocker(userId);
            }
        }

        internal virtual ItemStack DropFull(
            UserId userId,
            PlayerInventory playerInventory, int i)
        {
            int totalSlots = GetTotalSlotCount();

            System.Diagnostics.Debug.Assert(i >= 0);
            System.Diagnostics.Debug.Assert(i < totalSlots + PlayerInventory.PrimarySlotCount);

            HoldLocker();

            try
            {
                InventorySlot slot = GetSlot(playerInventory, i);

                return slot.DropFull();
            }
            finally
            {
                ReleaseLocker(userId);
            }

        }

        protected override void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if (_disposed == false)
            {
                System.Diagnostics.Debug.Assert(Renderers.Empty == true);
                System.Diagnostics.Debug.Assert(_lockDepth == 0);

                // If disposing equals true, dispose all managed
                // and unmanaged resources.
                if (disposing == true)
                {
                    // Dispose managed resources.
                    Locker.Dispose();
                    Renderers.Dispose();
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

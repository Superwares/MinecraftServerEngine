

using Containers;
using Sync;
using static System.Reflection.Metadata.BlobBuilder;

namespace MinecraftServerEngine
{

    public abstract class SharedInventory : Inventory
    {
        internal const int MaxLineCount = 6;

        protected abstract string Title { get; }

        private bool _disposed = false;


        private protected readonly Locker Locker = new();  // Disposable

        private readonly Map<PlayerInventory, PublicInventoryRenderer> Renderers = new();

        internal SharedInventory(int totalLineCount) : base(totalLineCount * SlotCountPerLine)
        {
            System.Diagnostics.Debug.Assert(totalLineCount > 0);
            System.Diagnostics.Debug.Assert(totalLineCount <= MaxLineCount);
        }

        ~SharedInventory()
        {
            System.Diagnostics.Debug.Assert(false);

            Dispose(false);
        }

        internal void UpdateRendering(
            UserId userId, PlayerInventory playerInventory)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(playerInventory != null);

            Locker.Hold();

            using Buffer buffer = new();

            System.Diagnostics.Debug.Assert(Slots != null);
            foreach (InventorySlot slot in Slots)
            {
                System.Diagnostics.Debug.Assert(slot != null);
                slot.WriteData(buffer);
            }

            int sharedDataSize = buffer.Size;

            foreach (InventorySlot slot in playerInventory.GetPrimarySlots())
            {
                System.Diagnostics.Debug.Assert(slot != null);
                slot.WriteData(buffer);
            }

            byte[] data = buffer.ReadData(),
                sharedData = new byte[sharedDataSize];

            System.Array.Copy(data, 0, sharedData, 0, sharedDataSize);

            int totalSharedSlots = GetTotalSlotCount();
            int totalSlots = totalSharedSlots + PlayerInventory.PrimarySlotCount;

            System.Diagnostics.Debug.Assert(totalSharedSlots > 0);
            System.Diagnostics.Debug.Assert(Renderers != null);
            foreach (PublicInventoryRenderer renderer in Renderers.GetValues())
            {
                if (renderer.UserId == userId)
                {
                    renderer.Update(totalSlots, data);
                }
                else
                {
                    renderer.Update(totalSharedSlots, sharedData);
                }
            }

            Locker.Release();
        }

        internal void Open(
            UserId userId,
            PlayerInventory playerInventory,
            ConcurrentQueue<ClientboundPlayingPacket> outPackets)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(playerInventory != null);
            System.Diagnostics.Debug.Assert(outPackets != null);

            PublicInventoryRenderer renderer = new(userId, outPackets);

            Locker.Hold();

            using Buffer buffer = new();

            System.Diagnostics.Debug.Assert(Slots != null);
            foreach (InventorySlot slot in Slots)
            {
                System.Diagnostics.Debug.Assert(slot != null);
                slot.WriteData(buffer);
            }

            foreach (InventorySlot slot in playerInventory.GetPrimarySlots())
            {
                System.Diagnostics.Debug.Assert(slot != null);
                slot.WriteData(buffer);
            }

            byte[] data = buffer.ReadData();

            int totalSharedSlots = GetTotalSlotCount();
            int totalSlots = totalSharedSlots + PlayerInventory.PrimarySlotCount;

            renderer.Open(Title, totalSharedSlots, totalSlots, data);

            System.Diagnostics.Debug.Assert(!Renderers.Contains(playerInventory));
            Renderers.Insert(playerInventory, renderer);

            Locker.Release();
        }

        internal void Close(PlayerInventory inv)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(inv != null);

            Locker.Hold();

            System.Diagnostics.Debug.Assert(Renderers.Contains(inv) == true);
            Renderers.Extract(inv);

            Locker.Release();
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
            UserId userId, PlayerInventory playerInventory,
            int i, InventorySlot cursor)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(userId != UserId.Null);
            System.Diagnostics.Debug.Assert(playerInventory != null);

            int totalSlots = GetTotalSlotCount();
            System.Diagnostics.Debug.Assert(totalSlots > 0);

            System.Diagnostics.Debug.Assert(i >= 0);
            System.Diagnostics.Debug.Assert(i < totalSlots + PlayerInventory.PrimarySlotCount);
            System.Diagnostics.Debug.Assert(cursor != null);

            Locker.Hold();

            System.Diagnostics.Debug.Assert(Renderers.Contains(playerInventory));

            if (i < totalSlots)
            {
                InventorySlot slot = Slots[i];
                System.Diagnostics.Debug.Assert(slot != null);

                slot.LeftClick(cursor);

            }
            else
            {
                int j = i - totalSlots;
                playerInventory.LeftClickInPrimary(j, cursor);
            }

            UpdateRendering(userId, playerInventory);

            Locker.Release();
        }

        internal virtual void RightClick(
            UserId userId, PlayerInventory playerInventory,
            int i, InventorySlot cursor)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(userId != UserId.Null);
            System.Diagnostics.Debug.Assert(playerInventory != null);

            int totalSlots = GetTotalSlotCount();
            System.Diagnostics.Debug.Assert(totalSlots > 0);

            System.Diagnostics.Debug.Assert(i >= 0);
            System.Diagnostics.Debug.Assert(i < totalSlots + PlayerInventory.PrimarySlotCount);
            System.Diagnostics.Debug.Assert(cursor != null);
            System.Diagnostics.Debug.Assert(playerInventory != null);

            Locker.Hold();

            System.Diagnostics.Debug.Assert(Renderers.Contains(playerInventory));

            if (i < totalSlots)
            {
                InventorySlot slot = Slots[i];
                System.Diagnostics.Debug.Assert(slot != null);

                slot.RightClick(cursor);

            }
            else
            {
                int j = i - totalSlots;
                playerInventory.RightClickInPrimary(j, cursor);
            }

            UpdateRendering(userId, playerInventory);

            Locker.Release();
        }

        private void QuickMoveFromLeft(InventorySlot slot)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

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
                    j = (j < 0) ? i : j;

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
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(userId != UserId.Null);
            System.Diagnostics.Debug.Assert(playerInventory != null);

            int totalSlots = GetTotalSlotCount();
            System.Diagnostics.Debug.Assert(totalSlots > 0);

            System.Diagnostics.Debug.Assert(i >= 0);
            System.Diagnostics.Debug.Assert(i < totalSlots + PlayerInventory.PrimarySlotCount);

            Locker.Hold();

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

            UpdateRendering(userId, playerInventory);

            Locker.Release();
        }

        internal void SwapItems(
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

            Locker.Hold();

            InventorySlot slot_i = GetSlot(playerInventory, i), slot_j = GetSlot(playerInventory, j);

            SetSlot(playerInventory, i, slot_j);
            SetSlot(playerInventory, j, slot_i);

            UpdateRendering(userId, playerInventory);

            Locker.Release();
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

            int k = totalSlots + (PlayerInventory.HotbarSlotsOffset - PlayerInventory.PrimarySlotsOffset) + j;
            SwapItems(userId, playerInventory, i, k);
        }

        protected override void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if (_disposed == false)
            {
                System.Diagnostics.Debug.Assert(Renderers.Empty == true);

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



using Containers;
using Sync;

namespace MinecraftServerEngine
{

    public abstract class PublicInventory : Inventory
    {
        private bool _disposed = false;

        protected abstract string Title { get; }

        private protected readonly Locker Locker = new();  // Disposable

        private readonly Map<PlayerInventory, PublicInventoryRenderer> Renderers = new();

        internal PublicInventory(int totalSlotCount) : base(totalSlotCount) { }

        ~PublicInventory()
        {
            System.Diagnostics.Debug.Assert(false);

            Dispose(false);
        }

        private protected void Update()
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            Locker.Hold();

            using Buffer buffer = new();

            System.Diagnostics.Debug.Assert(Slots != null);
            foreach (InventorySlot slot in Slots)
            {
                System.Diagnostics.Debug.Assert(slot != null);
                slot.WriteData(buffer);
            }

            byte[] data = buffer.ReadData();

            System.Diagnostics.Debug.Assert(TotalSlotCount > 0);
            System.Diagnostics.Debug.Assert(Renderers != null);
            foreach (PublicInventoryRenderer renderer in Renderers.GetValues())
            {
                renderer.Update(TotalSlotCount, data);
            }

            Locker.Release();
        }

        internal void Open(
            PlayerInventory inv,
            ConcurrentQueue<ClientboundPlayingPacket> outPackets)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(inv != null);
            System.Diagnostics.Debug.Assert(outPackets != null);

            PublicInventoryRenderer renderer = new(outPackets);

            Locker.Hold();

            renderer.Open(Title, Slots);

            System.Diagnostics.Debug.Assert(!Renderers.Contains(inv));
            Renderers.Insert(inv, renderer);

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

        private InventorySlot GetSlot(
            PlayerInventory invPlayer,
            int i)
        {
            System.Diagnostics.Debug.Assert(i >= 0);
            System.Diagnostics.Debug.Assert(i < TotalSlotCount + PlayerInventory.PrimarySlotCount);

            if (i < TotalSlotCount)
            {
                return Slots[i];
            }
            else
            {
                int j = i - TotalSlotCount;
                System.Diagnostics.Debug.Assert(j >= 0);
                return invPlayer.GetPrimarySlot(j);
            }

        }

        private void SetSlot(
            PlayerInventory invPlayer,
            int i, InventorySlot slot)
        {
            System.Diagnostics.Debug.Assert(i >= 0);
            System.Diagnostics.Debug.Assert(i < TotalSlotCount + PlayerInventory.PrimarySlotCount);

            if (i < TotalSlotCount)
            {Slots[i] = slot;
            }
            else
            {
                int j = i - TotalSlotCount;
                System.Diagnostics.Debug.Assert(j >= 0);
                invPlayer.SetPrimarySlot(j, slot);
            }

        }

        internal virtual void LeftClick(
            UserId id, PlayerInventory invPlayer,
            int i, InventorySlot cursor)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(id != UserId.Null);
            System.Diagnostics.Debug.Assert(invPlayer != null);

            System.Diagnostics.Debug.Assert(i >= 0);
            System.Diagnostics.Debug.Assert(i < TotalSlotCount + PlayerInventory.PrimarySlotCount);
            System.Diagnostics.Debug.Assert(cursor != null);

            Locker.Hold();

            System.Diagnostics.Debug.Assert(Renderers.Contains(invPlayer));

            if (i < TotalSlotCount)
            {
                InventorySlot slot = Slots[i];
                System.Diagnostics.Debug.Assert(slot != null);

                slot.LeftClick(cursor);

                Update();
            }
            else
            {
                int j = i - TotalSlotCount;
                invPlayer.LeftClickInPrimary(j, cursor);
            }

            Locker.Release();
        }

        internal virtual void RightClick(
            UserId id, PlayerInventory invPlayer,
            int i, InventorySlot cursor)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(id != UserId.Null);
            System.Diagnostics.Debug.Assert(invPlayer != null);

            System.Diagnostics.Debug.Assert(i >= 0);
            System.Diagnostics.Debug.Assert(i < TotalSlotCount + PlayerInventory.PrimarySlotCount);
            System.Diagnostics.Debug.Assert(cursor != null);
            System.Diagnostics.Debug.Assert(invPlayer != null);

            Locker.Hold();

            System.Diagnostics.Debug.Assert(Renderers.Contains(invPlayer));

            if (i < TotalSlotCount)
            {
                InventorySlot slot = Slots[i];
                System.Diagnostics.Debug.Assert(slot != null);

                slot.RightClick(cursor);

                Update();
            }
            else
            {
                int j = i - TotalSlotCount;
                invPlayer.RightClick(j, cursor);
            }

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

            int j = -1;

            InventorySlot slotInside;
            for (int i = 0; i < TotalSlotCount; ++i)
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

            System.Diagnostics.Debug.Assert(j <= TotalSlotCount);
            if (!slot.Empty && j >= 0)
            {
                slotInside = Slots[j];
                System.Diagnostics.Debug.Assert(slotInside != null);
                System.Diagnostics.Debug.Assert(slotInside.Empty);

                slotInside.Move(slot);
            }
        }

        internal virtual void QuickMove(
            UserId id, PlayerInventory invPlayer,
            int i)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(id != UserId.Null);
            System.Diagnostics.Debug.Assert(invPlayer != null);

            System.Diagnostics.Debug.Assert(i >= 0);
            System.Diagnostics.Debug.Assert(i < TotalSlotCount + PlayerInventory.PrimarySlotCount);

            Locker.Hold();

            if (i < TotalSlotCount)
            {
                InventorySlot slot = Slots[i];
                System.Diagnostics.Debug.Assert(slot != null);

                invPlayer.QuickMoveFromRightInPrimary(slot);
            }
            else
            {
                int j = i - TotalSlotCount;
                System.Diagnostics.Debug.Assert(j >= 0);
                InventorySlot slot = invPlayer.GetPrimarySlot(j);
                System.Diagnostics.Debug.Assert(slot != null);

                QuickMoveFromLeft(slot);
            }

            Update();

            Locker.Release();
        }

        internal void SwapItems(
            UserId id, PlayerInventory invPlayer,
            int i, int j)
        {
            System.Diagnostics.Debug.Assert(i >= 0);
            System.Diagnostics.Debug.Assert(i < TotalSlotCount + PlayerInventory.PrimarySlotCount);
            System.Diagnostics.Debug.Assert(j >= 0);
            System.Diagnostics.Debug.Assert(i < TotalSlotCount + PlayerInventory.PrimarySlotCount);

            if (i == j)
            {
                return;
            }

            Locker.Hold();

            InventorySlot slot_i = GetSlot(invPlayer, i), slot_j = GetSlot(invPlayer, j);


            SetSlot(invPlayer, i, slot_j);
            SetSlot(invPlayer, j, slot_i);

            Update();

            Locker.Release();
        }

        internal void SwapItemsWithHotbarSlot(
            UserId id, PlayerInventory invPlayer,
            int i, int j)
        {
            System.Diagnostics.Debug.Assert(i >= 0);
            System.Diagnostics.Debug.Assert(i < TotalSlotCount + PlayerInventory.PrimarySlotCount);
            System.Diagnostics.Debug.Assert(j >= 0);
            System.Diagnostics.Debug.Assert(j < PlayerInventory.HotbarSlotCount);

            int k = TotalSlotCount + (PlayerInventory.HotbarSlotsOffset - PlayerInventory.PrimarySlotsOffset) + j;
            SwapItems(id, invPlayer, i, k);
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

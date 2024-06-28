using Common;
using Containers;
using Sync;

namespace MinecraftServerEngine
{
    

    public abstract class Inventory : System.IDisposable
    {
        private bool _disposed = false;

        public readonly int TotalSlotCount;

        private protected int _count = 0;
        private protected readonly ItemSlot[] Slots;

        internal Inventory(int totalSlotCount)
        {
            TotalSlotCount = totalSlotCount;

            System.Diagnostics.Debug.Assert(totalSlotCount > 0);
            System.Diagnostics.Debug.Assert(_count == 0);

            Slots = new ItemSlot[TotalSlotCount];
            System.Diagnostics.Debug.Assert((ItemSlot)default == null);
            Array<ItemSlot>.Fill(Slots, default);
        }

        ~Inventory() => System.Diagnostics.Debug.Assert(false);

        public void Print()
        {
            Console.Printl($"Inventory: ");
            Console.Printl($"\tCount: {_count}");
            for (int i = 0; i < TotalSlotCount; ++i)
            {
                if (i % 9 == 0)
                {
                    Console.NewLine();
                    Console.NewTab();
                }

                ItemSlot item = Slots[i];
                if (item == null)
                {
                    Console.Print($"[None]");
                    continue;
                }

                Console.Print($"[{item.Item}x{item.Count}]");
                System.Diagnostics.Debug.Assert(item.Count >= item.MinCount);
            }
            Console.NewLine();
        }

        public virtual void Dispose()
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            // Assertion.
            System.Diagnostics.Debug.Assert(_count == 0);

            // Release resources.

            // Finish.
            System.GC.SuppressFinalize(this);
            _disposed = true;
        }

    }

    internal abstract class PrivateInventory : Inventory
    {
        private bool _disposed = false;

        internal abstract int PrimarySlotCount { get; }
        internal abstract System.Collections.Generic.IEnumerable<ItemSlot> GetPrimarySlots();
        internal abstract ItemSlot GetPrimarySlot(int i);
        internal abstract void SetPrimarySlot(int i, ItemSlot slot);
        internal abstract void EmptyPrimarySlot(int i);

        internal PrivateInventory(int totalSlotCount) : base(totalSlotCount) { }

        ~PrivateInventory() => System.Diagnostics.Debug.Assert(false);

        public void Open(InventoryRenderer renderer)
        {
            System.Diagnostics.Debug.Assert(renderer != null);

            renderer.SetSlots(Slots);
        }

        protected void Refresh(InventoryRenderer renderer)
        {

        }

        internal virtual void TakeAll(
            int index, ref ItemSlot cursor, SlotData slotData,
            InventoryRenderer renderer)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(cursor == null);

            System.Diagnostics.Debug.Assert(_count >= 0);
            System.Diagnostics.Debug.Assert(_count <= TotalSlotCount);

            System.Diagnostics.Debug.Assert(index >= 0);
            System.Diagnostics.Debug.Assert(index < TotalSlotCount);

            bool f;

            ItemSlot slotTaked = Slots[index];

            if (slotTaked != null)
            {
                f = slotTaked.CompareWithProtocolFormat(slotData);

                cursor = slotTaked;

                Slots[index] = null;

                _count--;
                System.Diagnostics.Debug.Assert(_count >= 0);
            }
            else
            {
                f = (slotData.Id == -1);

                System.Diagnostics.Debug.Assert(cursor == null);
            }

            if (!f)
            {
                Refresh(renderer);
            }
        }

        internal virtual void PutAll(
            int index, ref ItemSlot cursor, SlotData slotData,
            InventoryRenderer renderer)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(cursor != null);

            System.Diagnostics.Debug.Assert(_count >= 0);
            System.Diagnostics.Debug.Assert(_count <= TotalSlotCount);

            System.Diagnostics.Debug.Assert(index >= 0);
            System.Diagnostics.Debug.Assert(index < TotalSlotCount);

            bool f;

            ItemSlot slotTaked = Slots[index];

            if (slotTaked != null)
            {
                System.Diagnostics.Debug.Assert(!ReferenceEquals(slotTaked, cursor));

                f = slotTaked.CompareWithProtocolFormat(slotData);

                if (cursor.Item == slotTaked.Item)
                {
                    int spend = slotTaked.Stack(cursor.Count);
                    if (spend == cursor.Count)
                    {
                        cursor = null;
                    }
                    else
                    {
                        System.Diagnostics.Debug.Assert(spend < cursor.Count);
                        cursor.Spend(spend);
                    }
                }
                else
                {
                    // Swap
                    Slots[index] = cursor;
                    cursor = slotTaked;
                }
            }
            else
            {
                Slots[index] = cursor;
                cursor = null;

                ++_count;
                System.Diagnostics.Debug.Assert(_count >= 0);
                System.Diagnostics.Debug.Assert(_count <= TotalSlotCount);

                f = (slotData.Id == -1);
            }

            if (!f)
            {
                Refresh(renderer);
            }
        }

        internal virtual void TakeHalf(
            int index, ref ItemSlot cursor, SlotData slotData,
            InventoryRenderer renderer)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(cursor == null);

            System.Diagnostics.Debug.Assert(_count >= 0);
            System.Diagnostics.Debug.Assert(_count <= TotalSlotCount);

            System.Diagnostics.Debug.Assert(index >= 0);
            System.Diagnostics.Debug.Assert(index < TotalSlotCount);

            bool f;

            ItemSlot slotTaked = Slots[index];

            if (slotTaked == null)
            {
                System.Diagnostics.Debug.Assert(cursor == null);

                f = (slotData.Id == -1);
            }
            else
            {
                f = slotTaked.CompareWithProtocolFormat(slotData);

                if (slotTaked.Count == 1)
                {
                    Slots[index] = null;

                    --_count;
                    System.Diagnostics.Debug.Assert(_count >= 0);
                    System.Diagnostics.Debug.Assert(_count <= TotalSlotCount);

                    cursor = slotTaked;
                    System.Diagnostics.Debug.Assert(cursor.Count == 1);
                }
                else
                {
                    System.Diagnostics.Debug.Assert(slotTaked.Count > 1);

                    cursor = slotTaked.DivideHalf();
                    System.Diagnostics.Debug.Assert(slotTaked.Count > 0);
                    System.Diagnostics.Debug.Assert(cursor != null);
                }
            }

            if (!f)
            {
                Refresh(renderer);
            }
        }

        internal virtual void PutOne(
            int index, ref ItemSlot cursor, SlotData slotData,
            InventoryRenderer renderer)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(cursor != null);

            System.Diagnostics.Debug.Assert(_count >= 0);
            System.Diagnostics.Debug.Assert(_count <= TotalSlotCount);

            System.Diagnostics.Debug.Assert(index >= 0);
            System.Diagnostics.Debug.Assert(index < TotalSlotCount);

            bool f;

            ItemSlot slotTaked = Slots[index];

            if (slotTaked != null)
            {
                System.Diagnostics.Debug.Assert(!ReferenceEquals(slotTaked, cursor));

                f = slotTaked.CompareWithProtocolFormat(slotData);

                if (cursor.Item == slotTaked.Item)
                {
                    if (cursor.Count == 1)
                    {
                        int spend = slotTaked.Stack(1);
                        if (spend == 1)
                        {
                            cursor = null;
                        }
                        else
                        {
                            System.Diagnostics.Debug.Assert(cursor.Count == 1);
                            System.Diagnostics.Debug.Assert(slotTaked.Count == slotTaked.MaxCount);
                        }
                    }
                    else
                    {
                        System.Diagnostics.Debug.Assert(cursor.Count > 1);

                        int spend = slotTaked.Stack(1);
                        if (spend == 1)
                        {
                            cursor.Spend(1);
                        }
                    }

                }
                else
                {
                    // Swap

                    Slots[index] = cursor;
                    cursor = slotTaked;
                }

            }
            else
            {
                if (cursor.Count > 1)
                {
                    Slots[index] = cursor.DivideOne();
                    System.Diagnostics.Debug.Assert(cursor.Count >= cursor.MinCount);
                }
                else
                {
                    System.Diagnostics.Debug.Assert(cursor.Count == 1);
                    System.Diagnostics.Debug.Assert(slotTaked == null);

                    Slots[index] = cursor;
                    cursor = null;
                }

                ++_count;
                System.Diagnostics.Debug.Assert(_count >= 0);
                System.Diagnostics.Debug.Assert(_count <= TotalSlotCount);

                f = (slotData.Id == -1);
            }

            if (!f)
            {
                Refresh(renderer);
            }
        }

        public override void Dispose()
        {
            // Assertions.
            System.Diagnostics.Debug.Assert(!_disposed);

            // Release resources.

            // Finish.
            base.Dispose();
            _disposed = true;
        }

    }

    internal sealed class PlayerInventory : PrivateInventory
    {
        private bool _disposed = false;

        private const int PrimarySlotsOffset = 9;
        internal override int PrimarySlotCount 
        {
            get
            {
                return 36;
            }
        }

        internal override System.Collections.Generic.IEnumerable<ItemSlot> GetPrimarySlots()
        {
            System.Diagnostics.Debug.Assert(!_disposed);
            for (int i = 0; i < PrimarySlotCount; ++i)
            {
                yield return Slots[i + PrimarySlotsOffset];
            }
        }

        internal override ItemSlot GetPrimarySlot(int i)
        {
            System.Diagnostics.Debug.Assert(i >= 0);
            System.Diagnostics.Debug.Assert(i < PrimarySlotCount);

            return Slots[i + PrimarySlotsOffset];
        }

        internal override void SetPrimarySlot(int i, ItemSlot slot)
        {
            System.Diagnostics.Debug.Assert(i >= 0);
            System.Diagnostics.Debug.Assert(i < PrimarySlotCount);

            System.Diagnostics.Debug.Assert(slot != null);

            Slots[i + PrimarySlotsOffset] = slot;
        }

        internal override void EmptyPrimarySlot(int i)
        {
            System.Diagnostics.Debug.Assert(i >= 0);
            System.Diagnostics.Debug.Assert(i < PrimarySlotCount);

            Slots[i + PrimarySlotsOffset] = null;
        }

        /*public System.Collections.Generic.IEnumerable<ItemSlot> NotPrimarySlots
        {
            get
            {
                System.Diagnostics.Debug.Assert(!_disposed);
                for (int i = 0; i < 9; ++i)
                {
                    yield return _slots[i];
                }
            }
        }

        internal Item? OffhandItem
        {
            get
            {
                System.Diagnostics.Debug.Assert(!_disposed);
                return _items[45];
            }
        }*/

        internal PlayerInventory() : base(46) { }

        ~PlayerInventory() => System.Diagnostics.Debug.Assert(false);

        internal override void PutAll(
            int index, ref ItemSlot cursor, SlotData slotData,
            InventoryRenderer renderer)
        {
            if (index == 0)
            {
                bool f;

                ItemSlot slotTaked = Slots[index];
                if (slotTaked != null)
                {
                    f = slotTaked.CompareWithProtocolFormat(slotData);
                }
                else
                {
                    f = (slotData.Id == -1);
                }

                if (!f)
                {
                    Refresh(renderer);
                }
            }
            else if (index > 0 && index <= 4)
            {
                throw new System.NotImplementedException();
            }
            else if (index > 4 && index <= 8)
            {
                throw new System.NotImplementedException();
            }
            else
            {
                System.Diagnostics.Debug.Assert(index > 8 && index < TotalSlotCount);
                base.PutAll(index, ref cursor, slotData, renderer);
            }
        }

        internal override void PutOne(
            int index, ref ItemSlot cursor, SlotData slotData, 
            InventoryRenderer renderer)
        {
            if (index == 0)
            {
                bool f;

                ItemSlot slotTaked = Slots[index];
                if (slotTaked != null)
                {
                    f = slotTaked.CompareWithProtocolFormat(slotData);
                }
                else
                {
                    f = (slotData.Id == -1);
                }

                if (!f)
                {
                    Refresh(renderer);
                }
            }
            else if (index > 0 && index <= 4)
            {
                throw new System.NotImplementedException();
            }
            else if (index > 4 && index <= 8)
            {
                throw new System.NotImplementedException();
            }
            else
            {
                System.Diagnostics.Debug.Assert(index > 8 && index < TotalSlotCount);
                base.PutOne(index, ref cursor, slotData, renderer);
            }
            
        }

        public override void Dispose()
        {
            // Assertions.
            System.Diagnostics.Debug.Assert(!_disposed);

            // Release resources.

            // Finish.
            base.Dispose();
            _disposed = true;
        }

    }

    public abstract class PublicInventory : Inventory
    {
        internal const int SlotCountPerLine = 9;

        private bool _disposed = false;

        internal PublicInventory(int totalSlotCount) : base(totalSlotCount) { }

        ~PublicInventory() => System.Diagnostics.Debug.Assert(false);

        private protected void Refresh(
            PrivateInventory invPrivate, InventoryRenderer renderer)
        {
            System.Diagnostics.Debug.Assert(invPrivate != null);
            System.Diagnostics.Debug.Assert(renderer != null);

            int n = TotalSlotCount + invPrivate.PrimarySlotCount;
            int i = 0;
            var slots = new ItemSlot[n];

            foreach (ItemSlot slot in Slots)
            {
                slots[i++] = slot;
            }

            foreach (ItemSlot slot in invPrivate.GetPrimarySlots())
            {
                slots[i++] = slot;
            }

            System.Diagnostics.Debug.Assert(i == n);

            renderer.SetSlots(slots);
        }

        internal virtual void Open(
            PrivateInventory invPrivate, InventoryRenderer renderer)
        {
            System.Diagnostics.Debug.Assert(invPrivate != null);
            System.Diagnostics.Debug.Assert(renderer != null);

            renderer.OpenWindow(TotalSlotCount);
            Refresh(invPrivate, renderer);
        }

        internal abstract void Close(InventoryRenderer renderer);
        
        private protected ItemSlot GetSlot(PrivateInventory invPrivate, int i)
        {
            System.Diagnostics.Debug.Assert(invPrivate != null);

            System.Diagnostics.Debug.Assert(i >= 0);
            System.Diagnostics.Debug.Assert(i < TotalSlotCount + invPrivate.PrimarySlotCount);

            ItemSlot slot;

            if (i < TotalSlotCount)
            {
                slot = Slots[i];
            }
            else
            {
                int j = i - TotalSlotCount;
                slot = invPrivate.GetPrimarySlot(j);
            }

            return slot;
        }

        private protected void SetSlot(
            PrivateInventory invPrivate, int i, ItemSlot slot)
        {
            System.Diagnostics.Debug.Assert(invPrivate != null);

            System.Diagnostics.Debug.Assert(i >= 0);
            System.Diagnostics.Debug.Assert(i < TotalSlotCount + invPrivate.PrimarySlotCount);

            System.Diagnostics.Debug.Assert(slot != null);

            if (i < TotalSlotCount)
            {
                Slots[i] = slot;
            }
            else
            {
                int j = i - TotalSlotCount;
                invPrivate.SetPrimarySlot(j, slot);
            }

        }

        private protected void EmptySlot(PrivateInventory invPrivate, int i)
        {
            System.Diagnostics.Debug.Assert(invPrivate != null);

            System.Diagnostics.Debug.Assert(i >= 0);
            System.Diagnostics.Debug.Assert(i < TotalSlotCount + invPrivate.PrimarySlotCount);

            if (i < TotalSlotCount)
            {
                Slots[i] = null;
            }
            else
            {
                int j = i - TotalSlotCount;
                invPrivate.EmptyPrimarySlot(j);
            }
        }

        internal virtual void TakeAll(
            PrivateInventory invPrivate,
            int i, ref ItemSlot cursor, SlotData slotData,
            InventoryRenderer renderer)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(cursor == null);

            System.Diagnostics.Debug.Assert(_count >= 0);
            System.Diagnostics.Debug.Assert(_count <= TotalSlotCount);

            bool f;

            ItemSlot slotTaked = GetSlot(invPrivate, i);

            if (slotTaked != null)
            {
                f = slotTaked.CompareWithProtocolFormat(slotData);

                cursor = slotTaked;

                EmptySlot(invPrivate, i);

                _count--;
                System.Diagnostics.Debug.Assert(_count >= 0);
            }
            else
            {
                System.Diagnostics.Debug.Assert(cursor == null);

                f = (slotData.Id == -1);

            }

            if (!f)
            {
                Refresh(invPrivate, renderer);
            }

        }

        internal virtual void PutAll(
            PrivateInventory invPrivate,
            int i, ref ItemSlot cursor, SlotData slotData,
            InventoryRenderer renderer)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(cursor != null);

            System.Diagnostics.Debug.Assert(_count >= 0);
            System.Diagnostics.Debug.Assert(_count <= TotalSlotCount);

            bool f;

            ItemSlot slotTaked = GetSlot(invPrivate, i);

            if (slotTaked != null)
            {
                System.Diagnostics.Debug.Assert(!ReferenceEquals(slotTaked, cursor));

                f = slotTaked.CompareWithProtocolFormat(slotData);

                if (cursor.Item == slotTaked.Item)
                {
                    int spend = slotTaked.Stack(cursor.Count);
                    System.Diagnostics.Debug.Assert(spend <= cursor.Count);
                    if (spend == cursor.Count)
                    {
                        cursor = null;
                    }
                    else
                    {
                        System.Diagnostics.Debug.Assert(spend < cursor.Count);
                        cursor.Spend(spend);
                    }
                }
                else
                {
                    // Swap
                    SetSlot(invPrivate, i, cursor);
                    cursor = slotTaked;
                }
            }
            else
            {
                SetSlot(invPrivate, i, cursor);
                cursor = null;

                ++_count;
                System.Diagnostics.Debug.Assert(_count >= 0);
                System.Diagnostics.Debug.Assert(_count <= TotalSlotCount);

                f = (slotData.Id == -1);
            }

            if (!f)
            {
                Refresh(invPrivate, renderer);
            }
        }

        internal virtual void TakeHalf(
            PrivateInventory invPrivate,
            int i, ref ItemSlot cursor, SlotData slotData,
            InventoryRenderer renderer)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(cursor == null);

            System.Diagnostics.Debug.Assert(_count >= 0);
            System.Diagnostics.Debug.Assert(_count <= TotalSlotCount);

            bool f;

            ItemSlot slotTaked = GetSlot(invPrivate, i);

            if (slotTaked == null)
            {
                System.Diagnostics.Debug.Assert(cursor == null);

                f = (slotData.Id == -1);
            }
            else
            {
                f = slotTaked.CompareWithProtocolFormat(slotData);

                if (slotTaked.Count == 1)
                {
                    EmptySlot(invPrivate, i);

                    --_count;
                    System.Diagnostics.Debug.Assert(_count >= 0);
                    System.Diagnostics.Debug.Assert(_count <= TotalSlotCount);

                    cursor = slotTaked;
                    System.Diagnostics.Debug.Assert(cursor.Count == 1);
                }
                else
                {
                    System.Diagnostics.Debug.Assert(slotTaked.Count > 1);

                    cursor = slotTaked.DivideHalf();
                    System.Diagnostics.Debug.Assert(slotTaked.Count > 0);
                    System.Diagnostics.Debug.Assert(cursor != null);
                }
            }

            if (!f)
            {
                Refresh(invPrivate, renderer);
            }
        }

        internal virtual void PutOne(
            PrivateInventory invPrivate,
            int i, ref ItemSlot cursor, SlotData slotData,
            InventoryRenderer renderer)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(cursor != null);

            System.Diagnostics.Debug.Assert(_count >= 0);
            System.Diagnostics.Debug.Assert(_count <= TotalSlotCount);

            bool f;

            ItemSlot slotTaked = GetSlot(invPrivate, i);

            if (slotTaked != null)
            {
                System.Diagnostics.Debug.Assert(!ReferenceEquals(slotTaked, cursor));

                f = slotTaked.CompareWithProtocolFormat(slotData);

                if (cursor.Item == slotTaked.Item)
                {
                    int spend = slotTaked.Stack(1);
                    System.Diagnostics.Debug.Assert(spend <= 1);
                    if (spend == cursor.Count)
                    {
                        cursor = null;
                    }
                    else
                    {
                        System.Diagnostics.Debug.Assert(spend < cursor.Count);
                        cursor.Spend(spend);
                    }

                }
                else
                {
                    // Swap
                    SetSlot(invPrivate, i, cursor);
                    cursor = slotTaked;
                }

            }
            else
            {
                if (cursor.Count > 1)
                {
                    SetSlot(invPrivate, i, cursor.DivideOne());
                    System.Diagnostics.Debug.Assert(cursor.Count >= cursor.MinCount);
                }
                else
                {
                    System.Diagnostics.Debug.Assert(cursor.Count == 1);
                    System.Diagnostics.Debug.Assert(slotTaked == null);

                    SetSlot(invPrivate, i, cursor);
                    cursor = null;
                }

                ++_count;
                System.Diagnostics.Debug.Assert(_count >= 0);
                System.Diagnostics.Debug.Assert(_count <= TotalSlotCount);

                f = (slotData.Id == -1);
            }

            if (!f)
            {
                Refresh(invPrivate, renderer);
            }
        }

        public override void Dispose()
        {
            // Assertions.
            System.Diagnostics.Debug.Assert(!_disposed);

            // Release resources.

            // Finish.
            base.Dispose();
            _disposed = true;
        }

    }

    public sealed class Chest : PublicInventory
    {
        private bool _disposed = false;

        public Chest(int countLine) : base(3 * SlotCountPerLine) { }

        ~Chest() => System.Diagnostics.Debug.Assert(false);

        internal override void Open(
            PrivateInventory invPrivate, InventoryRenderer renderer)
        {
            System.Diagnostics.Debug.Assert(invPrivate != null);
            System.Diagnostics.Debug.Assert(renderer != null);

            base.Open(invPrivate, renderer);

            throw new System.NotImplementedException();
        }

        internal override void Close(InventoryRenderer renderer)
        {
            throw new System.NotImplementedException();
        }

        public override void Dispose()
        {
            // Assertions.
            System.Diagnostics.Debug.Assert(!_disposed);

            // Release resources.

            // Finish.
            base.Dispose();
            _disposed = true;
        }
    }

    public abstract class ItemInterfaceInventory : PublicInventory
    {
        private const int MaxLineCount = 6;

        private bool _disposed = false;

        private Tree<InventoryRenderer> Renderers = new();

        public ItemInterfaceInventory(int countLine) : 
            base(countLine * SlotCountPerLine) 
        {
            System.Diagnostics.Debug.Assert(countLine > 0);
            System.Diagnostics.Debug.Assert(countLine <= MaxLineCount);
        }

        ~ItemInterfaceInventory() => System.Diagnostics.Debug.Assert(false);

        internal override void Open(
            PrivateInventory invPrivate, InventoryRenderer renderer)
        {
            System.Diagnostics.Debug.Assert(invPrivate != null);
            System.Diagnostics.Debug.Assert(renderer != null);

            base.Open(invPrivate, renderer);

            throw new System.NotImplementedException();
        }

        internal override void Close(InventoryRenderer renderer)
        {
            throw new System.NotImplementedException();
        }

        protected void Update(int i, Items item, int count)
        {
            throw new System.NotImplementedException();
        }

        public override void Dispose()
        {
            // Assertions.
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(Renderers.Empty);

            // Release resources.
            Renderers.Dispose();

            // Finish.
            base.Dispose();
            _disposed = true;
        }
    }

}

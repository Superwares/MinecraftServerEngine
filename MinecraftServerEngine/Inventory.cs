using Common;
using Containers;
using Sync;
using static System.Reflection.Metadata.BlobBuilder;

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

        protected void Refresh(WindowRenderer renderer)
        {
            System.Diagnostics.Debug.Assert(renderer != null);

            renderer.SetSlots(Slots);
        }

        public void Open(WindowRenderer renderer)
        {
            System.Diagnostics.Debug.Assert(renderer != null);

            renderer.SetSlots(Slots);
        }

        internal bool GiveToMain(ItemSlot slot)
        {

        }

        // Search from right bottom to left top.
        internal bool GiveToHotbar(ItemSlot slot)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(i >= 0);
            System.Diagnostics.Debug.Assert(i < PrimarySlotCount);

            System.Diagnostics.Debug.Assert(count >= 0);

            for (int i = PrimarySlotCount)
        }

        internal virtual void TakeAll(
            int i, ref ItemSlot cursor, WindowRenderer renderer)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(i >= 0);
            System.Diagnostics.Debug.Assert(i < TotalSlotCount);
            System.Diagnostics.Debug.Assert(cursor == null);
            System.Diagnostics.Debug.Assert(renderer != null);

            System.Diagnostics.Debug.Assert(_count >= 0);
            System.Diagnostics.Debug.Assert(_count <= TotalSlotCount);

            ItemSlot slotTaked = Slots[i];
            System.Diagnostics.Debug.Assert(!ReferenceEquals(slotTaked, cursor));

            if (slotTaked != null)
            {
                cursor = slotTaked;

                Slots[i] = null;

                _count--;
                System.Diagnostics.Debug.Assert(_count >= 0);
            }
            else
            {
                System.Diagnostics.Debug.Assert(cursor == null);
            }

            Refresh(renderer);
        }

        internal virtual void PutAll(
            int i, ref ItemSlot cursor, WindowRenderer renderer)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(i >= 0);
            System.Diagnostics.Debug.Assert(i < TotalSlotCount);
            System.Diagnostics.Debug.Assert(cursor != null);
            System.Diagnostics.Debug.Assert(renderer != null);

            System.Diagnostics.Debug.Assert(_count >= 0);
            System.Diagnostics.Debug.Assert(_count <= TotalSlotCount);
            
            ItemSlot slotTaked = Slots[i];
            System.Diagnostics.Debug.Assert(!ReferenceEquals(slotTaked, cursor));

            if (slotTaked != null)
            {
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
                    Slots[i] = cursor;
                    cursor = slotTaked;
                }
            }
            else
            {
                Slots[i] = cursor;
                cursor = null;

                ++_count;
                System.Diagnostics.Debug.Assert(_count >= 0);
                System.Diagnostics.Debug.Assert(_count <= TotalSlotCount);

            }

            Refresh(renderer);
        }

        internal virtual void TakeHalf(
            int i, ref ItemSlot cursor, WindowRenderer renderer)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(_count >= 0);
            System.Diagnostics.Debug.Assert(_count <= TotalSlotCount);

            System.Diagnostics.Debug.Assert(i >= 0);
            System.Diagnostics.Debug.Assert(i < TotalSlotCount);
            System.Diagnostics.Debug.Assert(cursor == null);
            System.Diagnostics.Debug.Assert(renderer != null);

            ItemSlot slotTaked = Slots[i];
            System.Diagnostics.Debug.Assert(!ReferenceEquals(slotTaked, cursor));

            if (slotTaked == null)
            {
                System.Diagnostics.Debug.Assert(cursor == null);

            }
            else
            {
                if (slotTaked.Count == 1)
                {
                    Slots[i] = null;

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

            Refresh(renderer);
        }

        internal virtual void PutOne(
            int i, ref ItemSlot cursor, WindowRenderer renderer)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(_count >= 0);
            System.Diagnostics.Debug.Assert(_count <= TotalSlotCount);

            System.Diagnostics.Debug.Assert(i >= 0);
            System.Diagnostics.Debug.Assert(i < TotalSlotCount);
            System.Diagnostics.Debug.Assert(cursor != null);
            System.Diagnostics.Debug.Assert(renderer != null);

            ItemSlot slotTaked = Slots[i];
            System.Diagnostics.Debug.Assert(!ReferenceEquals(slotTaked, cursor));

            if (slotTaked != null)
            {
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

                    Slots[i] = cursor;
                    cursor = slotTaked;
                }

            }
            else
            {
                if (cursor.Count > 1)
                {
                    Slots[i] = cursor.DivideOne();
                    System.Diagnostics.Debug.Assert(cursor.Count >= cursor.MinCount);
                }
                else
                {
                    System.Diagnostics.Debug.Assert(cursor.Count == 1);
                    System.Diagnostics.Debug.Assert(slotTaked == null);

                    Slots[i] = cursor;
                    cursor = null;
                }

                ++_count;
                System.Diagnostics.Debug.Assert(_count >= 0);
                System.Diagnostics.Debug.Assert(_count <= TotalSlotCount);

            }

            Refresh(renderer);
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
        internal override int PrimarySlotCount => 36;

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
            int i, ref ItemSlot cursor, WindowRenderer renderer)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(i >= 0);
            System.Diagnostics.Debug.Assert(i < TotalSlotCount);
            System.Diagnostics.Debug.Assert(cursor != null);
            System.Diagnostics.Debug.Assert(renderer != null);

            if (i == 0)
            {
                Refresh(renderer);
            }
            else if (i > 0 && i <= 4)
            {
                throw new System.NotImplementedException();
            }
            else if (i > 4 && i <= 8)
            {
                throw new System.NotImplementedException();
            }
            else
            {
                System.Diagnostics.Debug.Assert(i > 8 && i < TotalSlotCount);
                base.PutAll(i, ref cursor, renderer);
            }
        }

        internal override void PutOne(
            int i, ref ItemSlot cursor, WindowRenderer renderer)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(i >= 0);
            System.Diagnostics.Debug.Assert(i < TotalSlotCount);
            System.Diagnostics.Debug.Assert(cursor != null);
            System.Diagnostics.Debug.Assert(renderer != null);

            if (i == 0)
            {
                Refresh(renderer);
            }
            else if (i > 0 && i <= 4)
            {
                throw new System.NotImplementedException();
            }
            else if (i > 4 && i <= 8)
            {
                throw new System.NotImplementedException();
            }
            else
            {
                System.Diagnostics.Debug.Assert(i > 8 && i < TotalSlotCount);
                base.PutOne(i, ref cursor, renderer);
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

        private protected readonly Locker Locker = new();  // Disposable

        private readonly Tree<WindowRenderer> Renderers = new();

        internal PublicInventory(int totalSlotCount) : base(totalSlotCount) { }

        ~PublicInventory() => System.Diagnostics.Debug.Assert(false);

        private protected void Refresh(
            PrivateInventory invPrivate, WindowRenderer renderer)
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

        internal bool Open(PrivateInventory invPrivate, WindowRenderer renderer)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(invPrivate != null);
            System.Diagnostics.Debug.Assert(renderer != null);

            bool f;

            Locker.Hold();

            if (Renderers.Contains(renderer))
            {
                f = false;
            }
            else
            {
                renderer.OpenWindow(TotalSlotCount);
                Refresh(invPrivate, renderer);

                Renderers.Insert(renderer);

                f = true;
            }

            Locker.Release();

            return f;
        }

        internal void Close(WindowRenderer renderer)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(renderer != null);

            Locker.Hold();

            System.Diagnostics.Debug.Assert(Renderers.Contains(renderer));
            Renderers.Extract(renderer);

            Locker.Release();
        }

        private protected void Broadcast()
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            Locker.Hold();

            foreach (WindowRenderer renderer in Renderers.GetKeys())
            {
                renderer.SetSlots(Slots);
            }

            Locker.Release();
        }

        private protected void Broadcast(WindowRenderer selfRenderer)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            Locker.Hold();

            foreach (WindowRenderer renderer in Renderers.GetKeys())
            {
                if (ReferenceEquals(selfRenderer, renderer))
                {
                    continue;
                }

                renderer.SetSlots(Slots);
            }

            Locker.Release();
        }

        private ItemSlot GetSlot(
            PrivateInventory invPrivate, int i, bool isPublic)
        {
            System.Diagnostics.Debug.Assert(invPrivate != null);

            System.Diagnostics.Debug.Assert(i >= 0);
            System.Diagnostics.Debug.Assert(i < TotalSlotCount + invPrivate.PrimarySlotCount);

            return isPublic ? Slots[i] : invPrivate.GetPrimarySlot(i);
        }

        private void SetSlot(
            PrivateInventory invPrivate, int i, ItemSlot slot, bool isPublic)
        {
            System.Diagnostics.Debug.Assert(invPrivate != null);

            System.Diagnostics.Debug.Assert(i >= 0);
            System.Diagnostics.Debug.Assert(i < TotalSlotCount + invPrivate.PrimarySlotCount);

            System.Diagnostics.Debug.Assert(slot != null);

            if (isPublic)
            {
                Slots[i] = slot;
            }
            else
            {
                invPrivate.SetPrimarySlot(i, slot);
            }

        }

        private void EmptySlot(
            PrivateInventory invPrivate, int i, bool isPublic)
        {
            System.Diagnostics.Debug.Assert(invPrivate != null);

            System.Diagnostics.Debug.Assert(i >= 0);
            System.Diagnostics.Debug.Assert(i < TotalSlotCount + invPrivate.PrimarySlotCount);

            if (isPublic)
            {
                Slots[i] = null;
            }
            else
            {
                invPrivate.EmptyPrimarySlot(i);
            }

        }

        internal virtual void TakeAll(
            PrivateInventory invPrivate, int i, ref ItemSlot cursor,
            WindowRenderer renderer)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(invPrivate != null);
            System.Diagnostics.Debug.Assert(i >= 0);
            System.Diagnostics.Debug.Assert(i < TotalSlotCount + invPrivate.PrimarySlotCount);
            System.Diagnostics.Debug.Assert(cursor == null);
            System.Diagnostics.Debug.Assert(renderer != null);

            System.Diagnostics.Debug.Assert(Renderers.Contains(renderer));

            System.Diagnostics.Debug.Assert(_count >= 0);
            System.Diagnostics.Debug.Assert(_count <= TotalSlotCount);

            bool isPublic = (i < TotalSlotCount);
            int j = isPublic ? i : i - TotalSlotCount;

            Locker.Hold();

            ItemSlot slotTaked = GetSlot(invPrivate, j, isPublic);
            System.Diagnostics.Debug.Assert(!ReferenceEquals(slotTaked, cursor));

            if (slotTaked != null)
            {
                cursor = slotTaked;

                EmptySlot(invPrivate, j, isPublic);

                --_count;
                System.Diagnostics.Debug.Assert(_count >= 0);
            }
            else
            {
                System.Diagnostics.Debug.Assert(cursor == null);

            }

            Refresh(invPrivate, renderer);

            if (isPublic)
            {
                Broadcast(renderer);
            }

            Locker.Release();
        }

        internal virtual void PutAll(
            PrivateInventory invPrivate, int i, ref ItemSlot cursor,
            WindowRenderer renderer)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(invPrivate != null);
            System.Diagnostics.Debug.Assert(i >= 0);
            System.Diagnostics.Debug.Assert(i < TotalSlotCount + invPrivate.PrimarySlotCount);
            System.Diagnostics.Debug.Assert(cursor != null);
            System.Diagnostics.Debug.Assert(renderer != null);

            System.Diagnostics.Debug.Assert(Renderers.Contains(renderer));

            System.Diagnostics.Debug.Assert(_count >= 0);
            System.Diagnostics.Debug.Assert(_count <= TotalSlotCount);

            bool isPublic = (i < TotalSlotCount);
            int j = isPublic ? i : i - TotalSlotCount;

            Locker.Hold();

            ItemSlot slotTaked = GetSlot(invPrivate, j, isPublic);
            System.Diagnostics.Debug.Assert(!ReferenceEquals(slotTaked, cursor));

            if (slotTaked != null)
            {
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
                    SetSlot(invPrivate, j, cursor, isPublic);
                    cursor = slotTaked;
                }
            }
            else
            {
                SetSlot(invPrivate, j, cursor, isPublic);
                cursor = null;

                ++_count;
                System.Diagnostics.Debug.Assert(_count >= 0);
                System.Diagnostics.Debug.Assert(_count <= TotalSlotCount);

            }

            Refresh(invPrivate, renderer);

            if (isPublic)
            {
                Broadcast(renderer);
            }

            Locker.Release();

        }

        internal virtual void TakeHalf(
            PrivateInventory invPrivate, int i, ref ItemSlot cursor,
            WindowRenderer renderer)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(invPrivate != null);
            System.Diagnostics.Debug.Assert(i >= 0);
            System.Diagnostics.Debug.Assert(i < TotalSlotCount + invPrivate.PrimarySlotCount);
            System.Diagnostics.Debug.Assert(cursor == null);
            System.Diagnostics.Debug.Assert(renderer != null);

            System.Diagnostics.Debug.Assert(Renderers.Contains(renderer));

            System.Diagnostics.Debug.Assert(_count >= 0);
            System.Diagnostics.Debug.Assert(_count <= TotalSlotCount);

            bool isPublic = (i < TotalSlotCount);
            int j = isPublic ? i : i - TotalSlotCount;

            Locker.Hold();

            ItemSlot slotTaked = GetSlot(invPrivate, j, isPublic);
            System.Diagnostics.Debug.Assert(!ReferenceEquals(slotTaked, cursor));

            if (slotTaked == null)
            {
                System.Diagnostics.Debug.Assert(cursor == null);

            }
            else
            {
                if (slotTaked.Count == 1)
                {
                    EmptySlot(invPrivate, j, isPublic);

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

            Refresh(invPrivate, renderer);

            if (isPublic)
            {
                Broadcast(renderer);
            }

            Locker.Release();

        }

        internal virtual void PutOne(
            PrivateInventory invPrivate, int i, ref ItemSlot cursor, 
            WindowRenderer renderer)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(invPrivate != null);
            System.Diagnostics.Debug.Assert(i >= 0);
            System.Diagnostics.Debug.Assert(i < TotalSlotCount + invPrivate.PrimarySlotCount);
            System.Diagnostics.Debug.Assert(cursor != null);
            System.Diagnostics.Debug.Assert(renderer != null);

            System.Diagnostics.Debug.Assert(Renderers.Contains(renderer));

            System.Diagnostics.Debug.Assert(_count >= 0);
            System.Diagnostics.Debug.Assert(_count <= TotalSlotCount);

            bool isPublic = (i < TotalSlotCount);
            int j = isPublic ? i : i - TotalSlotCount;

            Locker.Hold();

            ItemSlot slotTaked = GetSlot(invPrivate, j, isPublic);
            System.Diagnostics.Debug.Assert(!ReferenceEquals(slotTaked, cursor));

            if (slotTaked != null)
            {
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
                    SetSlot(invPrivate, j, cursor, isPublic);
                    cursor = slotTaked;
                }

            }
            else
            {
                if (cursor.Count > 1)
                {
                    SetSlot(invPrivate, j, cursor.DivideOne(), isPublic);
                    System.Diagnostics.Debug.Assert(cursor.Count >= cursor.MinCount);
                }
                else
                {
                    System.Diagnostics.Debug.Assert(cursor.Count == 1);
                    System.Diagnostics.Debug.Assert(slotTaked == null);

                    SetSlot(invPrivate, j, cursor, isPublic);
                    cursor = null;
                }

                ++_count;
                System.Diagnostics.Debug.Assert(_count >= 0);
                System.Diagnostics.Debug.Assert(_count <= TotalSlotCount);
            }

            Refresh(invPrivate, renderer);

            if (isPublic)
            {
                Broadcast(renderer);
            }
            
            Locker.Release();
        }

        public override void Dispose()
        {
            // Assertions.
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(Renderers.Empty);

            // Release resources.
            Locker.Dispose();
            Renderers.Dispose();

            // Finish.
            base.Dispose();
            _disposed = true;
        }

    }

    public sealed class Chest : PublicInventory
    {
        private bool _disposed = false;

        public Chest(int line) : base(3 * SlotCountPerLine) { }

        ~Chest() => System.Diagnostics.Debug.Assert(false);

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
        protected abstract class ClickContext
        {
            private readonly PrivateInventory Inventory;

            internal ClickContext(PrivateInventory inv)
            {
                System.Diagnostics.Debug.Assert(inv != null);
                Inventory = inv;
            }

            public bool Give(int i, Items item, int count)
            {
                System.Diagnostics.Debug.Assert(Inventory != null);

                System.Diagnostics.Debug.Assert(i >= 0);
                System.Diagnostics.Debug.Assert(i < Inventory.PrimarySlotCount);

                System.Diagnostics.Debug.Assert(count >= 0);

                throw new System.NotImplementedException();
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


        public ItemInterfaceInventory(int line) : base(line * SlotCountPerLine)
        {
            System.Diagnostics.Debug.Assert(line > 0);
            System.Diagnostics.Debug.Assert(line <= MaxLineCount);
        }

        ~ItemInterfaceInventory() => System.Diagnostics.Debug.Assert(false);

        internal override void TakeAll(
            PrivateInventory invPrivate, int i, ref ItemSlot cursor, 
            WindowRenderer renderer)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(invPrivate != null);
            System.Diagnostics.Debug.Assert(i >= 0);
            System.Diagnostics.Debug.Assert(i < TotalSlotCount + invPrivate.PrimarySlotCount);
            System.Diagnostics.Debug.Assert(cursor == null);
            System.Diagnostics.Debug.Assert(renderer != null);

            Locker.Hold();
                
            LeftClickContext ctx = new(invPrivate);
            LeftClick(ctx, i);

            Refresh(invPrivate, renderer);

            Locker.Release();
        }

        internal override void PutAll(
            PrivateInventory invPrivate, int i, ref ItemSlot cursor,  
            WindowRenderer renderer)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(invPrivate != null);
            System.Diagnostics.Debug.Assert(i >= 0);
            System.Diagnostics.Debug.Assert(i < TotalSlotCount + invPrivate.PrimarySlotCount);
            System.Diagnostics.Debug.Assert(cursor != null);
            System.Diagnostics.Debug.Assert(renderer != null);

            Locker.Hold();

            LeftClickContext ctx = new(invPrivate);
            LeftClick(ctx, i);

            Refresh(invPrivate, renderer);

            Locker.Release();
        }

        internal override void TakeHalf(
            PrivateInventory invPrivate, int i, ref ItemSlot cursor, 
            WindowRenderer renderer)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(invPrivate != null);
            System.Diagnostics.Debug.Assert(i >= 0);
            System.Diagnostics.Debug.Assert(i < TotalSlotCount + invPrivate.PrimarySlotCount);
            System.Diagnostics.Debug.Assert(cursor == null);
            System.Diagnostics.Debug.Assert(renderer != null);

            Locker.Hold();

            RightClickContext ctx = new(invPrivate);
            RightClick(ctx, i);

            Refresh(invPrivate, renderer);

            Locker.Release();
        }

        internal override void PutOne(
            PrivateInventory invPrivate, int i, ref ItemSlot cursor, 
            WindowRenderer renderer)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(invPrivate != null);
            System.Diagnostics.Debug.Assert(i >= 0);
            System.Diagnostics.Debug.Assert(i < TotalSlotCount + invPrivate.PrimarySlotCount);
            System.Diagnostics.Debug.Assert(cursor != null);
            System.Diagnostics.Debug.Assert(renderer != null);

            Locker.Hold();
            
            RightClickContext ctx = new(invPrivate);
            RightClick(ctx, i);

            Refresh(invPrivate, renderer);

            Locker.Release();
        }

        protected abstract void LeftClick(LeftClickContext ctx, int i);

        protected abstract void RightClick(RightClickContext ctx, int i);

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

}

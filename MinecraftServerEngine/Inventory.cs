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

        internal abstract int MainSlotCount { get; }
        internal abstract System.Collections.Generic.IEnumerable<ItemSlot> GetMainSlots();
        internal abstract ItemSlot GetMainSlot(int i);
        internal abstract void SetMainSlot(int i, ItemSlot slot);
        internal abstract void EmptyMainSlot(int i);


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
                    System.Diagnostics.Debug.Assert(spend <= cursor.Count);
                    if (spend == cursor.Count)
                    {
                        cursor = null;
                    }
                    else
                    {
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

        // TODO: Refactoring
        internal bool GiveFromLeftInMain(Items item, int count)
        {
            System.Diagnostics.Debug.Assert(count >= 0);
            if (count == 0)
            {
                return true;
            }

            /*const int countMin = 1;*/
            int countMax = item.GetMaxCount();

            int empty = TotalSlotCount - _count;
            int countPre = count;

            int k = 0;
            int i;
            for (i = 0; i < MainSlotCount; ++i)
            {
                System.Diagnostics.Debug.Assert(countPre > 0);

                if (k == _count)
                {
                    break;
                }

                ItemSlot slotInMain = GetMainSlot(i);

                if (slotInMain == null)
                {
                    continue;
                }

                ++k;

                if (slotInMain.Item != item)
                {
                    continue;
                }

                System.Diagnostics.Debug.Assert(slotInMain.RemainingCount >= 0);
                System.Diagnostics.Debug.Assert(slotInMain.RemainingCount < countMax);
                if (countPre <= slotInMain.RemainingCount)
                {
                    countPre = 0;
                    break;
                }
                else
                {
                    System.Diagnostics.Debug.Assert(countPre > slotInMain.RemainingCount);
                    countPre -= slotInMain.RemainingCount;
                }
                
                System.Diagnostics.Debug.Assert(countPre > 0);
            }

            System.Diagnostics.Debug.Assert(k >= 0);
            System.Diagnostics.Debug.Assert(k <= _count);

            System.Diagnostics.Debug.Assert(empty >= 0);
            System.Diagnostics.Debug.Assert(empty <= TotalSlotCount);
            int countMaxEmpty = countMax * empty;
            if (countPre > countMaxEmpty)
            {
                return false;
            }

            k = 0;
            for (i = 0; i < MainSlotCount; ++i)
            {
                System.Diagnostics.Debug.Assert(count > 0);

                if (k == _count)
                {
                    break;
                }

                ItemSlot slotInMain = GetMainSlot(i);
                if (slotInMain == null)
                {
                    continue;
                }

                ++k;

                if (slotInMain.Item != item)
                {
                    continue;
                }

                int spend = slotInMain.Stack(count);
                System.Diagnostics.Debug.Assert(spend <= count);
                if (spend == count)
                {
                    count = 0;
                    break;
                }
                else
                {
                    count -= spend;
                }

                System.Diagnostics.Debug.Assert(count > 0);
            }
            System.Diagnostics.Debug.Assert(k >= 0);
            System.Diagnostics.Debug.Assert(k <= _count);

            System.Diagnostics.Debug.Assert(count >= 0);
            if (count == 0)
            {
                return true;
            }

            System.Diagnostics.Debug.Assert(count <= countMaxEmpty);
            for (i = 0; i < MainSlotCount; ++i)
            {
                System.Diagnostics.Debug.Assert(count > 0);

                ItemSlot slotInMain = GetMainSlot(i);
                if (slotInMain != null)
                {
                    continue;
                }

                if (count > countMax)
                {
                    SetMainSlot(i, new ItemSlot(item, countMax));
                    count -= countMax;
                }
                else
                {
                    SetMainSlot(i, new ItemSlot(item, count));
                    /*count = 0;*/
                    break;
                }

                System.Diagnostics.Debug.Assert(count > 0);
            }

            return true;
        }

        internal void QuickMoveFromLeftInMain(ref ItemSlot slot)
        { 
            if (slot == null)
            {
                return;
            }
            System.Diagnostics.Debug.Assert(slot.Count > 0);

            int k = 0;
            int j = 0;

            for (int i = 0; i < MainSlotCount; ++i)
            {
                System.Diagnostics.Debug.Assert(slot != null);

                if (k == _count)
                {
                    break;
                }

                ItemSlot slotInMain = GetMainSlot(i);
                System.Diagnostics.Debug.Assert(!ReferenceEquals(slotInMain, slot));

                if (slotInMain == null)
                {
                    j = i;
                    continue;
                }

                ++k;

                if (slotInMain.Item != slot.Item)
                {
                    continue;
                }

                int spend = slotInMain.Stack(slot.Count);
                System.Diagnostics.Debug.Assert(spend <= slot.Count);
                if (spend == slot.Count)
                {
                    break;
                }
                else
                {
                    slot.Spend(spend);
                }
            }

            System.Diagnostics.Debug.Assert(k >= 0);
            System.Diagnostics.Debug.Assert(k <= _count);

            if (slot != null && j >= 0)
            {
                SetMainSlot(j, slot);
                slot = null;
            }
        }

        internal void QuickMoveFromLeftInHotbar(ref ItemSlot slot)
        {
            throw new System.NotImplementedException();
        }

        internal void QuickMoveFromLeftInPrimary(ref ItemSlot slot)
        {
            throw new System.NotImplementedException();
        }

        internal void QuickMoveFromRightInPrimary(ref ItemSlot slot)
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

        private const int MainSlotsOffset = 9;
        internal override int MainSlotCount => 27;
        internal override System.Collections.Generic.IEnumerable<ItemSlot> GetMainSlots()
        {
            System.Diagnostics.Debug.Assert(!_disposed);
            for (int i = 0; i < MainSlotCount; ++i)
            {
                yield return Slots[i + MainSlotsOffset];
            }
        }
        internal override ItemSlot GetMainSlot(int i)
        {
            System.Diagnostics.Debug.Assert(i >= 0);
            System.Diagnostics.Debug.Assert(i < MainSlotCount);

            return Slots[i + MainSlotsOffset];
        }
        internal override void SetMainSlot(int i, ItemSlot slot)
        {
            System.Diagnostics.Debug.Assert(i >= 0);
            System.Diagnostics.Debug.Assert(i < MainSlotCount);

            System.Diagnostics.Debug.Assert(slot != null);

            Slots[i + MainSlotsOffset] = slot;
        }
        internal override void EmptyMainSlot(int i)
        {
            System.Diagnostics.Debug.Assert(i >= 0);
            System.Diagnostics.Debug.Assert(i < MainSlotCount);

            Slots[i + MainSlotsOffset] = null;
        }

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
                base.PutAll(i, ref cursor, renderer);
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
                base.PutOne(i, ref cursor, renderer);
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

        // Search from right bottom to left top.
        internal void QuickMove(int i, WindowRenderer renderer)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(i >= 0);
            System.Diagnostics.Debug.Assert(i < TotalSlotCount);

            if (i == 0)
            {
                QuickMoveFromRightInPrimary(ref Slots[i]);
            }
            else if (i > 0 && i < 5)
            {
                QuickMoveFromLeftInPrimary(ref Slots[i]);
            }
            else if (i >= 5 && i < 9)
            {
                throw new System.NotImplementedException();
            }
            else if (i >= 9 && i < 36)
            {
                QuickMoveFromLeftInHotbar(ref Slots[i]);
            }
            else if (i >= 36 && i < 45)
            {
                QuickMoveFromLeftInMain(ref Slots[i]);
            }
            else
            {
                System.Diagnostics.Debug.Assert(i < TotalSlotCount);
                QuickMoveFromLeftInPrimary(ref Slots[i]);
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

            public bool Give(Items item, int count)
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

using Common;
using Containers;
using Sync;

namespace MinecraftServerEngine
{
    
    public abstract class Inventory : System.IDisposable
    {
        internal const int SlotsPerLine = 9;

        private bool _disposed = false;

        public readonly int TotalSlotCount;

        internal readonly InventorySlot[] Slots;

        internal Inventory(int totalSlotCount)
        {
            TotalSlotCount = totalSlotCount;

            System.Diagnostics.Debug.Assert(TotalSlotCount > 0);
            System.Diagnostics.Debug.Assert(_count == 0);

            Slots = new InventorySlot[TotalSlotCount];
            for (int i = 0; i < TotalSlotCount; ++i)
            {
                Slots[i] = new InventorySlot();
            }
        }

        ~Inventory() => System.Diagnostics.Debug.Assert(false);

        public void Print()
        {
            Console.Printl($"Inventory: ");
            Console.Printl($"\tCount: {_count}");
            for (int i = 0; i < TotalSlotCount; ++i)
            {
                if (i % SlotsPerLine == 0)
                {
                    Console.NewLine();
                    Console.NewTab();
                }

                InventorySlot slot = Slots[i];
                System.Diagnostics.Debug.Assert(slot != null);

                Console.Print($"{i}:D2:[{slot}]");
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
        internal abstract System.Collections.Generic.IEnumerable<InventorySlot> GetPrimarySlots();
        internal abstract InventorySlot GetPrimarySlot(int i);
        /*internal abstract void SetPrimarySlot(int i, ItemSlot slot);
        internal abstract void EmptyPrimarySlot(int i);*/

        /*internal abstract int MainSlotCount { get; }
        internal abstract System.Collections.Generic.IEnumerable<ItemSlot> GetMainSlots();
        internal abstract ref ItemSlot GetMainSlot(int i);*/
        /*internal abstract void SetMainSlot(int i, ItemSlot slot);
        internal abstract void EmptyMainSlot(int i);*/


        internal PrivateInventory(int totalSlotCount) : base(totalSlotCount) { }

        ~PrivateInventory() => System.Diagnostics.Debug.Assert(false);

        protected void Refresh(WindowRenderer renderer)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(renderer != null);

            renderer.SetSlots(Slots);
        }

        public void Open(WindowRenderer renderer)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(renderer != null);

            renderer.SetSlots(Slots);
        }

        /*internal virtual void TakeAll(
            int i, ref ItemSlot cursor, WindowRenderer renderer)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(i >= 0);
            System.Diagnostics.Debug.Assert(i < TotalSlotCount);
            System.Diagnostics.Debug.Assert(cursor == null);
            System.Diagnostics.Debug.Assert(renderer != null);

            System.Diagnostics.Debug.Assert(_count >= 0);
            System.Diagnostics.Debug.Assert(_count <= TotalSlotCount);

            ref ItemSlot slot = ref Slots[i];

            if (slot != null)
            {
                System.Diagnostics.Debug.Assert(!ReferenceEquals(slot, cursor));
                cursor = slot;
                slot = null;

                --_count;
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
            
            ref ItemSlot slot = ref Slots[i];
            System.Diagnostics.Debug.Assert(!ReferenceEquals(slot, cursor));

            if (slot != null)
            {
                if (cursor.Item == slot.Item)
                {
                    int spend = slot.Stack(cursor.Count);
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
                    ItemSlot temp = cursor;
                    cursor = slot;
                    slot = temp;
                }
            }
            else
            {
                slot = cursor;
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

            ref ItemSlot slot = ref Slots[i];

            if (slot == null)
            {
                System.Diagnostics.Debug.Assert(cursor == null);

            }
            else
            {
                System.Diagnostics.Debug.Assert(!ReferenceEquals(slot, cursor));

                if (slot.Count == 1)
                {
                    --_count;
                    System.Diagnostics.Debug.Assert(_count >= 0);
                    System.Diagnostics.Debug.Assert(_count <= TotalSlotCount);

                    cursor = slot;
                    slot = null;
                    System.Diagnostics.Debug.Assert(cursor.Count == 1);
                }
                else
                {
                    System.Diagnostics.Debug.Assert(slot.Count > 1);

                    cursor = slot.DivideHalf();
                    System.Diagnostics.Debug.Assert(slot.Count > 0);
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

            ref ItemSlot slot = ref Slots[i];
            System.Diagnostics.Debug.Assert(!ReferenceEquals(slot, cursor));

            if (slot != null)
            {
                if (cursor.Item == slot.Item)
                {
                    if (cursor.Count == 1)
                    {
                        int spend = slot.Stack(1);
                        if (spend == 1)
                        {
                            cursor = null;
                        }
                        else
                        {
                            System.Diagnostics.Debug.Assert(cursor.Count == 1);
                            System.Diagnostics.Debug.Assert(slot.Count == slot.MaxCount);
                        }
                    }
                    else
                    {
                        System.Diagnostics.Debug.Assert(cursor.Count > 1);

                        int spend = slot.Stack(1);
                        if (spend == 1)
                        {
                            cursor.Spend(1);
                        }
                    }

                }
                else
                {
                    // Swap
                    ItemSlot temp = cursor;
                    cursor = slot;
                    slot = temp;
                }

            }
            else
            {
                if (cursor.Count > 1)
                {
                    slot = cursor.DivideOne();
                    System.Diagnostics.Debug.Assert(cursor.Count >= cursor.MinCount);
                }
                else
                {
                    System.Diagnostics.Debug.Assert(cursor.Count == 1);
                    System.Diagnostics.Debug.Assert(slot == null);

                    slot = cursor;
                    cursor = null;
                }

                ++_count;
                System.Diagnostics.Debug.Assert(_count >= 0);
                System.Diagnostics.Debug.Assert(_count <= TotalSlotCount);

            }

            Refresh(renderer);
        }*/

        internal virtual void LeftClick(int i, InventorySlot cursor)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(i >= 0);
            System.Diagnostics.Debug.Assert(i < TotalSlotCount);
            System.Diagnostics.Debug.Assert(cursor != null);

            InventorySlot slot = Slots[i];
            System.Diagnostics.Debug.Assert(slot != null);

            slot.LeftClick(cursor);
        }

        internal virtual void RightClick(int i, InventorySlot cursor)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(i >= 0);
            System.Diagnostics.Debug.Assert(i < TotalSlotCount);
            System.Diagnostics.Debug.Assert(cursor != null);

            InventorySlot slot = Slots[i];
            System.Diagnostics.Debug.Assert(slot != null);

            slot.RightClick(cursor);
        }

        // TODO: Refactoring
        internal bool GiveFromLeftInMain(ItemType item, int count)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

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

                ref ItemSlot slotInMain = ref GetMainSlot(i);
                if (slotInMain != null)
                {
                    continue;
                }

                ++_count;
                System.Diagnostics.Debug.Assert(_count > 0);
                System.Diagnostics.Debug.Assert(_count <= TotalSlotCount);

                if (count > countMax)
                {
                    slotInMain = new ItemSlot(item, countMax);
                    count -= countMax;
                }
                else
                {
                    slotInMain = new ItemSlot(item, count);
                    /*count = 0;*/
                    break;
                }

                System.Diagnostics.Debug.Assert(count > 0);
            }

            return true;
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

        internal const int PrimarySlotsOffset = 9;
        internal override int PrimarySlotCount => 36;
        internal override System.Collections.Generic.IEnumerable<InventorySlot> GetPrimarySlots()
        {
            System.Diagnostics.Debug.Assert(!_disposed);
            for (int i = 0; i < PrimarySlotCount; ++i)
            {
                yield return Slots[i + PrimarySlotsOffset];
            }
        }
        internal override InventorySlot GetPrimarySlot(int i)
        {
            System.Diagnostics.Debug.Assert(i >= 0);
            System.Diagnostics.Debug.Assert(i < PrimarySlotCount);

            return Slots[i + PrimarySlotsOffset];
        }
        /*internal override void SetPrimarySlot(int i, ItemSlot slot)
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
        }*/

        private const int MainSlotsOffset = 9;
        internal int MainSlotCount => 27;
        internal System.Collections.Generic.IEnumerable<InventorySlot> GetMainSlots()
        {
            System.Diagnostics.Debug.Assert(!_disposed);
            for (int i = 0; i < MainSlotCount; ++i)
            {
                yield return Slots[i + MainSlotsOffset];
            }
        }
        internal InventorySlot GetMainSlot(int i)
        {
            System.Diagnostics.Debug.Assert(i >= 0);
            System.Diagnostics.Debug.Assert(i < MainSlotCount);

            System.Diagnostics.Debug.Assert(MainSlotsOffset >= 0);
            System.Diagnostics.Debug.Assert(i + MainSlotsOffset < TotalSlotCount);
            return Slots[i + MainSlotsOffset];
        }

        internal PlayerInventory() : base(46) 
        {
            GiveFromLeftInMain(ItemType.Stick, 100);
            GiveFromLeftInMain(ItemType.Snowball, 100);
        }

        ~PlayerInventory() => System.Diagnostics.Debug.Assert(false);

        internal override void LeftClick(int i, InventorySlot cursor)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(i >= 0);
            System.Diagnostics.Debug.Assert(i < TotalSlotCount);
            System.Diagnostics.Debug.Assert(cursor != null);

            if (i == 0)
            {
                InventorySlot slot = Slots[i];
                System.Diagnostics.Debug.Assert(slot != null);

                slot.LeftClick(cursor, true);

                return;
            }
            else if (i >= 5 && i < 9)
            {
                throw new System.NotImplementedException();

                return;
            }

            base.LeftClick(i, cursor);
        }

        internal override void RightClick(int i, InventorySlot cursor)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(i >= 0);
            System.Diagnostics.Debug.Assert(i < TotalSlotCount);
            System.Diagnostics.Debug.Assert(cursor != null);

            if (i == 0)
            {
                InventorySlot slot = Slots[i];
                System.Diagnostics.Debug.Assert(slot != null);

                slot.RightClick(cursor, true);

                return;
            }
            else if (i >= 5 && i < 9)
            {
                throw new System.NotImplementedException();
                
                return;
            }

            base.RightClick(i, cursor);
        }

        internal void QuickMoveFromLeftInMain(InventorySlot slot)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(slot != null);

            if (slot.Empty)
            {
                return;
            }

            int j = -1;

            InventorySlot slotMain;
            for (int i = 0; i < MainSlotCount; ++i)
            {
                System.Diagnostics.Debug.Assert(!slot.Empty);

                slotMain = GetMainSlot(i);
                System.Diagnostics.Debug.Assert(slotMain != null);
                System.Diagnostics.Debug.Assert(!ReferenceEquals(slotMain, slot));

                if (slotMain.Empty)
                {
                    j = (j < 0) ? i : j;

                    continue;
                }

                slotMain.Move(slot);

                if (slot.Empty)
                {
                    break;
                }
            }

            System.Diagnostics.Debug.Assert(j <= TotalSlotCount);
            if (!slot.Empty && j >= 0)
            {
                slotMain = GetMainSlot(j);
                System.Diagnostics.Debug.Assert(slotMain != null);
                System.Diagnostics.Debug.Assert(slotMain.Empty);

                slotMain.Move(slot);
            }

            System.Diagnostics.Debug.Assert(slot != null);
        }

        internal void QuickMoveFromLeftInHotbar(InventorySlot slot)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            throw new System.NotImplementedException();
        }

        internal void QuickMoveFromLeftInPrimary(InventorySlot slot)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            throw new System.NotImplementedException();
        }

        internal void QuickMoveFromRightInPrimary(InventorySlot slot)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            throw new System.NotImplementedException();
        }

        // Search from right bottom to left top.
        internal void QuickMove(int i)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(i >= 0);
            System.Diagnostics.Debug.Assert(i < TotalSlotCount);

            InventorySlot slot = Slots[i];
            System.Diagnostics.Debug.Assert(slot != null);

            if (i == 0)
            {
                QuickMoveFromRightInPrimary(slot);
            }
            else if (i > 0 && i < 5)
            {
                QuickMoveFromLeftInPrimary(slot);
            }
            else if (i >= 5 && i < 9)
            {
                throw new System.NotImplementedException();
            }
            else if (i >= 9 && i < 36)
            {
                QuickMoveFromLeftInHotbar(slot);
            }
            else if (i >= 36 && i < 45)
            {
                QuickMoveFromLeftInMain(slot);
            }
            else
            {
                System.Diagnostics.Debug.Assert(i < TotalSlotCount);
                QuickMoveFromLeftInPrimary(slot);
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
        

        private bool _disposed = false;

        protected abstract string Title { get; }

        private protected readonly Locker Locker = new();  // Disposable

        private readonly Map<PrivateInventory, PublicInventoryRenderer> Renderers = new();

        internal PublicInventory(int totalSlotCount) : base(totalSlotCount) { }

        ~PublicInventory() => System.Diagnostics.Debug.Assert(false);

        private protected void Refresh(
            PrivateInventory invPrivate, WindowRenderer renderer)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

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

        internal void Open(PrivateInventory invPrivate, ConcurrentQueue<ClientboundPlayingPacket> outPackets)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(invPrivate != null);
            System.Diagnostics.Debug.Assert(outPackets != null);

            PublicInventoryRenderer renderer = new(outPackets);

            Locker.Hold();

            renderer.Open(Title, Slots);

            System.Diagnostics.Debug.Assert(!Renderers.Contains(invPrivate));
            Renderers.Insert(invPrivate, renderer);

            Locker.Release();
        }

        internal void Close(PrivateInventory invPrivate)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(invPrivate != null);

            Locker.Hold();

            System.Diagnostics.Debug.Assert(Renderers.Contains(invPrivate));
            Renderers.Extract(invPrivate);

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

            System.Diagnostics.Debug.Assert(selfRenderer != null);

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

        private ref ItemSlot GetSlot(
            PrivateInventory invPrivate, int i, bool isPublic)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(invPrivate != null);

            System.Diagnostics.Debug.Assert(i >= 0);
            System.Diagnostics.Debug.Assert(i < TotalSlotCount + invPrivate.PrimarySlotCount);

            return ref isPublic ? ref Slots[i] : ref invPrivate.GetPrimarySlot(i);
        }

        /*private void SetSlot(
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

        }*/

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

            ref ItemSlot slot = ref GetSlot(invPrivate, j, isPublic);
            
            if (slot != null)
            {
                System.Diagnostics.Debug.Assert(!ReferenceEquals(slot, cursor));

                cursor = slot;
                slot = null;

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
            
            ref ItemSlot slot = ref GetSlot(invPrivate, j, isPublic);
            System.Diagnostics.Debug.Assert(!ReferenceEquals(slot, cursor));

            if (slot != null)
            {
                if (cursor.Item == slot.Item)
                {
                    int spend = slot.Stack(cursor.Count);
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
                    ItemSlot temp = cursor;
                    cursor = slot;
                    slot = temp;
                }
            }
            else
            {
                slot = cursor;
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

            ref ItemSlot slot = ref GetSlot(invPrivate, j, isPublic);

            if (slot == null)
            {
                System.Diagnostics.Debug.Assert(cursor == null);

            }
            else
            {
                System.Diagnostics.Debug.Assert(!ReferenceEquals(slot, cursor));

                if (slot.Count == 1)
                {
                    --_count;
                    System.Diagnostics.Debug.Assert(_count >= 0);
                    System.Diagnostics.Debug.Assert(_count <= TotalSlotCount);

                    cursor = slot;
                    slot = null;

                    System.Diagnostics.Debug.Assert(cursor.Count == 1);
                }
                else
                {
                    System.Diagnostics.Debug.Assert(slot.Count > 1);

                    cursor = slot.DivideHalf();
                    System.Diagnostics.Debug.Assert(slot.Count > 0);
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

            ref ItemSlot slot = ref GetSlot(invPrivate, j, isPublic);
            System.Diagnostics.Debug.Assert(!ReferenceEquals(slot, cursor));

            if (slot != null)
            {
                if (cursor.Item == slot.Item)
                {
                    int spend = slot.Stack(1);
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
                    ItemSlot temp = cursor;
                    cursor = slot;
                    slot = temp;
                }

            }
            else
            {
                if (cursor.Count > 1)
                {
                    slot = cursor.DivideOne();
                    System.Diagnostics.Debug.Assert(cursor.Count >= cursor.MinCount);
                }
                else
                {
                    System.Diagnostics.Debug.Assert(cursor.Count == 1);
                    System.Diagnostics.Debug.Assert(slot == null);

                    slot = cursor;
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

        internal virtual void LeftClick(int i, InventorySlot cursor, PrivateInventory invPrivate)
        {
            Locker.Hold();

            if (i < TotalSlotCount)
            {
                throw new System.NotImplementedException();
            }
            else
            {
                invPrivate.LeftClick(i, cursor);
            }

            Locker.Release();
        }

        private void QuickMoveFromLeft(ref ItemSlot slot)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            if (slot == null)
            {
                return;
            }
            System.Diagnostics.Debug.Assert(slot.Count > 0);

            int k = 0;
            int j = -1;

            for (int i = 0; i < TotalSlotCount; ++i)
            {
                System.Diagnostics.Debug.Assert(slot != null);

                System.Diagnostics.Debug.Assert(k <= _count);
                if (k == _count)
                {
                    break;
                }

                ItemSlot slotInMain = Slots[i];
                System.Diagnostics.Debug.Assert(!ReferenceEquals(slotInMain, slot));

                if (slotInMain == null)
                {
                    if (j == -1)
                    {
                        j = i;
                    }

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
                    slot = null;
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
                ++_count;
                System.Diagnostics.Debug.Assert(_count > 0);
                System.Diagnostics.Debug.Assert(_count <= TotalSlotCount);

                System.Diagnostics.Debug.Assert(Slots[j] == null);
                Slots[j] = slot;
                slot = null;
            }
        }

        internal virtual void QuickMove(PrivateInventory invPrivate, int i)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(invPrivate != null);
            System.Diagnostics.Debug.Assert(i >= 0);
            System.Diagnostics.Debug.Assert(i < TotalSlotCount + invPrivate.PrimarySlotCount);
            System.Diagnostics.Debug.Assert(renderer != null);

            bool isPublic = (i < TotalSlotCount);
            int j = isPublic ? i : i - TotalSlotCount;

            Locker.Hold();

            ref ItemSlot slot = ref GetSlot(invPrivate, j, isPublic);

            if (isPublic)
            {
                invPrivate.QuickMoveFromRightInPrimary(ref slot);
            }
            else
            {
                QuickMoveFromLeft(ref slot);
            }

            Refresh(invPrivate, renderer);
            Broadcast(renderer);

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

        protected override string Title => "Chest";

        public Chest(int line) : base(3 * SlotsPerLine) { }

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

            public bool Give(ItemType item, int count)
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


        public ItemInterfaceInventory(int line) : base(line * SlotsPerLine)
        {
            System.Diagnostics.Debug.Assert(line > 0);
            System.Diagnostics.Debug.Assert(line <= MaxLineCount);
        }

        ~ItemInterfaceInventory() => System.Diagnostics.Debug.Assert(false);

        /*internal override void TakeAll(
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
        }*/

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

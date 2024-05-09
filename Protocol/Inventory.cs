
using Containers;

namespace Protocol
{
    public abstract class Inventory : System.IDisposable
    {
        private bool _disposed = false;

        public readonly int TotalSlotCount;

        private int _count = 0;
        public int Count => _count;
        protected readonly Item?[] _items;
        public System.Collections.Generic.IEnumerable<Item?> Items
        {
            get
            {
                System.Diagnostics.Debug.Assert(!_disposed);
                return _items;
            }
        }

        public Inventory(int totalCount)
        {
            TotalSlotCount = totalCount;

            _items = new Item?[totalCount];
            System.Array.Fill(_items, null);
        }

        ~Inventory() => System.Diagnostics.Debug.Assert(false);

        internal virtual (bool, Item?) TakeAll(int index, SlotData slotData)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(_count >= 0);
            System.Diagnostics.Debug.Assert(_count <= TotalSlotCount);
            System.Diagnostics.Debug.Assert(index >= 0);
            System.Diagnostics.Debug.Assert(index < TotalSlotCount);

            bool f;

            Item? itemTaked = _items[index];
            _items[index] = null;

            if (itemTaked != null)
            {
                _count--;

                f = itemTaked.CompareWithPacketFormat(slotData);
            }
            else
            {
                f = (slotData.Id == -1);
            }

            return (f, itemTaked);
        }

        internal virtual (bool, Item?) PutAll(int index, Item itemCursor, SlotData slotData)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(_count >= 0);
            System.Diagnostics.Debug.Assert(_count <= TotalSlotCount);
            System.Diagnostics.Debug.Assert(index >= 0);
            System.Diagnostics.Debug.Assert(index < TotalSlotCount);

            bool f;

            Item? itemTaked = _items[index];

            if (itemTaked != null)
            {
                System.Diagnostics.Debug.Assert(!ReferenceEquals(itemTaked, itemCursor));

                f = itemTaked.CompareWithPacketFormat(slotData);

                _items[index] = itemCursor;

                if (itemCursor.TYPE == itemTaked.TYPE)
                {
                    int spend = itemCursor.Stack(itemTaked.Count);

                    if (spend == itemTaked.Count)
                    {
                        itemTaked = null;
                    }
                    else
                    {
                        System.Diagnostics.Debug.Assert(spend < itemCursor.Count);
                        itemTaked.Spend(spend);
                    }
                }
                else
                {
                    
                }
            }
            else
            {
                _items[index] = itemCursor;

                _count++;

                f = (slotData.Id == -1);
            }
            
            return (f, itemTaked);
        }

        internal virtual (bool, Item?) TakeHalf(int index, SlotData slotData)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(_count >= 0);
            System.Diagnostics.Debug.Assert(_count <= TotalSlotCount);
            System.Diagnostics.Debug.Assert(index >= 0);
            System.Diagnostics.Debug.Assert(index < TotalSlotCount);

            bool f;

            Item? itemTaked = _items[index];

            if (itemTaked == null)
            {
                f = (slotData.Id == -1);
            }
            else
            {
                f = itemTaked.CompareWithPacketFormat(slotData);

                if (itemTaked.Count == 1)
                {
                    _items[index] = null;
                    _count--;
                }
                else
                {
                    System.Diagnostics.Debug.Assert(itemTaked.Count > 1);

                    itemTaked = itemTaked.DivideHalf();
                    System.Diagnostics.Debug.Assert(itemTaked != null);
                }
            }

            return (f, itemTaked);
        }

        internal virtual (bool, Item?) PutOne(int index, Item itemCursor, SlotData slotData)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(_count >= 0);
            System.Diagnostics.Debug.Assert(_count <= TotalSlotCount);
            System.Diagnostics.Debug.Assert(index >= 0);
            System.Diagnostics.Debug.Assert(index < TotalSlotCount);

            bool f;

            Item? itemTaked = _items[index];

            if (itemTaked != null)
            {
                System.Diagnostics.Debug.Assert(!ReferenceEquals(itemTaked, itemCursor));

                f = itemTaked.CompareWithPacketFormat(slotData);

                _items[index] = itemCursor;

                if (itemCursor.TYPE == itemTaked.TYPE)
                {
                    if (itemCursor.Count == 1)
                    {
                        int spend = itemTaked.Stack(1);
                        if (spend == 1)
                        {
                            itemCursor.SetCount(itemTaked.Count);
                            itemTaked = null;
                        }
                        else
                        {
                            System.Diagnostics.Debug.Assert(spend == 0);

                            int temp = itemCursor.Count;
                            itemCursor.SetCount(itemTaked.Count);
                            itemTaked.SetCount(temp);
                        }
                    }
                    else
                    {
                        System.Diagnostics.Debug.Assert(itemCursor.Count > 1);

                        int spend = itemTaked.Stack(1);
                        if (spend == 1)
                        {
                            itemCursor.Spend(1);
                        }
                        else
                        {
                            System.Diagnostics.Debug.Assert(spend == 0);

                        }

                        int temp = itemCursor.Count;
                        itemCursor.SetCount(itemTaked.Count);
                        itemTaked.SetCount(temp);
                    }

                    /*int temp = itemCursor.Count;
                    itemCursor.SetCount(itemTaked.Count);
                    itemTaked.SetCount(temp);*/
                }
                else
                {

                }

                return (f, itemTaked);
            }
            else
            {
                _items[index] = itemCursor;

                if (itemCursor.Count > 1)
                {
                    itemTaked = itemCursor.DivideExceptOne();
                }
                else
                {
                    System.Diagnostics.Debug.Assert(itemCursor.Count == 1);
                    System.Diagnostics.Debug.Assert(itemTaked == null);
                }

                _count++;

                f = (slotData.Id == -1);

                
            }

            return (f, itemTaked);
        }

        public void Print()
        {
            System.Console.WriteLine($"Count: {_count}");
            for (int i = 0; i < TotalSlotCount; ++i)
            {
                if (i % 9 == 0)
                {
                    System.Console.WriteLine();
                }

                Item? item = _items[i];
                if (item == null)
                {
                    System.Console.Write($"[{-1}, {0}]");
                    continue;
                }

                System.Console.Write($"[{item.TYPE}, {item.Count}]");
            }
            System.Console.WriteLine();
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

    internal sealed class SelfInventory : Inventory
    {
        private bool _disposed = false;

        public int PrimarySlotCount = 36;

        public System.Collections.Generic.IEnumerable<Item?> PrimaryItems
        {
            get
            {
                System.Diagnostics.Debug.Assert(!_disposed);
                for (int i = 9; i < 45; ++i)
                    yield return _items[i];
            }
        }

        public System.Collections.Generic.IEnumerable<Item?> NotPrimaryItems
        {
            get
            {
                System.Diagnostics.Debug.Assert(!_disposed);
                for (int i = 0; i < 9; ++i)
                    yield return _items[i];
            }
        }

        internal Item? OffhandItem
        {
            get
            {
                System.Diagnostics.Debug.Assert(!_disposed);
                return _items[45];
            }
        }

        public SelfInventory() : base(46) 
        {
         /*   PutAll(15, new Item(Item.Types.Stone, 64), new(-1, 0));
            PutAll(16, new Item(Item.Types.Stone, 1), new(-1, 0));
            PutAll(17, new Item(Item.Types.Grass, 64), new(-1, 0));*/

        }

        ~SelfInventory() => System.Diagnostics.Debug.Assert(false);

        internal override (bool, Item?) TakeAll(int index, SlotData slotData)
        {
            if (index == 0)
            {
                return base.TakeAll(index, slotData);
            }
            else if (index > 0 && index <= 4)
            {
                return base.TakeAll(index, slotData);
            }
            else if (index > 4 && index <= 8)
            {
                return base.TakeAll(index, slotData);
            }
            else
            {
                return base.TakeAll(index, slotData);
            }
        }

        internal override (bool, Item?) PutAll(int index, Item itemCursor, SlotData slotData)
        {
            if (index == 0)
            {
                bool f;
                Item? itemTaked = _items[index];
                if (itemTaked != null)
                {
                    f = itemTaked.CompareWithPacketFormat(slotData);
                }
                else
                {
                    if (slotData.Id == -1)
                        f = true;
                    else
                        f = false;
                }

                return (f, itemCursor);
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
                return base.PutAll(index, itemCursor, slotData);
            }
        }

        public void DistributeItem(int[] indexes, Item item)
        {
            throw new System.NotImplementedException();
        }

        public void CollectItems()
        {
            throw new System.NotImplementedException();
        }

        public override void Dispose()
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            // Assertion.

            // Release resources.

            // Finish.
            base.Dispose();
            _disposed = true;
        }

    }

    public abstract class PublicInventory : Inventory
    {
        private bool _disposed = false;

        internal abstract string WindowType { get; }
        /*public abstract string WindowTitle { get; }*/

        private readonly object _SharedObject = new();

        private readonly NumList _IdList = new();  // Disposable
        private readonly PublicInventoryRenderer _Renderer = new();  // Disposable

        public PublicInventory(int count) : base(count) { }

        ~PublicInventory() => System.Diagnostics.Debug.Assert(false);

        internal int Open(
            int windowId, Queue<ClientboundPlayingPacket> outPackets,
            SelfInventory selfInventory)
        {

            System.Diagnostics.Debug.Assert(!_disposed);

            {
                int n = 9;
                var arr = new SlotData[n];

                for (int i = 0; i < n; ++i)
                {
                    Item? item = _items[i];

                    if (item == null)
                    {
                        arr[i] = new();
                        continue;
                    }

                    arr[i] = item.ConventToPacketFormat();
                }

                outPackets.Enqueue(new SetWindowItemsPacket(0, arr));

                Item? offHandItem = selfInventory.OffhandItem;
                if (offHandItem == null)
                {
                    outPackets.Enqueue(new SetSlotPacket(0, 45, new()));
                }
                else
                {
                    outPackets.Enqueue(new SetSlotPacket(0, 45, offHandItem.ConventToPacketFormat()));
                }
            }

            lock (_SharedObject)
            {

                System.Diagnostics.Debug.Assert(windowId >= byte.MinValue);
                System.Diagnostics.Debug.Assert(windowId <= byte.MaxValue);
                System.Diagnostics.Debug.Assert(TotalSlotCount >= byte.MinValue);
                System.Diagnostics.Debug.Assert(TotalSlotCount <= byte.MaxValue);
                outPackets.Enqueue(new OpenWindowPacket(
                    (byte)windowId, WindowType, "", (byte)TotalSlotCount));

                int count = selfInventory.PrimarySlotCount + TotalSlotCount;

                int i = 0;
                var arr = new SlotData[count];

                foreach (Item? item in Items)
                {
                    if (item == null)
                    {
                        arr[i++] = new();
                        continue;
                    }

                    arr[i++] = item.ConventToPacketFormat();
                }

                foreach (Item? item in selfInventory.PrimaryItems)
                {
                    if (item == null)
                    {
                        arr[i++] = new();
                        continue;
                    }

                    arr[i++] = item.ConventToPacketFormat();
                }

                System.Diagnostics.Debug.Assert(windowId >= byte.MinValue);
                System.Diagnostics.Debug.Assert(windowId <= byte.MaxValue);
                outPackets.Enqueue(new SetWindowItemsPacket((byte)windowId, arr));

                System.Diagnostics.Debug.Assert(i == count);

                int id = _IdList.Alloc();
                _Renderer.Add(id, windowId, outPackets);

                return id;
            }

        }

        internal void Close(int id, int _windowId)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            lock (_SharedObject)
            {
                int windowId = _Renderer.Remove(id);
                System.Diagnostics.Debug.Assert(_windowId == windowId);
            }
        }

        internal override (bool, Item?) TakeAll(int index, SlotData slotData)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            lock (_SharedObject)
            {
                (bool f, Item? item) = base.TakeAll(index, slotData);

                System.Diagnostics.Debug.Assert(_items[index] is null);
                _Renderer.RenderToEmpty(index);

                return (f, item);
            }
        }

        internal override (bool, Item?) PutAll(int index, Item itemCursor, SlotData slotData)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            lock (_SharedObject)
            {
                (bool f, Item? result) = base.PutAll(index, itemCursor, slotData);

                {
                    Item? item = _items[index];
                    System.Diagnostics.Debug.Assert(item != null);
                    _Renderer.RenderToSet(index, item);
                }

                return (f, result);
            }
        }

        internal override (bool, Item?) TakeHalf(int index, SlotData slotData)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            lock (_SharedObject)
            {
                (bool f, Item? result) = base.TakeHalf(index, slotData);

                {
                    Item? item = _items[index];
                    if (item == null)
                    {
                        _Renderer.RenderToEmpty(index);
                    }
                    else
                    {
                        _Renderer.RenderToSet(index, item);
                    }
                }

                return (f, result);
            }
        }

        internal override (bool, Item?) PutOne(int index, Item itemCursor, SlotData slotData)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            lock (_SharedObject)
            {
                (bool f, Item? result) = base.PutOne(index, itemCursor, slotData);

                {
                    Item? item = _items[index];
                    System.Diagnostics.Debug.Assert(item != null);
                    _Renderer.RenderToSet(index, item);
                }

                return (f, result);
            }
        }

        internal virtual void DistributeItem(
            int[] indexes, Item item, SelfInventory privateInventory)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            throw new System.NotImplementedException();
        }

        public virtual void CollectItems()
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            throw new System.NotImplementedException();
        }

        public void CloseForcibly()
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            lock (_SharedObject)
            {
                int[] ids = _Renderer.CloseForciblyAndFlush();
                for (int i = 0; i < ids.Length; ++i)
                {
                    _IdList.Dealloc(ids[i]);
                }
            }

        }

        public override void Dispose()
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            // Assertion.
            System.Diagnostics.Debug.Assert(_IdList.Empty);

            // Release resources.
            _IdList.Dispose();
            _Renderer.Dispose();

            // Finish.
            base.Dispose();
            _disposed = true;
        }

    }

    public sealed class ChestInventory : PublicInventory
    {
        private bool _disposed = false;

        internal override string WindowType => "minecraft:chest";

        public ChestInventory() : base(27) { }

        ~ChestInventory() => System.Diagnostics.Debug.Assert(false);

        public override void Dispose()
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            // Assertion.

            // Release resources.

            // Finish.
            base.Dispose();
            _disposed = true;
        }

    }

}

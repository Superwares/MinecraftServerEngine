
using Containers;
using System;
using System.Diagnostics;
using System.Xml.Serialization;

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

        internal virtual (bool, Item?) TakeItem(int index, SlotData slotData)
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
                if (slotData.Id == -1)
                    f = true;
                else
                    f = false;
            }

            return (f, itemTaked);
        }

        internal virtual (bool, Item?) PutItem(int index, Item item, SlotData slotData)
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
                System.Diagnostics.Debug.Assert(!ReferenceEquals(itemTaked, item));

                f = itemTaked.CompareWithPacketFormat(slotData);

                _items[index] = item;

                if (item.TYPE == itemTaked.TYPE)
                {
                    int spend = item.Stack(itemTaked.Count);

                    if (spend == itemTaked.Count)
                    {
                        itemTaked = null;
                    }
                    else
                    {
                        System.Diagnostics.Debug.Assert(spend < item.Count);
                        itemTaked.Spend(spend);
                    }
                }
                else
                {
                    
                }
            }
            else
            {
                _items[index] = item;

                _count++;

                if (slotData.Id == -1)
                    f = true;
                else
                    f = false;
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

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;

            // Assertion.
            Debug.Assert(_count == 0);

            if (disposing == true)
            {
                // Release managed resources.
                
            }

            // Release unmanaged resources.

            _disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            System.GC.SuppressFinalize(this);
        }

        public void Close() => Dispose();

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
            PutItem(15, new Item(Item.Types.Stone, 64), new(-1, 0));
            PutItem(16, new Item(Item.Types.Stone, 1), new(-1, 0));

        }

        ~SelfInventory() => System.Diagnostics.Debug.Assert(false);

        internal override (bool, Item?) TakeItem(int index, SlotData slotData)
        {
            if (index == 0)
            {
                return base.TakeItem(index, slotData);
            }
            else if (index > 0 && index <= 4)
            {
                return base.TakeItem(index, slotData);
            }
            else if (index > 4 && index <= 8)
            {
                return base.TakeItem(index, slotData);
            }
            else
            {
                return base.TakeItem(index, slotData);
            }
        }

        internal override (bool, Item?) PutItem(int index, Item itemCursor, SlotData slotData)
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
                return base.PutItem(index, itemCursor, slotData);
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

        protected override void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                // Assertion.

                if (disposing == true)
                {
                    // Release managed resources.
                }

                // Release unmanaged resources.

                _disposed = true;
            }

            base.Dispose(disposing);
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

            lock (_SharedObject)
            {

                Debug.Assert(windowId >= byte.MinValue);
                Debug.Assert(windowId <= byte.MaxValue);
                Debug.Assert(TotalSlotCount >= byte.MinValue);
                Debug.Assert(TotalSlotCount <= byte.MaxValue);
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

                Debug.Assert(windowId >= byte.MinValue);
                Debug.Assert(windowId <= byte.MaxValue);
                outPackets.Enqueue(new SetWindowItemsPacket((byte)windowId, arr));

                Debug.Assert(i == count);

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


        internal override (bool, Item?) TakeItem(int index, SlotData slotData)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            lock (_SharedObject)
            {
                _Renderer.RenderToEmpty(index);

                return base.TakeItem(index, slotData);
            }
        }

        internal override (bool, Item?) PutItem(int index, Item item, SlotData slotData)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            lock (_SharedObject)
            {
                (bool f, Item? result) = base.PutItem(index, item, slotData);
                _Renderer.RenderToSet(index, item);

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

        protected override void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                // Assertion.
                System.Diagnostics.Debug.Assert(_IdList.Empty);

                if (disposing == true)
                {
                    // Release managed resources.
                    _IdList.Dispose();
                    _Renderer.Dispose();
                }

                // Release unmanaged resources.

                _disposed = true;
            }

            base.Dispose(disposing);
        }

    }

    public sealed class ChestInventory : PublicInventory
    {
        private bool _disposed = false;

        internal override string WindowType => "minecraft:chest";

        public ChestInventory() : base(27) { }

        ~ChestInventory() => System.Diagnostics.Debug.Assert(false);

        protected override void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                // Assertion.

                if (disposing == true)
                {
                    // Release managed resources.
                }

                // Release unmanaged resources.

                _disposed = true;
            }

            base.Dispose(disposing);
        }

    }

}

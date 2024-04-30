
using Containers;
using System;
using System.Diagnostics;
using System.Xml.Serialization;

namespace Protocol
{
    public abstract class Inventory : System.IDisposable
    {
        private bool _disposed = false;

        public readonly int Count;
        protected readonly Item?[] _items;
        public System.Collections.Generic.IEnumerable<Item?> Items
        {
            get
            {
                System.Diagnostics.Debug.Assert(!_disposed);
                return _items;
            }
        }

        public Inventory(int count)
        {
            Count = count;

            _items = new Item?[count];
            System.Array.Fill(_items, null);
        }

        ~Inventory() => System.Diagnostics.Debug.Assert(false);

        private Item CloneItem(Item item, int count)
        {
            switch (item.Id)
            {
                default:
                    throw new NotImplementedException();
                case 280:
                    return new Stick(count);
            }
        }

        public virtual Item? TakeItem()
        {
            throw new System.NotImplementedException();
        }

        public virtual Item? PutItem()
        {
            throw new System.NotImplementedException();
        }

        public void Print()
        {
            for (int i = 0; i < Count; ++i)
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

                System.Console.Write($"[{item.Id}, {item.Count}]");
            }
            System.Console.WriteLine();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;

            // Assertion.

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

        public SelfInventory() : base(46) { }

        ~SelfInventory() => System.Diagnostics.Debug.Assert(false);

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
                Debug.Assert(Count >= byte.MinValue);
                Debug.Assert(Count <= byte.MaxValue);
                outPackets.Enqueue(new OpenWindowPacket(
                    (byte)windowId, WindowType, "", (byte)Count));

                int count = selfInventory.PrimarySlotCount + Count;

                int i = 0;
                var arr = new SlotData[count];

                foreach (Item? item in Items)
                {
                    if (item == null)
                    {
                        arr[i++] = new();
                        continue;
                    }

                    Debug.Assert(item.Id >= short.MinValue);
                    Debug.Assert(item.Id <= short.MaxValue);
                    Debug.Assert(item.Count >= byte.MinValue);
                    Debug.Assert(item.Count <= byte.MaxValue);
                    arr[i++] = new((short)item.Id, (byte)item.Count);
                }

                foreach (Item? item in selfInventory.PrimaryItems)
                {
                    if (item == null)
                    {
                        arr[i++] = new();
                        continue;
                    }

                    Debug.Assert(item.Id >= short.MinValue);
                    Debug.Assert(item.Id <= short.MaxValue);
                    Debug.Assert(item.Count >= byte.MinValue);
                    Debug.Assert(item.Count <= byte.MaxValue);
                    arr[i++] = new((short)item.Id, (byte)item.Count);
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
                (int windowId, Queue<ClientboundPlayingPacket> outPackets) = _Renderer.Remove(id);
                System.Diagnostics.Debug.Assert(_windowId == windowId);
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

        public void Flush()
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            lock (_SharedObject)
            {
                int[] ids = _Renderer.Flush();
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


using Containers;
using System.Diagnostics;

namespace Protocol
{
    internal abstract class Inventory : System.IDisposable
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

        public virtual void ClickLeftMouseButton(int index, ref Item? itemCursor)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            Item? itemTaked = _items[index];
            if (itemTaked != null && itemCursor != null)
            {
                if (itemTaked.Id == itemCursor.Id)
                {
                    int count = itemTaked.Stack(itemCursor.Count);
                    if (count == itemCursor.Count)
                    {
                        itemCursor = null;
                    }
                    else
                    {
                        Debug.Assert(count < itemCursor.Count);
                        itemCursor.Waste(count);
                    }
                }
                else
                {
                    Item temp = itemCursor;
                    itemCursor = _items[index];
                    _items[index] = temp;
                }
            }
            else if (itemTaked != null)
            {
                System.Diagnostics.Debug.Assert(itemCursor == null);

                itemCursor = itemTaked;
                _items[index] = null;
            }
            else if (itemCursor != null)
            {
                System.Diagnostics.Debug.Assert(itemTaked == null);

                _items[index] = itemCursor;
                itemCursor = null;
            }
            else
            {
                // Nothing.
            }
        }

        public virtual void ClickRightMouseButton(int index, ref Item? itemCursor)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            Item? itemTaked = _items[index];
            if (itemTaked != null && itemCursor != null)
            {
                if (itemTaked.Id == itemCursor.Id)
                {
                    int count = itemTaked.Stack(1);

                    System.Diagnostics.Debug.Assert(itemCursor.Count >= count);
                    if (itemCursor.Count == count)
                    {
                        itemCursor = null;
                    }
                    else
                    {
                        Debug.Assert(count < itemCursor.Count);
                        itemCursor.Waste(count);
                    }
                }
                else
                {
                    Item temp = itemCursor;
                    itemCursor = _items[index];
                    _items[index] = temp;
                }
            }
            else if (itemTaked != null)
            {
                System.Diagnostics.Debug.Assert(itemCursor == null);

                if (itemTaked.Count == 1)
                {
                    _items[index] = null;
                    itemCursor = itemTaked;
                }
                else
                {
                    itemCursor = itemTaked.DivideHalf();
                }

            }
            else if (itemCursor != null)
            {
                System.Diagnostics.Debug.Assert(itemTaked == null);

                System.Diagnostics.Debug.Assert(itemCursor.Count >= itemCursor.MinCount);
                if (itemCursor.Count == 1)
                {
                    _items[index] = itemCursor;
                    itemCursor = null;
                }
                else
                {
                    Debug.Assert(itemCursor != null);
                    _items[index] = itemCursor.DivideOne();
                }
                
            }
            else
            {
                // Nothing.
            }
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

    }

    internal sealed class PlayerInventory : Inventory
    {
        private bool _disposed = false;

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

        public PlayerInventory() : base(46)
        {
            _items[10] = new Stick(64);
            _items[11] = new Stick(64);
        }

        ~PlayerInventory() => System.Diagnostics.Debug.Assert(false);

        public override void ClickLeftMouseButton(int index, ref Item? itemCursor)
        {
            if (index == 0)
            {
                // Nothing
            }
            else if (index > 0 && index <= 4)
            {
                throw new System.NotImplementedException();
            }
            else if (index >4 && index <= 8)
            {
                throw new System.NotImplementedException();
            }
            else
            {
                base.ClickLeftMouseButton(index, ref itemCursor);
            }
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

    internal abstract class PublicInventory : Inventory
    {
        private bool _disposed = false;

        public abstract string WindowType { get; }
        /*public abstract string WindowTitle { get; }*/

        internal readonly object _SharedObject = new();

        protected readonly Table<int, PublicInventoryRenderer> _renderers = new();

        public PublicInventory(int count) : base(count) { }

        ~PublicInventory() => System.Diagnostics.Debug.Assert(false);

        public void AddRenderer(
            int idConn,
            int windowId, Queue<ClientboundPlayingPacket> outPackets)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            PublicInventoryRenderer renderer = new(windowId, outPackets);
            _renderers.Insert(idConn, renderer);
        }

        public void RemoveRenderer(int idConn)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            _renderers.Extract(idConn);
        }

        private void RenderToSet(int index, Item item)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            // TODO: Check locking.

            if (_renderers.Count <= 1)
                return;

            foreach (var renderer in _renderers.GetValues())
                renderer.Set(index, item);
        }

        private void RenderToEmpty(int index)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            // TODO: Check locking.

            if (_renderers.Count <= 1)
                return;

            foreach (var renderer in _renderers.GetValues())
                renderer.Empty(index);
        }

        public override void ClickLeftMouseButton(int index, ref Item? itemCursor)
        {
            lock (_SharedObject)
            {
                base.ClickLeftMouseButton(index, ref itemCursor);

                Item? itemTaked = _items[index];
                if (itemTaked != null)
                {
                    RenderToSet(index, itemTaked);
                }
                else
                {
                    RenderToEmpty(index);
                }

            }
        }

        public override void ClickRightMouseButton(int index, ref Item? itemCursor)
        {
            lock (_SharedObject)
            {
                base.ClickRightMouseButton(index, ref itemCursor);

                Item? itemTaked = _items[index];
                if (itemTaked != null)
                {
                    RenderToSet(index, itemTaked);
                }
                else
                {
                    RenderToEmpty(index);
                }

            }
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

    internal sealed class ChestInventory : PublicInventory
    {
        private bool _disposed = false;

        public override string WindowType => "minecraft:chest";

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

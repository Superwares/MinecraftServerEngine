
using System;
using System.Diagnostics;
using Containers;

namespace Protocol
{
    internal abstract class Inventory : IDisposable
    {
        private bool _disposed = false;

        protected readonly Table<int, InventoryRenderer> _renderers = new();

        public readonly int Count;
        private readonly Item?[] _items;

        public Inventory(int count)
        {
            Count = count;

            _items = new Item?[count];
            Array.Fill(_items, null);
        }

        ~Inventory() => Debug.Assert(false);
        
        protected System.Collections.Generic.IEnumerable<Item?>
             _Init(int idConn, InventoryRenderer renderer)
        {
            _renderers.Insert(idConn, renderer);

            return _items;
        }

        public void RemoveRenderer(int idConn)
        {
            _renderers.Extract(idConn);
        }

        private void RenderToSet(int index, Item item)
        {
            foreach (var renderer in _renderers.GetValues())
                renderer.Set(index, item);
        }

        private void RenderToEmpty(int index)
        {
            foreach (var renderer in _renderers.GetValues())
                renderer.Empty(index);
        }

        protected virtual Item? Take(int index)
        {
            Debug.Assert(!_disposed);

            Debug.Assert(index >= 0);
            Debug.Assert(index <= Count);

            Item? itemTaked = _items[index];
            _items[index] = null;

            RenderToEmpty(index);

            return itemTaked;
        }

        protected virtual Item? Put(int index, Item item)
        {
            Debug.Assert(!_disposed);

            Debug.Assert(index >= 0);
            Debug.Assert(index <= Count);

            Item? itemTaked = _items[index];
            Debug.Assert(itemTaked != null ? 
                !ReferenceEquals(item, itemTaked) : true);

            _items[index] = item;

            RenderToSet(index, item);

            return itemTaked;
        }

        /*public void SwapAt*/

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
            GC.SuppressFinalize(this);
        }

    }

    internal sealed class PlayerInventory : Inventory
    {
        public const int TotalCount = 46;

        private bool _disposed = false;

        public PlayerInventory() : base(TotalCount) { }

        ~PlayerInventory() => Debug.Assert(false);

        internal System.Collections.Generic.IEnumerable<Item?> 
            Init(int idConn, PlayerInventoryRenderer renderer)
        {
            return _Init(idConn, renderer);
        }

        public Item? TakeFromCraftingOutput()
        {
            return Take(0);
        }

        public Item? TakeFromMain(int index)
        {
            Debug.Assert(index >= 0 && index < 27);
            return Take(index + 9);
        }
        
        public Item? PutIntoMain(int index, Item item)
        {
            Debug.Assert(index >= 0 && index < 27);
            return Put(index + 9, item);
        }

        public Item? TakeFromHotbar(int index)
        {
            Debug.Assert(index >= 0 && index < 9);
            return Take(index + 36);
        }

        public Item? PutIntoHotbar(int index, Item item)
        {
            Debug.Assert(index >= 0 && index < 9);
            return Put(index + 36, item);
        }

        public Item? TakeFromOffhand(int index)
        {
            throw new NotImplementedException();
        }

        public Item? PutIntoOffhand(int index, Item item)
        {
            throw new NotImplementedException();
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

    internal sealed class ChestInventory : Inventory
    {
        private bool _disposed = false;

        public ChestInventory() : base(27) { }

        ~ChestInventory() => Debug.Assert(false);

        internal System.Collections.Generic.IEnumerable<Item?>
            Init(int idConn, ChestInventoryRenderer renderer)
        {
            return _Init(idConn, renderer);
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

    

}

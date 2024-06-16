using Common;
using Containers;

namespace MinecraftServerEngine
{
    

    internal abstract class Inventory : System.IDisposable
    {
        private bool _disposed = false;

        public readonly int TOTAL_SLOT_COUNT;

        protected readonly ItemSlot[] _slots;

        public System.Collections.Generic.IEnumerable<ItemSlot> AllSlots
        {
            get
            {
                System.Diagnostics.Debug.Assert(!_disposed);
                return _slots;
            }
        }

        public Inventory(int totalSlotCount)
        {
            TOTAL_SLOT_COUNT = totalSlotCount;

            _slots = new ItemSlot[TOTAL_SLOT_COUNT];
            System.Diagnostics.Debug.Assert((ItemSlot)default == null);
            Array<ItemSlot>.Fill(_slots, default);
        }

        ~Inventory() => System.Diagnostics.Debug.Assert(false);

        internal virtual (bool, ItemSlot) TakeAll(int index, SlotData slotData)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(_count >= 0);
            System.Diagnostics.Debug.Assert(_count <= TOTAL_SLOT_COUNT);
            System.Diagnostics.Debug.Assert(index >= 0);
            System.Diagnostics.Debug.Assert(index < TOTAL_SLOT_COUNT);

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

        internal virtual (bool, ItemSlot) PutAll(int index, ItemSlot cursor, SlotData slotData)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(_count >= 0);
            System.Diagnostics.Debug.Assert(_count <= TOTAL_SLOT_COUNT);
            System.Diagnostics.Debug.Assert(index >= 0);
            System.Diagnostics.Debug.Assert(index < TOTAL_SLOT_COUNT);

            bool f;

            Item? itemTaked = _items[index];

            if (itemTaked != null)
            {
                System.Diagnostics.Debug.Assert(!ReferenceEquals(itemTaked, cursor));

                f = itemTaked.CompareWithPacketFormat(slotData);

                _items[index] = cursor;

                if (cursor.Type == itemTaked.Type)
                {
                    int spend = cursor.Stack(itemTaked.Count);

                    if (spend == itemTaked.Count)
                    {
                        itemTaked = null;
                    }
                    else
                    {
                        System.Diagnostics.Debug.Assert(spend < cursor.Count);
                        itemTaked.Spend(spend);
                    }
                }
                else
                {
                    
                }
            }
            else
            {
                _items[index] = cursor;

                _count++;

                f = (slotData.Id == -1);
            }
            
            return (f, itemTaked);
        }

        internal virtual (bool, ItemSlot) TakeHalf(int index, SlotData slotData)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(_count >= 0);
            System.Diagnostics.Debug.Assert(_count <= TOTAL_SLOT_COUNT);
            System.Diagnostics.Debug.Assert(index >= 0);
            System.Diagnostics.Debug.Assert(index < TOTAL_SLOT_COUNT);

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

        internal virtual (bool, ItemSlot) PutOne(int index, ItemSlot cursor, SlotData slotData)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(_count >= 0);
            System.Diagnostics.Debug.Assert(_count <= TOTAL_SLOT_COUNT);
            System.Diagnostics.Debug.Assert(index >= 0);
            System.Diagnostics.Debug.Assert(index < TOTAL_SLOT_COUNT);

            bool f;

            Item? itemTaked = _items[index];

            if (itemTaked != null)
            {
                System.Diagnostics.Debug.Assert(!ReferenceEquals(itemTaked, itemCursor));

                f = itemTaked.CompareWithPacketFormat(slotData);

                _items[index] = itemCursor;

                if (itemCursor.Type == itemTaked.Type)
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
            for (int i = 0; i < TOTAL_SLOT_COUNT; ++i)
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

                System.Console.Write($"[{item.Type}, {item.Count}]");
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

    internal sealed class PlayerInventory : Inventory
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

        public PlayerInventory() : base(46) 
        {
         /*   PutAll(15, new Item(Item.Types.Stone, 64), new(-1, 0));
            PutAll(16, new Item(Item.Types.Stone, 1), new(-1, 0));
            PutAll(17, new Item(Item.Types.Grass, 64), new(-1, 0));*/

        }

        ~PlayerInventory() => System.Diagnostics.Debug.Assert(false);

        internal override (bool, ItemSlot) TakeAll(int index, SlotData slotData)
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

        internal override (bool, ItemSlot) PutAll(int index, Item itemCursor, SlotData slotData)
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

}

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

        public virtual bool TakeAll(int index, ref ItemSlot cursor, SlotData slotData)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(cursor == null);

            System.Diagnostics.Debug.Assert(index >= 0);
            System.Diagnostics.Debug.Assert(index < TOTAL_SLOT_COUNT);

            bool f;

            ItemSlot slotTaked = _slots[index];
            _slots[index] = null;

            if (slotTaked != null)
            {
                f = slotTaked.CompareWithProtocolFormat(slotData);

                cursor = slotTaked;
            }
            else
            {
                f = (slotData.Id == -1);

                System.Diagnostics.Debug.Assert(cursor == null);
            }

            return f;
        }

        public virtual bool PutAll(int index, ref ItemSlot cursor, SlotData slotData)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(cursor != null);

            System.Diagnostics.Debug.Assert(index >= 0);
            System.Diagnostics.Debug.Assert(index < TOTAL_SLOT_COUNT);

            bool f;

            ItemSlot slotTaked = _slots[index];

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
                    _slots[index] = cursor;
                    cursor = slotTaked;
                }
            }
            else
            {
                _slots[index] = cursor;
                cursor = null;

                f = (slotData.Id == -1);
            }

            return f;
        }

        public virtual bool TakeHalf(int index, ref ItemSlot cursor, SlotData slotData)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(cursor == null);

            System.Diagnostics.Debug.Assert(index >= 0);
            System.Diagnostics.Debug.Assert(index < TOTAL_SLOT_COUNT);

            bool f;

            ItemSlot slotTaked = _slots[index];

            if (slotTaked == null)
            {
                f = (slotData.Id == -1);

                cursor = null;
            }
            else
            {
                f = slotTaked.CompareWithProtocolFormat(slotData);

                if (slotTaked.Count == 1)
                {
                    _slots[index] = null;

                    cursor = slotTaked;
                    System.Diagnostics.Debug.Assert(cursor.Count == 1);
                }
                else
                {
                    System.Diagnostics.Debug.Assert(slotTaked.Count > 1);

                    cursor = slotTaked.DivideHalf();
                    System.Diagnostics.Debug.Assert(cursor != null);
                }
            }

            return f;
        }

        public virtual bool PutOne(int index, ref ItemSlot cursor, SlotData slotData)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(cursor != null);

            System.Diagnostics.Debug.Assert(index >= 0);
            System.Diagnostics.Debug.Assert(index < TOTAL_SLOT_COUNT);

            bool f;

            ItemSlot slotTaked = _slots[index];

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

                    _slots[index] = cursor;
                    cursor = slotTaked;
                }

            }
            else
            {
                if (cursor.Count > 1)
                {
                    _slots[index] = cursor.DivideOne();
                }
                else
                {
                    System.Diagnostics.Debug.Assert(cursor.Count == 1);
                    System.Diagnostics.Debug.Assert(slotTaked == null);

                    _slots[index] = cursor;
                    cursor = null;
                }

                f = (slotData.Id == -1);
            }

            return f;
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

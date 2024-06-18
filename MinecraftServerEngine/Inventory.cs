using Common;
using Containers;

namespace MinecraftServerEngine
{
    

    internal abstract class Inventory : System.IDisposable
    {
        private bool _disposed = false;

        public readonly int TotalSlotCount;

        private protected int _count = 0;
        private protected readonly ItemSlot[] _slots;

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
            TotalSlotCount = totalSlotCount;

            System.Diagnostics.Debug.Assert(totalSlotCount > 0);
            System.Diagnostics.Debug.Assert(_count == 0);

            _slots = new ItemSlot[TotalSlotCount];
            System.Diagnostics.Debug.Assert((ItemSlot)default == null);
            Array<ItemSlot>.Fill(_slots, default);
        }

        ~Inventory() => System.Diagnostics.Debug.Assert(false);

        public virtual bool TakeAll(int index, ref ItemSlot cursor, SlotData slotData)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(cursor == null);

            System.Diagnostics.Debug.Assert(_count >= 0);
            System.Diagnostics.Debug.Assert(_count <= TotalSlotCount);

            System.Diagnostics.Debug.Assert(index >= 0);
            System.Diagnostics.Debug.Assert(index < TotalSlotCount);

            bool f;

            ItemSlot slotTaked = _slots[index];
            _slots[index] = null;

            if (slotTaked != null)
            {
                f = slotTaked.CompareWithProtocolFormat(slotData);

                cursor = slotTaked;

                _count--;
                System.Diagnostics.Debug.Assert(_count >= 0);
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

            System.Diagnostics.Debug.Assert(_count >= 0);
            System.Diagnostics.Debug.Assert(_count <= TotalSlotCount);

            System.Diagnostics.Debug.Assert(index >= 0);
            System.Diagnostics.Debug.Assert(index < TotalSlotCount);

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

                ++_count;
                System.Diagnostics.Debug.Assert(_count >= 0);
                System.Diagnostics.Debug.Assert(_count <= TotalSlotCount);

                f = (slotData.Id == -1);
            }

            return f;
        }

        public virtual bool TakeHalf(int index, ref ItemSlot cursor, SlotData slotData)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(cursor == null);

            System.Diagnostics.Debug.Assert(_count >= 0);
            System.Diagnostics.Debug.Assert(_count <= TotalSlotCount);

            System.Diagnostics.Debug.Assert(index >= 0);
            System.Diagnostics.Debug.Assert(index < TotalSlotCount);

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

            return f;
        }

        public virtual bool PutOne(int index, ref ItemSlot cursor, SlotData slotData)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(cursor != null);

            System.Diagnostics.Debug.Assert(_count >= 0);
            System.Diagnostics.Debug.Assert(_count <= TotalSlotCount);

            System.Diagnostics.Debug.Assert(index >= 0);
            System.Diagnostics.Debug.Assert(index < TotalSlotCount);

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
                    System.Diagnostics.Debug.Assert(cursor.Count >= cursor.MinCount);
                }
                else
                {
                    System.Diagnostics.Debug.Assert(cursor.Count == 1);
                    System.Diagnostics.Debug.Assert(slotTaked == null);

                    _slots[index] = cursor;
                    cursor = null;
                }

                ++_count;
                System.Diagnostics.Debug.Assert(_count >= 0);
                System.Diagnostics.Debug.Assert(_count <= TotalSlotCount);

                f = (slotData.Id == -1);
            }

            return f;
        }

        public void Print()
        {
            System.Console.WriteLine($"Inventory: ");
            System.Console.WriteLine($"\tCount: {_count}");
            for (int i = 0; i < TotalSlotCount; ++i)
            {
                if (i % 9 == 0)
                {
                    System.Console.WriteLine();
                    System.Console.Write("\t");
                }

                ItemSlot item = _slots[i];
                if (item == null)
                {
                    System.Console.Write($"[None]");
                    continue;
                }

                System.Console.Write($"[{item.Item}x{item.Count}]");
                System.Diagnostics.Debug.Assert(item.Count >= item.MinCount);
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

        /*public System.Collections.Generic.IEnumerable<ItemSlot> PrimarySlots
        {
            get
            {
                System.Diagnostics.Debug.Assert(!_disposed);
                for (int i = 9; i < 45; ++i)
                {
                    yield return _slots[i];
                }
            }
        }

        public System.Collections.Generic.IEnumerable<ItemSlot> NotPrimarySlots
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

        public PlayerInventory() : base(46) 
        {
         /*   PutAll(15, new Item(Item.Types.Stone, 64), new(-1, 0));
            PutAll(16, new Item(Item.Types.Stone, 1), new(-1, 0));
            PutAll(17, new Item(Item.Types.Grass, 64), new(-1, 0));*/

        }

        ~PlayerInventory() => System.Diagnostics.Debug.Assert(false);

        public override bool PutAll(int index, ref ItemSlot cursor, SlotData slotData)
        {
            if (index == 0)
            {
                bool f;

                ItemSlot slotTaked = _slots[index];
                if (slotTaked != null)
                {
                    f = slotTaked.CompareWithProtocolFormat(slotData);
                }
                else
                {
                    f = (slotData.Id == -1);
                }

                return f;
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
                System.Diagnostics.Debug.Assert(index > 8 && index < TotalSlotCount);
                return base.PutAll(index, ref cursor, slotData);
            }
        }

        /*public void DistributeItem(int[] indexes, Item item)
        {
            throw new System.NotImplementedException();
        }

        public void CollectItems()
        {
            throw new System.NotImplementedException();
        }*/

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

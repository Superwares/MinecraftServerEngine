

using System;

namespace MinecraftServerEngine
{
    internal sealed class InventorySlot
    {
        private ItemStack _stack = null;
        public ItemStack Stack => _stack;
        internal bool Empty => (_stack == null);

        public InventorySlot()
        {

        }

        internal void TakeAll(
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

        internal void PutAll(
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

        internal void LeftClick(InventorySlot cursor, bool takeOnly)
        {
            System.Diagnostics.Debug.Assert(cursor != null);

            if (cursor.Empty)
            {
                throw new System.NotImplementedException();
            }
            else
            {
                throw new System.NotImplementedException();
            }
        }

        internal void LeftClick(InventorySlot cursor)
        {
            System.Diagnostics.Debug.Assert(cursor != null);

            LeftClick(cursor, false);
        }

        internal void RightClick(InventorySlot cursor, bool takeOnly)
        {
            System.Diagnostics.Debug.Assert(cursor != null);

            if (cursor.Empty)
            {
                throw new System.NotImplementedException();
            }
            else
            {
                throw new System.NotImplementedException();
            }
        }

        internal void RightClick(InventorySlot cursor)
        {
            System.Diagnostics.Debug.Assert(cursor != null);

            RightClick(cursor, false);
        }

        internal void Move(InventorySlot from)
        {
            System.Diagnostics.Debug.Assert(from != null);

            if (from.Empty)
            {
                return;
            }

            if (!_stack.Equals(from._stack))
            {
                return;
            }

            if (_stack == null)
            {
                _stack = from._stack;
                from._stack = null;

                return;
            }

            int countFrom = from._stack.Count;
            System.Diagnostics.Debug.Assert(countFrom >= 0);
            int countSpend = _stack.Stack(countFrom);
            System.Diagnostics.Debug.Assert(countSpend >= 0);
            System.Diagnostics.Debug.Assert(countSpend <= countFrom);
            if (countSpend == countFrom)
            {
                from._stack = null;
            }
            else
            {
                from._stack.Spend(countSpend);
            }
        }

        internal void Give(ItemStack stack)
        {
            System.Diagnostics.Debug.Assert(stack != null);
            System.Diagnostics.Debug.Assert(stack.Count >= ItemStack.MinCount);

            System.Diagnostics.Debug.Assert(Empty);

            _stack = stack;
        }

        internal void WriteData(Buffer buffer)
        {
            System.Diagnostics.Debug.Assert(buffer != null);

            if (_stack == null)
            {
                buffer.WriteShort(-1);
                return;
            }

            _stack.WriteData(buffer);
        }

        internal byte[] WriteData()
        {
            using Buffer buffer = new();

            if (_stack == null)
            {
                buffer.WriteShort(-1);
            }
            else
            {
                _stack.WriteData(buffer);
            }

            return buffer.ReadData();
        }

        public override string ToString()
        {
            if (_stack == null)
            {
                return $"[None]";
            }

            System.Diagnostics.Debug.Assert(_stack != null);
            return $"[{_stack}]";
        }
    }
}

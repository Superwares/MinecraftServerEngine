
using MinecraftPrimitives;

namespace MinecraftServerEngine
{


    internal sealed class InventorySlot
    {
        /*internal enum AccessLevel
        {
            Full,
            HelmetOnly,
            ChestplateOnly,
            LeggingsOnly,
            BootsOnly,
        }*/

        /*private readonly AccessLevel Level;*/

        private ItemStack _itemStack = null;
        public ItemStack Stack => _itemStack;
        internal bool Empty => (_itemStack == null);

        public InventorySlot() { }

        /*public InventorySlot(AccessLevel level)
        {
            Level = level;
        }*/

        internal ItemStack Reset(ItemStack itemStack)
        {
            ItemStack prevItemStack = _itemStack;
            _itemStack = itemStack;
            return prevItemStack;
        }

        internal void LeftClick(InventorySlot cursor)
        {
            System.Diagnostics.Debug.Assert(cursor != null);

            ref ItemStack stackCursor = ref cursor._itemStack;

            if (Empty == false)
            {
                if (stackCursor == null)
                {
                    stackCursor = _itemStack;
                    _itemStack = null;
                }
                else if (_itemStack.Move(ref stackCursor) == false)
                {
                    (_itemStack, stackCursor) = (stackCursor, _itemStack);
                }
            }
            else
            {
                _itemStack = stackCursor;
                stackCursor = null;
            }

        }

        internal void RightClick(InventorySlot cursor)
        {
            System.Diagnostics.Debug.Assert(cursor != null);

            ref ItemStack stackCursor = ref cursor._itemStack;

            if (stackCursor == null)
            {
                if (Empty == false && _itemStack.DivideHalf(ref stackCursor) == false)
                {
                    stackCursor = _itemStack;
                    _itemStack = null;
                }
            }
            else
            {
                if (Empty == true)
                {
                    if (stackCursor.DivideMinToEmpty(ref _itemStack) == false)
                    {
                        _itemStack = stackCursor;
                        stackCursor = null;
                    }
                }
                else
                {
                    if (_itemStack.DivideMinFrom(ref stackCursor) == false)
                    {
                        ItemStack stackTemp = stackCursor;
                        stackCursor = _itemStack;
                        _itemStack = stackTemp;

                    }
                }

            }
        }

        internal void Move(InventorySlot fromSlot)
        {
            System.Diagnostics.Debug.Assert(fromSlot != null);

            if (fromSlot.Empty == true)
            {
                return;
            }

            ref ItemStack stackFrom = ref fromSlot._itemStack;

            if (Empty == true)
            {
                _itemStack = stackFrom;
                stackFrom = null;

                return;
            }

            _itemStack.Move(ref stackFrom);
        }

        internal bool Move(ref ItemStack fromItemStack)
        {
            if (fromItemStack == null)
            {
                return true;
            }

            if (Empty == true)
            {
                _itemStack = fromItemStack;
                fromItemStack = null;

                return true;
            }

            return _itemStack.Move(ref fromItemStack);
        }

        internal bool MoveAll(ref ItemStack fromItemStack)
        {
            if (fromItemStack == null)
            {
                return true;
            }

            if (Empty == true)
            {
                _itemStack = fromItemStack;
                fromItemStack = null;

                return true;
            }

            return _itemStack.MoveAll(ref fromItemStack);
        }

        //internal int PreMove(ItemStack from, int count)
        //{
        //    System.Diagnostics.Debug.Assert(count >= 0);

        //    if (count == 0)
        //    {
        //        return 0;
        //    }

        //    if (from.Equals(_stack) == false || _stack.IsFull == true)
        //    {
        //        return count;
        //    }

        //    int canMovedAmount = (_stack.MaxCount - _stack.Count);

        //    if (count < canMovedAmount)
        //    {
        //        return 0;
        //    }

        //    return count - canMovedAmount;
        //}

        internal int Move(IReadOnlyItem fromItem, int count)
        {
            System.Diagnostics.Debug.Assert(count >= 0);

            if (count == 0)
            {
                return 0;
            }

            if (_itemStack == null)
            {
                if (count >= fromItem.MaxCount)
                {
                    _itemStack = ItemStack.Create(fromItem, fromItem.MaxCount);
                    return count - fromItem.MaxCount;
                }
                else
                {
                    _itemStack = ItemStack.Create(fromItem, count);
                    return 0;
                }


            }

            return _itemStack.Move(fromItem, count);  // // remaning
        }

        internal int PreMove(IReadOnlyItem fromItem, int count)
        {
            System.Diagnostics.Debug.Assert(count >= 0);

            if (count == 0)
            {
                return 0;
            }

            if (_itemStack == null)
            {
                if (count >= fromItem.MaxCount)
                {
                    return count - fromItem.MaxCount;
                }
                else
                {
                    return 0;
                }


            }

            if (fromItem.Equals(_itemStack) == false || _itemStack.IsFull == true)
            {
                return count;
            }

            int canMovedAmount = (_itemStack.MaxCount - _itemStack.Count);

            if (count < canMovedAmount)
            {
                return 0;
            }

            return count - canMovedAmount;
        }

        internal int PreMove(int count)
        {
            System.Diagnostics.Debug.Assert(count >= 0);

            if (count == 0)
            {
                return 0;
            }

            if (_itemStack.IsFull == true)
            {
                return count;
            }

            int canMovedAmount = (_itemStack.MaxCount - _itemStack.Count);

            if (count < canMovedAmount)
            {
                return 0;
            }

            return count - canMovedAmount;
        }

        internal int Take(out ItemStack itemStack, int count)
        {
            System.Diagnostics.Debug.Assert(count >= 0);

            if (count == 0)
            {
                itemStack = null;
                return 0;
            }

            if (_itemStack == null)
            {
                itemStack = null;
                return 0;
            }

            if (_itemStack.Count <= count)
            {
                int temp = _itemStack.Count;
                itemStack = _itemStack;
                _itemStack = null;
                return temp;
            }

            itemStack = _itemStack.Clone(count);
            _itemStack.Spend(count);

            return count;
        }

        internal int PreTake(int count)
        {
            System.Diagnostics.Debug.Assert(count >= 0);

            if (count == 0)
            {
                return 0;
            }

            if (_itemStack == null)
            {
                return 0;
            }

            if (_itemStack.Count <= count)
            {
                return _itemStack.Count;
            }

            return count;
        }

        internal ItemStack DropSingle()
        {
            ItemStack stack = _itemStack;

            if (stack != null && stack.Count > 1)
            {
                stack.Spend(1);

                return _itemStack.Clone(1);
            }

            _itemStack = null;

            return stack;
        }

        internal ItemStack DropFull()
        {
            ItemStack stack = _itemStack;

            _itemStack = null;

            return stack;
        }

        internal void WriteData(MinecraftProtocolDataStream buffer)
        {
            System.Diagnostics.Debug.Assert(buffer != null);

            if (_itemStack == null)
            {
                buffer.WriteShort(-1);
                return;
            }

            _itemStack.WriteData(buffer);
        }

        //internal byte[] WriteData()
        //{
        //    using MinecraftProtocolDataStream buffer = new();
        //    WriteData(buffer);
        //    return buffer.ReadData();
        //}

        public override string ToString()
        {
            if (_itemStack == null)
            {
                return $"[None]";
            }

            System.Diagnostics.Debug.Assert(_itemStack != null);
            return $"[{_itemStack}]";
        }
    }
}

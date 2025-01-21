


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

        private ItemStack _stack = null;
        public ItemStack Stack => _stack;
        internal bool Empty => (_stack == null);

        public InventorySlot() { }

        /*public InventorySlot(AccessLevel level)
        {
            Level = level;
        }*/

        internal void LeftClick(InventorySlot cursor)
        {
            System.Diagnostics.Debug.Assert(cursor != null);

            ref ItemStack stackCursor = ref cursor._stack;

            if (!Empty)
            {
                if (stackCursor == null)
                {
                    stackCursor = _stack;
                    _stack = null;
                }
                else if (!_stack.Move(ref stackCursor))
                {
                    ItemStack stackTemp = stackCursor;
                    stackCursor = _stack;
                    _stack = stackTemp;
                }
            }
            else
            {
                _stack = stackCursor;
                stackCursor = null;
            }

        }

        internal void RightClick(InventorySlot cursor)
        {
            System.Diagnostics.Debug.Assert(cursor != null);

            ref ItemStack stackCursor = ref cursor._stack;

            if (stackCursor == null)
            {
                if (!Empty && !_stack.DivideHalf(ref stackCursor))
                {
                    stackCursor = _stack;
                    _stack = null;
                }
            }
            else
            {
                if (Empty)
                {
                    if (!stackCursor.DivideMinToEmpty(ref _stack))
                    {
                        _stack = stackCursor;
                        stackCursor = null;
                    }
                }
                else
                {
                    if (!_stack.DivideMinFrom(ref stackCursor))
                    {
                        ItemStack stackTemp = stackCursor;
                        stackCursor = _stack;
                        _stack = stackTemp;

                    }
                }

            }
        }

        internal void Move(InventorySlot from)
        {
            System.Diagnostics.Debug.Assert(from != null);

            if (from.Empty)
            {
                return;
            }

            ref ItemStack stackFrom = ref from._stack;

            if (Empty)
            {
                _stack = stackFrom;
                stackFrom = null;

                return;
            }

            _stack.Move(ref stackFrom);
        }

        internal void Move(ref ItemStack from)
        {
            if (from == null)
            {
                return;
            }

            if (Empty)
            {
                _stack = from;
                from = null;

                return;
            }

            _stack.Move(ref from);
        }

        internal int PreMove(ItemType type, int count)
        {
            System.Diagnostics.Debug.Assert(count >= 0);

            if (count == 0)
            {
                return 0;
            }

            System.Diagnostics.Debug.Assert(count >= ItemStack.MinCount);

            if (type != _stack.Type || _stack.IsFull() == true)
            {
                return count;
            }

            int canMovedAmount = (_stack.MaxCount - _stack.Count);

            if (count < canMovedAmount)
            {
                return 0;
            }

            return count - canMovedAmount;
        }

        internal ItemStack DropSingle()
        {
            ItemStack stack = _stack;

            if (stack != null && stack.Count > 1)
            {
                stack.Spend(1);

                return new ItemStack(stack.Type, 1);
            }

            _stack = null;

            return stack;
        }

        internal ItemStack DropFull()
        {
            ItemStack stack = _stack;

            _stack = null;

            return stack;
        }

        internal void WriteData(MinecraftDataStream buffer)
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
            using MinecraftDataStream buffer = new();
            WriteData(buffer);
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

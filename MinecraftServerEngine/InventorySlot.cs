


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

        internal void Give(ItemStack stack)
        {
            System.Diagnostics.Debug.Assert(stack != null);
            System.Diagnostics.Debug.Assert(stack.Count >= ItemStack.MinCount);

            System.Diagnostics.Debug.Assert(Empty);

            _stack = stack;
        }

        internal ItemStack Drop()
        {
            ItemStack stack = _stack;

            _stack = null;
            
            return stack;
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

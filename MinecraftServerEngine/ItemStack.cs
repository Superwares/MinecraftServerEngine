namespace MinecraftServerEngine
{
    public sealed class ItemStack
    {

        private readonly ItemType Type;
        /*private readonly string Name;*/
        private int _count;
        public int Count => _count;
        public const int MinCount = 1;
        public readonly int MaxCount;

        public ItemStack(ItemType type, int count)
        {
            Type = type;
            MaxCount = Type.GetMaxCount();

            System.Diagnostics.Debug.Assert(MaxCount >= MinCount);
            System.Diagnostics.Debug.Assert(count >= MinCount);
            System.Diagnostics.Debug.Assert(count <= MaxCount);
            _count = count;
        }

        public ItemStack(ItemType type)
        {
            Type = type;
            MaxCount = Type.GetMaxCount();

            System.Diagnostics.Debug.Assert(MaxCount >= MinCount);
            _count = MaxCount;
        }

        public int Stack(int count)
        {
            System.Diagnostics.Debug.Assert(count >= 0);

            System.Diagnostics.Debug.Assert(_count >= MinCount);
            System.Diagnostics.Debug.Assert(_count <= MaxCount);

            if (count == 0)
            {
                return 0;
            }

            int unused;
            _count += count;

            if (_count > MaxCount)
            {
                unused = _count - MaxCount;
                _count = MaxCount;
            }
            else
            {
                unused = 0;
            }

            return count - unused;  // spend
        }

        public void Spend(int count)
        {
            System.Diagnostics.Debug.Assert(count >= 0);
            System.Diagnostics.Debug.Assert(count < MaxCount);

            System.Diagnostics.Debug.Assert(_count >= MinCount);
            System.Diagnostics.Debug.Assert(_count <= MaxCount);

            if (count == 0)
            {
                return;
            }

            System.Diagnostics.Debug.Assert(count < _count);
            _count -= count;
            System.Diagnostics.Debug.Assert(_count > 0);
        }

        internal void WriteData(Buffer buffer)
        {
            System.Diagnostics.Debug.Assert(buffer != null);

            int id = Type.GetId();

            System.Diagnostics.Debug.Assert(id >= short.MinValue);
            System.Diagnostics.Debug.Assert(id <= short.MaxValue);
            buffer.WriteShort((short)id);

            System.Diagnostics.Debug.Assert(_count >= byte.MinValue);
            System.Diagnostics.Debug.Assert(_count <= byte.MaxValue);
            buffer.WriteByte((byte)_count);

            buffer.WriteShort(0);  // damage
            buffer.WriteByte(0x00);  // no NBT
        }

        public override string ToString()
        {
            /*if (_count == MinConut)
            {
                return $"{Type}";
            }*/

            return $"{Type}*{_count}";
        }

    }
}

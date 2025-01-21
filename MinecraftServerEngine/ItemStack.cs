using System;

namespace MinecraftServerEngine
{
    public sealed class ItemStack
    {

        public readonly ItemType Type;

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

        internal bool IsFull()
        {
            System.Diagnostics.Debug.Assert(_count >= MinCount);
            System.Diagnostics.Debug.Assert(_count <= MaxCount);
            return _count == MaxCount;
        }

        internal int Stack(int count)
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

            return count - unused;  // used
        }

        internal void Spend(int count)
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

        internal bool Move(ref ItemStack from)
        {
            System.Diagnostics.Debug.Assert(from != null);

            if (Type != from.Type)
            {
                return false;
            }

            int used = Stack(from.Count);
            System.Diagnostics.Debug.Assert(used >= 0);
            System.Diagnostics.Debug.Assert(used <= from.Count);

            if (used == from.Count)
            {
                from = null;
            }
            else
            {
                from.Spend(used);
            }

            return true;
        }

        internal bool DivideHalf(ref ItemStack to)
        {
            System.Diagnostics.Debug.Assert(to == null);

            if (_count == MinCount)
            {
                return false;
            }

            int a = (_count % 2);
            _count /= 2;
            int count = _count + a;

            to = new ItemStack(Type, count);

            return true;
        }

        internal bool DivideMinToEmpty(ref ItemStack to)
        {
            System.Diagnostics.Debug.Assert(to == null);

            if (_count == MinCount)
            {
                return false;
            }

            Spend(MinCount);
            to = new ItemStack(Type, MinCount);

            return true;
        }

        internal bool DivideMinFrom(ref ItemStack from)
        {
            System.Diagnostics.Debug.Assert(from != null);

            if (Type != from.Type)
            {
                return false;
            }

            int used = Stack(MinCount);
            if (used == from.Count)
            {
                from = null;
            }
            else
            {
                from.Spend(MinCount);
            }

            return true;
        }

        internal void WriteData(MinecraftDataStream buffer)
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

        internal byte[] WriteData()
        {
            using MinecraftDataStream buffer = new();
            WriteData(buffer);
            return buffer.ReadData();
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

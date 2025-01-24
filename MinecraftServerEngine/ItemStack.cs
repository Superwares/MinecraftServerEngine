
using MinecraftPrimitives;

namespace MinecraftServerEngine
{
    public sealed class ItemStack
    {

        public readonly ItemType Type;

        public readonly string Name;

        public readonly string Description;
        public readonly (string, string)[] Attributes;

        public int MaxCount => Type.GetMaxCount();
        public const int MinCount = 1;

        private int _count;
        public int Count => _count;

        public ItemStack(
            ItemType type, string name, int count,
            string description, params (string, string)[] attributes)
        {
            System.Diagnostics.Debug.Assert(name != null && string.IsNullOrEmpty(name) == false);

            Type = type;

            Name = name;

            Description = description;
            Attributes = attributes;

            System.Diagnostics.Debug.Assert(Type.GetMaxCount() >= MinCount);
            System.Diagnostics.Debug.Assert(count >= MinCount);
            System.Diagnostics.Debug.Assert(count <= Type.GetMaxCount());
            _count = count;
        }

        public ItemStack(
            ItemType type, string name, int count)
        {
            System.Diagnostics.Debug.Assert(name != null && string.IsNullOrEmpty(name) == false);

            Type = type;

            Name = name;

            Description = null;
            Attributes = null;

            System.Diagnostics.Debug.Assert(Type.GetMaxCount() >= MinCount);
            System.Diagnostics.Debug.Assert(count >= MinCount);
            System.Diagnostics.Debug.Assert(count <= Type.GetMaxCount());
            _count = count;
        }

        public ItemStack(
            ItemType type, string name,
            string description, params (string, string)[] attributes)
        {
            System.Diagnostics.Debug.Assert(name != null && string.IsNullOrEmpty(name) == false);

            Type = type;

            Name = name;

            Description = description;
            Attributes = attributes;

            System.Diagnostics.Debug.Assert(Type.GetMaxCount() >= MinCount);
            _count = Type.GetMaxCount();
        }

        public ItemStack(
       ItemType type, string name)
        {
            System.Diagnostics.Debug.Assert(name != null && string.IsNullOrEmpty(name) == false);

            Type = type;

            Name = name;

            Description = null;
            Attributes = null;

            System.Diagnostics.Debug.Assert(Type.GetMaxCount() >= MinCount);
            _count = Type.GetMaxCount();
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

            if (Type != from.Type || Name != from.Name)
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

            to = new ItemStack(Type, Name, count, Description, Attributes);

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
            to = new ItemStack(Type, Name, MinCount, Description, Attributes);

            return true;
        }

        internal bool DivideMinFrom(ref ItemStack from)
        {
            System.Diagnostics.Debug.Assert(from != null);

            if (Type != from.Type || Name != from.Name)
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

        internal void WriteData(MinecraftDataStream s)
        {
            System.Diagnostics.Debug.Assert(s != null);

            int id = Type.GetId();

            System.Diagnostics.Debug.Assert(id >= short.MinValue);
            System.Diagnostics.Debug.Assert(id <= short.MaxValue);
            s.WriteShort((short)id);

            System.Diagnostics.Debug.Assert(_count >= byte.MinValue);
            System.Diagnostics.Debug.Assert(_count <= byte.MaxValue);
            s.WriteByte((byte)_count);

            s.WriteShort(0);  // damage
            //s.WriteByte(0x00);  // no NBT

            using NBTTagCompound compound = new();
            NBTTagCompound displayCompound = new();

            displayCompound.Add("Name", new NBTTagString($"HELL3O"));

            //NBTTagList<NBTTagString> lore = new([
            //    new NBTTagString("HELLO"),
            //    new NBTTagString("DURABILITY"),
            //    ]);
            //displayCompound.Add("Lore", lore);

            if (Description != null)
            {
                int offset = 2;
                NBTTagString[] _lore = new NBTTagString[offset + Attributes.Length];
                _lore[0] = new NBTTagString(Description);
                _lore[1] = new NBTTagString("");

                int maxKeyLength = 0;
                int maxValueLength = 0;

                foreach (var (key, value) in Attributes)
                {
                    maxKeyLength = System.Math.Max(maxKeyLength, key.Length);
                    maxValueLength = System.Math.Max(maxValueLength, value.Length);
                }

                for (int i = 0; i < Attributes.Length; ++i)
                {
                    (string, string) attribute = Attributes[i];

                    _lore[i + offset] = new NBTTagString($"{attribute.Item1.PadRight(maxKeyLength + 2)}{attribute.Item2.PadLeft(maxValueLength)}");
                }

                NBTTagList<NBTTagString> lore = new(_lore);

                displayCompound.Add("Lore", lore);
            }


            compound.Add("HideFlags", new NBTTagInt(0xFF));
            compound.Add("display", displayCompound);

            compound.WriteAsRoot(s);
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

            return $"{Type}(\"{Name}\")*{_count}";
        }

    }
}


using MinecraftPrimitives;
using System.Linq;

namespace MinecraftServerEngine
{
    public sealed class ItemStack : Item
    {

        private int _count;
        public int Count => _count;


        public ItemStack(
            ItemType type, string name, int count,
            int maxDurability, int currentDurability,
            //string description, params (string, string)[] attributes,
            params string[] lore)
            : base(type, name, maxDurability, currentDurability, lore)
        {
            System.Diagnostics.Debug.Assert(count >= type.GetMinStackCount());
            System.Diagnostics.Debug.Assert(count <= type.GetMaxStackCount());
            _count = count;
        }

        public ItemStack(
            ItemType type, string name, int count,
            //string description, params (string, string)[] attributes,
            params string[] lore)
            : this(type, name, count, 0, 0, lore)
        {

        }

        public ItemStack(
            ItemType type, string name,
            int maxDurability, int currentDurability,
            //string description, params (string, string)[] attributes,
            params string[] lore)
            : this(type, name, type.GetMaxStackCount(), maxDurability, currentDurability, lore)
        {

        }

        public ItemStack(
            ItemType type, string name,
            //string description, params (string, string)[] attributes,
            params string[] lore)
            : this(type, name, type.GetMaxStackCount(), 0, 0, lore)
        {

        }

        public static ItemStack Create(
            IReadOnlyItem item, int count,
            params string[] additionalLore)
        {
            string[] lore = item.Lore.Concat(additionalLore).ToArray();

            return new ItemStack(
                item.Type, item.Name, count,
                item.MaxDurability, item.CurrentDurability,
                lore);
        }

        public static ItemStack Create(
            IReadOnlyItem item,
            params string[] additionalLore)
        {
            string[] lore = item.Lore.Concat(additionalLore).ToArray();

            return new ItemStack(
                item.Type, item.Name,
                item.MaxDurability, item.CurrentDurability,
                lore);
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

            if (AreByteArraysEqual(Hash, from.Hash) == false)
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

            to = Clone(count);

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
            to = Clone(MinCount);

            return true;
        }

        internal bool DivideMinFrom(ref ItemStack from)
        {
            System.Diagnostics.Debug.Assert(from != null);

            if (AreByteArraysEqual(Hash, from.Hash) == false)
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

        public ItemStack Clone(int count)
        {
            return new ItemStack(
                Type, Name, count,
                MaxDurability, CurrentDurability,
                Lore);
        }

        public ItemStack Clone()
        {
            return Clone(Count);
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

            displayCompound.Add("Name", new NBTTagString(Name));

            int currentLoreLine = 0;
            int loreLines = 0;

            bool displayDurability = MaxDurability > 1;

            const bool EndBr = true;

            if (Lore != null)
            {
                loreLines += Lore.Length;
            }

            if (displayDurability)
            {
                loreLines += 2;
            }

            bool StartBr = loreLines > 0;

            if (StartBr == true)
            {
                loreLines++;
            }

            if (EndBr == true)
            {
                loreLines++;
            }

            NBTTagString[] _lore = new NBTTagString[loreLines];

            if (StartBr == true)
            {
                _lore[currentLoreLine++] = new NBTTagString("");
            }

            if (Lore != null)
            {
                for (int i = 0; i < Lore.Length; ++i)
                {
                    _lore[currentLoreLine++] = new NBTTagString(Lore[i]);
                }
            }

            if (displayDurability)
            {
                _lore[currentLoreLine++] = new NBTTagString("");
                _lore[currentLoreLine++] = new NBTTagString($"Durability [{CurrentDurability}/{MaxDurability}]");
            }

            if (EndBr == true)
            {
                _lore[currentLoreLine++] = new NBTTagString("");
            }

            NBTTagList<NBTTagString> lore = new(_lore);
            displayCompound.Add("Lore", lore);

            //NBTTagList<NBTTagString> lore = new([
            //    new NBTTagString("HELLO"),
            //    new NBTTagString("DURABILITY"),
            //    ]);
            //displayCompound.Add("Lore", lore);

            //if (Description != null)
            //{


            //    int minWidth = 20;
            //    int maxKeyLength = 0;
            //    int maxValueLength = 0;

            //    foreach (var (key, value) in Attributes)
            //    {
            //        maxKeyLength = System.Math.Max(maxKeyLength, key.Length);
            //        maxValueLength = System.Math.Max(maxValueLength, value.Length);
            //    }

            //    if (maxKeyLength + maxValueLength < minWidth)
            //    {
            //        maxKeyLength += minWidth - (maxKeyLength + maxValueLength);
            //    }

            //    int width = maxKeyLength + maxValueLength;

            //    int minLines = (int)System.Math.Ceiling((double)Description.Length / (double)width);

            //    int br = 1;

            //    string line;

            //    int startIndex;
            //    int currentIndex = 0;
            //    NBTTagString[] _lore = new NBTTagString[minLines + br + Attributes.Length];

            //    for (int i = 0; i < minLines; ++i)
            //    {
            //        startIndex = i * width;
            //        int length = System.Math.Min(width, Description.Length - startIndex);
            //        line = Description.Substring(startIndex, length).Replace(" ", "  ");

            //        _lore[currentIndex++] = new NBTTagString(line);
            //    }

            //    for (int i = 0; i < br; ++i)
            //    {
            //        _lore[currentIndex++] = new NBTTagString("");
            //    }

            //    for (int i = 0; i < Attributes.Length; ++i)
            //    {
            //        (string, string) attribute = Attributes[i];

            //        line = $"{attribute.Item1.PadRight(maxKeyLength)}{attribute.Item2.PadLeft(maxValueLength - 2)}".Replace(" ", "  ");
            //        _lore[currentIndex++] = new NBTTagString(line);
            //    }

            //    NBTTagList<NBTTagString> lore = new(_lore);

            //    displayCompound.Add("Lore", lore);
            //}


            compound.Add("HideFlags", new NBTTagInt(0xFF));
            compound.Add("Unbreakable", new NBTTagInt(0x01));
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

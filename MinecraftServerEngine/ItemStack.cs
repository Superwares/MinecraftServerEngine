
using MinecraftPrimitives;
using System.Diagnostics;

namespace MinecraftServerEngine
{
    public sealed class ItemStack : System.IEquatable<ItemStack>
    {

        public readonly ItemType Type;

        public readonly string Name;

        //public readonly string Description;
        //public readonly (string, string)[] Attributes;
        public readonly string[] Lore;

        public int MaxCount => Type.GetMaxStackCount();
        public int MinCount => Type.GetMinStackCount();

        private int _count;
        public int Count => _count;


        public readonly int MaxDurability;
        public int _currentDurability;
        public int CurrentDurability => _currentDurability;
        public bool IsBreakable => MaxDurability > 0;
        public bool IsBreaked
        {
            get
            {
                System.Diagnostics.Debug.Assert(MaxDurability >= 0);
                System.Diagnostics.Debug.Assert(CurrentDurability >= 0);
                System.Diagnostics.Debug.Assert(MaxDurability >= CurrentDurability);

                return MaxDurability > 0 && CurrentDurability == 0;
            }
        }



        internal readonly byte[] Hash;

        // TODO: Check  additional validation or safeguards should be implemented.
        private static byte[] GenerateHash(string input)
        {
            using (System.Security.Cryptography.SHA256 sha256 = System.Security.Cryptography.SHA256.Create())
            {
                byte[] inputBytes = System.Text.Encoding.UTF8.GetBytes(input);
                byte[] hashBytes = sha256.ComputeHash(inputBytes);

                // Convert byte array to a hexadecimal string
                //StringBuilder sb = new StringBuilder();
                //foreach (byte b in hashBytes)
                //{
                //    sb.Append(b.ToString("x2"));
                //}
                //return sb.ToString();

                return hashBytes;
            }
        }

        private byte[] GenerateItemStackHash(
            ItemType type, string name,
            int maxDurability, int currentDurability,
            string[] lore)
        {

            string hashString = $"{type.ToString()}_{name}_";


            System.Diagnostics.Debug.Assert(maxDurability >= 0);
            System.Diagnostics.Debug.Assert(maxDurability >= currentDurability);
            if (maxDurability > 0)
            {
                hashString += $"Breakable_{maxDurability.ToString()}_{currentDurability.ToString()}";
            }

            hashString += "@";

            foreach (string line in lore)
            {
                hashString += line + "\n";
            }

            return GenerateHash(hashString);
        }

        private static bool AreByteArraysEqual(byte[] array1, byte[] array2)
        {
            if (array1 == null && array2 == null)
            {
                return true;
            }

            // Check if the references are the same (or both are null)
            if (ReferenceEquals(array1, array2))
            {
                return true;
            }

            // If either is null or lengths are different, return false
            if (array1 == null || array2 == null || array1.Length != array2.Length)
            {
                return false;
            }

            // Compare each element
            for (int i = 0; i < array1.Length; i++)
            {
                if (array1[i] != array2[i])
                {
                    return false;
                }
            }

            //array1.SequenceEqual(array2)

            return true;
        }

        public ItemStack(
            ItemType type, string name, int count,
            int maxDurability, int currentDurability,
            //string description, params (string, string)[] attributes,
            params string[] lore)
        {
            System.Diagnostics.Debug.Assert(type.GetMaxStackCount() >= type.GetMinStackCount());
            //if (breakable == true && type.GetMaxStackCount() > 1)
            //{
            //    throw new System.ArgumentException(
            //        "The item is breakable and cannot have a maximum stack count greater than 1.",
            //        nameof(breakable));
            //}

            if (maxDurability < 0)
            {
                throw new System.ArgumentOutOfRangeException(
                    nameof(maxDurability),
                    "Max durability must be greater than or equal to 0.");
            }

            if (currentDurability < 0 || maxDurability < currentDurability)
            {
                throw new System.ArgumentOutOfRangeException(
                    nameof(currentDurability),
                    "Current durability must be greater than 0 and less than or equal to max durability.");
            }

            System.Diagnostics.Debug.Assert(name != null && string.IsNullOrEmpty(name) == false);

            Type = type;

            Name = name;


            System.Diagnostics.Debug.Assert(count >= type.GetMinStackCount());
            System.Diagnostics.Debug.Assert(count <= type.GetMaxStackCount());
            _count = count;

            MaxDurability = maxDurability;
            _currentDurability = currentDurability;

            //Description = description;
            //Attributes = attributes;
            Lore = lore;

            Hash = GenerateItemStackHash(type, name, maxDurability, currentDurability, lore);
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
          bool breakable, int maxDurability, int currentDurability,
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

        internal bool CheckHash(byte[] hash)
        {
            if (hash == null)
            {
                System.Diagnostics.Debug.Assert(Hash != null);
                return false;
            }

            return AreByteArraysEqual(Hash, hash);
        }

        internal bool IsFull()
        {
            System.Diagnostics.Debug.Assert(_count >= MinCount);
            System.Diagnostics.Debug.Assert(_count <= MaxCount);
            return _count == MaxCount;
        }

        public void Damage(int amount)
        {
            if (_currentDurability - amount <= 0)
            {
                _currentDurability = 0;
            }
            else
            {
                _currentDurability -= amount;
            }

            System.Diagnostics.Debug.Assert(_currentDurability >= 0);
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

            const bool EndBr = true;

            if (Lore != null)
            {
                loreLines += Lore.Length;
            }

            if (IsBreakable == true)
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

            if (IsBreakable == true)
            {
                _lore[currentLoreLine++] = new NBTTagString("");
                _lore[currentLoreLine++] = new NBTTagString($"Durability ({CurrentDurability}/{MaxDurability})");
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

        public bool Equals(ItemStack other)
        {
            if (other == null)
            {
                return false;
            }

            return AreByteArraysEqual(other.Hash, Hash);
        }

        public override bool Equals(object obj)
        {
            if (obj is ItemStack other)
            {
                return this.Equals(other);
            }

            return false;
        }

        public override int GetHashCode()
        {
            System.Diagnostics.Debug.Assert(Hash != null);
            if (Hash.Length == 0)
            {
                return 0;
            }

            unchecked
            {
                int hash = 17;
                foreach (byte b in Hash)
                {
                    hash = hash * 31 + b;
                }
                return hash;
            }
        }

    }
}

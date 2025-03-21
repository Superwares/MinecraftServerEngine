﻿using MinecraftServerEngine.NBT;
using MinecraftServerEngine.Protocols;

namespace MinecraftServerEngine.Items
{
    public sealed class ItemStack : Item
    {

        private int _count;
        public int Count
        {
            get
            {
                System.Diagnostics.Debug.Assert(_count >= MinCount);
                return _count;
            }
        }


        public int RemainingCount
        {
            get
            {
                System.Diagnostics.Debug.Assert(_count >= MinCount);
                System.Diagnostics.Debug.Assert(_count <= MaxCount);
                return MaxCount - _count;
            }
        }

        public bool IsFull
        {
            get
            {
                System.Diagnostics.Debug.Assert(_count >= MinCount);
                System.Diagnostics.Debug.Assert(_count <= MaxCount);
                return _count == MaxCount;
            }
        }

        public ItemStack(
            ItemType type, string name, int count,
            int maxDurability, int currentDurability,
            params string[] lore)
            : base(type, name, maxDurability, currentDurability, lore)
        {
            System.Diagnostics.Debug.Assert(count >= MinCount);
            System.Diagnostics.Debug.Assert(count <= MaxCount);
            _count = count;
        }

        public ItemStack(
            ItemType type, string name, int count,
            params string[] lore)
            : this(type, name, count, 0, 0, lore)
        {

        }

        public ItemStack(
            ItemType type, string name,
            int maxDurability, int currentDurability,
            params string[] lore)
            : this(type, name, MinCount, maxDurability, currentDurability, lore)
        {

        }

        public ItemStack(
            ItemType type, string name,
            params string[] lore)
            : this(type, name, MinCount, 0, 0, lore)
        {

        }

        public static ItemStack Create(
            IReadOnlyItem item, int count,
            params string[] additionalLore)
        {
            if (additionalLore == null)
            {
                additionalLore = [];
            }

            string[] lore = System.Linq.Enumerable.ToArray(System.Linq.Enumerable.Concat(item.Lore, additionalLore));

            return new ItemStack(
                item.Type, item.Name, count,
                item.MaxDurability, item.CurrentDurability,
                lore);
        }

        public static ItemStack Create(
            IReadOnlyItem item,
            params string[] additionalLore)
        {
            string[] lore = System.Linq.Enumerable.ToArray(
                System.Linq.Enumerable.Concat(item.Lore, additionalLore)
                );

            return new ItemStack(
                item.Type, item.Name,
                item.MaxDurability, item.CurrentDurability,
                lore);
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

        internal int Stack2(int count)
        {
            System.Diagnostics.Debug.Assert(count >= 0);

            System.Diagnostics.Debug.Assert(_count >= MinCount);
            System.Diagnostics.Debug.Assert(_count <= MaxCount);

            if (count == 0)
            {
                return 0;
            }

            int remaning;
            _count += count;

            if (_count > MaxCount)
            {
                remaning = _count - MaxCount;
                _count = MaxCount;
            }
            else
            {
                remaning = 0;
            }

            return remaning;  // used
        }

        internal int PreStack(int count)
        {
            System.Diagnostics.Debug.Assert(count >= 0);

            System.Diagnostics.Debug.Assert(_count >= MinCount);
            System.Diagnostics.Debug.Assert(_count <= MaxCount);

            if (count == 0)
            {
                return 0;
            }

            int remaning;
            int preCount = _count + count;

            if (preCount > MaxCount)
            {
                remaning = preCount - MaxCount;
                //preCount = MaxCount;
            }
            else
            {
                remaning = 0;
            }

            return remaning;
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fromItemStack"></param>
        /// <returns>isMoved</returns>
        internal bool Move(ref ItemStack fromItemStack)
        {
            System.Diagnostics.Debug.Assert(fromItemStack != null);

            if (AreByteArraysEqual(Hash, fromItemStack.Hash) == false)
            {
                return false;
            }

            int remaning = Stack2(fromItemStack.Count);
            System.Diagnostics.Debug.Assert(remaning >= 0);
            System.Diagnostics.Debug.Assert(remaning <= fromItemStack.Count);

            if (remaning == 0)
            {
                fromItemStack = null;
            }
            else
            {
                fromItemStack.Spend(fromItemStack.Count - remaning);
            }

            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fromItemStack"></param>
        /// <returns>isAllMoved</returns>
        internal bool MoveAll(ref ItemStack fromItemStack)
        {
            System.Diagnostics.Debug.Assert(fromItemStack != null);

            if (AreByteArraysEqual(Hash, fromItemStack.Hash) == false)
            {
                return false;
            }

            int remaning = Stack2(fromItemStack.Count);
            System.Diagnostics.Debug.Assert(remaning >= 0);
            System.Diagnostics.Debug.Assert(remaning <= fromItemStack.Count);

            if (remaning == 0)
            {
                fromItemStack = null;

                return true;
            }
            else
            {
                fromItemStack.Spend(fromItemStack.Count - remaning);

                return false;
            }

        }

        internal int Move(IReadOnlyItem fromItem, int count)
        {
            System.Diagnostics.Debug.Assert(count >= 0);

            if (count == 0)
            {
                return 0;
            }

            if (AreByteArraysEqual(Hash, fromItem.Hash) == false)
            {
                return count;
            }

            if (IsFull == true)
            {
                return count;
            }


            return Stack2(count);
        }

        internal int PreMove(IReadOnlyItem fromItem, int count)
        {
            System.Diagnostics.Debug.Assert(count >= 0);

            if (count == 0)
            {
                return 0;
            }

            if (AreByteArraysEqual(Hash, fromItem.Hash) == false)
            {
                return count;
            }

            if (IsFull == true)
            {
                return count;
            }

            return PreStack(count);  // remaning
        }

        internal bool DivideHalf(ref ItemStack to)
        {
            System.Diagnostics.Debug.Assert(to == null);

            if (_count == MinCount)
            {
                return false;
            }

            int a = _count % 2;
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
            if (count < MinCount || count > MaxCount)
            {
                throw new System.ArgumentOutOfRangeException(nameof(count));
            }

            return new ItemStack(
                Type, Name, count,
                MaxDurability, CurrentDurability,
                Lore);
        }

        public ItemStack Clone()
        {
            System.Diagnostics.Debug.Assert(Count >= MinCount);
            System.Diagnostics.Debug.Assert(Count <= MaxCount);
            return Clone(Count);
        }

        internal void WriteData(MinecraftProtocolDataStream s)
        {
            System.Diagnostics.Debug.Assert(s != null);

            int id = Type.GetId();

            System.Diagnostics.Debug.Assert(id >= short.MinValue);
            System.Diagnostics.Debug.Assert(id <= short.MaxValue);
            s.WriteShort((short)id);

            System.Diagnostics.Debug.Assert(_count >= byte.MinValue);
            System.Diagnostics.Debug.Assert(_count <= byte.MaxValue);
            s.WriteByte((byte)_count);

            int metadata = Type.GetMetadata();

            System.Diagnostics.Debug.Assert(metadata >= short.MinValue);
            System.Diagnostics.Debug.Assert(metadata <= short.MaxValue);
            s.WriteShort((short)metadata);  // can be damage
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

            if (Type == ItemType.PlayerSkull)
            {
                compound.Add("SkullOwner", new NBTTagString(Name));
            }

            compound.Add("HideFlags", new NBTTagInt(0xFF));
            compound.Add("Unbreakable", new NBTTagInt(0x01));
            compound.Add("display", displayCompound);

            compound.WriteAsRoot(s);
        }

        internal byte[] WriteData()
        {
            using MinecraftProtocolDataStream buffer = new();
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

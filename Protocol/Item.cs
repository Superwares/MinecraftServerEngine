
using System;
using System.Diagnostics;
using System.Threading;

// item: type(code?), name(#code/number?), NBT?, count, maxCount,,,

namespace Protocol
{
    public sealed class Item : IEquatable<Item>
    {
        public enum Types : uint
        {
            Stone = 1,
            Grass = 2,
            Dirt = 3,
            Cobbestone = 4,
            planks = 5,

            chest = 54,

            IronSword = 267,

            WoodenSword = 268,

            StoneSword = 272,

            DiamondSword = 276,

            GoldenSword = 283,

            LeatherHelmet = 298,
            LeatherChestPlate = 299,
            LeatherLeggins = 300,
            LeatherBoots = 301,

            ChainmailHelmet = 302,
            ChainmailChestPlate = 303,
            ChainmailLeggins = 304,
            ChainmailBoots = 305,

            IronHelmet = 306,
            IronChestPlate = 307,
            IronLeggins = 308,
            IronBoots = 309,

            DiamondHelmet = 310,
            DiamondChestPlate = 311,
            DiamondLeggins = 312,
            DiamondBoots = 313,

            GoldenHelmet = 314,
            GoldenChestPlate = 315,
            GoldenLeggins = 316,
            GoldenBoots = 317,

            Shield = 442,
        }

        private readonly Types _type;
        public Types Type => _type;
        
        private static bool IsArmorType(Types type)
        {
            // IsHelmet || IsChestPlate || IsLeggins || IsBoots = true
            // else false

            switch (type)
            {
                default:
                    throw new System.NotImplementedException();
                case Types.Stone:
                    return false;
                case Types.Grass:
                    return false;
                case Types.Dirt:
                    return false;
                case Types.Cobbestone:
                    return false;
                case Types.planks:
                    return false;

                case Types.chest:
                    return false;

                case Types.IronSword:
                    return false;
                case Types.WoodenSword:
                    return false;
                case Types.StoneSword:
                    return false;
                case Types.DiamondSword:
                    return false;
                case Types.GoldenSword:
                    return false;

                case Types.LeatherHelmet:
                case Types.ChainmailHelmet:
                case Types.IronHelmet:
                case Types.DiamondHelmet:
                case Types.GoldenHelmet:
                    return true;

                case Types.DiamondChestPlate:
                case Types.GoldenChestPlate:
                case Types.IronChestPlate:
                case Types.ChainmailChestPlate:
                case Types.LeatherChestPlate:
                    return true;

                case Types.DiamondLeggins:
                case Types.GoldenLeggins:
                case Types.IronLeggins:
                case Types.ChainmailLeggins:
                case Types.LeatherLeggins:
                    return true;

                case Types.DiamondBoots:
                case Types.GoldenBoots:
                case Types.IronBoots:
                case Types.ChainmailBoots:
                case Types.LeatherBoots:
                    return true;

                case Types.Shield:
                    return false;
            }
        }
        public bool IsArmor => IsArmorType(_type);

        private static bool IsShieldType(Types type)
        {
            switch (type)
            {
                default:
                    return false;
                case Types.Shield:
                    return true;
            }
        }

        public bool IsShield => IsShieldType(_type);

        private static bool IsHelmetType(Types type)
        {
            switch (type)
            {
                default:
                    return false;
                case Types.DiamondHelmet:
                case Types.GoldenHelmet:
                case Types.IronHelmet:
                case Types.ChainmailHelmet:
                case Types.LeatherHelmet:
                    return true;
            }
        }

        public bool IsHelmet => IsHelmetType(_type);

        private static bool IsChestPlateType(Types type)
        {
            switch (type)
            {
                default:
                    return false;
                case Types.DiamondChestPlate:
                case Types.GoldenChestPlate:
                case Types.IronChestPlate:
                case Types.ChainmailChestPlate:
                case Types.LeatherChestPlate:
                    return true;
            }
        }

        public bool IsChestPlate => IsChestPlateType(_type);

        private static bool IsLegginsType(Types type)
        {
            switch (type)
            {
                default:
                    return false;
                case Types.DiamondLeggins:
                case Types.GoldenLeggins:
                case Types.IronLeggins:
                case Types.ChainmailLeggins:
                case Types.LeatherLeggins:
                    return true;
            }
        }

        public bool IsLeggins => IsLegginsType(_type);

        private static bool IsBootsType(Types type)
        {
            switch (type)
            {
                default:
                    return false;
                case Types.DiamondBoots:
                case Types.GoldenBoots:
                case Types.IronBoots:
                case Types.ChainmailBoots:
                case Types.LeatherBoots:
                    return true;
            }
        }

        public bool IsBoots => IsBootsType(_type);


        public const int MAX_COUNT = 1;
        public int MinCount => MAX_COUNT;

        private static int GetItemMaxCountByType(Types type)
        {
            switch (type)
            {
                default:
                    throw new System.NotImplementedException();
                case Types.Stone:
                    return 64;
                case Types.Grass:
                    return 64;
                case Types.Dirt:
                    return 64;
                case Types.Cobbestone:
                    return 64;
                case Types.planks:
                    return 64;

                case Types.chest:
                    return 64;

                case Types.IronSword:
                    return 1;
                case Types.WoodenSword:
                    return 1;

                case Types.StoneSword:
                    return 1;

                case Types.DiamondSword:
                    return 1;

                case Types.GoldenSword:
                    return 1;

                case Types.LeatherHelmet:
                case Types.ChainmailHelmet:
                case Types.IronHelmet:
                case Types.DiamondHelmet:
                case Types.GoldenHelmet:
                    return 1;

                case Types.DiamondChestPlate:
                case Types.GoldenChestPlate:
                case Types.IronChestPlate:
                case Types.ChainmailChestPlate:
                case Types.LeatherChestPlate:
                    return 1;

                case Types.DiamondLeggins:
                case Types.GoldenLeggins:
                case Types.IronLeggins:
                case Types.ChainmailLeggins:
                case Types.LeatherLeggins:
                    return 1;

                case Types.DiamondBoots:
                case Types.GoldenBoots:
                case Types.IronBoots:
                case Types.ChainmailBoots:
                case Types.LeatherBoots:
                    return 1;

                case Types.Shield:
                    return 1;

            }
        }
        public int MaxCount => GetItemMaxCountByType(_type);

        private int _count;
        public int Count
        {
            get
            {
                Debug.Assert(_count <= MaxCount);
                Debug.Assert(_count >= MinCount);
                return _count;
            }
        }

        public Item(Types type, int count)
        {
            _type = type;
            _count = count;

            Debug.Assert(MaxCount >= MinCount);
            Debug.Assert(count <= MaxCount);
            Debug.Assert(count >= MinCount);
        }

        internal int Stack(int count)
        {
            System.Diagnostics.Debug.Assert(_count >= MinCount);
            System.Diagnostics.Debug.Assert(_count <= MaxCount);
            System.Diagnostics.Debug.Assert(count >= MinCount);
            System.Diagnostics.Debug.Assert(count <= MaxCount);

            int rest;
            _count += count;

            if (_count > MaxCount)
            {
                rest = _count - MaxCount;
                _count = MaxCount;
            }
            else
            {
                rest = 0;
            }

            return count - rest;  // spend
        }

        internal void Spend(int count)
        {
            System.Diagnostics.Debug.Assert(_count >= MinCount);
            System.Diagnostics.Debug.Assert(_count <= MaxCount);

            if (count == 0)
                return;

            System.Diagnostics.Debug.Assert(count >= MinCount);
            System.Diagnostics.Debug.Assert(count <= MaxCount);

            System.Diagnostics.Debug.Assert(count < _count);

            _count -= count;
            System.Diagnostics.Debug.Assert(_count > 0);
        }

        internal Item DivideHalf()
        {
            System.Diagnostics.Debug.Assert(_count >= MinCount);
            System.Diagnostics.Debug.Assert(_count <= MaxCount);

            int count = (_count / 2) + (_count % 2);
            _count /= 2;

            System.Diagnostics.Debug.Assert(count >= MinCount);
            System.Diagnostics.Debug.Assert(count <= MaxCount);
            System.Diagnostics.Debug.Assert(_count >= MinCount);
            System.Diagnostics.Debug.Assert(_count <= MaxCount);

            return new Item(_type, count);
        }

        internal Item DivideExceptOne()
        {
            System.Diagnostics.Debug.Assert(_count >= MinCount);
            System.Diagnostics.Debug.Assert(_count <= MaxCount);

            int count = _count - 1;
            _count = 1;

            System.Diagnostics.Debug.Assert(count >= MinCount);
            System.Diagnostics.Debug.Assert(_count >= MinCount);

            return new Item(_type, count);
        }

        internal void SetCount(int count)
        {
            System.Diagnostics.Debug.Assert(_count >= MinCount);
            System.Diagnostics.Debug.Assert(_count <= MaxCount);
            System.Diagnostics.Debug.Assert(count >= MinCount);
            System.Diagnostics.Debug.Assert(count <= MaxCount);

            _count = count;
        }

        internal SlotData ConvertToPacketFormat()
        {
            System.Diagnostics.Debug.Assert(_count >= MinCount);
            System.Diagnostics.Debug.Assert(_count <= MaxCount);

            System.Diagnostics.Debug.Assert((int)_type >= short.MinValue);
            System.Diagnostics.Debug.Assert((int)_type <= short.MaxValue);
            System.Diagnostics.Debug.Assert(_count >= byte.MinValue);
            System.Diagnostics.Debug.Assert(_count <= byte.MaxValue);

            return new((short)_type, (byte)_count);
        }

        internal bool CompareWithPacketFormat(SlotData slotData)
        {
            System.Diagnostics.Debug.Assert(_count >= MinCount);
            System.Diagnostics.Debug.Assert(_count <= MaxCount);

            if (slotData.Id == -1)
                return false;

            if (slotData.Id != (int)_type)
                return false;

            if (slotData.Count != _count)
                return false;

            return true;
        }

        public bool Equals(Item? other)
        {
            if (other == null)
            {
                return false;
            }

            System.Diagnostics.Debug.Assert(_count >= MinCount);
            System.Diagnostics.Debug.Assert(_count <= MaxCount);

            return (_type == other._type) && (_count == other._count);
        }
    }


}


using System;
using System.Diagnostics;
using System.Threading;

namespace Protocol
{
    public sealed class Item : IEquatable<Item>
    {
        public enum Types : int
        {
            Stone = 1,
            Grass = 2,
            Dirt = 3,
            Cobbestone = 4,

            IronSword = 267,
            WoodenSword = 268,

            StoneSword = 272,

            DiamondSword = 276,

            GoldenSword = 283,

            LeatherHelmet = 298,

            ChainmailHelmet = 302,

            IronHelmet = 306,

            DiamondHelmet = 310,

            GoldenHelmet = 314,
        }

        private readonly Types _type;
        public Types TYPE => _type;

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
                    return 1;

                case Types.ChainmailHelmet:
                    return 1;

                case Types.IronHelmet:
                    return 1;

                case Types.DiamondHelmet:
                    return 1;

                case Types.GoldenHelmet:
                    return 1;


            }
        }

        private static bool IsArmorType(Types type)
        {
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
                    return true;

                case Types.ChainmailHelmet:
                    return true;

                case Types.IronHelmet:
                    return true;

                case Types.DiamondHelmet:
                    return true;

                case Types.GoldenHelmet:
                    return true;
            }
        }

        public bool IsArmor() => IsArmorType(_type);

        public const int MIN_COUNT = 1;
        public int MAX_COUNT => GetItemMaxCountByType(_type);

        private int _count;
        public int Count
        {
            get
            {
                Debug.Assert(_count <= MAX_COUNT);
                Debug.Assert(_count >= MIN_COUNT);
                return _count;
            }
        }

        public Item(Types type, int count)
        {
            _type = type;
            _count = count;

            Debug.Assert(MAX_COUNT >= MIN_COUNT);
            Debug.Assert(count <= MAX_COUNT);
            Debug.Assert(count >= MIN_COUNT);
        }

        internal int Stack(int count)
        {
            System.Diagnostics.Debug.Assert(_count >= MIN_COUNT);
            System.Diagnostics.Debug.Assert(_count <= MAX_COUNT);
            System.Diagnostics.Debug.Assert(count >= MIN_COUNT);
            System.Diagnostics.Debug.Assert(count <= MAX_COUNT);

            int rest;
            _count += count;

            if (_count > MAX_COUNT)
            {
                rest = _count - MAX_COUNT;
                _count = MAX_COUNT;
            }
            else
            {
                rest = 0;
            }

            return count - rest;  // spend
        }

        internal void Spend(int count)
        {
            System.Diagnostics.Debug.Assert(_count >= MIN_COUNT);
            System.Diagnostics.Debug.Assert(_count <= MAX_COUNT);

            if (count == 0)
                return;

            System.Diagnostics.Debug.Assert(count >= MIN_COUNT);
            System.Diagnostics.Debug.Assert(count <= MAX_COUNT);

            System.Diagnostics.Debug.Assert(count < _count);

            _count -= count;
            System.Diagnostics.Debug.Assert(_count > 0);
        }

        internal Item DivideHalf()
        {
            System.Diagnostics.Debug.Assert(_count >= MIN_COUNT);
            System.Diagnostics.Debug.Assert(_count <= MAX_COUNT);

            int count = (_count / 2) + (_count % 2);
            _count = (_count / 2);
            System.Diagnostics.Debug.Assert(_count % 2 == 0);

            System.Diagnostics.Debug.Assert(count >= MIN_COUNT);
            System.Diagnostics.Debug.Assert(count <= MAX_COUNT);
            System.Diagnostics.Debug.Assert(_count >= MIN_COUNT);
            System.Diagnostics.Debug.Assert(_count <= MAX_COUNT);

            return new Item(_type, count);
        }

        internal Item DivideOne()
        {
            System.Diagnostics.Debug.Assert(_count >= MIN_COUNT);
            System.Diagnostics.Debug.Assert(_count <= MAX_COUNT);

            _count--;

            System.Diagnostics.Debug.Assert(_count >= MIN_COUNT);

            return new Item(_type, 1);
        }

        internal void SetCount(int count)
        {
            System.Diagnostics.Debug.Assert(_count >= MIN_COUNT);
            System.Diagnostics.Debug.Assert(_count <= MAX_COUNT);
            System.Diagnostics.Debug.Assert(count >= MIN_COUNT);
            System.Diagnostics.Debug.Assert(count <= MAX_COUNT);

            _count = count;
        }

        internal SlotData ConventToPacketFormat()
        {
            System.Diagnostics.Debug.Assert(_count >= MIN_COUNT);
            System.Diagnostics.Debug.Assert(_count <= MAX_COUNT);

            System.Diagnostics.Debug.Assert((int)_type >= short.MinValue);
            System.Diagnostics.Debug.Assert((int)_type <= short.MaxValue);
            System.Diagnostics.Debug.Assert(_count >= byte.MinValue);
            System.Diagnostics.Debug.Assert(_count <= byte.MaxValue);

            return new((short)_type, (byte)_count);
        }

        internal bool CompareWithPacketFormat(SlotData slotData)
        {
            System.Diagnostics.Debug.Assert(_count >= MIN_COUNT);
            System.Diagnostics.Debug.Assert(_count <= MAX_COUNT);

            if (slotData.Id == -1)
                return false;

            if (slotData.Id != (int)_type)
                return false;

            if (slotData.Count != _count)
                return false;

            return true;
        }

        public bool Equals(Item other)
        {
            System.Diagnostics.Debug.Assert(_count >= MIN_COUNT);
            System.Diagnostics.Debug.Assert(_count <= MAX_COUNT);

            if (_type != other._type) return false;
            if (Count != other.Count) return false;

            return true;
        }
    }


}



namespace MinecraftServerEngine
{
    public enum Items : int
    {
        IronSword,
        WoodenSword,

        StoneSword,

        DiamondSword,

        GoldenSword,

        LeatherHelmet,

        ChainmailHelmet,

        IronHelmet,

        DiamondHelmet,

        GoldenHelmet,

        Stick,
        
        Snowball,
    }

    /*public readonly struct Item : System.IEquatable<Item>
    {
        public enum Types : int
        {
            Stone,
            Grass,
            Dirt,
            Cobbestone,

            IronSword,
            WoodenSword,

            StoneSword,

            DiamondSword,

            GoldenSword,

            LeatherHelmet,

            ChainmailHelmet,

            IronHelmet,

            DiamondHelmet,

            GoldenHelmet,
        }

        private readonly Types _TYPE;
        public Types Type => _TYPE;

        private readonly int _COUNT;
        public int Count => _COUNT;

        public Item(Types type, int count)
        {
            _TYPE = type;
            _COUNT = count;
        }

        public readonly override string ToString()
        {
            return $"( Type: {_TYPE}, Count: {_COUNT} )";
        }

        public static bool operator ==(Item left, Item right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Item left, Item right)
        {
            return !left.Equals(right);
        }

        public readonly bool Equals(Item other)
        {
            return (_TYPE == other._TYPE) && (_COUNT == other._COUNT);
        }

        public readonly override bool Equals(object obj)
        {
            return (obj is Item other) && Equals(other);
        }

        public readonly override int GetHashCode()
        {
            return base.GetHashCode();
        }

    }*/

    /* public sealed class Item : System.IEquatable<Item>
    {
        public enum Types : uint
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
        public Types Type => _type;
        
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
        public bool IsArmor => IsArmorType(_type);

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

        internal SlotData ConventToPacketFormat()
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
    }*/


}


using Containers;

namespace MinecraftServerEngine
{
    public static class ItemExtensions
    {
        // TODO: Replace as IReadOnlyTable
        // TODO: 프로그램이 종료되었을 때 자원 해제하기. static destructor?
        private readonly static Table<ItemType, int> _ITEM_ENUM_TO_ID_MAP = new();
        private readonly static Table<int, ItemType> _ITEM_ID_TO_ENUM_MAP = new();

        static ItemExtensions()
        {
            _ITEM_ENUM_TO_ID_MAP.Insert(ItemType.IronSword, 267);
            _ITEM_ENUM_TO_ID_MAP.Insert(ItemType.WoodenSword, 268);

            _ITEM_ENUM_TO_ID_MAP.Insert(ItemType.StoneSword, 272);

            _ITEM_ENUM_TO_ID_MAP.Insert(ItemType.DiamondSword, 276);

            _ITEM_ENUM_TO_ID_MAP.Insert(ItemType.GoldenSword, 283);

            _ITEM_ENUM_TO_ID_MAP.Insert(ItemType.Stick, 280);

            /*_ITEM_ENUM_TO_ID_MAP.Insert(ItemType.Snowball, 332);*/

            _ITEM_ENUM_TO_ID_MAP.Insert(ItemType.GoldNugget, 371);

            _ITEM_ENUM_TO_ID_MAP.Insert(ItemType.PlayerSkull, 397);

            foreach ((ItemType item, int id) in _ITEM_ENUM_TO_ID_MAP.GetElements())
            {
                _ITEM_ID_TO_ENUM_MAP.Insert(id, item);
            }

            System.Diagnostics.Debug.Assert(_ITEM_ENUM_TO_ID_MAP.Count == _ITEM_ID_TO_ENUM_MAP.Count);
        }

        public static int GetMinStackCount(this ItemType item)
        {
            return 1;
        }

        public static int GetMaxStackCount(this ItemType item)
        {
            switch (item)
            {
                default:
                    throw new System.NotImplementedException();

                case ItemType.IronSword:
                    return 1;
                case ItemType.WoodenSword:
                    return 1;

                case ItemType.StoneSword:
                    return 1;

                case ItemType.DiamondSword:
                    return 1;

                case ItemType.GoldenSword:
                    return 1;

                case ItemType.Stick:
                    return 64;

                /*case ItemType.Snowball:
                    return 16;*/

                case ItemType.GoldNugget:
                    return 64;

                case ItemType.PlayerSkull:
                    return 64;
            }
        }

        internal static int GetId(this ItemType item)
        {
            System.Diagnostics.Debug.Assert(_ITEM_ENUM_TO_ID_MAP.Contains(item));
            int id = _ITEM_ENUM_TO_ID_MAP.Lookup(item);

            return id;
        }
    }
}

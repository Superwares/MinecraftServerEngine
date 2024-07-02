
using Containers;

namespace MinecraftServerEngine
{
    internal static class ItemExtensions
    {
        // TODO: Replace as IReadOnlyTable
        // TODO: 프로그램이 종료되었을 때 자원 해제하기. static destructor?
        private readonly static Table<ItemType, int> _ITEM_ENUM_TO_ID_MAP = new();
        private readonly static Table<int, ItemType> _ITEM_ID_TO_ENUM_MAP = new();

        static ItemExtensions()
        {
            _ITEM_ENUM_TO_ID_MAP.Insert(ItemType.Air, 0);

            _ITEM_ENUM_TO_ID_MAP.Insert(ItemType.IronSword, 267);
            _ITEM_ENUM_TO_ID_MAP.Insert(ItemType.WoodenSword, 268);

            _ITEM_ENUM_TO_ID_MAP.Insert(ItemType.StoneSword, 272);

            _ITEM_ENUM_TO_ID_MAP.Insert(ItemType.DiamondSword, 276);

            _ITEM_ENUM_TO_ID_MAP.Insert(ItemType.GoldenSword, 283);

            _ITEM_ENUM_TO_ID_MAP.Insert(ItemType.LeatherHelmet, 298);

            _ITEM_ENUM_TO_ID_MAP.Insert(ItemType.ChainmailHelmet, 302);

            _ITEM_ENUM_TO_ID_MAP.Insert(ItemType.IronHelmet, 306);

            _ITEM_ENUM_TO_ID_MAP.Insert(ItemType.DiamondHelmet, 310);

            _ITEM_ENUM_TO_ID_MAP.Insert(ItemType.GoldenHelmet, 314);

            _ITEM_ENUM_TO_ID_MAP.Insert(ItemType.Stick, 280);

            _ITEM_ENUM_TO_ID_MAP.Insert(ItemType.Snowball, 332);

            foreach ((ItemType item, int id) in _ITEM_ENUM_TO_ID_MAP.GetElements())
            {
                _ITEM_ID_TO_ENUM_MAP.Insert(id, item);
            }

            System.Diagnostics.Debug.Assert(_ITEM_ENUM_TO_ID_MAP.Count == _ITEM_ID_TO_ENUM_MAP.Count);
        }

        public static bool IsArmor(this ItemType item)
        {
            switch (item)
            {
                default:
                    throw new System.NotImplementedException();

                case ItemType.Air:
                    return false;

                case ItemType.IronSword:
                    return false;
                case ItemType.WoodenSword:
                    return false;

                case ItemType.StoneSword:
                    return false;

                case ItemType.DiamondSword:
                    return false;

                case ItemType.GoldenSword:
                    return false;

                case ItemType.LeatherHelmet:
                    return true;

                case ItemType.ChainmailHelmet:
                    return true;

                case ItemType.IronHelmet:
                    return true;

                case ItemType.DiamondHelmet:
                    return true;

                case ItemType.GoldenHelmet:
                    return true;

                case ItemType.Stick:
                    return false;

                case ItemType.Snowball:
                    return false;
            }
        }

        public static int GetMaxCount(this ItemType item)
        {
            switch (item)
            {
                default:
                    throw new System.NotImplementedException();

                case ItemType.Air:
                    return 1;

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

                case ItemType.LeatherHelmet:
                    return 1;

                case ItemType.ChainmailHelmet:
                    return 1;

                case ItemType.IronHelmet:
                    return 1;

                case ItemType.DiamondHelmet:
                    return 1;

                case ItemType.GoldenHelmet:
                    return 1;

                case ItemType.Stick:
                    return 64;

                case ItemType.Snowball:
                    return 16;
            }
        }

        public static int GetId(this ItemType item)
        {
            System.Diagnostics.Debug.Assert(_ITEM_ENUM_TO_ID_MAP.Contains(item));
            int id = _ITEM_ENUM_TO_ID_MAP.Lookup(item);

            return id;
        }
    }
}

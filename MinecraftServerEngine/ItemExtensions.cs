
using Containers;

namespace MinecraftServerEngine
{
    internal static class ItemExtensions
    {
        // TODO: Replace as IReadOnlyTable
        // TODO: 프로그램이 종료되었을 때 자원 해제하기. static destructor?
        private readonly static Table<Items, int> _ITEM_ENUM_TO_ID_MAP = new();
        private readonly static Table<int, Items> _ITEM_ID_TO_ENUM_MAP = new();

        static ItemExtensions()
        {
            /*_ITEM_ENUM_TO_ID_MAP.Insert(Items.Stone, 1);
            _ITEM_ENUM_TO_ID_MAP.Insert(Items.Grass, 2);
            _ITEM_ENUM_TO_ID_MAP.Insert(Items.Dirt, 3);
            _ITEM_ENUM_TO_ID_MAP.Insert(Items.Cobbestone, 4);*/

            _ITEM_ENUM_TO_ID_MAP.Insert(Items.IronSword, 267);
            _ITEM_ENUM_TO_ID_MAP.Insert(Items.WoodenSword, 268);

            _ITEM_ENUM_TO_ID_MAP.Insert(Items.StoneSword, 272);

            _ITEM_ENUM_TO_ID_MAP.Insert(Items.DiamondSword, 276);

            _ITEM_ENUM_TO_ID_MAP.Insert(Items.GoldenSword, 283);

            _ITEM_ENUM_TO_ID_MAP.Insert(Items.LeatherHelmet, 298);

            _ITEM_ENUM_TO_ID_MAP.Insert(Items.ChainmailHelmet, 302);

            _ITEM_ENUM_TO_ID_MAP.Insert(Items.IronHelmet, 306);

            _ITEM_ENUM_TO_ID_MAP.Insert(Items.DiamondHelmet, 310);

            _ITEM_ENUM_TO_ID_MAP.Insert(Items.GoldenHelmet, 314);

            _ITEM_ENUM_TO_ID_MAP.Insert(Items.Stick, 280);

            _ITEM_ENUM_TO_ID_MAP.Insert(Items.Snowball, 332);

            foreach ((Items item, int id) in _ITEM_ENUM_TO_ID_MAP.GetElements())
            {
                _ITEM_ID_TO_ENUM_MAP.Insert(id, item);
            }

            System.Diagnostics.Debug.Assert(_ITEM_ENUM_TO_ID_MAP.Count == _ITEM_ID_TO_ENUM_MAP.Count);
        }

        public static bool IsArmor(this Items item)
        {
            switch (item)
            {
                default:
                    throw new System.NotImplementedException();

                case Items.IronSword:
                    return false;
                case Items.WoodenSword:
                    return false;

                case Items.StoneSword:
                    return false;

                case Items.DiamondSword:
                    return false;

                case Items.GoldenSword:
                    return false;

                case Items.LeatherHelmet:
                    return true;

                case Items.ChainmailHelmet:
                    return true;

                case Items.IronHelmet:
                    return true;

                case Items.DiamondHelmet:
                    return true;

                case Items.GoldenHelmet:
                    return true;

                case Items.Stick:
                    return false;

                case Items.Snowball:
                    return false;
            }
        }

        public static int GetMaxCount(this Items item)
        {
            switch (item)
            {
                default:
                    throw new System.NotImplementedException();

                case Items.IronSword:
                    return 1;
                case Items.WoodenSword:
                    return 1;

                case Items.StoneSword:
                    return 1;

                case Items.DiamondSword:
                    return 1;

                case Items.GoldenSword:
                    return 1;

                case Items.LeatherHelmet:
                    return 1;

                case Items.ChainmailHelmet:
                    return 1;

                case Items.IronHelmet:
                    return 1;

                case Items.DiamondHelmet:
                    return 1;

                case Items.GoldenHelmet:
                    return 1;

                case Items.Stick:
                    return 64;

                case Items.Snowball:
                    return 16;
            }
        }

        public static int GetId(this Items item)
        {
            System.Diagnostics.Debug.Assert(_ITEM_ENUM_TO_ID_MAP.Contains(item));
            int id = _ITEM_ENUM_TO_ID_MAP.Lookup(item);

            return id;
        }
    }
}

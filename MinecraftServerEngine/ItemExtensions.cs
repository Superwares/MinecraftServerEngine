
using Containers;

namespace MinecraftServerEngine
{
    public static class ItemExtensions
    {
        private struct ItemContext
        {
            public readonly ItemType Type;
            public readonly int Id;
            public readonly string Name;
            public readonly int MinStackCount, MaxStackCount;
            public readonly int Metadata;

            public ItemContext(
                ItemType type,
                int id, string name,
                int minStackCount, int maxStackCount,
                int metadata)
            {
                System.Diagnostics.Debug.Assert(maxStackCount > 0);
                System.Diagnostics.Debug.Assert(minStackCount > 0);
                System.Diagnostics.Debug.Assert(minStackCount <= maxStackCount);

                Type = type;
                Id = id;
                Name = name;
                MinStackCount = minStackCount;
                MaxStackCount = maxStackCount;
                Metadata = metadata;
            }

        }

        // TODO: Replace as IReadOnlyTable
        // TODO: 프로그램이 종료되었을 때 자원 해제하기. static destructor?
        private readonly static Table<ItemType, ItemContext> _ITEM_ENUM_TO_CTX_MAP = new();

        static ItemExtensions()
        {
            _ITEM_ENUM_TO_CTX_MAP.Insert(ItemType.WhiteWool, new ItemContext(
                ItemType.WhiteWool, 35, "wool",
                1, 64,
                0));
            _ITEM_ENUM_TO_CTX_MAP.Insert(ItemType.OrangeWool, new ItemContext(
                ItemType.OrangeWool, 35, "wool",
                1, 64,
                1));
            _ITEM_ENUM_TO_CTX_MAP.Insert(ItemType.MagentaWool, new ItemContext(
                ItemType.MagentaWool, 35, "wool",
                1, 64,
                2));
            _ITEM_ENUM_TO_CTX_MAP.Insert(ItemType.LightBlueWool, new ItemContext(
                ItemType.LightBlueWool, 35, "wool",
                1, 64,
                3));
            _ITEM_ENUM_TO_CTX_MAP.Insert(ItemType.YellowWool, new ItemContext(
                ItemType.YellowWool, 35, "wool",
                1, 64,
                4));
            _ITEM_ENUM_TO_CTX_MAP.Insert(ItemType.LimeWool, new ItemContext(
                ItemType.LimeWool, 35, "wool",
                1, 64,
                5));
            _ITEM_ENUM_TO_CTX_MAP.Insert(ItemType.PinkWool, new ItemContext(
                ItemType.PinkWool, 35, "wool",
                1, 64,
                6));
            _ITEM_ENUM_TO_CTX_MAP.Insert(ItemType.GrayWool, new ItemContext(
                ItemType.GrayWool, 35, "wool",
                1, 64,
                7));
            _ITEM_ENUM_TO_CTX_MAP.Insert(ItemType.LightGrayWool, new ItemContext(
                ItemType.LightGrayWool, 35, "wool",
                1, 64,
                8));
            _ITEM_ENUM_TO_CTX_MAP.Insert(ItemType.CyanWool, new ItemContext(
                ItemType.CyanWool, 35, "wool",
                1, 64,
                9));
            _ITEM_ENUM_TO_CTX_MAP.Insert(ItemType.PurpleWool, new ItemContext(
                ItemType.PurpleWool, 35, "wool",
                1, 64,
                10));
            _ITEM_ENUM_TO_CTX_MAP.Insert(ItemType.BlueWool, new ItemContext(
                ItemType.BlueWool, 35, "wool",
                1, 64,
                11));
            _ITEM_ENUM_TO_CTX_MAP.Insert(ItemType.BrownWool, new ItemContext(
                ItemType.BrownWool, 35, "wool",
                1, 64,
                12));
            _ITEM_ENUM_TO_CTX_MAP.Insert(ItemType.GreenWool, new ItemContext(
                ItemType.GreenWool, 35, "wool",
                1, 64,
                13));
            _ITEM_ENUM_TO_CTX_MAP.Insert(ItemType.RedWool, new ItemContext(
                ItemType.RedWool, 35, "wool",
                1, 64,
                14));
            _ITEM_ENUM_TO_CTX_MAP.Insert(ItemType.BlackWool, new ItemContext(
                ItemType.BlackWool, 35, "wool",
                1, 64,
                15));

            _ITEM_ENUM_TO_CTX_MAP.Insert(ItemType.WhiteTerracotta, new ItemContext(
                ItemType.WhiteTerracotta, 159, "stained_hardened_clay",
                1, 64,
                0));
            _ITEM_ENUM_TO_CTX_MAP.Insert(ItemType.OrangeTerracotta, new ItemContext(
                ItemType.OrangeTerracotta, 159, "stained_hardened_clay",
                1, 64,
                1));
            _ITEM_ENUM_TO_CTX_MAP.Insert(ItemType.MagentaTerracotta, new ItemContext(
                ItemType.MagentaTerracotta, 159, "stained_hardened_clay",
                1, 64,
                2));
            _ITEM_ENUM_TO_CTX_MAP.Insert(ItemType.LightBlueTerracotta, new ItemContext(
                ItemType.LightBlueTerracotta, 159, "stained_hardened_clay",
                1, 64,
                3));
            _ITEM_ENUM_TO_CTX_MAP.Insert(ItemType.YellowTerracotta, new ItemContext(
                ItemType.YellowTerracotta, 159, "stained_hardened_clay",
                1, 64,
                4));
            _ITEM_ENUM_TO_CTX_MAP.Insert(ItemType.LimeTerracotta, new ItemContext(
                ItemType.LimeTerracotta, 159, "stained_hardened_clay",
                1, 64,
                5));
            _ITEM_ENUM_TO_CTX_MAP.Insert(ItemType.PinkTerracotta, new ItemContext(
                ItemType.PinkTerracotta, 159, "stained_hardened_clay",
                1, 64,
                6));
            _ITEM_ENUM_TO_CTX_MAP.Insert(ItemType.GrayTerracotta, new ItemContext(
                ItemType.GrayTerracotta, 159, "stained_hardened_clay",
                1, 64,
                7));
            _ITEM_ENUM_TO_CTX_MAP.Insert(ItemType.LightGrayTerracotta, new ItemContext(
                ItemType.LightGrayTerracotta, 159, "stained_hardened_clay",
                1, 64,
                8));
            _ITEM_ENUM_TO_CTX_MAP.Insert(ItemType.CyanTerracotta, new ItemContext(
                ItemType.CyanTerracotta, 159, "stained_hardened_clay",
                1, 64,
                9));
            _ITEM_ENUM_TO_CTX_MAP.Insert(ItemType.PurpleTerracotta, new ItemContext(
                ItemType.PurpleTerracotta, 159, "stained_hardened_clay",
                1, 64,
                10));
            _ITEM_ENUM_TO_CTX_MAP.Insert(ItemType.BlueTerracotta, new ItemContext(
                ItemType.BlueTerracotta, 159, "stained_hardened_clay",
                1, 64,
                11));
            _ITEM_ENUM_TO_CTX_MAP.Insert(ItemType.BrownTerracotta, new ItemContext(
                ItemType.BrownTerracotta, 159, "stained_hardened_clay",
                1, 64,
                12));
            _ITEM_ENUM_TO_CTX_MAP.Insert(ItemType.GreenTerracotta, new ItemContext(
                ItemType.GreenTerracotta, 159, "stained_hardened_clay",
                1, 64,
                13));
            _ITEM_ENUM_TO_CTX_MAP.Insert(ItemType.RedTerracotta, new ItemContext(
                ItemType.RedTerracotta, 159, "stained_hardened_clay",
                1, 64,
                14));
            _ITEM_ENUM_TO_CTX_MAP.Insert(ItemType.BlackTerracotta, new ItemContext(
                ItemType.BlackTerracotta, 159, "stained_hardened_clay",
                1, 64,
                15));


            _ITEM_ENUM_TO_CTX_MAP.Insert(ItemType.IronSword, new ItemContext(
                ItemType.IronSword, 267, "iron_sword",
                1, 1,
                0));
            _ITEM_ENUM_TO_CTX_MAP.Insert(ItemType.WoodenSword, new ItemContext(
                ItemType.WoodenSword, 268, "wooden_sword",
                1, 1,
                0));

            _ITEM_ENUM_TO_CTX_MAP.Insert(ItemType.StoneSword, new ItemContext(
                ItemType.StoneSword, 272, "stone_sword",
                1, 1,
                0));

            _ITEM_ENUM_TO_CTX_MAP.Insert(ItemType.DiamondSword, new ItemContext(
                ItemType.DiamondSword, 276, "diamond_sword",
                1, 1,
                0));

            _ITEM_ENUM_TO_CTX_MAP.Insert(ItemType.GoldenSword, new ItemContext(
                ItemType.GoldenSword, 283, "golden_sword",
                1, 1,
                0));

            _ITEM_ENUM_TO_CTX_MAP.Insert(ItemType.Stick, new ItemContext(
                ItemType.Stick, 280, "stick",
                1, 64,
                0));

            // This item is handled by clientside.
            //_ITEM_ENUM_TO_CTX_MAP.Insert(ItemType.Snowball, new ItemContext(
            //    ItemType.Snowball, 332, "snowball",
            //    1, 16,
            //    0));

            _ITEM_ENUM_TO_CTX_MAP.Insert(ItemType.GoldNugget, new ItemContext(
                ItemType.GoldNugget, 371, "gold_nugget",
                1, 64,
                0));

            _ITEM_ENUM_TO_CTX_MAP.Insert(ItemType.PlayerSkull, new ItemContext(
                ItemType.PlayerSkull, 397, "skull",
                1, 64,
                3));

        }

        public static int GetMinStackCount(this ItemType item)
        {
            return _ITEM_ENUM_TO_CTX_MAP.Lookup(item).MinStackCount;
        }

        public static int GetMaxStackCount(this ItemType item)
        {
            return _ITEM_ENUM_TO_CTX_MAP.Lookup(item).MaxStackCount;
        }

        public static int GetMetadata(this ItemType item)
        {
            return _ITEM_ENUM_TO_CTX_MAP.Lookup(item).Metadata;
        }

        internal static int GetId(this ItemType item)
        {
            return _ITEM_ENUM_TO_CTX_MAP.Lookup(item).Id;
        }
    }
}

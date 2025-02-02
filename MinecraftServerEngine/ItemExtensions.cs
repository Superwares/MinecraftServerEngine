
using Common;
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
            public readonly int MaxCount;
            public readonly int Metadata;

            public ItemContext(
                ItemType type,
                int id, string name,
                int maxCount,
                int metadata)
            {
                System.Diagnostics.Debug.Assert(maxCount > 0);
                System.Diagnostics.Debug.Assert(ItemStack.MinCount > 0);
                System.Diagnostics.Debug.Assert(ItemStack.MinCount <= maxCount);

                Type = type;
                Id = id;
                Name = name;
                MaxCount = maxCount;
                Metadata = metadata;
            }

        }

        // TODO: Replace as IReadOnlyTable
        // TODO: 프로그램이 종료되었을 때 자원 해제하기. static destructor?
        private readonly static Table<ItemType, ItemContext> _ITEM_TYPE_TO_CTX_MAP = new();
        private readonly static Table<(int, int), ItemType> _ITEM_ID_TO_ITEM_TYPE_MAP = new();

        static ItemExtensions()
        {
            ItemContext[] _map =
            {
                new ItemContext(
                    ItemType.WhiteWool, 35, "wool",
                    64,
                    0),
                new ItemContext(
                    ItemType.OrangeWool, 35, "wool",
                    64,
                    1),
                new ItemContext(
                    ItemType.MagentaWool, 35, "wool",
                    64,
                    2),
                new ItemContext(
                    ItemType.LightBlueWool, 35, "wool",
                    64,
                    3),
                new ItemContext(
                    ItemType.YellowWool, 35, "wool",
                    64,
                    4),
                new ItemContext(
                    ItemType.LimeWool, 35, "wool",
                    64,
                    5),
                new ItemContext(
                    ItemType.PinkWool, 35, "wool",
                    64,
                    6),
                new ItemContext(
                    ItemType.GrayWool, 35, "wool",
                    64,
                    7),
                new ItemContext(
                    ItemType.LightGrayWool, 35, "wool",
                    64,
                    8),
                new ItemContext(
                    ItemType.CyanWool, 35, "wool",
                    64,
                    9),
                new ItemContext(
                    ItemType.PurpleWool, 35, "wool",
                    64,
                    10),
                new ItemContext(
                    ItemType.BlueWool, 35, "wool",
                    64,
                    11),
                new ItemContext(
                    ItemType.BrownWool, 35, "wool",
                    64,
                    12),
                new ItemContext(
                    ItemType.GreenWool, 35, "wool",
                    64,
                    13),
                new ItemContext(
                    ItemType.RedWool, 35, "wool",
                    64,
                    14),
                new ItemContext(
                    ItemType.BlackWool, 35, "wool",
                    64,
                    15),

                new ItemContext(
                    ItemType.RedstoneOre, 73, "redstone_ore",
                    64,
                    0),
                // It is not shown in client...
                // It is only used for block.
                //new ItemContext(
                //    ItemType.GlowingRedstoneOre, 74, "lit_redstone_ore",
                //    64,
                //    0),

                new ItemContext(
                    ItemType.Pumpkin, 86, "pumpkin",
                    64,
                    0),
                new ItemContext(
                    ItemType.JackOLantern, 91, "lit_pumpkin",
                    64,
                    0),
                new ItemContext(
                    ItemType.IronBars, 101, "iron_bars",
                    64,
                    0),
                new ItemContext(
                    ItemType.WhiteTerracotta, 159, "stained_hardened_clay",
                    64,
                    0),
                new ItemContext(
                    ItemType.OrangeTerracotta, 159, "stained_hardened_clay",
                    64,
                    1),
                new ItemContext(
                    ItemType.MagentaTerracotta, 159, "stained_hardened_clay",
                    64,
                    2),
                new ItemContext(
                    ItemType.LightBlueTerracotta, 159, "stained_hardened_clay",
                    64,
                    3),
                new ItemContext(
                    ItemType.YellowTerracotta, 159, "stained_hardened_clay",
                    64,
                    4),
                new ItemContext(
                    ItemType.LimeTerracotta, 159, "stained_hardened_clay",
                    64,
                    5),
                new ItemContext(
                    ItemType.PinkTerracotta, 159, "stained_hardened_clay",
                    64,
                    6),
                new ItemContext(
                    ItemType.GrayTerracotta, 159, "stained_hardened_clay",
                    64,
                    7),
                new ItemContext(
                    ItemType.LightGrayTerracotta, 159, "stained_hardened_clay",
                    64,
                    8),
                new ItemContext(
                    ItemType.CyanTerracotta, 159, "stained_hardened_clay",
                    64,
                    9),
                new ItemContext(
                    ItemType.PurpleTerracotta, 159, "stained_hardened_clay",
                    64,
                    10),
                new ItemContext(
                    ItemType.BlueTerracotta, 159, "stained_hardened_clay",
                    64,
                    11),
                new ItemContext(
                    ItemType.BrownTerracotta, 159, "stained_hardened_clay",
                    64,
                    12),
                new ItemContext(
                    ItemType.GreenTerracotta, 159, "stained_hardened_clay",
                    64,
                    13),
                new ItemContext(
                    ItemType.RedTerracotta, 159, "stained_hardened_clay",
                    64,
                    14),
                new ItemContext(
                    ItemType.BlackTerracotta, 159, "stained_hardened_clay",
                    64,
                    15),
                new ItemContext(
                    ItemType.WhiteStainedGlassPane, 160, "stained_glass_pane",
                    64,
                    0),
                new ItemContext(
                    ItemType.OrangeStainedGlassPane, 160, "stained_glass_pane",
                    64,
                    1),
                new ItemContext(
                    ItemType.MagentaStainedGlassPane, 160, "stained_glass_pane",
                    64,
                    2),
                new ItemContext(
                    ItemType.LightBlueStainedGlassPane, 160, "stained_glass_pane",
                    64,
                    3),
                new ItemContext(
                    ItemType.YellowStainedGlassPane, 160, "stained_glass_pane",
                    64,
                    4),
                new ItemContext(
                    ItemType.LimeStainedGlassPane, 160, "stained_glass_pane",
                    64,
                    5),
                new ItemContext(
                    ItemType.PinkStainedGlassPane, 160, "stained_glass_pane",
                    64,
                    6),
                new ItemContext(
                    ItemType.GrayStainedGlassPane, 160, "stained_glass_pane",
                    64,
                    7),
                new ItemContext(
                    ItemType.LightGrayStainedGlassPane, 160, "stained_glass_pane",
                    64,
                    8),
                new ItemContext(
                    ItemType.CyanStainedGlassPane, 160, "stained_glass_pane",
                    64,
                    9),
                new ItemContext(
                    ItemType.PurpleStainedGlassPane, 160, "stained_glass_pane",
                    64,
                    10),
                new ItemContext(
                    ItemType.BlueStainedGlassPane, 160, "stained_glass_pane",
                    64,
                    11),
                new ItemContext(
                    ItemType.BrownStainedGlassPane, 160, "stained_glass_pane",
                    64,
                    12),
                new ItemContext(
                    ItemType.GreenStainedGlassPane, 160, "stained_glass_pane",
                    64,
                    13),
                new ItemContext(
                    ItemType.RedStainedGlassPane, 160, "stained_glass_pane",
                    64,
                    14),
                new ItemContext(
                    ItemType.BlackStainedGlassPane, 160, "stained_glass_pane",
                    64,
                    15),
                new ItemContext(
                    ItemType.IronSword, 267, "iron_sword",
                    1,
                    0),
                new ItemContext(
                    ItemType.WoodenSword, 268, "wooden_sword",
                    1,
                    0),
                new ItemContext(
                    ItemType.StoneSword, 272, "stone_sword",
                    1,
                    0),
                new ItemContext(
                    ItemType.DiamondSword, 276, "diamond_sword",
                    1,
                    0),
                new ItemContext(
                    ItemType.DiamondShovel, 277, "diamond_shovel",
                    1,
                    0),
                new ItemContext(
                    ItemType.DiamondPickaxe, 278, "diamond_pickaxe",
                    1,
                    0),
                new ItemContext(
                    ItemType.DiamondAxe, 279, "diamond_axe",
                    1,
                    0),
                new ItemContext(
                    ItemType.Stick, 280, "stick",
                    64,
                    0),
                new ItemContext(
                    ItemType.GoldenSword, 283, "golden_sword",
                    1,
                    0),
                new ItemContext(
                    ItemType.Flint, 318, "flint",
                    64,
                    0),
                new ItemContext(
                    ItemType.Sign, 323, "sign",
                    64,
                    0),
                //new ItemContext(
                //    ItemType.Snowball, 332, "snowball",
                //    1, 16,
                //    0),
                new ItemContext(
                    ItemType.GoldNugget, 371, "gold_nugget",
                    64,
                    0),
                new ItemContext(
                    ItemType.EyeOfEnder, 381, "ender_eye",
                    64,
                    0),
                new ItemContext(
                    ItemType.PlayerSkull, 397, "skull",
                    64,
                    3),

                new ItemContext(
                    ItemType.EndCrystal, 426, "end_crystal",
                    64,
                    0),

                new ItemContext(
                    ItemType.MusicDisc_C418_13, 2256, "record_13",
                    1,
                    0),
                new ItemContext(
                    ItemType.MusicDisc_C418_cat, 2257, "record_cat",
                    1,
                    0),
                new ItemContext(
                    ItemType.MusicDisc_C418_blocks, 2258, "record_blocks",
                    1,
                    0),
                new ItemContext(
                    ItemType.MusicDisc_C418_chirp, 2259, "record_chirp",
                    1,
                    0),
                new ItemContext(
                    ItemType.MusicDisc_C418_far, 2260, "record_far",
                    1,
                    0),
                new ItemContext(
                    ItemType.MusicDisc_C418_mall, 2261, "record_mall",
                    1,
                    0),
                new ItemContext(
                    ItemType.MusicDisc_C418_mellohi, 2262, "record_mellohi",
                    1,
                    0),
                new ItemContext(
                    ItemType.MusicDisc_C418_stal, 2263, "record_stal",
                    1,
                    0),
                new ItemContext(
                    ItemType.MusicDisc_C418_strad, 2264, "record_strad",
                    1,
                    0),
                new ItemContext(
                    ItemType.MusicDisc_C418_ward, 2265, "record_ward",
                    1,
                    0),
                new ItemContext(
                    ItemType.MusicDisc_C418_11, 2266, "record_11",
                    1,
                    0),
                new ItemContext(
                    ItemType.MusicDisc_C418_wait, 2267, "record_wait",
                    1,
                    0),

            };

            foreach (ItemContext ctx in _map)
            {
                _ITEM_TYPE_TO_CTX_MAP.Insert(ctx.Type, ctx);
            }

            // Ensure to prevent the duplicated item ids...
            foreach (ItemContext ctx in _map)
            {
                _ITEM_ID_TO_ITEM_TYPE_MAP.Insert((ctx.Id, ctx.Metadata), ctx.Type);
            }


        }

        public static int GetMaxCount(this ItemType item)
        {
            return _ITEM_TYPE_TO_CTX_MAP.Lookup(item).MaxCount;
        }

        public static int GetMetadata(this ItemType item)
        {
            return _ITEM_TYPE_TO_CTX_MAP.Lookup(item).Metadata;
        }

        internal static int GetId(this ItemType item)
        {
            return _ITEM_TYPE_TO_CTX_MAP.Lookup(item).Id;
        }
    }
}

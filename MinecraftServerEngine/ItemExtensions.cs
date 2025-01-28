
using Common;
using Containers;
using System.ComponentModel;
using System.Net.NetworkInformation;
using System;

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
        private readonly static Table<ItemType, ItemContext> _ITEM_TYPE_ENUM_TO_CTX_MAP = new();

        static ItemExtensions()
        {
            ItemContext[] list =
            {
                new ItemContext(
                    ItemType.WhiteWool, 35, "wool",
                    1, 64,
                    0),
                new ItemContext(
                    ItemType.OrangeWool, 35, "wool",
                    1, 64,
                    1),
                new ItemContext(
                    ItemType.MagentaWool, 35, "wool",
                    1, 64,
                    2),
                new ItemContext(
                    ItemType.LightBlueWool, 35, "wool",
                    1, 64,
                    3),
                new ItemContext(
                    ItemType.YellowWool, 35, "wool",
                    1, 64,
                    4),
                new ItemContext(
                    ItemType.LimeWool, 35, "wool",
                    1, 64,
                    5),
                new ItemContext(
                    ItemType.PinkWool, 35, "wool",
                    1, 64,
                    6),
                new ItemContext(
                    ItemType.GrayWool, 35, "wool",
                    1, 64,
                    7),
                new ItemContext(
                    ItemType.LightGrayWool, 35, "wool",
                    1, 64,
                    8),
                new ItemContext(
                    ItemType.CyanWool, 35, "wool",
                    1, 64,
                    9),
                new ItemContext(
                    ItemType.PurpleWool, 35, "wool",
                    1, 64,
                    10),
                new ItemContext(
                    ItemType.BlueWool, 35, "wool",
                    1, 64,
                    11),
                new ItemContext(
                    ItemType.BrownWool, 35, "wool",
                    1, 64,
                    12),
                new ItemContext(
                    ItemType.GreenWool, 35, "wool",
                    1, 64,
                    13),
                new ItemContext(
                    ItemType.RedWool, 35, "wool",
                    1, 64,
                    14),
                new ItemContext(
                    ItemType.BlackWool, 35, "wool",
                    1, 64,
                    15),
                new ItemContext(
                    ItemType.Pumpkin, 86, "pumpkin",
                    1, 64,
                    0),
                new ItemContext(
                    ItemType.JackOLantern, 91, "lit_pumpkin",
                    1, 64,
                    0),
                new ItemContext(
                    ItemType.IronBars, 101, "iron_bars",
                    1, 64,
                    0),
                new ItemContext(
                    ItemType.WhiteTerracotta, 159, "stained_hardened_clay",
                    1, 64,
                    0),
                new ItemContext(
                    ItemType.OrangeTerracotta, 159, "stained_hardened_clay",
                    1, 64,
                    1),
                new ItemContext(
                    ItemType.MagentaTerracotta, 159, "stained_hardened_clay",
                    1, 64,
                    2),
                new ItemContext(
                    ItemType.LightBlueTerracotta, 159, "stained_hardened_clay",
                    1, 64,
                    3),
                new ItemContext(
                    ItemType.YellowTerracotta, 159, "stained_hardened_clay",
                    1, 64,
                    4),
                new ItemContext(
                    ItemType.LimeTerracotta, 159, "stained_hardened_clay",
                    1, 64,
                    5),
                new ItemContext(
                    ItemType.PinkTerracotta, 159, "stained_hardened_clay",
                    1, 64,
                    6),
                new ItemContext(
                    ItemType.GrayTerracotta, 159, "stained_hardened_clay",
                    1, 64,
                    7),
                new ItemContext(
                    ItemType.LightGrayTerracotta, 159, "stained_hardened_clay",
                    1, 64,
                    8),
                new ItemContext(
                    ItemType.CyanTerracotta, 159, "stained_hardened_clay",
                    1, 64,
                    9),
                new ItemContext(
                    ItemType.PurpleTerracotta, 159, "stained_hardened_clay",
                    1, 64,
                    10),
                new ItemContext(
                    ItemType.BlueTerracotta, 159, "stained_hardened_clay",
                    1, 64,
                    11),
                new ItemContext(
                    ItemType.BrownTerracotta, 159, "stained_hardened_clay",
                    1, 64,
                    12),
                new ItemContext(
                    ItemType.GreenTerracotta, 159, "stained_hardened_clay",
                    1, 64,
                    13),
                new ItemContext(
                    ItemType.RedTerracotta, 159, "stained_hardened_clay",
                    1, 64,
                    14),
                new ItemContext(
                    ItemType.BlackTerracotta, 159, "stained_hardened_clay",
                    1, 64,
                    15),
                new ItemContext(
                    ItemType.WhiteStainedGlassPane, 160, "stained_glass_pane",
                    1, 64,
                    0),
                new ItemContext(
                    ItemType.OrangeStainedGlassPane, 160, "stained_glass_pane",
                    1, 64,
                    1),
                new ItemContext(
                    ItemType.MagentaStainedGlassPane, 160, "stained_glass_pane",
                    1, 64,
                    2),
                new ItemContext(
                    ItemType.LightBlueStainedGlassPane, 160, "stained_glass_pane",
                    1, 64,
                    3),
                new ItemContext(
                    ItemType.YellowStainedGlassPane, 160, "stained_glass_pane",
                    1, 64,
                    4),
                new ItemContext(
                    ItemType.LimeStainedGlassPane, 160, "stained_glass_pane",
                    1, 64,
                    5),
                new ItemContext(
                    ItemType.PinkStainedGlassPane, 160, "stained_glass_pane",
                    1, 64,
                    6),
                new ItemContext(
                    ItemType.GrayStainedGlassPane, 160, "stained_glass_pane",
                    1, 64,
                    7),
                new ItemContext(
                    ItemType.LightGrayStainedGlassPane, 160, "stained_glass_pane",
                    1, 64,
                    8),
                new ItemContext(
                    ItemType.CyanStainedGlassPane, 160, "stained_glass_pane",
                    1, 64,
                    9),
                new ItemContext(
                    ItemType.PurpleStainedGlassPane, 160, "stained_glass_pane",
                    1, 64,
                    10),
                new ItemContext(
                    ItemType.BlueStainedGlassPane, 160, "stained_glass_pane",
                    1, 64,
                    11),
                new ItemContext(
                    ItemType.BrownStainedGlassPane, 160, "stained_glass_pane",
                    1, 64,
                    12),
                new ItemContext(
                    ItemType.GreenStainedGlassPane, 160, "stained_glass_pane",
                    1, 64,
                    13),
                new ItemContext(
                    ItemType.RedStainedGlassPane, 160, "stained_glass_pane",
                    1, 64,
                    14),
                new ItemContext(
                    ItemType.BlackStainedGlassPane, 160, "stained_glass_pane",
                    1, 64,
                    15),
                new ItemContext(
                    ItemType.IronSword, 267, "iron_sword",
                    1, 1,
                    0),
                new ItemContext(
                    ItemType.WoodenSword, 268, "wooden_sword",
                    1, 1,
                    0),
                new ItemContext(
                    ItemType.StoneSword, 272, "stone_sword",
                    1, 1,
                    0),
                new ItemContext(
                    ItemType.DiamondSword, 276, "diamond_sword",
                    1, 1,
                    0),
                new ItemContext(
                    ItemType.GoldenSword, 283, "golden_sword",
                    1, 1,
                    0),
                new ItemContext(
                    ItemType.Stick, 280, "stick",
                    1, 64,
                    0),
                //new ItemContext(
                //    ItemType.Snowball, 332, "snowball",
                //    1, 16,
                //    0),
                new ItemContext(
                    ItemType.GoldNugget, 371, "gold_nugget",
                    1, 64,
                    0),
                new ItemContext(
                    ItemType.PlayerSkull, 397, "skull",
                    1, 64,
                    3),

            };

            foreach (ItemContext ctx in list)
            {
                _ITEM_TYPE_ENUM_TO_CTX_MAP.Insert(ctx.Type, ctx);
            }

        }

        public static int GetMinStackCount(this ItemType item)
        {
            return _ITEM_TYPE_ENUM_TO_CTX_MAP.Lookup(item).MinStackCount;
        }

        public static int GetMaxStackCount(this ItemType item)
        {
            return _ITEM_TYPE_ENUM_TO_CTX_MAP.Lookup(item).MaxStackCount;
        }

        public static int GetMetadata(this ItemType item)
        {
            return _ITEM_TYPE_ENUM_TO_CTX_MAP.Lookup(item).Metadata;
        }

        internal static int GetId(this ItemType item)
        {
            return _ITEM_TYPE_ENUM_TO_CTX_MAP.Lookup(item).Id;
        }
    }
}


using Common;
using Containers;
using static System.Formats.Asn1.AsnWriter;
using System.Drawing;

namespace MinecraftServerEngine.Items
{
    public static class ItemExtensions
    {
        private struct ItemContext
        {
            public readonly ItemType Type;
            public readonly int Id;
            public readonly int Metadata;
            public readonly string Name;
            public readonly int MaxCount;

            public ItemContext(
                ItemType type,
                int id, int metadata, string name,
                int maxCount)
            {
                System.Diagnostics.Debug.Assert(maxCount > 0);
                System.Diagnostics.Debug.Assert(Item.MinCount > 0);
                System.Diagnostics.Debug.Assert(Item.MinCount <= maxCount);

                Type = type;
                Id = id;
                Metadata = metadata;
                Name = name;
                MaxCount = maxCount;
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
                    ItemType.Stone, 1, 0, "stone",
                    64),
                new ItemContext(
                    ItemType.Granite, 1, 1, "stone",
                    64),
                new ItemContext(
                    ItemType.PolishedGranite, 1, 2, "stone",
                    64),
                new ItemContext(
                    ItemType.Diorite, 1, 3, "stone",
                    64),
                new ItemContext(
                    ItemType.PolishedDiorite, 1, 4, "stone",
                    64),
                new ItemContext(
                    ItemType.Andesite, 1, 5, "stone",
                    64),
                new ItemContext(
                    ItemType.PolishedAndesite, 1, 6, "stone",
                    64),
                new ItemContext(
                    ItemType.GrassBlock, 2, 0, "grass",
                    64),
                new ItemContext(
                    ItemType.Dirt, 3, 0, "dirt",
                    64),
                new ItemContext(
                    ItemType.CoarseDirt, 3, 1, "dirt",
                    64),
                new ItemContext(
                    ItemType.Podzol, 3, 2, "dirt",
                    64),
                new ItemContext(
                    ItemType.Cobblestone, 4, 0, "cobblestone",
                    64),
                new ItemContext(
                    ItemType.OakWoodPlanks, 5, 0, "planks",
                    64),
                new ItemContext(
                    ItemType.SpruceWoodPlanks, 5, 1, "planks",
                    64),
                new ItemContext(
                    ItemType.BirchWoodPlanks, 5, 2, "planks",
                    64),
                new ItemContext(
                    ItemType.JungleWoodPlanks, 5, 3, "planks",
                    64),
                new ItemContext(
                    ItemType.AcaciaWoodPlanks, 5, 4, "planks",
                    64),
                new ItemContext(
                    ItemType.DarkOakWoodPlanks, 5, 5, "planks",
                    64),
                new ItemContext(
                    ItemType.OakSapling, 6, 0, "sapling",
                    64),
                new ItemContext(
                    ItemType.SpruceSapling, 6, 1, "sapling",
                    64),
                new ItemContext(
                    ItemType.BirchSapling, 6, 2, "sapling",
                    64),
                new ItemContext(
                    ItemType.JungleSapling, 6, 3, "sapling",
                    64),
                new ItemContext(
                    ItemType.AcaciaSapling, 6, 4, "sapling",
                    64),
                new ItemContext(
                    ItemType.DarkOakSapling, 6, 5, "sapling",
                    64),
                new ItemContext(
                    ItemType.Bedrock, 7, 0, "bedrock",
                    64),
                new ItemContext(
                    ItemType.FlowingWater, 8, 0, "flowing_water",
                    64),
                new ItemContext(
                    ItemType.StillWater, 9, 0, "water",
                    64),
                new ItemContext(
                    ItemType.FlowingLava, 10, 0, "flowing_lava",
                    64),
                new ItemContext(
                    ItemType.StillLava, 11, 0, "lava",
                    64),
                new ItemContext(
                    ItemType.Sand, 12, 0, "sand",
                    64),
                new ItemContext(
                    ItemType.RedSand, 12, 1, "sand",
                    64),
                new ItemContext(
                    ItemType.Gravel, 13, 0, "gravel",
                    64),
                new ItemContext(
                    ItemType.GoldOre, 14, 0, "gold_ore",
                    64),
                new ItemContext(
                    ItemType.IronOre, 15, 0, "iron_ore",
                    64),
                new ItemContext(
                    ItemType.CoalOre, 16, 0, "coal_ore",
                    64),
                new ItemContext(
                    ItemType.OakWood, 17, 0, "log",
                    64),
                new ItemContext(
                    ItemType.SpruceWood, 17, 1, "log",
                    64),
                new ItemContext(
                    ItemType.BirchWood, 17, 2, "log",
                    64),
                new ItemContext(
                    ItemType.JungleWood, 17, 3, "log",
                    64),
                new ItemContext(
                    ItemType.OakLeaves, 18, 0, "leaves",
                    64),
                new ItemContext(
                    ItemType.SpruceLeaves, 18, 1, "leaves",
                    64),
                new ItemContext(
                    ItemType.BirchLeaves, 18, 2, "leaves",
                    64),
                new ItemContext(
                    ItemType.JungleLeaves, 18, 3, "leaves",
                    64),
                new ItemContext(
                    ItemType.Sponge, 19, 0, "sponge",
                    64),
                new ItemContext(
                    ItemType.WetSponge, 19, 1, "sponge",
                    64),
                new ItemContext(
                    ItemType.Glass, 20, 0, "glass",
                    64),
                new ItemContext(
                    ItemType.LapisLazuliOre, 21, 0, "lapis_ore",
                    64),
                new ItemContext(
                    ItemType.LapisLazuliBlock, 22, 0, "lapis_block",
                    64),

                new ItemContext(
                    ItemType.Dispenser, 23, 0, "dispenser",
                    64),

                new ItemContext(
                    ItemType.Sandstone, 24, 0, "sandstone",
                    64),
                new ItemContext(
                    ItemType.ChiseledSandstone, 24, 1, "sandstone",
                    64),
                new ItemContext(
                    ItemType.SmoothSandstone, 24, 2, "sandstone",
                    64),
                new ItemContext(
                    ItemType.NoteBlock, 25, 0, "noteblock",
                    64),

                new ItemContext(
                    ItemType.StickyPiston, 29, 0, "sticky_piston",
                    64),

                new ItemContext(
                    ItemType.DeadShrub, 31, 0, "tallgrass",
                    64),
                new ItemContext(
                    ItemType.Grass, 31, 1, "tallgrass",
                    64),
                new ItemContext(
                    ItemType.Fern, 31, 2, "tallgrass",
                    64),
                new ItemContext(
                    ItemType.DeadBush, 32, 0, "deadbush",
                    64),

                new ItemContext(
                    ItemType.Piston, 33, 0, "piston",
                    64),

                new ItemContext(
                    ItemType.WhiteWool, 35, 0, "wool",
                    64),
                new ItemContext(
                    ItemType.OrangeWool, 35, 1, "wool",
                    64),
                new ItemContext(
                    ItemType.MagentaWool, 35, 2, "wool",
                    64),
                new ItemContext(
                    ItemType.LightBlueWool, 35, 3, "wool",
                    64),
                new ItemContext(
                    ItemType.YellowWool, 35, 4, "wool",
                    64),
                new ItemContext(
                    ItemType.LimeWool, 35, 5, "wool",
                    64),
                new ItemContext(
                    ItemType.PinkWool, 35, 6, "wool",
                    64),
                new ItemContext(
                    ItemType.GrayWool, 35, 7, "wool",
                    64),
                new ItemContext(
                    ItemType.LightGrayWool, 35, 8, "wool",
                    64),
                new ItemContext(
                    ItemType.CyanWool, 35, 9, "wool",
                    64),
                new ItemContext(
                    ItemType.PurpleWool, 35, 10, "wool",
                    64),
                new ItemContext(
                    ItemType.BlueWool, 35, 11, "wool",
                    64),
                new ItemContext(
                    ItemType.BrownWool, 35, 12, "wool",
                    64),
                new ItemContext(
                    ItemType.GreenWool, 35, 13, "wool",
                    64),
                new ItemContext(
                    ItemType.RedWool, 35, 14, "wool",
                    64),
                new ItemContext(
                    ItemType.BlackWool, 35, 15, "wool",
                    64),

                new ItemContext(
                    ItemType.Dandelion, 37, 0 , "yellow_flower",
                    64),
                new ItemContext(
                    ItemType.Poppy, 38, 0, "red_flower",
                    64),
                new ItemContext(
                    ItemType.BlueOrchid, 38, 1, "red_flower",
                    64),
                new ItemContext(
                    ItemType.Allium, 38, 2, "red_flower",
                    64),
                new ItemContext(
                    ItemType.AzureBluet, 38, 3, "red_flower",
                    64),
                new ItemContext(
                    ItemType.RedTulip, 38, 4, "red_flower",
                    64),
                new ItemContext(
                    ItemType.OrangeTulip, 38, 5, "red_flower",
                    64),
                new ItemContext(
                    ItemType.WhiteTulip, 38, 6, "red_flower",
                    64),
                new ItemContext(
                    ItemType.PinkTulip, 38, 7, "red_flower",
                    64),
                new ItemContext(
                    ItemType.OxeyeDaisy, 38, 8, "red_flower",
                    64),
                new ItemContext(
                    ItemType.BrownMushroom, 39, 0, "brown_mushroom",
                    64),
                new ItemContext(
                    ItemType.RedMushroom, 40, 0, "red_mushroom",
                    64),
                new ItemContext(
                    ItemType.GoldBlock, 41, 0, "gold_block",
                    64),
                new ItemContext(
                    ItemType.IronBlock, 42, 0, "iron_block",
                    64),
                new ItemContext(
                    ItemType.DoubleStoneSlab, 43, 0, "double_stone_slab",
                    64),
                new ItemContext(
                    ItemType.DoubleSandstoneSlab, 43, 1, "double_stone_slab",
                    64),
                new ItemContext(
                    ItemType.DoubleWoodenSlab, 43, 2, "double_stone_slab",
                    64),
                new ItemContext(
                    ItemType.DoubleCobblestoneSlab, 43, 3, "double_stone_slab",
                    64),
                new ItemContext(
                    ItemType.DoubleBrickSlab, 43, 4, "double_stone_slab",
                    64),
                new ItemContext(
                    ItemType.DoubleStoneBrickSlab, 43, 5, "double_stone_slab",
                    64),
                new ItemContext(
                    ItemType.DoubleNetherBrickSlab, 43, 6, "double_stone_slab",
                    64),
                new ItemContext(
                    ItemType.DoubleQuartzSlab, 43, 7, "double_stone_slab",
                    64),

                new ItemContext(
                    ItemType.StoneBottomSlab, 44, 0, "stone_slab",
                    64),
                new ItemContext(
                    ItemType.SandstoneBottomSlab, 44, 1, "stone_slab",
                    64),
                new ItemContext(
                    ItemType.WoodenBottomSlab, 44, 2, "stone_slab",
                    64),
                new ItemContext(
                    ItemType.CobblestoneBottomSlab, 44, 3, "stone_slab",
                    64),
                new ItemContext(
                    ItemType.BrickBottomSlab, 44, 4, "stone_slab",
                    64),
                new ItemContext(
                    ItemType.StoneBrickBottomSlab, 44, 5, "stone_slab",
                    64),
                new ItemContext(
                    ItemType.NetherBrickBottomSlab, 44, 6, "stone_slab",
                    64),
                new ItemContext(
                    ItemType.QuartzBottomSlab, 44, 7, "stone_slab",
                    64),
                new ItemContext(
                    ItemType.StoneTopSlab, 44, 8, "stone_slab",
                    64),
                new ItemContext(
                    ItemType.SandstoneTopSlab, 44, 9, "stone_slab",
                    64),
                new ItemContext(
                    ItemType.WoodenTopSlab, 44, 10, "stone_slab",
                    64),
                new ItemContext(
                    ItemType.CobblestoneTopSlab, 44, 11, "stone_slab",
                    64),
                new ItemContext(
                    ItemType.BrickTopSlab, 44, 12, "stone_slab",
                    64),
                new ItemContext(
                    ItemType.StoneBrickTopSlab, 44, 13, "stone_slab",
                    64),
                new ItemContext(
                    ItemType.NetherBrickTopSlab, 44, 14, "stone_slab",
                    64),
                new ItemContext(
                    ItemType.QuartzTopSlab, 44, 15, "stone_slab",
                    64),

                new ItemContext(
                    ItemType.Bricks, 45, 0, "brick_block",
                    64),
                new ItemContext(
                    ItemType.TNT, 46, 0, "tnt",
                    64),
                new ItemContext(
                    ItemType.Bookshelf, 47, 0, "bookshelf",
                    64),
                new ItemContext(
                    ItemType.MossStone, 48, 0, "mossy_cobblestone",
                    64),
                new ItemContext(
                    ItemType.Obsidian, 49, 0, "obsidian",
                    64),

                new ItemContext(
                    ItemType.Torch, 50, 0, "torch",
                    64),

                new ItemContext(
                    ItemType.OakWoodStairs, 53, 0, "oak_stairs",
                    64),

                new ItemContext(
                    ItemType.DiamondBlock, 57, 0, "diamond_block",
                    64),
                new ItemContext(
                    ItemType.CraftingTable, 58, 0, "crafting_table",
                    64),
                new ItemContext(
                    ItemType.WheatCrops, 59, 0, "wheat",
                    64),
                new ItemContext(
                    ItemType.Farmland, 60, 0, "farmland",
                    64),

                new ItemContext(
                    ItemType.CobblestoneStairs, 67, 0, "stone_stairs",
                    64),

                new ItemContext(
                    ItemType.RedstoneOre, 73, 0, "redstone_ore",
                    64),
                // It is not shown in client...
                // It is only used for block.
                //new ItemContext(
                //    ItemType.GlowingRedstoneOre, 740, , "lit_redstone_ore",
                //    64),

                new ItemContext(
                    ItemType.OakFence, 85, 0, "fence",
                    64),

                new ItemContext(
                    ItemType.Pumpkin, 86, 0, "pumpkin",
                    64),
                new ItemContext(
                    ItemType.JackOLantern, 91, 0, "lit_pumpkin",
                    64),

                new ItemContext(
                    ItemType.WhiteStainedGlass, 95, 0, "stained_glass",
                    64),
                new ItemContext(
                    ItemType.OrangeStainedGlass, 95, 1, "stained_glass",
                    64),
                new ItemContext(
                    ItemType.MagentaStainedGlass, 95, 2, "stained_glass",
                    64),
                new ItemContext(
                    ItemType.LightBlueStainedGlass, 95, 3, "stained_glass",
                    64),
                new ItemContext(
                    ItemType.YellowStainedGlass, 95, 4, "stained_glass",
                    64),
                new ItemContext(
                    ItemType.LimeStainedGlass, 95, 5, "stained_glass",
                    64),
                new ItemContext(
                    ItemType.PinkStainedGlass, 95, 6, "stained_glass",
                    64),
                new ItemContext(
                    ItemType.GrayStainedGlass, 95, 7, "stained_glass",
                    64),
                new ItemContext(
                    ItemType.LightGrayStainedGlass, 95, 8, "stained_glass",
                    64),
                new ItemContext(
                    ItemType.CyanStainedGlass, 95, 9, "stained_glass",
                    64),
                new ItemContext(
                    ItemType.PurpleStainedGlass, 95, 10, "stained_glass",
                    64),
                new ItemContext(
                    ItemType.BlueStainedGlass, 95, 11, "stained_glass",
                    64),
                new ItemContext(
                    ItemType.BrownStainedGlass, 95, 12, "stained_glass",
                    64),
                new ItemContext(
                    ItemType.GreenStainedGlass, 95, 13, "stained_glass",
                    64),
                new ItemContext(
                    ItemType.RedStainedGlass, 95, 14, "stained_glass",
                    64),
                new ItemContext(
                    ItemType.BlackStainedGlass, 95, 15, "stained_glass",
                    64),

                new ItemContext(
                    ItemType.StoneMonsterEgg, 97, 0, "monster_egg",
                    64),
                new ItemContext(
                    ItemType.CobblestoneMonsterEgg, 97, 1, "monster_egg",
                    64),
                new ItemContext(
                    ItemType.StoneBrickMonsterEgg, 97, 2, "monster_egg",
                    64),
                new ItemContext(
                    ItemType.MossyStoneBrickMonsterEgg, 97, 3, "monster_egg",
                    64),
                new ItemContext(
                    ItemType.CrackedStoneBrickMonsterEgg, 97, 4, "monster_egg",
                    64),
                new ItemContext(
                    ItemType.ChiseledStoneBrickMonsterEgg, 97, 5, "monster_egg",
                    64),

                new ItemContext(
                    ItemType.StoneBricks, 98, 0, "stonebrick",
                    64),
                new ItemContext(
                    ItemType.MossyStoneBricks, 98, 1, "stonebrick",
                    64),
                new ItemContext(
                    ItemType.CrackedStoneBricks, 98, 2, "stonebrick",
                    64),
                new ItemContext(
                    ItemType.ChiseledStoneBricks, 98, 3, "stonebrick",
                    64),
                new ItemContext(
                    ItemType.BrownMushroomBlock, 99, 0, "brown_mushroom_block",
                    64),
                new ItemContext(
                    ItemType.RedMushroomBlock, 100, 0, "red_mushroom_block",
                    64),

                new ItemContext(
                    ItemType.IronBars, 101, 0, "iron_bars",
                    64),
                new ItemContext(
                    ItemType.GlassPane, 102, 0, "glass_pane",
                    64),
                new ItemContext(
                    ItemType.MelonBlock, 103, 0, "melon_block",
                    64),

                new ItemContext(
                    ItemType.Vines, 106, 0, "vine",
                    64),

                new ItemContext(
                    ItemType.StoneBrickStairs, 109, 0, "stone_brick_stairs",
                    64),

                new ItemContext(
                    ItemType.InactiveRedstoneLamp, 123, 0, "redstone_lamp",
                    64),

                // It is not working...
                //new ItemContext(
                //    ItemType.ActiveRedstoneLamp, 124, 0, "lit_redstone_lamp",
                //    64),

                new ItemContext(
                    ItemType.DoubleOakWoodSlab, 125, 0, "double_wooden_slab",
                    64),
                new ItemContext(
                    ItemType.DoubleSpruceWoodSlab, 125, 1, "double_wooden_slab",
                    64),
                new ItemContext(
                    ItemType.DoubleBirchWoodSlab, 125, 2, "double_wooden_slab",
                    64),
                new ItemContext(
                    ItemType.DoubleJungleWoodSlab, 125, 3, "double_wooden_slab",
                    64),
                new ItemContext(
                    ItemType.DoubleAcaciaWoodSlab, 125, 4, "double_wooden_slab",
                    64),
                new ItemContext(
                    ItemType.DoubleDarkOakWoodSlab, 125, 5, "double_wooden_slab",
                    64),

                new ItemContext(
                    ItemType.OakWoodSlab, 126, 0, "wooden_slab",
                    64),

                new ItemContext(
                    ItemType.SandstoneStairs, 128, 0, "sandstone_stairs",
                    64),

                new ItemContext(
                    ItemType.EmeraldBlock, 133, 0, "emerald_block",
                    64),

                new ItemContext(
                    ItemType.SpruceWoodStairs, 134, 0, "spruce_stairs",
                    64),
                new ItemContext(
                    ItemType.BirchWoodStairs, 135, 0, "birch_stairs",
                    64),
                new ItemContext(
                    ItemType.JungleWoodStairs, 136, 0, "jungle_stairs",
                    64),

                new ItemContext(
                    ItemType.CobblestoneWall, 139, 0, "cobblestone_wall",
                    64),
                new ItemContext(
                    ItemType.MossyCobblestoneWall, 139, 1, "cobblestone_wall",
                    64),

                new ItemContext(
                    ItemType.RedstoneBlock, 152, 0, "redstone_block",
                    64),

                new ItemContext(
                    ItemType.WhiteTerracotta, 159, 0, "stained_hardened_clay",
                    64),
                new ItemContext(
                    ItemType.OrangeTerracotta, 159, 1, "stained_hardened_clay",
                    64),
                new ItemContext(
                    ItemType.MagentaTerracotta, 159, 2, "stained_hardened_clay",
                    64),
                new ItemContext(
                    ItemType.LightBlueTerracotta, 159, 3, "stained_hardened_clay",
                    64),
                new ItemContext(
                    ItemType.YellowTerracotta, 159, 4, "stained_hardened_clay",
                    64),
                new ItemContext(
                    ItemType.LimeTerracotta, 159, 5, "stained_hardened_clay",
                    64),
                new ItemContext(
                    ItemType.PinkTerracotta, 159, 6, "stained_hardened_clay",
                    64),
                new ItemContext(
                    ItemType.GrayTerracotta, 159, 7, "stained_hardened_clay",
                    64),
                new ItemContext(
                    ItemType.LightGrayTerracotta, 159, 8, "stained_hardened_clay",
                    64),
                new ItemContext(
                    ItemType.CyanTerracotta, 159, 9, "stained_hardened_clay",
                    64),
                new ItemContext(
                    ItemType.PurpleTerracotta, 159, 10, "stained_hardened_clay",
                    64),
                new ItemContext(
                    ItemType.BlueTerracotta, 159, 11, "stained_hardened_clay",
                    64),
                new ItemContext(
                    ItemType.BrownTerracotta, 159, 12, "stained_hardened_clay",
                    64),
                new ItemContext(
                    ItemType.GreenTerracotta, 159, 13, "stained_hardened_clay",
                    64),
                new ItemContext(
                    ItemType.RedTerracotta, 159, 14, "stained_hardened_clay",
                    64),
                new ItemContext(
                    ItemType.BlackTerracotta, 159, 15, "stained_hardened_clay",
                    64),
                new ItemContext(
                    ItemType.WhiteStainedGlassPane, 160, 0, "stained_glass_pane",
                    64),
                new ItemContext(
                    ItemType.OrangeStainedGlassPane, 160, 1, "stained_glass_pane",
                    64),
                new ItemContext(
                    ItemType.MagentaStainedGlassPane, 160, 2, "stained_glass_pane",
                    64),
                new ItemContext(
                    ItemType.LightBlueStainedGlassPane, 160, 3, "stained_glass_pane",
                    64),
                new ItemContext(
                    ItemType.YellowStainedGlassPane, 160, 4, "stained_glass_pane",
                    64),
                new ItemContext(
                    ItemType.LimeStainedGlassPane, 160, 5, "stained_glass_pane",
                    64),
                new ItemContext(
                    ItemType.PinkStainedGlassPane, 160, 6, "stained_glass_pane",
                    64),
                new ItemContext(
                    ItemType.GrayStainedGlassPane, 160, 7, "stained_glass_pane",
                    64),
                new ItemContext(
                    ItemType.LightGrayStainedGlassPane, 160, 8, "stained_glass_pane",
                    64),
                new ItemContext(
                    ItemType.CyanStainedGlassPane, 160, 9, "stained_glass_pane",
                    64),
                new ItemContext(
                    ItemType.PurpleStainedGlassPane, 160, 10, "stained_glass_pane",
                    64),
                new ItemContext(
                    ItemType.BlueStainedGlassPane, 160, 11, "stained_glass_pane",
                    64),
                new ItemContext(
                    ItemType.BrownStainedGlassPane, 160, 12, "stained_glass_pane",
                    64),
                new ItemContext(
                    ItemType.GreenStainedGlassPane, 160, 13, "stained_glass_pane",
                    64),
                new ItemContext(
                    ItemType.RedStainedGlassPane, 160, 14, "stained_glass_pane",
                    64),
                new ItemContext(
                    ItemType.BlackStainedGlassPane, 160, 15, "stained_glass_pane",
                    64),

                new ItemContext(
                    ItemType.HayBale, 170, 0, "hay_block",
                    64),

                new ItemContext(
                    ItemType.WhiteCarpet, 171, 0, "carpet",
                    64),
                new ItemContext(
                    ItemType.OrangeCarpet, 171, 1, "carpet",
                    64),
                new ItemContext(
                    ItemType.MagentaCarpet, 171, 2, "carpet",
                    64),
                new ItemContext(
                    ItemType.LightBlueCarpet, 171, 3, "carpet",
                    64),
                new ItemContext(
                    ItemType.YellowCarpet, 171, 4, "carpet",
                    64),
                new ItemContext(
                    ItemType.LimeCarpet, 171, 5, "carpet",
                    64),
                new ItemContext(
                    ItemType.PinkCarpet, 171, 6, "carpet",
                    64),
                new ItemContext(
                    ItemType.GrayCarpet, 171, 7, "carpet",
                    64),
                new ItemContext(
                    ItemType.LightGrayCarpet, 171, 8, "carpet",
                    64),
                new ItemContext(
                    ItemType.CyanCarpet, 171, 9, "carpet",
                    64),
                new ItemContext(
                    ItemType.PurpleCarpet, 171, 10, "carpet",
                    64),
                new ItemContext(
                    ItemType.BlueCarpet, 171, 11, "carpet",
                    64),
                new ItemContext(
                    ItemType.BrownCarpet, 171, 12, "carpet",
                    64),
                new ItemContext(
                    ItemType.GreenCarpet, 171, 13, "carpet",
                    64),
                new ItemContext(
                    ItemType.RedCarpet, 171, 14, "carpet",
                    64),
                new ItemContext(
                    ItemType.BlackCarpet, 171, 15, "carpet",
                    64),

                new ItemContext(
                    ItemType.IronSword, 267, 0, "iron_sword",
                    1),
                new ItemContext(
                    ItemType.WoodenSword, 268, 0, "wooden_sword",
                    1),
                new ItemContext(
                    ItemType.StoneSword, 272, 0, "stone_sword",
                    1),
                new ItemContext(
                    ItemType.DiamondSword, 276, 0, "diamond_sword",
                    1),
                new ItemContext(
                    ItemType.DiamondShovel, 277, 0, "diamond_shovel",
                    1),
                new ItemContext(
                    ItemType.DiamondPickaxe, 278, 0, "diamond_pickaxe",
                    1),
                new ItemContext(
                    ItemType.DiamondAxe, 279, 0, "diamond_axe",
                    1),
                new ItemContext(
                    ItemType.Stick, 280, 0, "stick",
                    64),
                new ItemContext(
                    ItemType.GoldenSword, 283, 0, "golden_sword",
                    1),
                new ItemContext(
                    ItemType.Feather, 288, 0, "feather",
                    64),
                new ItemContext(
                    ItemType.Flint, 318, 0, "flint",
                    64),
                new ItemContext(
                    ItemType.Sign, 323, 0, "sign",
                    64),
                //new ItemContext(
                //    ItemType.Snowball, 3320, , "snowball",
                //    1, 16),
                
                new ItemContext(
                    ItemType.Paper, 339, 0, "paper",
                    64),

                new ItemContext(
                    ItemType.RedstoneRepeater, 356, 0, "repeater",
                    64),

                new ItemContext(
                    ItemType.GoldNugget, 371, 0, "gold_nugget",
                    64),
                new ItemContext(
                    ItemType.EyeOfEnder, 381, 0, "ender_eye",
                    64),
                new ItemContext(
                    ItemType.PlayerSkull, 397, 3, "skull",
                    64),

                new ItemContext(
                    ItemType.ArmorStand, 416, 0, "armor_stand",
                    16),

                new ItemContext(
                    ItemType.IronHorseArmor, 417, 0, "iron_horse_armor",
                    1),
                new ItemContext(
                    ItemType.GoldenHorseArmor, 418, 0, "golden_horse_armor",
                    1),
                new ItemContext(
                    ItemType.DiamondHorseArmor, 419, 0, "diamond_horse_armor",
                    1),

                new ItemContext(
                    ItemType.EndCrystal, 426, 0, "end_crystal",
                    64),

                new ItemContext(
                    ItemType.MusicDisc_C418_13, 2256, 0, "record_13",
                    1),
                new ItemContext(
                    ItemType.MusicDisc_C418_cat, 2257, 0, "record_cat",
                    1),
                new ItemContext(
                    ItemType.MusicDisc_C418_blocks, 2258, 0, "record_blocks",
                    1),
                new ItemContext(
                    ItemType.MusicDisc_C418_chirp, 2259, 0, "record_chirp",
                    1),
                new ItemContext(
                    ItemType.MusicDisc_C418_far, 2260, 0, "record_far",
                    1),
                new ItemContext(
                    ItemType.MusicDisc_C418_mall, 2261, 0, "record_mall",
                    1),
                new ItemContext(
                    ItemType.MusicDisc_C418_mellohi, 2262, 0, "record_mellohi",
                    1),
                new ItemContext(
                    ItemType.MusicDisc_C418_stal, 2263, 0, "record_stal",
                    1),
                new ItemContext(
                    ItemType.MusicDisc_C418_strad, 2264, 0, "record_strad",
                    1),
                new ItemContext(
                    ItemType.MusicDisc_C418_ward, 2265, 0, "record_ward",
                    1),
                new ItemContext(
                    ItemType.MusicDisc_C418_11, 2266, 0, "record_11",
                    1),
                new ItemContext(
                    ItemType.MusicDisc_C418_wait, 2267, 0, "record_wait",
                    1),

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


using Common;
using Containers;
using System.ComponentModel;
using System.Net.NetworkInformation;
using System;
using static System.Formats.Asn1.AsnWriter;
using System.Diagnostics.Metrics;
using static MinecraftPrimitives.Packet;
using System.Runtime.InteropServices;

namespace MinecraftServerEngine
{
    internal static class BlockExtensions
    {
        private struct BlockContext
        {
            public Block Block { get; }
            public int Id { get; }
            public string Name;
            public BlockShape Shape { get; }
            public BlockContext(
                Block block,
                int id,
                string name,
                BlockShape shape)
            {
                Block = block;
                Id = id;
                Name = name;
                Shape = shape;
            }

        }

        private readonly static Table<Block, BlockContext> _BLOCK_ENUM_TO_CTX_MAP = new();
        private readonly static Table<int, Block> _BLOCK_ID_TO_ENUM_MAP = new();

        static BlockExtensions()
        {
            BlockContext[] _map =
            {
                new BlockContext(
                    Block.Air,
                    (0 << 4) | 0,
                    "air",
                    BlockShape.None),
                new BlockContext(
                    Block.Stone,
                    (1 << 4) | 0,
                    "stone",
                    BlockShape.Cube),
                new BlockContext(
                    Block.Granite,
                    (1 << 4) | 1,
                    "stone",
                    BlockShape.Cube),
                new BlockContext(
                    Block.PolishedGranite,
                    (1 << 4) | 2,
                    "stone",
                    BlockShape.Cube),
                new BlockContext(
                    Block.Diorite,
                    (1 << 4) | 3,
                    "stone",
                    BlockShape.Cube),
                new BlockContext(
                    Block.PolishedDiorite,
                    (1 << 4) | 4,
                    "stone",
                    BlockShape.Cube),
                new BlockContext(
                    Block.Andesite,
                    (1 << 4) | 5,
                    "stone",
                    BlockShape.Cube),
                new BlockContext(
                    Block.PolishedAndesite,
                    (1 << 4) | 6,
                    "stone",
                    BlockShape.Cube),
                new BlockContext(
                    Block.GrassBlock,
                    (2 << 4) | 0,
                    "grass",
                    BlockShape.Cube),
                new BlockContext(
                    Block.Dirt,
                    (3 << 4) | 0,
                    "dirt",
                    BlockShape.Cube),
                new BlockContext(
                    Block.CoarseDirt,
                    (3 << 4) | 1,
                    "dirt",
                    BlockShape.Cube),
                new BlockContext(
                    Block.Podzol,
                    (3 << 4) | 2,
                    "dirt",
                    BlockShape.Cube),
                new BlockContext(
                    Block.Cobblestone,
                    (4 << 4) | 0,
                    "cobblestone",
                    BlockShape.Cube),
                new BlockContext(
                    Block.OakWoodPlanks,
                    (5 << 4) | 0,
                    "planks",
                    BlockShape.Cube),
                new BlockContext(
                    Block.SpruceWoodPlanks,
                    (5 << 4) | 1,
                    "planks",
                    BlockShape.Cube),
                new BlockContext(
                    Block.BirchWoodPlanks,
                    (5 << 4) | 2,
                    "planks",
                    BlockShape.Cube),
                new BlockContext(
                    Block.JungleWoodPlanks,
                    (5 << 4) | 3,
                    "planks",
                    BlockShape.Cube),
                new BlockContext(
                    Block.AcaciaWoodPlanks,
                    (5 << 4) | 4,
                    "planks",
                    BlockShape.Cube),
                new BlockContext(
                    Block.DarkOakWoodPlanks,
                    (5 << 4) | 5,
                    "planks",
                    BlockShape.Cube),
                new BlockContext(
                    Block.OakSapling,
                    (6 << 4) | 0,
                    "sapling",
                    BlockShape.None),
                new BlockContext(
                    Block.SpruceSapling,
                    (6 << 4) | 1,
                    "sapling",
                    BlockShape.None),
                new BlockContext(
                    Block.BirchSapling,
                    (6 << 4) | 2,
                    "sapling",
                    BlockShape.None),
                new BlockContext(
                    Block.JungleSapling,
                    (6 << 4) | 3,
                    "sapling",
                    BlockShape.None),
                new BlockContext(
                    Block.AcaciaSapling,
                    (6 << 4) | 4,
                    "sapling",
                    BlockShape.None),
                new BlockContext(
                    Block.DarkOakSapling,
                    (6 << 4) | 5,
                    "sapling",
                    BlockShape.None),
                new BlockContext(
                    Block.Bedrock,
                    (7 << 4) | 0,
                    "bedrock",
                    BlockShape.Cube),

                new BlockContext(
                    Block.FlowingWater,
                    (8 << 4) | 0,
                    "flowing_water",
                    BlockShape.None),
                new BlockContext(
                    Block.StillWater,
                    (9 << 4) | 0,
                    "water",
                    BlockShape.None),
                new BlockContext(
                    Block.FlowingLava,
                    (10 << 4) | 0,
                    "flowing_lava",
                    BlockShape.None),
                new BlockContext(
                    Block.StillLava,
                    (11 << 4) | 0,
                    "lava",
                    BlockShape.None),
                new BlockContext(
                    Block.Sand,
                    (12 << 4) | 0,
                    "sand",
                    BlockShape.Cube),
                new BlockContext(
                    Block.RedSand,
                    (12 << 4) | 1,
                    "sand",
                    BlockShape.Cube),
                new BlockContext(
                    Block.Gravel,
                    (13 << 4) | 0,
                    "gravel",
                    BlockShape.Cube),
                new BlockContext(
                    Block.GoldOre,
                    (14 << 4) | 0,
                    "gold_ore",
                    BlockShape.Cube),
                new BlockContext(
                    Block.IronOre,
                    (15 << 4) | 0,
                    "iron_ore",
                    BlockShape.Cube),
                new BlockContext(
                    Block.CoalOre,
                    (16 << 4) | 0,
                    "coal_ore",
                    BlockShape.Cube),
                new BlockContext(
                    Block.OakWood,
                    (17 << 4) | 0,
                    "log",
                    BlockShape.Cube),
                new BlockContext(
                    Block.SpruceWood,
                    (17 << 4) | 1,
                    "log",
                    BlockShape.Cube),
                new BlockContext(
                    Block.BirchWood,
                    (17 << 4) | 2,
                    "log",
                    BlockShape.Cube),
                new BlockContext(
                    Block.JungleWood,
                    (17 << 4) | 3,
                    "log",
                    BlockShape.Cube),
                new BlockContext(
                    Block.OakLeaves,
                    (18 << 4) | 0,
                    "leaves",
                    BlockShape.Cube),
                new BlockContext(
                    Block.SpruceLeaves,
                    (18 << 4) | 1,
                    "leaves",
                    BlockShape.Cube),
                new BlockContext(
                    Block.BirchLeaves,
                    (18 << 4) | 2,
                    "leaves",
                    BlockShape.Cube),
                new BlockContext(
                    Block.JungleLeaves,
                    (18 << 4) | 3,
                    "leaves",
                    BlockShape.Cube),
                new BlockContext(
                    Block.Sponge,
                    (19 << 4) | 0,
                    "sponge",
                    BlockShape.Cube),
                new BlockContext(
                    Block.WetSponge,
                    (19 << 4) | 1,
                    "sponge",
                    BlockShape.Cube),
                new BlockContext(
                    Block.Glass,
                    (20 << 4) | 0,
                    "glass",
                    BlockShape.Cube),
                new BlockContext(
                    Block.LapisLazuliOre,
                    (21 << 4) | 0,
                    "lapis_ore",
                    BlockShape.Cube),
                new BlockContext(
                    Block.LapisLazuliBlock,
                    (22 << 4) | 0,
                    "lapis_block",
                    BlockShape.Cube),
                new BlockContext(
                    Block.Dispenser,
                    (23 << 4) | 0,
                    "dispenser",
                    BlockShape.Cube),
                new BlockContext(
                    Block.Sandstone,
                    (24 << 4) | 0,
                    "sandstone",
                    BlockShape.Cube),
                new BlockContext(
                    Block.ChiseledSandstone,
                    (24 << 4) | 1,
                     "sandstone",
                     BlockShape.Cube),
                new BlockContext(
                    Block.SmoothSandstone,
                    (24 << 4) | 2,
                     "sandstone",
                     BlockShape.Cube),
                new BlockContext(
                    Block.NoteBlock,
                    (25 << 4) | 0,
                     "noteblock",
                     BlockShape.Cube),


                new BlockContext(
                    Block.BottomStickyPiston,
                    (29 << 4) | 0,
                    "sticky_piston",
                    BlockShape.Cube),
                new BlockContext(
                   Block.TopStickyPiston,
                   (29 << 4) | 1,
                   "sticky_piston",
                   BlockShape.Cube),
                new BlockContext(
                   Block.NorthStickyPiston,
                   (29 << 4) | 2,
                   "sticky_piston",
                   BlockShape.Cube),
                new BlockContext(
                   Block.SouthStickyPiston,
                   (29 << 4) | 3,
                   "sticky_piston",
                   BlockShape.Cube),
                new BlockContext(
                   Block.WestStickyPiston,
                   (29 << 4) | 4,
                   "sticky_piston",
                   BlockShape.Cube),
                new BlockContext(
                   Block.EastStickyPiston,
                   (29 << 4) | 5,
                   "sticky_piston",
                   BlockShape.Cube),



                new BlockContext(
                    Block.DeadShrub,
                    (31 << 4) | 0,
                    "tallgrass",
                    BlockShape.None),
                new BlockContext(
                    Block.Grass,
                    (31 << 4) | 1,
                    "tallgrass",
                    BlockShape.None),
                new BlockContext(
                    Block.Fern,
                    (31 << 4) | 2,
                    "tallgrass",
                    BlockShape.None),
                new BlockContext(
                    Block.DeadBush,
                    (32 << 4) | 0,
                    "deadbush",
                    BlockShape.None),
                new BlockContext(
                    Block.BottomPiston,
                    (33 << 4) | 0,
                    "piston",
                    BlockShape.Cube),
                new BlockContext(
                    Block.TopPiston,
                    (33 << 4) | 1,
                    "piston",
                    BlockShape.Cube),
                new BlockContext(
                    Block.NorthPiston,
                    (33 << 4) | 2,
                    "piston",
                    BlockShape.Cube),
                new BlockContext(
                    Block.SouthPiston,
                    (33 << 4) | 3,
                    "piston",
                    BlockShape.Cube),
                new BlockContext(
                    Block.WestPiston,
                    (33 << 4) | 4,
                    "piston",
                    BlockShape.Cube),
                new BlockContext(
                    Block.EastPiston,
                    (33 << 4) | 5,
                    "piston",
                    BlockShape.Cube),


                new BlockContext(
                    Block.WhiteWool,
                    (35 << 4) | 0,
                    "wool",
                    BlockShape.Cube),
                new BlockContext(
                    Block.OrangeWool,
                    (35 << 4) | 1,
                    "wool",
                    BlockShape.Cube),
                new BlockContext(
                    Block.MagentaWool,
                    (35 << 4) | 2,
                    "wool",
                    BlockShape.Cube),
                new BlockContext(
                    Block.LightBlueWool,
                    (35 << 4) | 3,
                    "wool",
                    BlockShape.Cube),
                new BlockContext(
                    Block.YellowWool,
                    (35 << 4) | 4,
                    "wool",
                    BlockShape.Cube),
                new BlockContext(
                    Block.LimeWool,
                    (35 << 4) | 5,
                    "wool",
                    BlockShape.Cube),
                new BlockContext(
                    Block.PinkWool,
                    (35 << 4) | 6,
                    "wool",
                    BlockShape.Cube),
                new BlockContext(
                    Block.GrayWool,
                    (35 << 4) | 7,
                    "wool",
                    BlockShape.Cube),
                new BlockContext(
                    Block.LightGrayWool,
                    (35 << 4) | 8,
                    "wool",
                    BlockShape.Cube),
                new BlockContext(
                    Block.CyanWool,
                    (35 << 4) | 9,
                    "wool",
                    BlockShape.Cube),
                new BlockContext(
                    Block.PurpleWool,
                    (35 << 4) | 10,
                    "wool",
                    BlockShape.Cube),
                new BlockContext(
                    Block.BlueWool,
                    (35 << 4) | 11,
                    "wool",
                    BlockShape.Cube),
                new BlockContext(
                    Block.BrownWool,
                    (35 << 4) | 12,
                    "wool",
                    BlockShape.Cube),
                new BlockContext(
                    Block.GreenWool,
                    (35 << 4) | 13,
                    "wool",
                    BlockShape.Cube),
                new BlockContext(
                    Block.RedWool,
                    (35 << 4) | 14,
                    "wool",
                    BlockShape.Cube),
                new BlockContext(
                    Block.BlackWool,
                    (35 << 4) | 15,
                    "wool",
                    BlockShape.Cube),


                new BlockContext(
                    Block.Dandelion,
                    (37 << 4) | 0,
                    "yellow_flower",
                    BlockShape.None),
                new BlockContext(
                    Block.Poppy,
                    (38 << 4) | 0,
                    "red_flower",
                    BlockShape.None),
                new BlockContext(
                    Block.BlueOrchid,
                    (38 << 4) | 1,
                    "red_flower",
                    BlockShape.None),
                new BlockContext(
                    Block.Allium,
                    (38 << 4) | 2,
                    "red_flower",
                    BlockShape.None),
                new BlockContext(
                    Block.AzureBluet,
                    (38 << 4) | 3,
                    "red_flower",
                    BlockShape.None),
                new BlockContext(
                    Block.RedTulip,
                    (38 << 4) | 4,
                    "red_flower",
                    BlockShape.None),
                new BlockContext(
                    Block.OrangeTulip,
                    (38 << 4) | 5,
                    "red_flower",
                    BlockShape.None),
                new BlockContext(
                    Block.WhiteTulip,
                    (38 << 4) | 6,
                    "red_flower",
                    BlockShape.None),
                new BlockContext(
                    Block.PinkTulip,
                    (38 << 4) | 7,
                    "red_flower",
                    BlockShape.None),
                new BlockContext(
                    Block.OxeyeDaisy,
                    (38 << 4) | 8,
                    "red_flower",
                    BlockShape.None),
                new BlockContext(
                    Block.BrownMushroom,
                    (39 << 4) | 0,
                    "brown_mushroom",
                    BlockShape.None),
                new BlockContext(
                    Block.RedMushroom,
                    (40 << 4) | 0,
                    "red_mushroom",
                    BlockShape.None),

                new BlockContext(
                    Block.GoldBlock,
                    (41 << 4) | 0,
                    "gold_block",
                    BlockShape.Cube),
                new BlockContext(
                    Block.IronBlock,
                    (42 << 4) | 0,
                    "iron_block",
                    BlockShape.Cube),
                new BlockContext(
                    Block.DoubleStoneSlab,
                    (43 << 4) | 0,
                    "double_stone_slab",
                    BlockShape.Cube),
                new BlockContext(
                    Block.DoubleSandstoneSlab,
                    (43 << 4) | 1,
                    "double_stone_slab",
                    BlockShape.Cube),
                new BlockContext(
                    Block.DoubleWoodenSlab,
                    (43 << 4) | 2,
                    "double_stone_slab",
                    BlockShape.Cube),
                new BlockContext(
                    Block.DoubleCobblestoneSlab,
                    (43 << 4) | 3,
                    "double_stone_slab",
                    BlockShape.Cube),
                new BlockContext(
                    Block.DoubleBrickSlab,
                    (43 << 4) | 4,
                    "double_stone_slab",
                    BlockShape.Cube),
                new BlockContext(
                    Block.DoubleStoneBrickSlab,
                    (43 << 4) | 5,
                    "double_stone_slab",
                    BlockShape.Cube),
                new BlockContext(
                    Block.DoubleNetherBrickSlab,
                    (43 << 4) | 6,
                    "double_stone_slab",
                    BlockShape.Cube),
                new BlockContext(
                    Block.DoubleQuartzSlab,
                    (43 << 4) | 7,
                    "double_stone_slab",
                    BlockShape.Cube),


                new BlockContext(
                    Block.StoneBottomSlab,
                    (44 << 4) | 0,
                    "stone_slab",
                    BlockShape.Slab),
                new BlockContext(
                    Block.SandstoneBottomSlab,
                    (44 << 4) | 1,
                    "stone_slab",
                    BlockShape.Slab),
                new BlockContext(
                    Block.WoodenBottomSlab,
                    (44 << 4) | 2,
                    "stone_slab",
                    BlockShape.Slab),
                new BlockContext(
                    Block.CobblestoneBottomSlab,
                    (44 << 4) | 3,
                    "stone_slab",
                    BlockShape.Slab),
                new BlockContext(
                    Block.BrickBottomSlab,
                    (44 << 4) | 4,
                    "stone_slab",
                    BlockShape.Slab),
                new BlockContext(
                    Block.StoneBrickBottomSlab,
                    (44 << 4) | 5,
                    "stone_slab",
                    BlockShape.Slab),
                new BlockContext(
                    Block.NetherBrickBottomSlab,
                    (44 << 4) | 6,
                    "stone_slab",
                    BlockShape.Slab),
                new BlockContext(
                    Block.QuartzBottomSlab,
                    (44 << 4) | 7,
                    "stone_slab",
                    BlockShape.Slab),

                new BlockContext(
                    Block.StoneTopSlab,
                    (44 << 4) | 8,
                    "stone_slab",
                    BlockShape.Slab),
                new BlockContext(
                    Block.SandstoneTopSlab,
                    (44 << 4) | 9,
                    "stone_slab",
                    BlockShape.Slab),
                new BlockContext(
                    Block.WoodenTopSlab,
                    (44 << 4) | 10,
                    "stone_slab",
                    BlockShape.Slab),
                new BlockContext(
                    Block.CobblestoneTopSlab,
                    (44 << 4) | 11,
                    "stone_slab",
                    BlockShape.Slab),
                new BlockContext(
                    Block.BrickTopSlab,
                    (44 << 4) | 12,
                    "stone_slab",
                    BlockShape.Slab),
                new BlockContext(
                    Block.StoneBrickTopSlab,
                    (44 << 4) | 13,
                    "stone_slab",
                    BlockShape.Slab),
                new BlockContext(
                    Block.NetherBrickTopSlab,
                    (44 << 4) | 14,
                    "stone_slab",
                    BlockShape.Slab),
                new BlockContext(
                    Block.QuartzTopSlab,
                    (44 << 4) | 15,
                    "stone_slab",
                    BlockShape.Slab),

                new BlockContext(
                    Block.Bricks,
                    (45 << 4) | 0,
                    "brick_block",
                    BlockShape.Cube),
                new BlockContext(
                    Block.TNT,
                    (46 << 4) | 0,
                    "tnt",
                    BlockShape.Cube),
                new BlockContext(
                    Block.Bookshelf,
                    (47 << 4) | 0,
                    "bookshelf",
                    BlockShape.Cube),
                new BlockContext(
                    Block.MossStone,
                    (48 << 4) | 0,
                    "mossy_cobblestone",
                    BlockShape.Cube),
                new BlockContext(
                    Block.Obsidian,
                    (49 << 4) | 0,
                    "obsidian",
                    BlockShape.Cube),


                new BlockContext(
                    Block.Torch,
                    (50 << 4) | 0,
                    "torch",
                    BlockShape.None),
                new BlockContext(
                    Block.Fire,
                    (51 << 4) | 0,
                    "fire",
                    BlockShape.None),


                new BlockContext(
                    Block.EastBottomOakWoodStairs,
                    (53 << 4) | 0,
                    "oak_stairs",
                    BlockShape.Stairs),
                new BlockContext(
                    Block.WestBottomOakWoodStairs,
                    (53 << 4) | 1,
                    "oak_stairs",
                    BlockShape.Stairs),
                new BlockContext(
                    Block.SouthBottomOakWoodStairs,
                    (53 << 4) | 2,
                    "oak_stairs",
                    BlockShape.Stairs),
                new BlockContext(
                    Block.NorthBottomOakWoodStairs,
                    (53 << 4) | 3,
                    "oak_stairs",
                    BlockShape.Stairs),
                new BlockContext(
                    Block.EastTopOakWoodStairs,
                    (53 << 4) | 4,
                    "oak_stairs",
                    BlockShape.Stairs),
                new BlockContext(
                    Block.WestTopOakWoodStairs,
                    (53 << 4) | 5,
                    "oak_stairs",
                    BlockShape.Stairs),
                new BlockContext(
                    Block.SouthTopOakWoodStairs,
                    (53 << 4) | 6,
                    "oak_stairs",
                    BlockShape.Stairs),
                new BlockContext(
                    Block.NorthTopOakWoodStairs,
                    (53 << 4) | 7,
                    "oak_stairs",
                    BlockShape.Stairs),

                new BlockContext(
                    Block.DiamondBlock,
                    (57 << 4) | 0,
                    "diamond_block",
                    BlockShape.Cube),
                new BlockContext(
                    Block.CraftingTable,
                    (58 << 4) | 0,
                    "crafting_table",
                    BlockShape.Cube),
                new BlockContext(
                    Block.WheatCrops,
                    (59 << 4) | 0,
                    "wheat",
                    BlockShape.None),
                new BlockContext(
                    Block.Farmland,
                    (60 << 4) | 0,
                    "farmland",
                    BlockShape.Cube),

                new BlockContext(
                    Block.EastBottomCobblestoneStairs,
                    (67 << 4) | 0,
                    "stone_stairs",
                    BlockShape.Stairs),
                new BlockContext(
                    Block.WestBottomCobblestoneStairs,
                    (67 << 4) | 1,
                    "stone_stairs",
                    BlockShape.Stairs),
                new BlockContext(
                    Block.SouthBottomCobblestoneStairs,
                    (67 << 4) | 2,
                    "stone_stairs",
                    BlockShape.Stairs),
                new BlockContext(
                    Block.NorthBottomCobblestoneStairs,
                    (67 << 4) | 3,
                    "stone_stairs",
                    BlockShape.Stairs),
                new BlockContext(
                    Block.EastTopCobblestoneStairs,
                    (67 << 4) | 4,
                    "stone_stairs",
                    BlockShape.Stairs),
                new BlockContext(
                    Block.WestTopCobblestoneStairs,
                    (67 << 4) | 5,
                    "stone_stairs",
                    BlockShape.Stairs),
                new BlockContext(
                    Block.SouthTopCobblestoneStairs,
                    (67 << 4) | 6,
                    "stone_stairs",
                    BlockShape.Stairs),
                new BlockContext(
                    Block.NorthTopCobblestoneStairs,
                    (67 << 4) | 7,
                    "stone_stairs",
                    BlockShape.Stairs),

                new BlockContext(
                    Block.OakFence,
                    (85 << 4) | 0,
                    "fence",
                    BlockShape.Fence),

                new BlockContext(
                    Block.SouthPumpkin,
                    (86 << 4) | 0,
                    "pumpkin",
                    BlockShape.Cube),
                new BlockContext(
                    Block.WestPumpkin,
                    (86 << 4) | 1,
                    "pumpkin",
                    BlockShape.Cube),
                new BlockContext(
                    Block.NorthPumpkin,
                    (86 << 4) | 2,
                    "pumpkin",
                    BlockShape.Cube),
                new BlockContext(
                    Block.EastPumpkin,
                    (86 << 4) | 3,
                    "pumpkin",
                    BlockShape.Cube),


                new BlockContext(
                    Block.WhiteStainedGlass,
                    (95 << 4) | 0,
                    "stained_glass",
                    BlockShape.Cube),
                new BlockContext(
                    Block.OrangeStainedGlass,
                    (95 << 4) | 1,
                    "stained_glass",
                    BlockShape.Cube),
                new BlockContext(
                    Block.MagentaStainedGlass,
                    (95 << 4) | 2,
                    "stained_glass",
                    BlockShape.Cube),
                new BlockContext(
                    Block.LightBlueStainedGlass,
                    (95 << 4) | 3,
                    "stained_glass",
                    BlockShape.Cube),
                new BlockContext(
                    Block.YellowStainedGlass,
                    (95 << 4) | 4,
                    "stained_glass",
                    BlockShape.Cube),
                new BlockContext(
                    Block.LimeStainedGlass,
                    (95 << 4) | 5,
                    "stained_glass",
                    BlockShape.Cube),
                new BlockContext(
                    Block.PinkStainedGlass,
                    (95 << 4) | 6,
                    "stained_glass",
                    BlockShape.Cube),
                new BlockContext(
                    Block.GrayStainedGlass,
                    (95 << 4) | 7,
                    "stained_glass",
                    BlockShape.Cube),
                new BlockContext(
                    Block.LightGrayStainedGlass,
                    (95 << 4) | 8,
                    "stained_glass",
                    BlockShape.Cube),
                new BlockContext(
                    Block.CyanStainedGlass,
                    (95 << 4) | 9,
                    "stained_glass",
                    BlockShape.Cube),
                new BlockContext(
                    Block.PurpleStainedGlass,
                    (95 << 4) | 10,
                    "stained_glass",
                    BlockShape.Cube),
                new BlockContext(
                    Block.BlueStainedGlass,
                    (95 << 4) | 11,
                    "stained_glass",
                    BlockShape.Cube),
                new BlockContext(
                    Block.BrownStainedGlass,
                    (95 << 4) | 12,
                    "stained_glass",
                    BlockShape.Cube),
                new BlockContext(
                    Block.GreenStainedGlass,
                    (95 << 4) | 13,
                    "stained_glass",
                    BlockShape.Cube),
                new BlockContext(
                    Block.RedStainedGlass,
                    (95 << 4) | 14,
                    "stained_glass",
                    BlockShape.Cube),
                new BlockContext(
                    Block.BlackStainedGlass,
                    (95 << 4) | 15,
                    "stained_glass",
                    BlockShape.Cube),


                new BlockContext(
                    Block.StoneMonsterEgg,
                    (97 << 4) | 0,
                    "monster_egg",
                    BlockShape.Cube),
                new BlockContext(
                    Block.CobblestoneMonsterEgg,
                    (97 << 4) | 1,
                    "monster_egg",
                    BlockShape.Cube),
                new BlockContext(
                    Block.StoneBrickMonsterEgg,
                    (97 << 4) | 2,
                    "monster_egg",
                    BlockShape.Cube),
                new BlockContext(
                    Block.MossyStoneBrickMonsterEgg,
                    (97 << 4) | 3,
                    "monster_egg",
                    BlockShape.Cube),
                new BlockContext(
                    Block.CrackedStoneBrickMonsterEgg,
                    (97 << 4) | 4,
                    "monster_egg",
                    BlockShape.Cube),
                new BlockContext(
                    Block.ChiseledStoneBrickMonsterEgg,
                    (97 << 4) | 5,
                    "monster_egg",
                    BlockShape.Cube),


                new BlockContext(
                    Block.StoneBricks,
                    (98 << 4) | 0,
                    "stonebrick",
                    BlockShape.Cube),
                new BlockContext(
                    Block.MossyStoneBricks,
                    (98 << 4) | 1,
                    "stonebrick",
                    BlockShape.Cube),
                new BlockContext(
                    Block.CrackedStoneBricks,
                    (98 << 4) | 2,
                    "stonebrick",
                    BlockShape.Cube),
                new BlockContext(
                    Block.ChiseledStoneBricks,
                    (98 << 4) | 3,
                    "stonebrick",
                    BlockShape.Cube),
                new BlockContext(
                    Block.BrownMushroomBlock,
                    (99 << 4) | 0,
                    "brown_mushroom_block",
                    BlockShape.Cube),
                new BlockContext(
                    Block.RedMushroomBlock,
                    (100 << 4) | 0,
                    "red_mushroom_block",
                    BlockShape.Cube),

                new BlockContext(
                    Block.IronBars,
                    (101 << 4) | 0,
                    "iron_bars",
                    BlockShape.Bars),
                new BlockContext(
                    Block.GlassPane,
                    (102 << 4) | 0,
                    "glass_pane",
                    BlockShape.Bars),
                new BlockContext(
                    Block.MelonBlock,
                    (103 << 4) | 0,
                    "melon_block",
                    BlockShape.Cube),


                new BlockContext(
                    Block.InactiveRedstoneLamp,
                    (123 << 4) | 0,
                    "redstone_lamp",
                    BlockShape.Cube),
                new BlockContext(
                    Block.ActiveRedstoneLamp,
                    (124 << 4) | 0,
                    "lit_redstone_lamp",
                    BlockShape.Cube),

                new BlockContext(
                    Block.DoubleOakWoodSlab,
                    (125 << 4) | 0,
                    "double_wooden_slab",
                    BlockShape.Cube),
                new BlockContext(
                    Block.DoubleSpruceWoodSlab,
                    (125 << 4) | 1,
                    "double_wooden_slab",
                    BlockShape.Cube),
                new BlockContext(
                    Block.DoubleBirchWoodSlab,
                    (125 << 4) | 2,
                    "double_wooden_slab",
                    BlockShape.Cube),
                new BlockContext(
                    Block.DoubleJungleWoodSlab,
                    (125 << 4) | 3,
                    "double_wooden_slab",
                    BlockShape.Cube),
                new BlockContext(
                    Block.DoubleAcaciaWoodSlab,
                    (125 << 4) | 4,
                    "double_wooden_slab",
                    BlockShape.Cube),
                new BlockContext(
                    Block.DoubleDarkOakWoodSlab,
                    (125 << 4) | 5,
                    "double_wooden_slab",
                    BlockShape.Cube),

                new BlockContext(
                    Block.OakWoodSlab,
                    (126 << 4) | 0,
                    "wooden_slab",
                    BlockShape.Slab),
                new BlockContext(
                    Block.SpruceWoodSlab,
                    (126 << 4) | 1,
                    "wooden_slab",
                    BlockShape.Slab),
                new BlockContext(
                    Block.BirchWoodSlab,
                    (126 << 4) | 2,
                    "wooden_slab",
                    BlockShape.Slab),
                new BlockContext(
                    Block.JungleWoodSlab,
                    (126 << 4) | 3,
                    "wooden_slab",
                    BlockShape.Slab),
                new BlockContext(
                    Block.AcaciaWoodSlab,
                    (126 << 4) | 4,
                    "wooden_slab",
                    BlockShape.Slab),
                new BlockContext(
                    Block.DarkOakWoodSlab,
                    (126 << 4) | 5,
                    "wooden_slab",
                    BlockShape.Slab),

                new BlockContext(
                    Block.EastBottomSandstoneStairs,
                    (128 << 4) | 0,
                    "sandstone_stairs",
                    BlockShape.Stairs),
                new BlockContext(
                    Block.WestBottomSandstoneStairs,
                    (128 << 4) | 1,
                    "sandstone_stairs",
                    BlockShape.Stairs),
                new BlockContext(
                    Block.SouthBottomSandstoneStairs,
                    (128 << 4) | 2,
                    "sandstone_stairs",
                    BlockShape.Stairs),
                new BlockContext(
                    Block.NorthBottomSandstoneStairs,
                    (128 << 4) | 3,
                    "sandstone_stairs",
                    BlockShape.Stairs),
                new BlockContext(
                    Block.EastTopSandstoneStairs,
                    (128 << 4) | 4,
                    "sandstone_stairs",
                    BlockShape.Stairs),
                new BlockContext(
                    Block.WestTopSandstoneStairs,
                    (128 << 4) | 5,
                    "sandstone_stairs",
                    BlockShape.Stairs),
                new BlockContext(
                    Block.SouthTopSandstoneStairs,
                    (128 << 4) | 6,
                    "sandstone_stairs",
                    BlockShape.Stairs),
                new BlockContext(
                    Block.NorthTopSandstoneStairs,
                    (128 << 4) | 7,
                    "sandstone_stairs",
                    BlockShape.Stairs),

                new BlockContext(
                    Block.EmeraldBlock,
                    (133 << 4) | 0,
                    "emerald_block",
                    BlockShape.Cube),

                new BlockContext(
                    Block.EastBottomSpruceWoodStairs,
                    (134 << 4) | 0,
                    "spruce_stairs",
                    BlockShape.Stairs),
                new BlockContext(
                    Block.WestBottomSpruceWoodStairs,
                    (134 << 4) | 1,
                    "spruce_stairs",
                    BlockShape.Stairs),
                new BlockContext(
                    Block.SouthBottomSpruceWoodStairs,
                    (134 << 4) | 2,
                    "spruce_stairs",
                    BlockShape.Stairs),
                new BlockContext(
                    Block.NorthBottomSpruceWoodStairs,
                    (134 << 4) | 3,
                    "spruce_stairs",
                    BlockShape.Stairs),
                new BlockContext(
                    Block.EastTopSpruceWoodStairs,
                    (134 << 4) | 4,
                    "spruce_stairs",
                    BlockShape.Stairs),
                new BlockContext(
                    Block.WestTopSpruceWoodStairs,
                    (134 << 4) | 5,
                    "spruce_stairs",
                    BlockShape.Stairs),
                new BlockContext(
                    Block.SouthTopSpruceWoodStairs,
                    (134 << 4) | 6,
                    "spruce_stairs",
                    BlockShape.Stairs),
                new BlockContext(
                    Block.NorthTopSpruceWoodStairs,
                    (134 << 4) | 7,
                    "spruce_stairs",
                    BlockShape.Stairs),

                new BlockContext(
                    Block.EastBottomBirchWoodStairs,
                    (135 << 4) | 0,
                    "birch_stairs",
                    BlockShape.Stairs),
                new BlockContext(
                    Block.WestBottomBirchWoodStairs,
                    (135 << 4) | 1,
                    "birch_stairs",
                    BlockShape.Stairs),
                new BlockContext(
                    Block.SouthBottomBirchWoodStairs,
                    (135 << 4) | 2,
                    "birch_stairs",
                    BlockShape.Stairs),
                new BlockContext(
                    Block.NorthBottomBirchWoodStairs,
                    (135 << 4) | 3,
                    "birch_stairs",
                    BlockShape.Stairs),
                new BlockContext(
                    Block.EastTopBirchWoodStairs,
                    (135 << 4) | 4,
                    "birch_stairs",
                    BlockShape.Stairs),
                new BlockContext(
                    Block.WestTopBirchWoodStairs,
                    (135 << 4) | 5,
                    "birch_stairs",
                    BlockShape.Stairs),
                new BlockContext(
                    Block.SouthTopBirchWoodStairs,
                    (135 << 4) | 6,
                    "birch_stairs",
                    BlockShape.Stairs),
                new BlockContext(
                    Block.NorthTopBirchWoodStairs,
                    (135 << 4) | 7,
                    "birch_stairs",
                    BlockShape.Stairs),

                new BlockContext(
                    Block.EastBottomJungleWoodStairs,
                    (136 << 4) | 0,
                    "jungle_stairs",
                    BlockShape.Stairs),
                new BlockContext(
                    Block.WestBottomJungleWoodStairs,
                    (136 << 4) | 1,
                    "jungle_stairs",
                    BlockShape.Stairs),
                new BlockContext(
                    Block.SouthBottomJungleWoodStairs,
                    (136 << 4) | 2,
                    "jungle_stairs",
                    BlockShape.Stairs),
                new BlockContext(
                    Block.NorthBottomJungleWoodStairs,
                    (136 << 4) | 3,
                    "jungle_stairs",
                    BlockShape.Stairs),
                new BlockContext(
                    Block.EastTopJungleWoodStairs,
                    (136 << 4) | 4,
                    "jungle_stairs",
                    BlockShape.Stairs),
                new BlockContext(
                    Block.WestTopJungleWoodStairs,
                    (136 << 4) | 5,
                    "jungle_stairs",
                    BlockShape.Stairs),
                new BlockContext(
                    Block.SouthTopJungleWoodStairs,
                    (136 << 4) | 6,
                    "jungle_stairs",
                    BlockShape.Stairs),
                new BlockContext(
                    Block.NorthTopJungleWoodStairs,
                    (136 << 4) | 7,
                    "jungle_stairs",
                    BlockShape.Stairs),


                new BlockContext(
                    Block.CobblestoneWall,
                    (139 << 4) | 0,
                    "cobblestone_wall",
                    BlockShape.Wall),
                new BlockContext(
                    Block.MossyCobblestoneWall,
                    (139 << 4) | 1,
                    "cobblestone_wall",
                    BlockShape.Wall),


                new BlockContext(
                    Block.WhiteCarpet,
                    (171 << 4) | 0,
                    "carpet",
                    BlockShape.Carpet),
                new BlockContext(
                    Block.OrangeCarpet,
                    (171 << 4) | 1,
                    "carpet",
                    BlockShape.Carpet),
                new BlockContext(
                    Block.MagentaCarpet,
                    (171 << 4) | 2,
                    "carpet",
                    BlockShape.Carpet),
                new BlockContext(
                    Block.LightBlueCarpet,
                    (171 << 4) | 3,
                    "carpet",
                    BlockShape.Carpet),
                new BlockContext(
                    Block.YellowCarpet,
                    (171 << 4) | 4,
                    "carpet",
                    BlockShape.Carpet),
                new BlockContext(
                    Block.LimeCarpet,
                    (171 << 4) | 5,
                    "carpet",
                    BlockShape.Carpet),
                new BlockContext(
                    Block.PinkCarpet,
                    (171 << 4) | 6,
                    "carpet",
                    BlockShape.Carpet),
                new BlockContext(
                    Block.GrayCarpet,
                    (171 << 4) | 7,
                    "carpet",
                    BlockShape.Carpet),
                new BlockContext(
                    Block.LightGrayCarpet,
                    (171 << 4) | 8,
                    "carpet",
                    BlockShape.Carpet),
                new BlockContext(
                    Block.CyanCarpet,
                    (171 << 4) | 9,
                    "carpet",
                    BlockShape.Carpet),
                new BlockContext(
                    Block.PurpleCarpet,
                    (171 << 4) | 10,
                    "carpet",
                    BlockShape.Carpet),
                new BlockContext(
                    Block.BlueCarpet,
                    (171 << 4) | 11,
                    "carpet",
                    BlockShape.Carpet),
                new BlockContext(
                    Block.BrownCarpet,
                    (171 << 4) | 12,
                    "carpet",
                    BlockShape.Carpet),
                new BlockContext(
                    Block.GreenCarpet,
                    (171 << 4) | 13,
                    "carpet",
                    BlockShape.Carpet),
                new BlockContext(
                    Block.RedCarpet,
                    (171 << 4) | 14,
                    "carpet",
                    BlockShape.Carpet),
                new BlockContext(
                    Block.BlackCarpet,
                    (171 << 4) | 15,
                    "carpet",
                    BlockShape.Carpet),
            };

            foreach (BlockContext ctx in _map)
            {
                _BLOCK_ID_TO_ENUM_MAP.Insert(ctx.Id, ctx.Block);
            }

            foreach (BlockContext ctx in _map)
            {
                _BLOCK_ENUM_TO_CTX_MAP.Insert(ctx.Block, ctx);
            }

        }

        public static Block ToBlock(int id)
        {
            return _BLOCK_ID_TO_ENUM_MAP.Lookup(id);
        }

        public static Block ToBlock(int id, Block defaultBlock)
        {
            try
            {
                return _BLOCK_ID_TO_ENUM_MAP.Lookup(id);
            }
            catch (KeyNotFoundException)
            {
                return defaultBlock;
            }
        }

        public static int GetId(this Block block)
        {
            return _BLOCK_ENUM_TO_CTX_MAP.Lookup(block).Id;
        }

        public static BlockShape GetShape(this Block block)
        {
            return _BLOCK_ENUM_TO_CTX_MAP.Lookup(block).Shape;
        }

        public static bool IsStairs(this Block block)
        {
            return block.GetShape() == BlockShape.Stairs;
        }

        public static bool IsVerticalStairs(this Block block)
        {
            if (block.GetShape() != BlockShape.Stairs)
            {
                return false;
            }

            int id = block.GetId();
            int metadata = id & 0b_1111;

            return metadata == 0 || metadata == 1 || metadata == 4 || metadata == 5;
        }

        public static bool IsBottomStairs(this Block block)
        {
            if (block.GetShape() != BlockShape.Stairs)
            {
                return false;
            }

            int id = block.GetId();
            int metadata = id & 0b_1111;
            return metadata < 4;
        }

        public static bool IsTopStairs(this Block block)
        {
            if (block.GetShape() != BlockShape.Stairs)
            {
                return false;
            }
            int id = block.GetId();
            int metadata = id & 0b_1111;
            return metadata >= 4;
        }

        public static bool IsBottomSlab(this Block block)
        {
            if (block.GetShape() != BlockShape.Slab)
            {
                return false;
            }

            int id = block.GetId();
            int metadata = id & 0b_1000;
            return metadata <= 0;
        }

        public static bool IsTopSlab(this Block block)
        {
            if (block.GetShape() != BlockShape.Slab)
            {
                return false;
            }
            int id = block.GetId();
            int metadata = id & 0b_1000;
            return metadata > 0;
        }

        public static BlockDirection GetStairsDirection(this Block block)
        {
            System.Diagnostics.Debug.Assert(block.IsStairs() == true);

            int id = block.GetId();
            int metadata = id & 0b_1111;
            switch (metadata % 4)
            {
                default:
                    throw new System.NotImplementedException();
                case 0:
                    return BlockDirection.Right;
                case 1:
                    return BlockDirection.Left;
                case 2:
                    return BlockDirection.Back;
                case 3:
                    return BlockDirection.Front;
            }
        }
    }
}


using Common;
using Containers;
using MinecraftServerEngine.Items;

namespace MinecraftServerEngine.Blocks
{
    public static class BlockExtensions
    {
        private readonly struct BlockContext
        {
            public readonly Block Block;
            public readonly int Id;
            public readonly string Name;

            public readonly bool IsItemable;
            public readonly ItemType ItemType;
            public readonly BlockShape Shape;

            public BlockContext(
                Block block,
                int id,
                string name,
                ItemType itemType,
                BlockShape shape)
            {
                Block = block;
                Id = id;
                Name = name;

                IsItemable = true;
                ItemType = itemType;

                Shape = shape;
            }

            public BlockContext(
                Block block,
                int id,
                string name,
                BlockShape shape)
            {
                Block = block;
                Id = id;
                Name = name;

                IsItemable = false;

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
                    0 << 4 | 0,
                    "air",
                    BlockShape.None),
                new BlockContext(
                    Block.Stone,
                    1 << 4 | 0,
                    "stone",
                    ItemType.Stone,
                    BlockShape.Cube),
                new BlockContext(
                    Block.Granite,
                    1 << 4 | 1,
                    "stone",
                    ItemType.Granite,
                    BlockShape.Cube),
                new BlockContext(
                    Block.PolishedGranite,
                    1 << 4 | 2,
                    "stone",
                    ItemType.PolishedGranite,
                    BlockShape.Cube),
                new BlockContext(
                    Block.Diorite,
                    1 << 4 | 3,
                    "stone",
                    ItemType.Diorite,
                    BlockShape.Cube),
                new BlockContext(
                    Block.PolishedDiorite,
                    1 << 4 | 4,
                    "stone",
                    ItemType.PolishedDiorite,
                    BlockShape.Cube),
                new BlockContext(
                    Block.Andesite,
                    1 << 4 | 5,
                    "stone",
                    ItemType.Andesite,
                    BlockShape.Cube),
                new BlockContext(
                    Block.PolishedAndesite,
                    1 << 4 | 6,
                    "stone",
                    ItemType.PolishedAndesite,
                    BlockShape.Cube),
                new BlockContext(
                    Block.GrassBlock,
                    2 << 4 | 0,
                    "grass",
                    ItemType.GrassBlock,
                    BlockShape.Cube),
                new BlockContext(
                    Block.Dirt,
                    3 << 4 | 0,
                    "dirt",
                    ItemType.Dirt,
                    BlockShape.Cube),
                new BlockContext(
                    Block.CoarseDirt,
                    3 << 4 | 1,
                    "dirt",
                    ItemType.CoarseDirt,
                    BlockShape.Cube),
                new BlockContext(
                    Block.Podzol,
                    3 << 4 | 2,
                    "dirt",
                    ItemType.Podzol,
                    BlockShape.Cube),
                new BlockContext(
                    Block.Cobblestone,
                    4 << 4 | 0,
                    "cobblestone",
                    ItemType.Cobblestone,
                    BlockShape.Cube),
                new BlockContext(
                    Block.OakWoodPlanks,
                    5 << 4 | 0,
                    "planks",
                    ItemType.OakWoodPlanks,
                    BlockShape.Cube),
                new BlockContext(
                    Block.SpruceWoodPlanks,
                    5 << 4 | 1,
                    "planks",
                    ItemType.SpruceWoodPlanks,
                    BlockShape.Cube),
                new BlockContext(
                    Block.BirchWoodPlanks,
                    5 << 4 | 2,
                    "planks",
                    ItemType.BirchWoodPlanks,
                    BlockShape.Cube),
                new BlockContext(
                    Block.JungleWoodPlanks,
                    5 << 4 | 3,
                    "planks",
                    ItemType.JungleWoodPlanks,
                    BlockShape.Cube),
                new BlockContext(
                    Block.AcaciaWoodPlanks,
                    5 << 4 | 4,
                    "planks",
                    ItemType.AcaciaWoodPlanks,
                    BlockShape.Cube),
                new BlockContext(
                    Block.DarkOakWoodPlanks,
                    5 << 4 | 5,
                    "planks",
                    ItemType.DarkOakWoodPlanks,
                    BlockShape.Cube),
                new BlockContext(
                    Block.OakSapling,
                    6 << 4 | 0,
                    "sapling",
                    ItemType.OakSapling,
                    BlockShape.None),
                new BlockContext(
                    Block.SpruceSapling,
                    6 << 4 | 1,
                    "sapling",
                    ItemType.SpruceSapling,
                    BlockShape.None),
                new BlockContext(
                    Block.BirchSapling,
                    6 << 4 | 2,
                    "sapling",
                    ItemType.BirchSapling,
                    BlockShape.None),
                new BlockContext(
                    Block.JungleSapling,
                    6 << 4 | 3,
                    "sapling",
                    ItemType.JungleSapling,
                    BlockShape.None),
                new BlockContext(
                    Block.AcaciaSapling,
                    6 << 4 | 4,
                    "sapling",
                    ItemType.AcaciaSapling,
                    BlockShape.None),
                new BlockContext(
                    Block.DarkOakSapling,
                    6 << 4 | 5,
                    "sapling",
                    ItemType.DarkOakSapling,
                    BlockShape.None),
                new BlockContext(
                    Block.Bedrock,
                    7 << 4 | 0,
                    "bedrock",
                    ItemType.Bedrock,
                    BlockShape.Cube),

                new BlockContext(
                    Block.FlowingWater,
                    8 << 4 | 0,
                    "flowing_water",
                    ItemType.FlowingWater,
                    BlockShape.None),
                new BlockContext(
                    Block.StillWater,
                    9 << 4 | 0,
                    "water",
                    ItemType.StillWater,
                    BlockShape.None),
                new BlockContext(
                    Block.FlowingLava,
                    10 << 4 | 0,
                    "flowing_lava",
                    ItemType.FlowingLava,
                    BlockShape.None),
                new BlockContext(
                    Block.StillLava,
                    11 << 4 | 0,
                    "lava",
                    ItemType.StillLava,
                    BlockShape.None),
                new BlockContext(
                    Block.Sand,
                    12 << 4 | 0,
                    "sand",
                    ItemType.Sand,
                    BlockShape.Cube),
                new BlockContext(
                    Block.RedSand,
                    12 << 4 | 1,
                    "sand",
                    ItemType.RedSand,
                    BlockShape.Cube),
                new BlockContext(
                    Block.Gravel,
                    13 << 4 | 0,
                    "gravel",
                    ItemType.Gravel,
                    BlockShape.Cube),
                new BlockContext(
                    Block.GoldOre,
                    14 << 4 | 0,
                    "gold_ore",
                    ItemType.GoldOre,
                    BlockShape.Cube),
                new BlockContext(
                    Block.IronOre,
                    15 << 4 | 0,
                    "iron_ore",
                    ItemType.IronOre,
                    BlockShape.Cube),
                new BlockContext(
                    Block.CoalOre,
                    16 << 4 | 0,
                    "coal_ore",
                    ItemType.CoalOre,
                    BlockShape.Cube),
                new BlockContext(
                    Block.OakWood,
                    17 << 4 | 0,
                    "log",
                    ItemType.OakWood,
                    BlockShape.Cube),
                new BlockContext(
                    Block.SpruceWood,
                    17 << 4 | 1,
                    "log",
                    ItemType.SpruceWood,
                    BlockShape.Cube),
                new BlockContext(
                    Block.BirchWood,
                    17 << 4 | 2,
                    "log",
                    ItemType.BirchWood,
                    BlockShape.Cube),
                new BlockContext(
                    Block.JungleWood,
                    17 << 4 | 3,
                    "log",
                    ItemType.JungleWood,
                    BlockShape.Cube),

                new BlockContext(
                    Block.OakLeaves,
                    18 << 4 | 0,
                    "leaves",
                    ItemType.OakLeaves,
                    BlockShape.Cube),
                new BlockContext(
                    Block.SpruceLeaves,
                    18 << 4 | 1,
                    "leaves",
                    ItemType.SpruceLeaves,
                    BlockShape.Cube),
                new BlockContext(
                    Block.BirchLeaves,
                    18 << 4 | 2,
                    "leaves",
                    ItemType.BirchLeaves,
                    BlockShape.Cube),
                new BlockContext(
                    Block.JungleLeaves,
                    18 << 4 | 3,
                    "leaves",
                    ItemType.JungleLeaves,
                    BlockShape.Cube),

                new BlockContext(
                    Block.Sponge,
                    19 << 4 | 0,
                    "sponge",
                    ItemType.Sponge,
                    BlockShape.Cube),
                new BlockContext(
                    Block.WetSponge,
                    19 << 4 | 1,
                    "sponge",
                    ItemType.WetSponge,
                    BlockShape.Cube),
                new BlockContext(
                    Block.Glass,
                    20 << 4 | 0,
                    "glass",
                    ItemType.Glass,
                    BlockShape.Cube),
                new BlockContext(
                    Block.LapisLazuliOre,
                    21 << 4 | 0,
                    "lapis_ore",
                    ItemType.LapisLazuliOre,
                    BlockShape.Cube),
                new BlockContext(
                    Block.LapisLazuliBlock,
                    22 << 4 | 0,
                    "lapis_block",
                    ItemType.LapisLazuliBlock,
                    BlockShape.Cube),

                new BlockContext(
                    Block.BottomDispenser,
                    23 << 4 | 0,
                    "dispenser",
                    ItemType.Dispenser,
                    BlockShape.Cube),
                new BlockContext(
                    Block.TopDispenser,
                    23 << 4 | 1,
                    "dispenser",
                    ItemType.Dispenser,
                    BlockShape.Cube),
                new BlockContext(
                    Block.NorthDispenser,
                    23 << 4 | 2,
                    "dispenser",
                    ItemType.Dispenser,
                    BlockShape.Cube),
                new BlockContext(
                    Block.SouthDispenser,
                    23 << 4 | 3,
                    "dispenser",
                    ItemType.Dispenser,
                    BlockShape.Cube),
                new BlockContext(
                    Block.WestDispenser,
                    23 << 4 | 4,
                    "dispenser",
                    ItemType.Dispenser,
                    BlockShape.Cube),
                new BlockContext(
                    Block.EastDispenser,
                    23 << 4 | 5,
                    "dispenser",
                    ItemType.Dispenser,
                    BlockShape.Cube),

                new BlockContext(
                    Block.Sandstone,
                    24 << 4 | 0,
                    "sandstone",
                    ItemType.Sandstone,
                    BlockShape.Cube),
                new BlockContext(
                    Block.ChiseledSandstone,
                    24 << 4 | 1,
                     "sandstone",
                     ItemType.ChiseledSandstone,
                     BlockShape.Cube),
                new BlockContext(
                    Block.SmoothSandstone,
                    24 << 4 | 2,
                     "sandstone",
                     ItemType.SmoothSandstone,
                     BlockShape.Cube),
                new BlockContext(
                    Block.NoteBlock,
                    25 << 4 | 0,
                     "noteblock",
                     ItemType.NoteBlock,
                     BlockShape.Cube),


                new BlockContext(
                    Block.BottomStickyPiston,
                    29 << 4 | 0,
                    "sticky_piston",
                    ItemType.StickyPiston,
                    BlockShape.Cube),
                new BlockContext(
                    Block.TopStickyPiston,
                    29 << 4 | 1,
                    "sticky_piston",
                    ItemType.StickyPiston,
                    BlockShape.Cube),
                new BlockContext(
                    Block.NorthStickyPiston,
                    29 << 4 | 2,
                    "sticky_piston",
                    ItemType.StickyPiston,
                    BlockShape.Cube),
                new BlockContext(
                    Block.SouthStickyPiston,
                    29 << 4 | 3,
                    "sticky_piston",
                    ItemType.StickyPiston,
                    BlockShape.Cube),
                new BlockContext(
                    Block.WestStickyPiston,
                    29 << 4 | 4,
                    "sticky_piston",
                    ItemType.StickyPiston,
                    BlockShape.Cube),
                new BlockContext(
                    Block.EastStickyPiston,
                    29 << 4 | 5,
                    "sticky_piston",
                    ItemType.StickyPiston,
                    BlockShape.Cube),

                new BlockContext(
                    Block.DeadShrub,
                    31 << 4 | 0,
                    "tallgrass",
                    ItemType.DeadShrub,
                    BlockShape.None),
                new BlockContext(
                    Block.Grass,
                    31 << 4 | 1,
                    "tallgrass",
                    ItemType.Grass,
                    BlockShape.None),
                new BlockContext(
                    Block.Fern,
                    31 << 4 | 2,
                    "tallgrass",
                    ItemType.Fern,
                    BlockShape.None),
                new BlockContext(
                    Block.DeadBush,
                    32 << 4 | 0,
                    "deadbush",
                    ItemType.DeadBush,
                    BlockShape.None),

                new BlockContext(
                    Block.BottomPiston,
                    33 << 4 | 0,
                    "piston",
                    ItemType.Piston,
                    BlockShape.Cube),
                new BlockContext(
                    Block.TopPiston,
                    33 << 4 | 1,
                    "piston",
                    ItemType.Piston,
                    BlockShape.Cube),
                new BlockContext(
                    Block.NorthPiston,
                    33 << 4 | 2,
                    "piston",
                    ItemType.Piston,
                    BlockShape.Cube),
                new BlockContext(
                    Block.SouthPiston,
                    33 << 4 | 3,
                    "piston",
                    ItemType.Piston,
                    BlockShape.Cube),
                new BlockContext(
                    Block.WestPiston,
                    33 << 4 | 4,
                    "piston",
                    ItemType.Piston,
                    BlockShape.Cube),
                new BlockContext(
                    Block.EastPiston,
                    33 << 4 | 5,
                    "piston",
                    ItemType.Piston,
                    BlockShape.Cube),


                new BlockContext(
                    Block.WhiteWool,
                    35 << 4 | 0,
                    "wool",
                    ItemType.WhiteWool,
                    BlockShape.Cube),
                new BlockContext(
                    Block.OrangeWool,
                    35 << 4 | 1,
                    "wool",
                    ItemType.OrangeWool,
                    BlockShape.Cube),
                new BlockContext(
                    Block.MagentaWool,
                    35 << 4 | 2,
                    "wool",
                    ItemType.MagentaWool,
                    BlockShape.Cube),
                new BlockContext(
                    Block.LightBlueWool,
                    35 << 4 | 3,
                    "wool",
                    ItemType.LightBlueWool,
                    BlockShape.Cube),
                new BlockContext(
                    Block.YellowWool,
                    35 << 4 | 4,
                    "wool",
                    ItemType.YellowWool,
                    BlockShape.Cube),
                new BlockContext(
                    Block.LimeWool,
                    35 << 4 | 5,
                    "wool",
                    ItemType.LimeWool,
                    BlockShape.Cube),
                new BlockContext(
                    Block.PinkWool,
                    35 << 4 | 6,
                    "wool",
                    ItemType.PinkWool,
                    BlockShape.Cube),
                new BlockContext(
                    Block.GrayWool,
                    35 << 4 | 7,
                    "wool",
                    ItemType.GrayWool,
                    BlockShape.Cube),
                new BlockContext(
                    Block.LightGrayWool,
                    35 << 4 | 8,
                    "wool",
                    ItemType.LightGrayWool,
                    BlockShape.Cube),
                new BlockContext(
                    Block.CyanWool,
                    35 << 4 | 9,
                    "wool",
                    ItemType.CyanWool,
                    BlockShape.Cube),
                new BlockContext(
                    Block.PurpleWool,
                    35 << 4 | 10,
                    "wool",
                    ItemType.PurpleWool,
                    BlockShape.Cube),
                new BlockContext(
                    Block.BlueWool,
                    35 << 4 | 11,
                    "wool",
                    ItemType.BlueWool,
                    BlockShape.Cube),
                new BlockContext(
                    Block.BrownWool,
                    35 << 4 | 12,
                    "wool",
                    ItemType.BrownWool,
                    BlockShape.Cube),
                new BlockContext(
                    Block.GreenWool,
                    35 << 4 | 13,
                    "wool",
                    ItemType.GreenWool,
                    BlockShape.Cube),
                new BlockContext(
                    Block.RedWool,
                    35 << 4 | 14,
                    "wool",
                    ItemType.RedWool,
                    BlockShape.Cube),
                new BlockContext(
                    Block.BlackWool,
                    35 << 4 | 15,
                    "wool",
                    ItemType.BlackWool,
                    BlockShape.Cube),


                new BlockContext(
                    Block.Dandelion,
                    37 << 4 | 0,
                    "yellow_flower",
                    ItemType.Dandelion,
                    BlockShape.None),
                new BlockContext(
                    Block.Poppy,
                    38 << 4 | 0,
                    "red_flower",
                    ItemType.Poppy,
                    BlockShape.None),
                new BlockContext(
                    Block.BlueOrchid,
                    38 << 4 | 1,
                    "red_flower",
                    ItemType.BlueOrchid,
                    BlockShape.None),
                new BlockContext(
                    Block.Allium,
                    38 << 4 | 2,
                    "red_flower",
                    ItemType.Allium,
                    BlockShape.None),
                new BlockContext(
                    Block.AzureBluet,
                    38 << 4 | 3,
                    "red_flower",
                    ItemType.AzureBluet,
                    BlockShape.None),
                new BlockContext(
                    Block.RedTulip,
                    38 << 4 | 4,
                    "red_flower",
                    ItemType.RedTulip,
                    BlockShape.None),
                new BlockContext(
                    Block.OrangeTulip,
                    38 << 4 | 5,
                    "red_flower",
                    ItemType.OrangeTulip,
                    BlockShape.None),
                new BlockContext(
                    Block.WhiteTulip,
                    38 << 4 | 6,
                    "red_flower",
                    ItemType.WhiteTulip,
                    BlockShape.None),
                new BlockContext(
                    Block.PinkTulip,
                    38 << 4 | 7,
                    "red_flower",
                    ItemType.PinkTulip,
                    BlockShape.None),
                new BlockContext(
                    Block.OxeyeDaisy,
                    38 << 4 | 8,
                    "red_flower",
                    ItemType.OxeyeDaisy,
                    BlockShape.None),
                new BlockContext(
                    Block.BrownMushroom,
                    39 << 4 | 0,
                    "brown_mushroom",
                    ItemType.BrownMushroom,
                    BlockShape.None),
                new BlockContext(
                    Block.RedMushroom,
                    40 << 4 | 0,
                    "red_mushroom",
                    ItemType.RedMushroom,
                    BlockShape.None),

                new BlockContext(
                    Block.GoldBlock,
                    41 << 4 | 0,
                    "gold_block",
                    ItemType.GoldBlock,
                    BlockShape.Cube),
                new BlockContext(
                    Block.IronBlock,
                    42 << 4 | 0,
                    "iron_block",
                    ItemType.IronBlock,
                    BlockShape.Cube),
                new BlockContext(
                    Block.DoubleStoneSlab,
                    43 << 4 | 0,
                    "double_stone_slab",
                    ItemType.DoubleStoneSlab,
                    BlockShape.Cube),
                new BlockContext(
                    Block.DoubleSandstoneSlab,
                    43 << 4 | 1,
                    "double_stone_slab",
                    ItemType.DoubleSandstoneSlab,
                    BlockShape.Cube),
                new BlockContext(
                    Block.DoubleWoodenSlab,
                    43 << 4 | 2,
                    "double_stone_slab",
                    ItemType.DoubleWoodenSlab,
                    BlockShape.Cube),
                new BlockContext(
                    Block.DoubleCobblestoneSlab,
                    43 << 4 | 3,
                    "double_stone_slab",
                    ItemType.DoubleCobblestoneSlab,
                    BlockShape.Cube),
                new BlockContext(
                    Block.DoubleBrickSlab,
                    43 << 4 | 4,
                    "double_stone_slab",
                    ItemType.DoubleBrickSlab,
                    BlockShape.Cube),
                new BlockContext(
                    Block.DoubleStoneBrickSlab,
                    43 << 4 | 5,
                    "double_stone_slab",
                    ItemType.DoubleStoneBrickSlab,
                    BlockShape.Cube),
                new BlockContext(
                    Block.DoubleNetherBrickSlab,
                    43 << 4 | 6,
                    "double_stone_slab",
                    ItemType.DoubleNetherBrickSlab,
                    BlockShape.Cube),
                new BlockContext(
                    Block.DoubleQuartzSlab,
                    43 << 4 | 7,
                    "double_stone_slab",
                    ItemType.DoubleQuartzSlab,
                    BlockShape.Cube),


                new BlockContext(
                    Block.StoneBottomSlab,
                    44 << 4 | 0,
                    "stone_slab",
                    ItemType.StoneBottomSlab,
                    BlockShape.Slab),
                new BlockContext(
                    Block.SandstoneBottomSlab,
                    44 << 4 | 1,
                    "stone_slab",
                    ItemType.SandstoneBottomSlab,
                    BlockShape.Slab),
                new BlockContext(
                    Block.WoodenBottomSlab,
                    44 << 4 | 2,
                    "stone_slab",
                    ItemType.WoodenBottomSlab,
                    BlockShape.Slab),
                new BlockContext(
                    Block.CobblestoneBottomSlab,
                    44 << 4 | 3,
                    "stone_slab",
                    ItemType.CobblestoneBottomSlab,
                    BlockShape.Slab),
                new BlockContext(
                    Block.BrickBottomSlab,
                    44 << 4 | 4,
                    "stone_slab",
                    ItemType.BrickBottomSlab,
                    BlockShape.Slab),
                new BlockContext(
                    Block.StoneBrickBottomSlab,
                    44 << 4 | 5,
                    "stone_slab",
                    ItemType.StoneBrickBottomSlab,
                    BlockShape.Slab),
                new BlockContext(
                    Block.NetherBrickBottomSlab,
                    44 << 4 | 6,
                    "stone_slab",
                    ItemType.NetherBrickBottomSlab,
                    BlockShape.Slab),
                new BlockContext(
                    Block.QuartzBottomSlab,
                    44 << 4 | 7,
                    "stone_slab",
                    ItemType.QuartzBottomSlab,
                    BlockShape.Slab),

                new BlockContext(
                    Block.StoneTopSlab,
                    44 << 4 | 8,
                    "stone_slab",
                    ItemType.StoneTopSlab,
                    BlockShape.Slab),
                new BlockContext(
                    Block.SandstoneTopSlab,
                    44 << 4 | 9,
                    "stone_slab",
                    ItemType.SandstoneTopSlab,
                    BlockShape.Slab),
                new BlockContext(
                    Block.WoodenTopSlab,
                    44 << 4 | 10,
                    "stone_slab",
                    ItemType.WoodenTopSlab,
                    BlockShape.Slab),
                new BlockContext(
                    Block.CobblestoneTopSlab,
                    44 << 4 | 11,
                    "stone_slab",
                    ItemType.CobblestoneTopSlab,
                    BlockShape.Slab),
                new BlockContext(
                    Block.BrickTopSlab,
                    44 << 4 | 12,
                    "stone_slab",
                    ItemType.BrickTopSlab,
                    BlockShape.Slab),
                new BlockContext(
                    Block.StoneBrickTopSlab,
                    44 << 4 | 13,
                    "stone_slab",
                    ItemType.StoneBrickTopSlab,
                    BlockShape.Slab),
                new BlockContext(
                    Block.NetherBrickTopSlab,
                    44 << 4 | 14,
                    "stone_slab",
                    ItemType.NetherBrickTopSlab,
                    BlockShape.Slab),
                new BlockContext(
                    Block.QuartzTopSlab,
                    44 << 4 | 15,
                    "stone_slab",
                    ItemType.QuartzTopSlab,
                    BlockShape.Slab),

                new BlockContext(
                    Block.Bricks,
                    45 << 4 | 0,
                    "brick_block",
                    ItemType.Bricks,
                    BlockShape.Cube),
                new BlockContext(
                    Block.TNT,
                    46 << 4 | 0,
                    "tnt",
                    ItemType.TNT,
                    BlockShape.Cube),
                new BlockContext(
                    Block.Bookshelf,
                    47 << 4 | 0,
                    "bookshelf",
                    ItemType.Bookshelf,
                    BlockShape.Cube),
                new BlockContext(
                    Block.MossStone,
                    48 << 4 | 0,
                    "mossy_cobblestone",
                    ItemType.MossStone,
                    BlockShape.Cube),
                new BlockContext(
                    Block.Obsidian,
                    49 << 4 | 0,
                    "obsidian",
                    ItemType.Obsidian,
                    BlockShape.Cube),


                //new BlockContext(
                //    Block.Torch,
                //    (50 << 4) | 0,
                //    "torch",
                //    ItemType.Torch,
                //    BlockShape.None),
                new BlockContext(
                    Block.EastTorch,
                    50 << 4 | 1,
                    "torch",
                    ItemType.Torch,
                    BlockShape.None),
                new BlockContext(
                    Block.WestTorch,
                    50 << 4 | 2,
                    "torch",
                    ItemType.Torch,
                    BlockShape.None),
                new BlockContext(
                    Block.SouthTorch,
                    50 << 4 | 3,
                    "torch",
                    ItemType.Torch,
                    BlockShape.None),
                new BlockContext(
                    Block.NorthTorch,
                    50 << 4 | 4,
                    "torch",
                    ItemType.Torch,
                    BlockShape.None),
                new BlockContext(
                    Block.Torch,
                    50 << 4 | 5,
                    "torch",
                    ItemType.Torch,
                    BlockShape.None),

                new BlockContext(
                    Block.Fire,
                    51 << 4 | 0,
                    "fire",
                    BlockShape.None),

                new BlockContext(
                    Block.EastBottomOakWoodStairs,
                    53 << 4 | 0,
                    "oak_stairs",
                    ItemType.OakWoodStairs,
                    BlockShape.Stairs),
                new BlockContext(
                    Block.WestBottomOakWoodStairs,
                    53 << 4 | 1,
                    "oak_stairs",
                    ItemType.OakWoodStairs,
                    BlockShape.Stairs),
                new BlockContext(
                    Block.SouthBottomOakWoodStairs,
                    53 << 4 | 2,
                    "oak_stairs",
                    ItemType.OakWoodStairs,
                    BlockShape.Stairs),
                new BlockContext(
                    Block.NorthBottomOakWoodStairs,
                    53 << 4 | 3,
                    "oak_stairs",
                    ItemType.OakWoodStairs,
                    BlockShape.Stairs),
                new BlockContext(
                    Block.EastTopOakWoodStairs,
                    53 << 4 | 4,
                    "oak_stairs",
                    ItemType.OakWoodStairs,
                    BlockShape.Stairs),
                new BlockContext(
                    Block.WestTopOakWoodStairs,
                    53 << 4 | 5,
                    "oak_stairs",
                    ItemType.OakWoodStairs,
                    BlockShape.Stairs),
                new BlockContext(
                    Block.SouthTopOakWoodStairs,
                    53 << 4 | 6,
                    "oak_stairs",
                    ItemType.OakWoodStairs,
                    BlockShape.Stairs),
                new BlockContext(
                    Block.NorthTopOakWoodStairs,
                    53 << 4 | 7,
                    "oak_stairs",
                    ItemType.OakWoodStairs,
                    BlockShape.Stairs),

                new BlockContext(
                    Block.DiamondBlock,
                    57 << 4 | 0,
                    "diamond_block",
                    ItemType.DiamondBlock,
                    BlockShape.Cube),
                new BlockContext(
                    Block.CraftingTable,
                    58 << 4 | 0,
                    "crafting_table",
                    ItemType.CraftingTable,
                    BlockShape.Cube),
                new BlockContext(
                    Block.WheatCrops,
                    59 << 4 | 0,
                    "wheat",
                    ItemType.WheatCrops,
                    BlockShape.None),
                new BlockContext(
                    Block.Farmland,
                    60 << 4 | 0,
                    "farmland",
                    ItemType.Farmland,
                    BlockShape.Cube),

                new BlockContext(
                    Block.EastBottomCobblestoneStairs,
                    67 << 4 | 0,
                    "stone_stairs",
                    ItemType.CobblestoneStairs,
                    BlockShape.Stairs),
                new BlockContext(
                    Block.WestBottomCobblestoneStairs,
                    67 << 4 | 1,
                    "stone_stairs",
                    ItemType.CobblestoneStairs,
                    BlockShape.Stairs),
                new BlockContext(
                    Block.SouthBottomCobblestoneStairs,
                    67 << 4 | 2,
                    "stone_stairs",
                    ItemType.CobblestoneStairs,
                    BlockShape.Stairs),
                new BlockContext(
                    Block.NorthBottomCobblestoneStairs,
                    67 << 4 | 3,
                    "stone_stairs",
                    ItemType.CobblestoneStairs,
                    BlockShape.Stairs),
                new BlockContext(
                    Block.EastTopCobblestoneStairs,
                    67 << 4 | 4,
                    "stone_stairs",
                    ItemType.CobblestoneStairs,
                    BlockShape.Stairs),
                new BlockContext(
                    Block.WestTopCobblestoneStairs,
                    67 << 4 | 5,
                    "stone_stairs",
                    ItemType.CobblestoneStairs,
                    BlockShape.Stairs),
                new BlockContext(
                    Block.SouthTopCobblestoneStairs,
                    67 << 4 | 6,
                    "stone_stairs",
                    ItemType.CobblestoneStairs,
                    BlockShape.Stairs),
                new BlockContext(
                    Block.NorthTopCobblestoneStairs,
                    67 << 4 | 7,
                    "stone_stairs",
                    ItemType.CobblestoneStairs,
                    BlockShape.Stairs),

                new BlockContext(
                    Block.OakFence,
                    85 << 4 | 0,
                    "fence",
                    ItemType.OakFence,
                    BlockShape.Fence),

                new BlockContext(
                    Block.SouthPumpkin,
                    86 << 4 | 0,
                    "pumpkin",
                    ItemType.Pumpkin,
                    BlockShape.Cube),
                new BlockContext(
                    Block.WestPumpkin,
                    86 << 4 | 1,
                    "pumpkin",
                    ItemType.Pumpkin,
                    BlockShape.Cube),
                new BlockContext(
                    Block.NorthPumpkin,
                    86 << 4 | 2,
                    "pumpkin",
                    ItemType.Pumpkin,
                    BlockShape.Cube),
                new BlockContext(
                    Block.EastPumpkin,
                    86 << 4 | 3,
                    "pumpkin",
                    ItemType.Pumpkin,
                    BlockShape.Cube),

                new BlockContext(
                    Block.SouthJackOLantern,
                    91 << 4 | 0,
                    "lit_pumpkin",
                    ItemType.JackOLantern,
                    BlockShape.Cube),
                new BlockContext(
                    Block.WestJackOLantern,
                    91 << 4 | 1,
                    "lit_pumpkin",
                    ItemType.JackOLantern,
                    BlockShape.Cube),
                new BlockContext(
                    Block.NorthJackOLantern,
                    91 << 4 | 2,
                    "lit_pumpkin",
                    ItemType.JackOLantern,
                    BlockShape.Cube),
                new BlockContext(
                    Block.EastJackOLantern,
                    91 << 4 | 3,
                    "lit_pumpkin",
                    ItemType.JackOLantern,
                    BlockShape.Cube),


                new BlockContext(
                    Block.WhiteStainedGlass,
                    95 << 4 | 0,
                    "stained_glass",
                    ItemType.WhiteStainedGlass,
                    BlockShape.Cube),
                new BlockContext(
                    Block.OrangeStainedGlass,
                    95 << 4 | 1,
                    "stained_glass",
                    ItemType.OrangeStainedGlass,
                    BlockShape.Cube),
                new BlockContext(
                    Block.MagentaStainedGlass,
                    95 << 4 | 2,
                    "stained_glass",
                    ItemType.MagentaStainedGlass,
                    BlockShape.Cube),
                new BlockContext(
                    Block.LightBlueStainedGlass,
                    95 << 4 | 3,
                    "stained_glass",
                    ItemType.LightBlueStainedGlass,
                    BlockShape.Cube),
                new BlockContext(
                    Block.YellowStainedGlass,
                    95 << 4 | 4,
                    "stained_glass",
                    ItemType.YellowStainedGlass,
                    BlockShape.Cube),
                new BlockContext(
                    Block.LimeStainedGlass,
                    95 << 4 | 5,
                    "stained_glass",
                    ItemType.LimeStainedGlass,
                    BlockShape.Cube),
                new BlockContext(
                    Block.PinkStainedGlass,
                    95 << 4 | 6,
                    "stained_glass",
                    ItemType.PinkStainedGlass,
                    BlockShape.Cube),
                new BlockContext(
                    Block.GrayStainedGlass,
                    95 << 4 | 7,
                    "stained_glass",
                    ItemType.GrayStainedGlass,
                    BlockShape.Cube),
                new BlockContext(
                    Block.LightGrayStainedGlass,
                    95 << 4 | 8,
                    "stained_glass",
                    ItemType.LightGrayStainedGlass,
                    BlockShape.Cube),
                new BlockContext(
                    Block.CyanStainedGlass,
                    95 << 4 | 9,
                    "stained_glass",
                    ItemType.CyanStainedGlass,
                    BlockShape.Cube),
                new BlockContext(
                    Block.PurpleStainedGlass,
                    95 << 4 | 10,
                    "stained_glass",
                    ItemType.PurpleStainedGlass,
                    BlockShape.Cube),
                new BlockContext(
                    Block.BlueStainedGlass,
                    95 << 4 | 11,
                    "stained_glass",
                    ItemType.BlueStainedGlass,
                    BlockShape.Cube),
                new BlockContext(
                    Block.BrownStainedGlass,
                    95 << 4 | 12,
                    "stained_glass",
                    ItemType.BrownStainedGlass,
                    BlockShape.Cube),
                new BlockContext(
                    Block.GreenStainedGlass,
                    95 << 4 | 13,
                    "stained_glass",
                    ItemType.GreenStainedGlass,
                    BlockShape.Cube),
                new BlockContext(
                    Block.RedStainedGlass,
                    95 << 4 | 14,
                    "stained_glass",
                    ItemType.RedStainedGlass,
                    BlockShape.Cube),
                new BlockContext(
                    Block.BlackStainedGlass,
                    95 << 4 | 15,
                    "stained_glass",
                    ItemType.BlackStainedGlass,
                    BlockShape.Cube),


                new BlockContext(
                    Block.StoneMonsterEgg,
                    97 << 4 | 0,
                    "monster_egg",
                    ItemType.StoneMonsterEgg,
                    BlockShape.Cube),
                new BlockContext(
                    Block.CobblestoneMonsterEgg,
                    97 << 4 | 1,
                    "monster_egg",
                    ItemType.CobblestoneMonsterEgg,
                    BlockShape.Cube),
                new BlockContext(
                    Block.StoneBrickMonsterEgg,
                    97 << 4 | 2,
                    "monster_egg",
                    ItemType.StoneBrickMonsterEgg,
                    BlockShape.Cube),
                new BlockContext(
                    Block.MossyStoneBrickMonsterEgg,
                    97 << 4 | 3,
                    "monster_egg",
                    ItemType.MossyStoneBrickMonsterEgg,
                    BlockShape.Cube),
                new BlockContext(
                    Block.CrackedStoneBrickMonsterEgg,
                    97 << 4 | 4,
                    "monster_egg",
                    ItemType.CrackedStoneBrickMonsterEgg,
                    BlockShape.Cube),
                new BlockContext(
                    Block.ChiseledStoneBrickMonsterEgg,
                    97 << 4 | 5,
                    "monster_egg",
                    ItemType.ChiseledStoneBrickMonsterEgg,
                    BlockShape.Cube),


                new BlockContext(
                    Block.StoneBricks,
                    98 << 4 | 0,
                    "stonebrick",
                    ItemType.StoneBricks,
                    BlockShape.Cube),
                new BlockContext(
                    Block.MossyStoneBricks,
                    98 << 4 | 1,
                    "stonebrick",
                    ItemType.MossyStoneBricks,
                    BlockShape.Cube),
                new BlockContext(
                    Block.CrackedStoneBricks,
                    98 << 4 | 2,
                    "stonebrick",
                    ItemType.CrackedStoneBricks,
                    BlockShape.Cube),
                new BlockContext(
                    Block.ChiseledStoneBricks,
                    98 << 4 | 3,
                    "stonebrick",
                    ItemType.ChiseledStoneBricks,
                    BlockShape.Cube),
                new BlockContext(
                    Block.BrownMushroomBlock,
                    99 << 4 | 0,
                    "brown_mushroom_block",
                    ItemType.BrownMushroomBlock,
                    BlockShape.Cube),
                new BlockContext(
                    Block.RedMushroomBlock,
                    100 << 4 | 0,
                    "red_mushroom_block",
                    ItemType.RedMushroomBlock,
                    BlockShape.Cube),

                new BlockContext(
                    Block.IronBars,
                    101 << 4 | 0,
                    "iron_bars",
                    ItemType.IronBars,
                    BlockShape.Bars),
                new BlockContext(
                    Block.GlassPane,
                    102 << 4 | 0,
                    "glass_pane",
                    ItemType.GlassPane,
                    BlockShape.Bars),
                new BlockContext(
                    Block.MelonBlock,
                    103 << 4 | 0,
                    "melon_block",
                    ItemType.MelonBlock,
                    BlockShape.Cube),

                new BlockContext(
                    Block.Vines,
                    106 << 4 | 0,
                    "vine",
                    ItemType.Vines,
                    BlockShape.None),
                new BlockContext(
                    Block.SouthVines,
                    106 << 4 | 1,
                    "vine",
                    ItemType.Vines,
                    BlockShape.None),
                new BlockContext(
                    Block.WestVines,
                    106 << 4 | 2,
                    "vine",
                    ItemType.Vines,
                    BlockShape.None),
                new BlockContext(
                    Block.SouthWestVines,
                    106 << 4 | 3,
                    "vine",
                    ItemType.Vines,
                    BlockShape.None),
                  new BlockContext(
                    Block.NorthVines,
                    106 << 4 | 4,
                    "vine",
                    ItemType.Vines,
                    BlockShape.None),
                new BlockContext(
                    Block.NorthSouthVines,
                    106 << 4 | 5,
                    "vine",
                    ItemType.Vines,
                    BlockShape.None),
                new BlockContext(
                    Block.NorthWestVines,
                    106 << 4 | 6,
                    "vine",
                    ItemType.Vines,
                    BlockShape.None),
                new BlockContext(
                    Block.NorthSouthWestVines,
                    106 << 4 | 7,
                    "vine",
                    ItemType.Vines,
                    BlockShape.None),
                  new BlockContext(
                    Block.EastVines,
                    106 << 4 | 8,
                    "vine",
                    ItemType.Vines,
                    BlockShape.None),
                new BlockContext(
                    Block.EastSouthVines,
                    106 << 4 | 9,
                    "vine",
                    ItemType.Vines,
                    BlockShape.None),
                new BlockContext(
                    Block.EastWestVines,
                    106 << 4 | 10,
                    "vine",
                    ItemType.Vines,
                    BlockShape.None),
                new BlockContext(
                    Block.EastSouthWestVines,
                    106 << 4 | 11,
                    "vine",
                    ItemType.Vines,
                    BlockShape.None),
                  new BlockContext(
                    Block.EastNorthVines,
                    106 << 4 | 12,
                    "vine",
                    ItemType.Vines,
                    BlockShape.None),
                new BlockContext(
                    Block.EastNorthSouthVines,
                    106 << 4 | 13,
                    "vine",
                    ItemType.Vines,
                    BlockShape.None),
                new BlockContext(
                    Block.EastNorthWestVines,
                    106 << 4 | 14,
                    "vine",
                    ItemType.Vines,
                    BlockShape.None),
                new BlockContext(
                    Block.EastNorthSouthWestVines,
                    106 << 4 | 15,
                    "vine",
                    ItemType.Vines,
                    BlockShape.None),

                new BlockContext(
                    Block.EastBottomStoneBrickStairs,
                    109 << 4 | 0,
                    "stone_brick_stairs",
                    ItemType.StoneBrickStairs,
                    BlockShape.Stairs),
                new BlockContext(
                    Block.WestBottomStoneBrickStairs,
                    109 << 4 | 1,
                    "stone_brick_stairs",
                    ItemType.StoneBrickStairs,
                    BlockShape.Stairs),
                new BlockContext(
                    Block.SouthBottomStoneBrickStairs,
                    109 << 4 | 2,
                    "stone_brick_stairs",
                    ItemType.StoneBrickStairs,
                    BlockShape.Stairs),
                new BlockContext(
                    Block.NorthBottomStoneBrickStairs,
                    109 << 4 | 3,
                    "stone_brick_stairs",
                    ItemType.StoneBrickStairs,
                    BlockShape.Stairs),
                new BlockContext(
                    Block.EastTopStoneBrickStairs,
                    109 << 4 | 4,
                    "stone_brick_stairs",
                    ItemType.StoneBrickStairs,
                    BlockShape.Stairs),
                new BlockContext(
                    Block.WestTopStoneBrickStairs,
                    109 << 4 | 5,
                    "stone_brick_stairs",
                    ItemType.StoneBrickStairs,
                    BlockShape.Stairs),
                new BlockContext(
                    Block.SouthTopStoneBrickStairs,
                    109 << 4 | 6,
                    "stone_brick_stairs",
                    ItemType.StoneBrickStairs,
                    BlockShape.Stairs),
                new BlockContext(
                    Block.NorthTopStoneBrickStairs,
                    109 << 4 | 7,
                    "stone_brick_stairs",
                    ItemType.StoneBrickStairs,
                    BlockShape.Stairs),

                new BlockContext(
                    Block.InactiveRedstoneLamp,
                    123 << 4 | 0,
                    "redstone_lamp",
                    ItemType.InactiveRedstoneLamp,
                    BlockShape.Cube),
                new BlockContext(
                    Block.ActiveRedstoneLamp,
                    124 << 4 | 0,
                    "lit_redstone_lamp",
                    BlockShape.Cube),

                new BlockContext(
                    Block.DoubleOakWoodSlab,
                    125 << 4 | 0,
                    "double_wooden_slab",
                    ItemType.DoubleOakWoodSlab,
                    BlockShape.Cube),
                new BlockContext(
                    Block.DoubleSpruceWoodSlab,
                    125 << 4 | 1,
                    "double_wooden_slab",
                    ItemType.DoubleSpruceWoodSlab,
                    BlockShape.Cube),
                new BlockContext(
                    Block.DoubleBirchWoodSlab,
                    125 << 4 | 2,
                    "double_wooden_slab",
                    ItemType.DoubleBirchWoodSlab,
                    BlockShape.Cube),
                new BlockContext(
                    Block.DoubleJungleWoodSlab,
                    125 << 4 | 3,
                    "double_wooden_slab",
                    ItemType.DoubleJungleWoodSlab,
                    BlockShape.Cube),
                new BlockContext(
                    Block.DoubleAcaciaWoodSlab,
                    125 << 4 | 4,
                    "double_wooden_slab",
                    ItemType.DoubleAcaciaWoodSlab,
                    BlockShape.Cube),
                new BlockContext(
                    Block.DoubleDarkOakWoodSlab,
                    125 << 4 | 5,
                    "double_wooden_slab",
                    ItemType.DoubleDarkOakWoodSlab,
                    BlockShape.Cube),

                new BlockContext(
                    Block.BottomOakWoodSlab,
                    126 << 4 | 0,
                    "wooden_slab",
                    ItemType.OakWoodSlab,
                    BlockShape.Slab),
                new BlockContext(
                    Block.BottomSpruceWoodSlab,
                    126 << 4 | 1,
                    "wooden_slab",
                    ItemType.OakWoodSlab,
                    BlockShape.Slab),
                new BlockContext(
                    Block.BottomBirchWoodSlab,
                    126 << 4 | 2,
                    "wooden_slab",
                    ItemType.OakWoodSlab,
                    BlockShape.Slab),
                new BlockContext(
                    Block.BottomJungleWoodSlab,
                    126 << 4 | 3,
                    "wooden_slab",
                    ItemType.OakWoodSlab,
                    BlockShape.Slab),
                new BlockContext(
                    Block.BottomAcaciaWoodSlab,
                    126 << 4 | 4,
                    "wooden_slab",
                    ItemType.OakWoodSlab,
                    BlockShape.Slab),
                new BlockContext(
                    Block.BottomDarkOakWoodSlab,
                    126 << 4 | 5,
                    "wooden_slab",
                    ItemType.OakWoodSlab,
                    BlockShape.Slab),
                new BlockContext(
                    Block.TopOakWoodSlab,
                    126 << 4 | 8,
                    "wooden_slab",
                    ItemType.OakWoodSlab,
                    BlockShape.Slab),
                new BlockContext(
                    Block.TopSpruceWoodSlab,
                    126 << 4 | 9,
                    "wooden_slab",
                    ItemType.OakWoodSlab,
                    BlockShape.Slab),
                new BlockContext(
                    Block.TopBirchWoodSlab,
                    126 << 4 | 10,
                    "wooden_slab",
                    ItemType.OakWoodSlab,
                    BlockShape.Slab),
                new BlockContext(
                    Block.TopJungleWoodSlab,
                    126 << 4 | 11,
                    "wooden_slab",
                    ItemType.OakWoodSlab,
                    BlockShape.Slab),
                new BlockContext(
                    Block.TopAcaciaWoodSlab,
                    126 << 4 | 12,
                    "wooden_slab",
                    ItemType.OakWoodSlab,
                    BlockShape.Slab),
                new BlockContext(
                    Block.TopDarkOakWoodSlab,
                    126 << 4 | 13,
                    "wooden_slab",
                    ItemType.OakWoodSlab,
                    BlockShape.Slab),

                new BlockContext(
                    Block.EastBottomSandstoneStairs,
                    128 << 4 | 0,
                    "sandstone_stairs",
                    ItemType.SandstoneStairs,
                    BlockShape.Stairs),
                new BlockContext(
                    Block.WestBottomSandstoneStairs,
                    128 << 4 | 1,
                    "sandstone_stairs",
                    ItemType.SandstoneStairs,
                    BlockShape.Stairs),
                new BlockContext(
                    Block.SouthBottomSandstoneStairs,
                    128 << 4 | 2,
                    "sandstone_stairs",
                    ItemType.SandstoneStairs,
                    BlockShape.Stairs),
                new BlockContext(
                    Block.NorthBottomSandstoneStairs,
                    128 << 4 | 3,
                    "sandstone_stairs",
                    ItemType.SandstoneStairs,
                    BlockShape.Stairs),
                new BlockContext(
                    Block.EastTopSandstoneStairs,
                    128 << 4 | 4,
                    "sandstone_stairs",
                    ItemType.SandstoneStairs,
                    BlockShape.Stairs),
                new BlockContext(
                    Block.WestTopSandstoneStairs,
                    128 << 4 | 5,
                    "sandstone_stairs",
                    ItemType.SandstoneStairs,
                    BlockShape.Stairs),
                new BlockContext(
                    Block.SouthTopSandstoneStairs,
                    128 << 4 | 6,
                    "sandstone_stairs",
                    ItemType.SandstoneStairs,
                    BlockShape.Stairs),
                new BlockContext(
                    Block.NorthTopSandstoneStairs,
                    128 << 4 | 7,
                    "sandstone_stairs",
                    ItemType.SandstoneStairs,
                    BlockShape.Stairs),

                new BlockContext(
                    Block.EmeraldBlock,
                    133 << 4 | 0,
                    "emerald_block",
                    ItemType.EmeraldBlock,
                    BlockShape.Cube),

                new BlockContext(
                    Block.EastBottomSpruceWoodStairs,
                    134 << 4 | 0,
                    "spruce_stairs",
                    ItemType.SpruceWoodStairs,
                    BlockShape.Stairs),
                new BlockContext(
                    Block.WestBottomSpruceWoodStairs,
                    134 << 4 | 1,
                    "spruce_stairs",
                    ItemType.SpruceWoodStairs,
                    BlockShape.Stairs),
                new BlockContext(
                    Block.SouthBottomSpruceWoodStairs,
                    134 << 4 | 2,
                    "spruce_stairs",
                    ItemType.SpruceWoodStairs,
                    BlockShape.Stairs),
                new BlockContext(
                    Block.NorthBottomSpruceWoodStairs,
                    134 << 4 | 3,
                    "spruce_stairs",
                    ItemType.SpruceWoodStairs,
                    BlockShape.Stairs),
                new BlockContext(
                    Block.EastTopSpruceWoodStairs,
                    134 << 4 | 4,
                    "spruce_stairs",
                    ItemType.SpruceWoodStairs,
                    BlockShape.Stairs),
                new BlockContext(
                    Block.WestTopSpruceWoodStairs,
                    134 << 4 | 5,
                    "spruce_stairs",
                    ItemType.SpruceWoodStairs,
                    BlockShape.Stairs),
                new BlockContext(
                    Block.SouthTopSpruceWoodStairs,
                    134 << 4 | 6,
                    "spruce_stairs",
                    ItemType.SpruceWoodStairs,
                    BlockShape.Stairs),
                new BlockContext(
                    Block.NorthTopSpruceWoodStairs,
                    134 << 4 | 7,
                    "spruce_stairs",
                    ItemType.SpruceWoodStairs,
                    BlockShape.Stairs),

                new BlockContext(
                    Block.EastBottomBirchWoodStairs,
                    135 << 4 | 0,
                    "birch_stairs",
                    ItemType.BirchWoodStairs,
                    BlockShape.Stairs),
                new BlockContext(
                    Block.WestBottomBirchWoodStairs,
                    135 << 4 | 1,
                    "birch_stairs",
                    ItemType.BirchWoodStairs,
                    BlockShape.Stairs),
                new BlockContext(
                    Block.SouthBottomBirchWoodStairs,
                    135 << 4 | 2,
                    "birch_stairs",
                    ItemType.BirchWoodStairs,
                    BlockShape.Stairs),
                new BlockContext(
                    Block.NorthBottomBirchWoodStairs,
                    135 << 4 | 3,
                    "birch_stairs",
                    ItemType.BirchWoodStairs,
                    BlockShape.Stairs),
                new BlockContext(
                    Block.EastTopBirchWoodStairs,
                    135 << 4 | 4,
                    "birch_stairs",
                    ItemType.BirchWoodStairs,
                    BlockShape.Stairs),
                new BlockContext(
                    Block.WestTopBirchWoodStairs,
                    135 << 4 | 5,
                    "birch_stairs",
                    ItemType.BirchWoodStairs,
                    BlockShape.Stairs),
                new BlockContext(
                    Block.SouthTopBirchWoodStairs,
                    135 << 4 | 6,
                    "birch_stairs",
                    ItemType.BirchWoodStairs,
                    BlockShape.Stairs),
                new BlockContext(
                    Block.NorthTopBirchWoodStairs,
                    135 << 4 | 7,
                    "birch_stairs",
                    ItemType.BirchWoodStairs,
                    BlockShape.Stairs),

                new BlockContext(
                    Block.EastBottomJungleWoodStairs,
                    136 << 4 | 0,
                    "jungle_stairs",
                    ItemType.JungleWoodStairs,
                    BlockShape.Stairs),
                new BlockContext(
                    Block.WestBottomJungleWoodStairs,
                    136 << 4 | 1,
                    "jungle_stairs",
                    ItemType.JungleWoodStairs,
                    BlockShape.Stairs),
                new BlockContext(
                    Block.SouthBottomJungleWoodStairs,
                    136 << 4 | 2,
                    "jungle_stairs",
                    ItemType.JungleWoodStairs,
                    BlockShape.Stairs),
                new BlockContext(
                    Block.NorthBottomJungleWoodStairs,
                    136 << 4 | 3,
                    "jungle_stairs",
                    ItemType.JungleWoodStairs,
                    BlockShape.Stairs),
                new BlockContext(
                    Block.EastTopJungleWoodStairs,
                    136 << 4 | 4,
                    "jungle_stairs",
                    ItemType.JungleWoodStairs,
                    BlockShape.Stairs),
                new BlockContext(
                    Block.WestTopJungleWoodStairs,
                    136 << 4 | 5,
                    "jungle_stairs",
                    ItemType.JungleWoodStairs,
                    BlockShape.Stairs),
                new BlockContext(
                    Block.SouthTopJungleWoodStairs,
                    136 << 4 | 6,
                    "jungle_stairs",
                    ItemType.JungleWoodStairs,
                    BlockShape.Stairs),
                new BlockContext(
                    Block.NorthTopJungleWoodStairs,
                    136 << 4 | 7,
                    "jungle_stairs",
                    ItemType.JungleWoodStairs,
                    BlockShape.Stairs),


                new BlockContext(
                    Block.CobblestoneWall,
                    139 << 4 | 0,
                    "cobblestone_wall",
                    ItemType.CobblestoneWall,
                    BlockShape.Wall),
                new BlockContext(
                    Block.MossyCobblestoneWall,
                    139 << 4 | 1,
                    "cobblestone_wall",
                    ItemType.MossyCobblestoneWall,
                    BlockShape.Wall),

                new BlockContext(
                    Block.RedstoneBlock,
                    152 << 4 | 0,
                    "redstone_block",
                    ItemType.RedstoneBlock,
                    BlockShape.Cube),

                new BlockContext(
                    Block.HayBale_AxisY,
                    170 << 4 | 0,  // 0~3, 12~15
                    "hay_block",
                    ItemType.HayBale,
                    BlockShape.Cube),
                new BlockContext(
                    Block.HayBale_AxisX,
                    170 << 4 | 4,  // 4~7
                    "hay_block",
                    ItemType.HayBale,
                    BlockShape.Cube),
                new BlockContext(
                    Block.HayBale_AxisZ,
                    170 << 4 | 8,  // 8~11
                    "hay_block",
                    ItemType.HayBale,
                    BlockShape.Cube),

                new BlockContext(
                    Block.WhiteCarpet,
                    171 << 4 | 0,
                    "carpet",
                    ItemType.WhiteCarpet,
                    BlockShape.Carpet),
                new BlockContext(
                    Block.OrangeCarpet,
                    171 << 4 | 1,
                    "carpet",
                    ItemType.OrangeCarpet,
                    BlockShape.Carpet),
                new BlockContext(
                    Block.MagentaCarpet,
                    171 << 4 | 2,
                    "carpet",
                    ItemType.MagentaCarpet,
                    BlockShape.Carpet),
                new BlockContext(
                    Block.LightBlueCarpet,
                    171 << 4 | 3,
                    "carpet",
                    ItemType.LightBlueCarpet,
                    BlockShape.Carpet),
                new BlockContext(
                    Block.YellowCarpet,
                    171 << 4 | 4,
                    "carpet",
                    ItemType.YellowCarpet,
                    BlockShape.Carpet),
                new BlockContext(
                    Block.LimeCarpet,
                    171 << 4 | 5,
                    "carpet",
                    ItemType.LimeCarpet,
                    BlockShape.Carpet),
                new BlockContext(
                    Block.PinkCarpet,
                    171 << 4 | 6,
                    "carpet",
                    ItemType.PinkCarpet,
                    BlockShape.Carpet),
                new BlockContext(
                    Block.GrayCarpet,
                    171 << 4 | 7,
                    "carpet",
                    ItemType.GrayCarpet,
                    BlockShape.Carpet),
                new BlockContext(
                    Block.LightGrayCarpet,
                    171 << 4 | 8,
                    "carpet",
                    ItemType.LightGrayCarpet,
                    BlockShape.Carpet),
                new BlockContext(
                    Block.CyanCarpet,
                    171 << 4 | 9,
                    "carpet",
                    ItemType.CyanCarpet,
                    BlockShape.Carpet),
                new BlockContext(
                    Block.PurpleCarpet,
                    171 << 4 | 10,
                    "carpet",
                    ItemType.PurpleCarpet,
                    BlockShape.Carpet),
                new BlockContext(
                    Block.BlueCarpet,
                    171 << 4 | 11,
                    "carpet",
                    ItemType.BlueCarpet,
                    BlockShape.Carpet),
                new BlockContext(
                    Block.BrownCarpet,
                    171 << 4 | 12,
                    "carpet",
                    ItemType.BrownCarpet,
                    BlockShape.Carpet),
                new BlockContext(
                    Block.GreenCarpet,
                    171 << 4 | 13,
                    "carpet",
                    ItemType.GreenCarpet,
                    BlockShape.Carpet),
                new BlockContext(
                    Block.RedCarpet,
                    171 << 4 | 14,
                    "carpet",
                    ItemType.RedCarpet,
                    BlockShape.Carpet),
                new BlockContext(
                    Block.BlackCarpet,
                    171 << 4 | 15,
                    "carpet",
                    ItemType.BlackCarpet,
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

        public static bool IsItemable(this Block block)
        {
            return _BLOCK_ENUM_TO_CTX_MAP.Lookup(block).IsItemable;
        }

        public static ItemType GetItemType(this Block block)
        {
            bool f = _BLOCK_ENUM_TO_CTX_MAP.Lookup(block).IsItemable;
            if (f == false)
            {
                throw new System.InvalidOperationException("Block is not itemable");
            }
            return _BLOCK_ENUM_TO_CTX_MAP.Lookup(block).ItemType;
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

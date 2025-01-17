
using Containers;

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
            _BLOCK_ENUM_TO_CTX_MAP.Insert(Block.Air, new BlockContext(
                Block.Air,
                (0 << 4) | 0,
                "air",
                BlockShape.None));
            _BLOCK_ENUM_TO_CTX_MAP.Insert(Block.Stone, new BlockContext(
                Block.Stone,
                (1 << 4) | 0,
                "stone",
                BlockShape.Cube));
            _BLOCK_ENUM_TO_CTX_MAP.Insert(Block.Granite, new BlockContext(
                Block.Granite,
                (1 << 4) | 1,
                "stone",
                BlockShape.Cube));
            _BLOCK_ENUM_TO_CTX_MAP.Insert(Block.PolishedGranite, new BlockContext(
                Block.PolishedGranite,
                (1 << 4) | 2,
                "stone",
                BlockShape.Cube));
            _BLOCK_ENUM_TO_CTX_MAP.Insert(Block.Diorite, new BlockContext(
                Block.Diorite,
                (1 << 4) | 3,
                "stone",
                BlockShape.Cube));
            _BLOCK_ENUM_TO_CTX_MAP.Insert(Block.PolishedDiorite, new BlockContext(
                Block.PolishedDiorite,
                (1 << 4) | 4,
                "stone",
                BlockShape.Cube));
            _BLOCK_ENUM_TO_CTX_MAP.Insert(Block.Andesite, new BlockContext(
                Block.Andesite,
                (1 << 4) | 5,
                "stone",
                BlockShape.Cube));
            _BLOCK_ENUM_TO_CTX_MAP.Insert(Block.PolishedAndesite, new BlockContext(
                Block.PolishedAndesite,
                (1 << 4) | 6,
                "stone",
                BlockShape.Cube));
            _BLOCK_ENUM_TO_CTX_MAP.Insert(Block.GrassBlock, new BlockContext(
                Block.GrassBlock,
                (2 << 4) | 0,
                "grass",
                BlockShape.Cube));
            _BLOCK_ENUM_TO_CTX_MAP.Insert(Block.Dirt, new BlockContext(
                Block.Dirt,
                (3 << 4) | 0,
                "dirt",
                BlockShape.Cube));
            _BLOCK_ENUM_TO_CTX_MAP.Insert(Block.CoarseDirt, new BlockContext(
                Block.CoarseDirt,
                (3 << 4) | 1,
                "dirt",
                BlockShape.Cube));
            _BLOCK_ENUM_TO_CTX_MAP.Insert(Block.Podzol, new BlockContext(
                Block.Podzol,
                (3 << 4) | 2,
                "dirt",
                BlockShape.Cube));
            _BLOCK_ENUM_TO_CTX_MAP.Insert(Block.Cobblestone, new BlockContext(
                Block.Cobblestone,
                (4 << 4) | 0,
                "cobblestone",
                BlockShape.Cube));
            _BLOCK_ENUM_TO_CTX_MAP.Insert(Block.OakWoodPlanks, new BlockContext(
                Block.OakWoodPlanks,
                (5 << 4) | 0,
                "planks",
                BlockShape.Cube));
            _BLOCK_ENUM_TO_CTX_MAP.Insert(Block.SpruceWoodPlanks, new BlockContext(
                Block.SpruceWoodPlanks,
                (5 << 4) | 1,
                "planks",
                BlockShape.Cube));
            _BLOCK_ENUM_TO_CTX_MAP.Insert(Block.BirchWoodPlanks, new BlockContext(
                Block.BirchWoodPlanks,
                (5 << 4) | 2,
                "planks",
                BlockShape.Cube));
            _BLOCK_ENUM_TO_CTX_MAP.Insert(Block.JungleWoodPlanks, new BlockContext(
                Block.JungleWoodPlanks,
                (5 << 4) | 3,
                "planks",
                BlockShape.Cube));
            _BLOCK_ENUM_TO_CTX_MAP.Insert(Block.AcaciaWoodPlanks, new BlockContext(
                Block.AcaciaWoodPlanks,
                (5 << 4) | 4,
                "planks",
                BlockShape.Cube));
            _BLOCK_ENUM_TO_CTX_MAP.Insert(Block.DarkOakWoodPlanks, new BlockContext(
                Block.DarkOakWoodPlanks,
                (5 << 4) | 5,
                "planks",
                BlockShape.Cube));
            _BLOCK_ENUM_TO_CTX_MAP.Insert(Block.OakSapling, new BlockContext(
                Block.OakSapling,
                (6 << 4) | 0,
                "sapling",
                BlockShape.None));
            _BLOCK_ENUM_TO_CTX_MAP.Insert(Block.SpruceSapling, new BlockContext(
                Block.SpruceSapling,
                (6 << 4) | 1,
                "sapling",
                BlockShape.None));
            _BLOCK_ENUM_TO_CTX_MAP.Insert(Block.BirchSapling, new BlockContext(
                Block.BirchSapling,
                (6 << 4) | 2,
                "sapling",
                BlockShape.None));
            _BLOCK_ENUM_TO_CTX_MAP.Insert(Block.JungleSapling, new BlockContext(
                Block.JungleSapling,
                (6 << 4) | 3,
                "sapling",
                BlockShape.None));
            _BLOCK_ENUM_TO_CTX_MAP.Insert(Block.AcaciaSapling, new BlockContext(
                Block.AcaciaSapling,
                (6 << 4) | 4,
                "sapling",
                BlockShape.None));
            _BLOCK_ENUM_TO_CTX_MAP.Insert(Block.DarkOakSapling, new BlockContext(
                Block.DarkOakSapling,
                (6 << 4) | 5,
                "sapling",
                BlockShape.None));
            _BLOCK_ENUM_TO_CTX_MAP.Insert(Block.Bedrock, new BlockContext(
                Block.Bedrock,
                (7 << 4) | 0,
                "bedrock",
                BlockShape.Cube));

            _BLOCK_ENUM_TO_CTX_MAP.Insert(Block.FlowingWater, new BlockContext(
                Block.FlowingWater,
                (8 << 4) | 0,
                "flowing_water",
                BlockShape.None));
            _BLOCK_ENUM_TO_CTX_MAP.Insert(Block.StillWater, new BlockContext(
                Block.StillWater,
                (9 << 4) | 0,
                "water",
                BlockShape.None));
            _BLOCK_ENUM_TO_CTX_MAP.Insert(Block.FlowingLava, new BlockContext(
                Block.FlowingLava,
                (10 << 4) | 0,
                "flowing_lava",
                BlockShape.None));
            _BLOCK_ENUM_TO_CTX_MAP.Insert(Block.StillLava, new BlockContext(
                Block.StillLava,
                (11 << 4) | 0,
                "lava",
                BlockShape.None));
            _BLOCK_ENUM_TO_CTX_MAP.Insert(Block.Sand, new BlockContext(
                Block.Sand,
                (12 << 4) | 0,
                "sand",
                BlockShape.Cube));
            _BLOCK_ENUM_TO_CTX_MAP.Insert(Block.RedSand, new BlockContext(
                Block.RedSand,
                (12 << 4) | 1,
                "sand",
                BlockShape.Cube));
            _BLOCK_ENUM_TO_CTX_MAP.Insert(Block.Gravel, new BlockContext(
                Block.Gravel,
                (13 << 4) | 0,
                "gravel",
                BlockShape.Cube));
            _BLOCK_ENUM_TO_CTX_MAP.Insert(Block.GoldOre, new BlockContext(
                Block.GoldOre,
                (14 << 4) | 0,
                "gold_ore",
                BlockShape.Cube));
            _BLOCK_ENUM_TO_CTX_MAP.Insert(Block.IronOre, new BlockContext(
                Block.IronOre,
                (15 << 4) | 0,
                "iron_ore",
                BlockShape.Cube));
            _BLOCK_ENUM_TO_CTX_MAP.Insert(Block.CoalOre, new BlockContext(
                Block.CoalOre,
                (16 << 4) | 0,
                "coal_ore",
                BlockShape.Cube));
            _BLOCK_ENUM_TO_CTX_MAP.Insert(Block.OakWood, new BlockContext(
                Block.OakWood,
                (17 << 4) | 0,
                "log",
                BlockShape.Cube));
            _BLOCK_ENUM_TO_CTX_MAP.Insert(Block.SpruceWood, new BlockContext(
                Block.SpruceWood,
                (17 << 4) | 1,
                "log",
                BlockShape.Cube));
            _BLOCK_ENUM_TO_CTX_MAP.Insert(Block.BirchWood, new BlockContext(
                Block.BirchWood,
                (17 << 4) | 2,
                "log",
                BlockShape.Cube));
            _BLOCK_ENUM_TO_CTX_MAP.Insert(Block.JungleWood, new BlockContext(
                Block.JungleWood,
                (17 << 4) | 3,
                "log",
                BlockShape.Cube));
            _BLOCK_ENUM_TO_CTX_MAP.Insert(Block.OakLeaves, new BlockContext(
                Block.OakLeaves,
                (18 << 4) | 0,
                "leaves",
                BlockShape.Cube));
            _BLOCK_ENUM_TO_CTX_MAP.Insert(Block.SpruceLeaves, new BlockContext(
                Block.SpruceLeaves,
                (18 << 4) | 1,
                "leaves",
                BlockShape.Cube));
            _BLOCK_ENUM_TO_CTX_MAP.Insert(Block.BirchLeaves, new BlockContext(
                Block.BirchLeaves,
                (18 << 4) | 2,
                "leaves",
                BlockShape.Cube));
            _BLOCK_ENUM_TO_CTX_MAP.Insert(Block.JungleLeaves, new BlockContext(
                Block.JungleLeaves,
                (18 << 4) | 3,
                "leaves",
                BlockShape.Cube));
            _BLOCK_ENUM_TO_CTX_MAP.Insert(Block.Sponge, new BlockContext(
                Block.Sponge,
                (19 << 4) | 0,
                "sponge",
                BlockShape.Cube));
            _BLOCK_ENUM_TO_CTX_MAP.Insert(Block.WetSponge, new BlockContext(
                Block.WetSponge,
                (19 << 4) | 1,
                "sponge",
                BlockShape.Cube));
            _BLOCK_ENUM_TO_CTX_MAP.Insert(Block.Glass, new BlockContext(
                Block.Glass,
                (20 << 4) | 0,
                "glass",
                BlockShape.Cube));
            _BLOCK_ENUM_TO_CTX_MAP.Insert(Block.LapisLazuliOre, new BlockContext(
                Block.LapisLazuliOre,
                (21 << 4) | 0,
                "lapis_ore",
                BlockShape.Cube));
            _BLOCK_ENUM_TO_CTX_MAP.Insert(Block.LapisLazuliBlock, new BlockContext(
                Block.LapisLazuliBlock,
                (22 << 4) | 0,
                "lapis_block",
                BlockShape.Cube));
            _BLOCK_ENUM_TO_CTX_MAP.Insert(Block.Dispenser, new BlockContext(
                Block.Dispenser,
                (23 << 4) | 0,
                "dispenser",
                BlockShape.Cube));
            _BLOCK_ENUM_TO_CTX_MAP.Insert(Block.Sandstone, new BlockContext(
                Block.Sandstone,
                (24 << 4) | 0,
                "sandstone",
                BlockShape.Cube));
            _BLOCK_ENUM_TO_CTX_MAP.Insert(Block.ChiseledSandstone, new BlockContext(
                Block.ChiseledSandstone,
                (24 << 4) | 1,
                 "sandstone",
                 BlockShape.Cube));
            _BLOCK_ENUM_TO_CTX_MAP.Insert(Block.SmoothSandstone, new BlockContext(
                Block.SmoothSandstone,
                (24 << 4) | 2,
                 "sandstone",
                 BlockShape.Cube));
            _BLOCK_ENUM_TO_CTX_MAP.Insert(Block.NoteBlock, new BlockContext(
                Block.NoteBlock,
                (25 << 4) | 0,
                 "noteblock",
                 BlockShape.Cube));

            _BLOCK_ENUM_TO_CTX_MAP.Insert(Block.EastBottomOakWoodStairs, new BlockContext(
                Block.EastBottomOakWoodStairs,
                (53 << 4) | 0,
                "oak_stairs",
                BlockShape.Stairs));
            _BLOCK_ENUM_TO_CTX_MAP.Insert(Block.WestBottomOakWoodStairs, new BlockContext(
                Block.WestBottomOakWoodStairs,
                (53 << 4) | 1,
                "oak_stairs",
                BlockShape.Stairs));
            _BLOCK_ENUM_TO_CTX_MAP.Insert(Block.SouthBottomOakWoodStairs, new BlockContext(
                Block.SouthBottomOakWoodStairs,
                (53 << 4) | 2,
                "oak_stairs",
                BlockShape.Stairs));
            _BLOCK_ENUM_TO_CTX_MAP.Insert(Block.NorthBottomOakWoodStairs, new BlockContext(
                Block.NorthBottomOakWoodStairs,
                (53 << 4) | 3,
                "oak_stairs",
                BlockShape.Stairs));
            _BLOCK_ENUM_TO_CTX_MAP.Insert(Block.EastTopOakWoodStairs, new BlockContext(
                Block.EastTopOakWoodStairs,
                (53 << 4) | 4,
                "oak_stairs",
                BlockShape.Stairs));
            _BLOCK_ENUM_TO_CTX_MAP.Insert(Block.WestTopOakWoodStairs, new BlockContext(
                Block.WestTopOakWoodStairs,
                (53 << 4) | 5,
                "oak_stairs",
                BlockShape.Stairs));
            _BLOCK_ENUM_TO_CTX_MAP.Insert(Block.SouthTopOakWoodStairs, new BlockContext(
                Block.SouthTopOakWoodStairs,
                (53 << 4) | 6,
                "oak_stairs",
                BlockShape.Stairs));
            _BLOCK_ENUM_TO_CTX_MAP.Insert(Block.NorthTopOakWoodStairs, new BlockContext(
                Block.NorthTopOakWoodStairs,
                (53 << 4) | 7,
                "oak_stairs",
                BlockShape.Stairs));

            foreach ((Block block, BlockContext ctx) in _BLOCK_ENUM_TO_CTX_MAP.GetElements())
            {
                _BLOCK_ID_TO_ENUM_MAP.Insert(ctx.Id, block);
            }

            System.Diagnostics.Debug.Assert(
                _BLOCK_ENUM_TO_CTX_MAP.Count == _BLOCK_ID_TO_ENUM_MAP.Count);

        }

        public static Block ToBlock(int id)
        {
            return _BLOCK_ID_TO_ENUM_MAP.Lookup(id);
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

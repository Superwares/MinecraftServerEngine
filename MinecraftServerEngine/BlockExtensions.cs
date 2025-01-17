
using Containers;

namespace MinecraftServerEngine
{
    internal static class BlockExtensions
    {
        private struct BlockContext
        {
            public Block Block { get; }
            public int Id { get; }
            public BlockShape Shape { get; }
            public BlockContext(
                Block block, 
                int id, 
                BlockShape shape)
            {
                Block = block;
                Id = id;
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
                BlockShape.None));
            _BLOCK_ENUM_TO_CTX_MAP.Insert(Block.Stone, new BlockContext(
                Block.Stone, 
                (1 << 4) | 0, 
                BlockShape.Cube));
            _BLOCK_ENUM_TO_CTX_MAP.Insert(Block.Granite, new BlockContext(
                Block.Granite, 
                (1 << 4) | 1, 
                BlockShape.Cube));
            _BLOCK_ENUM_TO_CTX_MAP.Insert(Block.PolishedGranite, new BlockContext(
                Block.PolishedGranite, 
                (1 << 4) | 2, 
                BlockShape.Cube));
            _BLOCK_ENUM_TO_CTX_MAP.Insert(Block.Diorite, new BlockContext(
                Block.Diorite, 
                (1 << 4) | 3, 
                BlockShape.Cube));
            _BLOCK_ENUM_TO_CTX_MAP.Insert(Block.PolishedDiorite, new BlockContext(
                Block.PolishedDiorite, 
                (1 << 4) | 4, 
                BlockShape.Cube));
            _BLOCK_ENUM_TO_CTX_MAP.Insert(Block.Andesite, new BlockContext(
                Block.Andesite, 
                (1 << 4) | 5, 
                BlockShape.Cube));
            _BLOCK_ENUM_TO_CTX_MAP.Insert(Block.PolishedAndesite, new BlockContext(
                Block.PolishedAndesite, 
                (1 << 4) | 6, 
                BlockShape.Cube));
            _BLOCK_ENUM_TO_CTX_MAP.Insert(Block.GrassBlock, new BlockContext(
                Block.GrassBlock, 
                (2 << 4) | 0, 
                BlockShape.Cube));
            _BLOCK_ENUM_TO_CTX_MAP.Insert(Block.Dirt, new BlockContext(
                Block.Dirt, 
                (3 << 4) | 0, 
                BlockShape.Cube));
            _BLOCK_ENUM_TO_CTX_MAP.Insert(Block.CoarseDirt, new BlockContext(
                Block.CoarseDirt, 
                (3 << 4) | 1, 
                BlockShape.Cube));
            _BLOCK_ENUM_TO_CTX_MAP.Insert(Block.Podzol, new BlockContext(
                Block.Podzol, 
                (3 << 4) | 2, 
                BlockShape.Cube));

            _BLOCK_ENUM_TO_CTX_MAP.Insert(Block.Cobblestone, new BlockContext(
                Block.Cobblestone, 
                (4 << 4) | 0, 
                BlockShape.Cube));
            _BLOCK_ENUM_TO_CTX_MAP.Insert(Block.OakWoodPlanks, new BlockContext(
                Block.OakWoodPlanks, 
                (5 << 4) | 0, 
                BlockShape.Cube));
            _BLOCK_ENUM_TO_CTX_MAP.Insert(Block.SpruceWoodPlanks, new BlockContext(
                Block.SpruceWoodPlanks, 
                (5 << 4) | 1, 
                BlockShape.Cube));
            _BLOCK_ENUM_TO_CTX_MAP.Insert(Block.BirchWoodPlanks, new BlockContext(
                Block.BirchWoodPlanks, 
                (5 << 4) | 2, 
                BlockShape.Cube));
            _BLOCK_ENUM_TO_CTX_MAP.Insert(Block.JungleWoodPlanks, new BlockContext(
                Block.JungleWoodPlanks, 
                (5 << 4) | 3, 
                BlockShape.Cube));
            _BLOCK_ENUM_TO_CTX_MAP.Insert(Block.AcaciaWoodPlanks, new BlockContext(
                Block.AcaciaWoodPlanks, 
                (5 << 4) | 4, 
                BlockShape.Cube));
            _BLOCK_ENUM_TO_CTX_MAP.Insert(Block.DarkOakWoodPlanks, new BlockContext(
                Block.DarkOakWoodPlanks, 
                (5 << 4) | 5, 
                BlockShape.Cube));
            _BLOCK_ENUM_TO_CTX_MAP.Insert(Block.OakSapling, new BlockContext(
                Block.OakSapling, 
                (6 << 4) | 0, 
                BlockShape.None));
            _BLOCK_ENUM_TO_CTX_MAP.Insert(Block.SpruceSapling, new BlockContext(
                Block.SpruceSapling, 
                (6 << 4) | 1, 
                BlockShape.None));
            _BLOCK_ENUM_TO_CTX_MAP.Insert(Block.BirchSapling, new BlockContext(
                Block.BirchSapling, 
                (6 << 4) | 2, 
                BlockShape.None));
            _BLOCK_ENUM_TO_CTX_MAP.Insert(Block.JungleSapling, new BlockContext(
                Block.JungleSapling, 
                (6 << 4) | 3, 
                BlockShape.None));
            _BLOCK_ENUM_TO_CTX_MAP.Insert(Block.AcaciaSapling, new BlockContext(
                Block.AcaciaSapling, 
                (6 << 4) | 4, 
                BlockShape.None));
            _BLOCK_ENUM_TO_CTX_MAP.Insert(Block.DarkOakSapling, new BlockContext(
                Block.DarkOakSapling, 
                (6 << 4) | 5, 
                BlockShape.None));
            _BLOCK_ENUM_TO_CTX_MAP.Insert(Block.Bedrock, new BlockContext(
                Block.Bedrock, 
                (7 << 4) | 0, 
                BlockShape.Cube));

            _BLOCK_ENUM_TO_CTX_MAP.Insert(Block.EastBottomOakWoodStairs, new BlockContext(
                Block.EastBottomOakWoodStairs, 
                (53 << 4) | 0, 
                BlockShape.Stairs));
            _BLOCK_ENUM_TO_CTX_MAP.Insert(Block.WestBottomOakWoodStairs, new BlockContext(
                Block.WestBottomOakWoodStairs, 
                (53 << 4) | 1, 
                BlockShape.Stairs));
            _BLOCK_ENUM_TO_CTX_MAP.Insert(Block.SouthBottomOakWoodStairs, new BlockContext(
                Block.SouthBottomOakWoodStairs, 
                (53 << 4) | 2, 
                BlockShape.Stairs));
            _BLOCK_ENUM_TO_CTX_MAP.Insert(Block.NorthBottomOakWoodStairs, new BlockContext(
                Block.NorthBottomOakWoodStairs, 
                (53 << 4) | 3, 
                BlockShape.Stairs));
            _BLOCK_ENUM_TO_CTX_MAP.Insert(Block.EastTopOakWoodStairs, new BlockContext(
                Block.EastTopOakWoodStairs, 
                (53 << 4) | 4, 
                BlockShape.Stairs));
            _BLOCK_ENUM_TO_CTX_MAP.Insert(Block.WestTopOakWoodStairs, new BlockContext(
                Block.WestTopOakWoodStairs, 
                (53 << 4) | 5, 
                BlockShape.Stairs));
            _BLOCK_ENUM_TO_CTX_MAP.Insert(Block.SouthTopOakWoodStairs, new BlockContext(
                Block.SouthTopOakWoodStairs, 
                (53 << 4) | 6, 
                BlockShape.Stairs));
            _BLOCK_ENUM_TO_CTX_MAP.Insert(Block.NorthTopOakWoodStairs, new BlockContext(
                Block.NorthTopOakWoodStairs, 
                (53 << 4) | 7, 
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
    }
}

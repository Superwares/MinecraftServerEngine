
using Containers;

namespace MinecraftServerEngine
{
    internal static class BlockExtensions
    {
        private readonly static Table<Blocks, int> _BLOCK_ENUM_TO_ID_MAP = new();
        private readonly static Table<int, Blocks> _BLOCK_ID_TO_ENUM_MAP = new();

        static BlockExtensions()
        {
            _BLOCK_ENUM_TO_ID_MAP.Insert(Blocks.Air, (0 << 4) | 0);
            _BLOCK_ENUM_TO_ID_MAP.Insert(Blocks.Stone, (1 << 4) | 0);
            _BLOCK_ENUM_TO_ID_MAP.Insert(Blocks.Granite, (1 << 4) | 1);
            _BLOCK_ENUM_TO_ID_MAP.Insert(Blocks.PolishedGranite, (1 << 4) | 2);
            _BLOCK_ENUM_TO_ID_MAP.Insert(Blocks.Diorite, (1 << 4) | 3);
            _BLOCK_ENUM_TO_ID_MAP.Insert(Blocks.PolishedDiorite, (1 << 4) | 4);
            _BLOCK_ENUM_TO_ID_MAP.Insert(Blocks.Andesite, (1 << 4) | 5);
            _BLOCK_ENUM_TO_ID_MAP.Insert(Blocks.PolishedAndesite, (1 << 4) | 6);
            _BLOCK_ENUM_TO_ID_MAP.Insert(Blocks.Grass, (2 << 4) | 0);
            _BLOCK_ENUM_TO_ID_MAP.Insert(Blocks.Dirt, (3 << 4) | 0);
            _BLOCK_ENUM_TO_ID_MAP.Insert(Blocks.CoarseDirt, (3 << 4) | 1);
            _BLOCK_ENUM_TO_ID_MAP.Insert(Blocks.Podzol, (3 << 4) | 2);

            _BLOCK_ENUM_TO_ID_MAP.Insert(Blocks.EastBottomOakWoodStairs, (53 << 4) | 0);
            _BLOCK_ENUM_TO_ID_MAP.Insert(Blocks.WestBottomOakWoodStairs, (53 << 4) | 1);
            _BLOCK_ENUM_TO_ID_MAP.Insert(Blocks.SouthBottomOakWoodStairs, (53 << 4) | 2);
            _BLOCK_ENUM_TO_ID_MAP.Insert(Blocks.NorthBottomOakWoodStairs, (53 << 4) | 3);
            _BLOCK_ENUM_TO_ID_MAP.Insert(Blocks.EastTopOakWoodStairs, (53 << 4) | 4);
            _BLOCK_ENUM_TO_ID_MAP.Insert(Blocks.WestTopOakWoodStairs, (53 << 4) | 5);
            _BLOCK_ENUM_TO_ID_MAP.Insert(Blocks.SouthTopOakWoodStairs, (53 << 4) | 6);
            _BLOCK_ENUM_TO_ID_MAP.Insert(Blocks.NorthTopOakWoodStairs, (53 << 4) | 7);

            foreach ((Blocks block, int id) in _BLOCK_ENUM_TO_ID_MAP.GetElements())
            {
                _BLOCK_ID_TO_ENUM_MAP.Insert(id, block);
            }

            System.Diagnostics.Debug.Assert(_BLOCK_ENUM_TO_ID_MAP.Count == _BLOCK_ID_TO_ENUM_MAP.Count);

        }

        public static Blocks ToBlock(int id)
        {
            System.Diagnostics.Debug.Assert(_BLOCK_ID_TO_ENUM_MAP.Contains(id));

            return _BLOCK_ID_TO_ENUM_MAP.Lookup(id);
        }

        public static int GetId(this Blocks block)
        {
            System.Diagnostics.Debug.Assert(_BLOCK_ENUM_TO_ID_MAP.Contains(block));

            return _BLOCK_ENUM_TO_ID_MAP.Lookup(block);
        }
    }
}

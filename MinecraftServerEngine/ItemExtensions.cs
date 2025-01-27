
using Containers;

namespace MinecraftServerEngine
{
    public static class ItemExtensions
    {
        private struct ItemContext
        {
            public readonly ItemType Type;
            public readonly int Id;
            public readonly int MinStackCount, MaxStackCount;
            public readonly int Metadata;

            public ItemContext(
                ItemType type,
                int id,
                int minStackCount, int maxStackCount,
                int metadata)
            {
                System.Diagnostics.Debug.Assert(maxStackCount > 0);
                System.Diagnostics.Debug.Assert(minStackCount > 0);
                System.Diagnostics.Debug.Assert(minStackCount <= maxStackCount);

                Type = type;
                Id = id;
                MinStackCount = minStackCount;
                MaxStackCount = maxStackCount;
                Metadata = metadata;
            }

        }

        // TODO: Replace as IReadOnlyTable
        // TODO: 프로그램이 종료되었을 때 자원 해제하기. static destructor?
        private readonly static Table<ItemType, ItemContext> _ITEM_ENUM_TO_CTX_MAP = new();
        private readonly static Table<int, ItemType> _ITEM_ID_TO_ENUM_MAP = new();

        static ItemExtensions()
        {
            _ITEM_ENUM_TO_CTX_MAP.Insert(ItemType.IronSword, new ItemContext(
                ItemType.IronSword, 267,
                1, 1,
                0));
            _ITEM_ENUM_TO_CTX_MAP.Insert(ItemType.WoodenSword, new ItemContext(
                ItemType.WoodenSword, 268,
                1, 1,
                0));

            _ITEM_ENUM_TO_CTX_MAP.Insert(ItemType.StoneSword, new ItemContext(
                ItemType.StoneSword, 272,
                1, 1,
                0));

            _ITEM_ENUM_TO_CTX_MAP.Insert(ItemType.DiamondSword, new ItemContext(
                ItemType.DiamondSword, 276,
                1, 1,
                0));

            _ITEM_ENUM_TO_CTX_MAP.Insert(ItemType.GoldenSword, new ItemContext(
                ItemType.GoldenSword, 283,
                1, 1,
                0));

            _ITEM_ENUM_TO_CTX_MAP.Insert(ItemType.Stick, new ItemContext(
                ItemType.Stick, 280,
                1, 64,
                0));

            // This item is handled by clientside.
            //_ITEM_ENUM_TO_CTX_MAP.Insert(ItemType.Snowball, new ItemContext(
            //    ItemType.Snowball, 332,
            //    1, 16,
            //    0));

            _ITEM_ENUM_TO_CTX_MAP.Insert(ItemType.GoldNugget, new ItemContext(
                ItemType.GoldNugget, 371,
                1, 64,
                0));

            _ITEM_ENUM_TO_CTX_MAP.Insert(ItemType.PlayerSkull, new ItemContext(
                ItemType.PlayerSkull, 397,
                1, 64,
                3));

            foreach ((ItemType item, ItemContext ctx) in _ITEM_ENUM_TO_CTX_MAP.GetElements())
            {
                _ITEM_ID_TO_ENUM_MAP.Insert(ctx.Id, item);
            }

            System.Diagnostics.Debug.Assert(_ITEM_ENUM_TO_CTX_MAP.Count == _ITEM_ID_TO_ENUM_MAP.Count);
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

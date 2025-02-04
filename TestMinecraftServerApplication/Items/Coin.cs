

using MinecraftServerEngine.Items;

namespace TestMinecraftServerApplication.Items
{
    public static class Coin
    {
        public const ItemType Type = ItemType.GoldNugget;
        public const string Name = "COIN";

        public readonly static IReadOnlyItem Item = new Item(
            Type,
            Name,
            [
                "게임의 기본 재화입니다.",
            ]);

        public static readonly int DefaultCount = MinecraftServerEngine.Items.Item.MinCount;
        public static readonly int MaxCount = Type.GetMaxCount();

    }

}

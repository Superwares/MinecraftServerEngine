using MinecraftServerEngine;

namespace TestMinecraftServerApplication.Items
{
    public static class ShopItem
    {
        public const ItemType Type = ItemType.MusicDisc_C418_13;
        public const string Name = "SHOP";

        public const int MaxPurchaseCount = int.MaxValue;
        public const int PurchasePrice = 0;
        public const int SellPrice = 0;

        public readonly static IReadOnlyItem Item = new Item(
            Type,
            Name,
            [
                $"상점을 이용할 수 있습니다.",
            ]);

        public static readonly int DefaultCount = MinecraftServerEngine.Item.MinCount;


    }
}

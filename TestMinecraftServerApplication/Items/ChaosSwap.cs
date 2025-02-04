
using MinecraftServerEngine;

namespace TestMinecraftServerApplication.Items
{
    public static class ChaosSwap
    {
        public const ItemQualityTier Tier = ItemQualityTier.Basic;
        public const ItemType Type = ItemType.RedstoneRepeater;

        // 혼란 스위치
        public const string Name = "Chaos Swap";

        public const int PurchasePrice = 17;
        public const int SellPrice = 3;

        public readonly static IReadOnlyItem Item = new Item(
            Type,
            Name,
            [
                $"Tier            {Tier.ToString()}",  // Quality Tier
            ]);

        public static readonly int DefaultCount = MinecraftServerEngine.Item.MinCount;
    }
}


using MinecraftServerEngine;

namespace TestMinecraftServerApplication.Items
{
    public static class Doombringer
    {
        public const ItemQualityTier Tier = ItemQualityTier.Unique;
        public const ItemType Type = ItemType.DiamondAxe;
        public const string Name = "Doombringer";

        public const int MaxDurability = 1;

        public const int Damage = 99999999;

        public const int PurchasePrice = 330;
        public const int SellPrice = 210;

        public readonly static IReadOnlyItem Item = new Item(
            Type,
            Name,
            MaxDurability,
            [
                $"Tier            {Tier.ToString()}",  // Quality Tier
                $"Damage          {Damage.ToString()}",
            ]);

        public static readonly int DefaultCount = MinecraftServerEngine.Item.MinCount;

        public static bool CanPurchase = true;
    }
}

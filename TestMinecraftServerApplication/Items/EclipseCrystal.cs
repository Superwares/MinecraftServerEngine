

using MinecraftServerEngine;

namespace TestMinecraftServerApplication.Items
{
    public static class EclipseCrystal
    {
        public const ItemQualityTier Tier = ItemQualityTier.Unique;
        public const ItemType Type = ItemType.EndCrystal;
        public const string Name = "Eclipse Crystal";

        public const double Damage = 2.0;

        //public const int PurchasePrice = 200;
        public const int PurchasePrice = 2;
        public const int SellPrice = 170;

        public readonly static IReadOnlyItem Item = new Item(
            Type,
            Name,
            [
                $"Tier            {Tier.ToString()}",  // Quality Tier
            ]);

        public static readonly int DefaultCount = MinecraftServerEngine.Item.MinCount;

        public static bool CanPurchase = true;

    }
}

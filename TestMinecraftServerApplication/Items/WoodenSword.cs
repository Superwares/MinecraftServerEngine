using MinecraftServerEngine.Items;

namespace TestMinecraftServerApplication.Items
{
    public static class WoodenSword
    {
        public const ItemQualityTier Tier = ItemQualityTier.Basic;
        public const ItemType Type = ItemType.WoodenSword;
        public const string Name = "Wooden Sword";

        public const int MaxDurability = 200;
        //public const int MaxDurability = 10;  // for debug

        public const double Damage = 2.0;

        public const int PurchasePrice = 5;
        public const int SellPrice = 1;

        public readonly static IReadOnlyItem Item = new Item(
            Type,
            Name,
        MaxDurability,
        [
                $"Tier            {Tier.ToString()}",  // Quality Tier
                $"Damage          {Damage:F2}",
            ]);

        public static readonly int DefaultCount = MinecraftServerEngine.Items.Item.MinCount;

    }
}

using MinecraftServerEngine.Items;

namespace TestMinecraftServerApplication.Items
{
    public static class Hint
    {
        public const ItemQualityTier Tier = ItemQualityTier.Basic;
        public const ItemType Type = ItemType.Paper;
        public const string Name = "Hint";

        public const int PurchasePrice = 110;
        public const int SellPrice = 59;

        public readonly static IReadOnlyItem Item = new Item(
            Type,
            Name,
            [
                $"Tier            {Tier.ToString()}",  // Quality Tier
            ]);

        public static readonly int DefaultCount = MinecraftServerEngine.Items.Item.MinCount;


    }
}

using MinecraftServerEngine.Items;

namespace TestMinecraftServerApplication.Items
{
    public static class Dash
    {
        public const ItemQualityTier Tier = ItemQualityTier.Basic;
        public const ItemType Type = ItemType.IronHorseArmor;
        public const string Name = "Dash";

        public const double Power = 2.0;

        public const int PurchasePrice = 10;
        public const int SellPrice = 7;

        public readonly static IReadOnlyItem Item = new Item(
            Type,
            Name,
            [
                $"Tier            {Tier.ToString()}",  // Quality Tier
                $"Power           {Power.ToString()}", 
            ]);

        public static readonly int DefaultCount = MinecraftServerEngine.Items.Item.MinCount;


    }
}

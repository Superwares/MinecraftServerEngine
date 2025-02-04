using Common;
using MinecraftServerEngine.Entities;
using MinecraftServerEngine.Items;

namespace TestMinecraftServerApplication.Items
{
    public static class StoneOfSwiftness
    {
        public const ItemQualityTier Tier = ItemQualityTier.Basic;

        public const ItemType Type = ItemType.Flint;
        public const string Name = "Stone of Swiftness";

        public const double MovementSpeed = LivingEntity.DefaultMovementSpeed + 0.2;
        public readonly static Time Duration = Time.FromSeconds(5);

        public const int PurchasePrice = 15;
        public const int SellPrice = 11;

        public readonly static IReadOnlyItem Item = new Item(
            Type,
            Name,
            [
                $"Tier            {Tier.ToString()}",  // Quality Tier
                $"Duration        {(double)Duration.Amount/(double)Time.FromSeconds(1).Amount}s",
            ]);

        public static readonly int DefaultCount = MinecraftServerEngine.Items.Item.MinCount;

    }
}

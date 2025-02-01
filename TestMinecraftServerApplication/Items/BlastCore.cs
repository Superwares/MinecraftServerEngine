
using MinecraftServerEngine;

namespace TestMinecraftServerApplication.Items
{
    public static class BlastCore
    {
        public const ItemQualityTier Tier = ItemQualityTier.Unique;
        public const ItemType Type = ItemType.RedstoneOre;
        public const string Name = "Blast Core";

        public const Particle EffectParticle = Particle.LargeExplode;
        public const double Radius = 3.0;
        public const double Damage = 4.0;

        public const int MaxDurability = 19;

        public const int PurchasePrice = 90;
        //public const int PurchasePrice = 2;
        public const int SellPrice = 40;

        public readonly static IReadOnlyItem Item = new Item(
            Type,
            Name,
            MaxDurability,
            [
                $"Tier            {Tier.ToString()}",  // Quality Tier
                $"Radius          {Radius}",
                $"Damage          {Damage}",
            ]);

        public static readonly int DefaultCount = MinecraftServerEngine.Item.MinCount;

        public static bool CanPurchase = true;
    }
}



using Common;

using MinecraftServerEngine.Items;

namespace TestMinecraftServerApplication.Items
{
    internal static class HyperBeam
    {
        internal const double Length = 20.0;
        internal const double HalfLength = Length / 2.0;
        internal const double Radius = 2.0;

        internal const double ChargingParticleInterval = 0.7;
        internal const double ParticleInterval = 0.3;

        internal const ItemQualityTier Tier = ItemQualityTier.Unique;

        internal const ItemType Type = ItemType.MusicDisc_C418_chirp;
        internal const string Name = "Hyper Beam";

        internal static readonly Time ChargingInterval = Time.FromMilliseconds(100);
        internal static readonly Time DamagingInterval = Time.FromMilliseconds(1);
        internal const int DamagingEmitCount = 5;

        internal const double Damage = 14.0;

        public const int PurchasePrice = 190;
        public const int SellPrice = 180;

        public readonly static IReadOnlyItem Item = new Item(
            Type,
            Name,
            [
                $"Tier            {Tier.ToString()}",  // Quality Tier
            ]);

        public static readonly int DefaultCount = MinecraftServerEngine.Items.Item.MinCount;

        public static bool CanPurchase = true;
    }
}

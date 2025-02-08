

using Common;

using MinecraftServerEngine.Items;

namespace TestMinecraftServerApplication.Items
{
    internal static class HyperBeam
    {
        internal const double Length = 10.0;
        internal const double HalfLength = Length / 2.0;
        internal const double Radius = 1.0;

        internal const ItemQualityTier Tier = ItemQualityTier.Unique;

        internal const ItemType Type = ItemType.MusicDisc_C418_blocks;
        internal const string Name = "Hyper Beam";

        internal static readonly Time ChargingInterval = Time.FromMilliseconds(100);

    }
}

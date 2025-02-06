
using Common;
using MinecraftServerEngine.Items;
using MinecraftServerEngine.Particles;

namespace TestMinecraftServerApplication.Items
{
    public static class PhoenixFeather
    {
        public const ItemQualityTier Tier = ItemQualityTier.Unique;
        public const ItemType Type = ItemType.Feather;
        public const string Name = "Phoenix Feather";

        public const Particle PhoenixParticle = Particle.Lava;
        public const Particle HealParticle = Particle.Heart;
        public const int HealParticleCountInOneEmit = 10;

        public readonly static Time EmitInterval = Time.FromMilliseconds(250);
        public readonly static int MaxEmits = 20;
        public readonly static Time Duration =  EmitInterval * MaxEmits;

        public const double AdditionalHeartsIncrease = 1.0;
        public const double MovementSpeedIncrease = 0.1;

        public const int PurchasePrice = 190;
        public const int SellPrice = 180;

        public readonly static IReadOnlyItem Item = new Item(
            Type,
            Name,
            [
                $"Tier            {Tier.ToString()}",  // Quality Tier
                $"+Hearts         {AdditionalHeartsIncrease * MaxEmits}",
                $"+Speed          +{MovementSpeedIncrease}/{Duration.Amount/Time.FromSeconds(1).Amount}s",
            ]);

        public static readonly int DefaultCount = MinecraftServerEngine.Items.Item.MinCount;

        public static bool CanPurchase = true;

        static PhoenixFeather()
        {

            System.Diagnostics.Debug.Assert(PurchasePrice > 0);
            System.Diagnostics.Debug.Assert(SellPrice > 0);
        }
    }
}

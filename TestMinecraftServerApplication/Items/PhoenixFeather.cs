
using Common;

using MinecraftPrimitives;
using MinecraftServerEngine;

namespace TestMinecraftServerApplication.Items
{
    public static class PhoenixFeather
    {
        public const ItemQualityTier Tier = ItemQualityTier.Unique;
        public const ItemType Type = ItemType.Feather;
        public const string Name = "Phoenix Feather";

        public const Particle EmitParticle = Particle.Flame;

        public const double AdditionalHearts = 23.0;
        public const double MovementSpeed = LivingEntity.DefaultMovementSpeed + 0.1;
        public readonly static Time MovementSpeedDuration = Time.FromSeconds(5);

        public const int PurchasePrice = 190;
        public const int SellPrice = 180;

        public readonly static IReadOnlyItem Item = new Item(
            Type,
            Name,
            [
                $"Tier            {Tier.ToString()}",  // Quality Tier
                $"+Hearts         {AdditionalHearts}",
                $"+Speed          {MovementSpeed}/{(double)MovementSpeedDuration.Amount/(double)Time.FromSeconds(1).Amount}s",
            ]);

        public static readonly int DefaultCount = MinecraftServerEngine.Item.MinCount;

        public static bool CanPurchase = true;

        static PhoenixFeather()
        {

            System.Diagnostics.Debug.Assert(PurchasePrice > 0);
            System.Diagnostics.Debug.Assert(SellPrice > 0);
        }
    }
}

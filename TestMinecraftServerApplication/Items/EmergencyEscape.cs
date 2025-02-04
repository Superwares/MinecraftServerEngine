using Common;

using MinecraftPrimitives;
using MinecraftServerEngine;
using MinecraftServerEngine.Items;

namespace TestMinecraftServerApplication.Items
{
    public static class EmergencyEscape
    {
        public const ItemQualityTier Tier = ItemQualityTier.Basic;
        public const ItemType Type = ItemType.EyeOfEnder;
        public const string Name = "Emergency Escape";

        public readonly static Time ParticleDuration = Time.FromSeconds(1);
        public const string LaunchSoundName = "entity.firework.launch";
        public const Particle LaunchParticle = Particle.Cloud;
        public const double Power = MinecraftPhysics.MaxVelocity;

        public const int PurchasePrice = 50;
        public const int SellPrice = 10;

        public readonly static IReadOnlyItem Item = new Item(
            Type,
            Name,
            [
                $"Tier            {Tier.ToString()}",  // Quality Tier
                $"Power           {Power}",
            ]);

        public static readonly int DefaultCount = MinecraftServerEngine.Items.Item.MinCount;

        static EmergencyEscape()
        {
            System.Diagnostics.Debug.Assert(Power > 0.0);

            System.Diagnostics.Debug.Assert(PurchasePrice > 0);
            System.Diagnostics.Debug.Assert(SellPrice > 0);
        }

    }
}

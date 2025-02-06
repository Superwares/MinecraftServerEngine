using Common;

using MinecraftServerEngine;
using MinecraftServerEngine.Entities;
using MinecraftServerEngine.Items;
using MinecraftServerEngine.Particles;

namespace TestMinecraftServerApplication.Items
{
    internal static class StoneOfSwiftness
    {
        internal const ItemQualityTier Tier = ItemQualityTier.Basic;

        internal const ItemType Type = ItemType.Flint;
        internal const string Name = "Stone of Swiftness";

        internal const Particle EmitParticle = Particle.Spell;

        internal readonly static Time EmitParticleInterval = MinecraftTimes.TimePerTick;
        internal readonly static Time Duration = Time.FromSeconds(5);
        internal readonly static int MaxParticleEmits = (int)System.Math.Ceiling(
                (double)Duration.Amount / (double)EmitParticleInterval.Amount
                );

        internal const double MovementSpeedIncrease = 0.2;

        internal const int PurchasePrice = 15;
        internal const int SellPrice = 11;

        internal readonly static IReadOnlyItem Item = new Item(
            Type,
            Name,
            [
                $"Tier            {Tier.ToString()}",  // Quality Tier
                $"Duration        {(double)Duration.Amount/(double)Time.FromSeconds(1).Amount}s",
            ]);

        internal static readonly int DefaultCount = MinecraftServerEngine.Items.Item.MinCount;

        static StoneOfSwiftness()
        {
         
        }

    }
}

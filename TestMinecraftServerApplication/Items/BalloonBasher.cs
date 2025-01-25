

using MinecraftServerEngine;

namespace TestMinecraftServerApplication.Items
{

    public static class BalloonBasher
    {
        public const ItemQualityTier Tier = ItemQualityTier.Basic;
        public const ItemType Type = ItemType.DiamondSword;
        public const string Name = "Balloon Basher";

        //public const int MaxDurability = 110;
        public const int MaxDurability = 10;

        public const float Damage = 3.0F;

        public const int MaxPurchaseCount = 10;
        public const int PurchasePrice = 30;
        public const int SellPrice = 5;

        public readonly static IReadOnlyItem Item = new Item(
            Type,
            Name,
            MaxDurability,
            [
                $"Tier            {Tier.ToString()}",  // Quality Tier
                $"Damage          {Damage:F2}",
            ]);

        public static readonly int DefaultCount = Item.Type.GetMinStackCount();

        public static ItemStack Create()
        {
            System.Diagnostics.Debug.Assert(Damage >= 0);
            return ItemStack.Create(Item, DefaultCount);
        }

        public static ItemStack Create(string[] additionalLore)
        {
            System.Diagnostics.Debug.Assert(Damage >= 0);
            return ItemStack.Create(Item, DefaultCount, additionalLore);
        }

    }

}

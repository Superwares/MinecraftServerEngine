﻿

using MinecraftServerEngine;

namespace TestMinecraftServerApplication.Items
{

    public static class BalloonBasher
    {
        public const ItemQualityTier Tier = ItemQualityTier.Basic;
        public const ItemType Type = ItemType.DiamondSword;
        public const string Name = "Balloon Basher";

        public const int MaxDurability = 110;

        public const double Damage = 3.0;

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

        public static ItemStack Create(int count = 1)
        {
            System.Diagnostics.Debug.Assert(Damage >= 0);
            System.Diagnostics.Debug.Assert(count >= Type.GetMinStackCount());

            return ItemStack.Create(Item, DefaultCount * count);
        }

        public static ItemStack CreateForShop(string[] descriptions)
        {
            System.Diagnostics.Debug.Assert(Damage >= 0);
            return ItemStack.Create(
                Item,
                DefaultCount,
                [
                    ..descriptions,
                ]);
        }

    }

}

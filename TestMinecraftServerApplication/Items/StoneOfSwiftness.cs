﻿using Common;

using MinecraftServerEngine;

namespace TestMinecraftServerApplication.Items
{
    public static class StoneOfSwiftness
    {
        public const ItemQualityTier Tier = ItemQualityTier.Utility;
        public const ItemType Type = ItemType.Flint;
        public const string Name = "Stone of Swiftness";

        public const double MovementSpeed = LivingEntity.DefaultMovementSpeed + 0.2;
        public readonly static Time Duration = Time.FromSeconds(5);

        public const int MaxPurchaseCount = 10;
        public const int PurchasePrice = 5;
        public const int SellPrice = 1;

        public readonly static IReadOnlyItem Item = new Item(
            Type,
            Name,
            [
                $"Tier            {Tier.ToString()}",  // Quality Tier
                $"Duration        {(double)Duration.Amount/(double)Time.FromSeconds(1).Amount}s",
            ]);

        public static readonly int DefaultCount = Item.Type.GetMinStackCount();

        public static ItemStack Create()
        {
            System.Diagnostics.Debug.Assert(Duration >= Time.Zero);
            return ItemStack.Create(Item, DefaultCount);
        }

        public static ItemStack CreateShopItemStack(string[] descriptions)
        {
            System.Diagnostics.Debug.Assert(Duration >= Time.Zero);
            return ItemStack.Create(
                Item,
                DefaultCount,
                [
                    ..descriptions,
                ]);
        }
    }
}

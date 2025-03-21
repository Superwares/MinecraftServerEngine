﻿using MinecraftServerEngine.Items;

namespace TestMinecraftServerApplication.Items
{

    public static class BalloonBasher
    {
        public const ItemQualityTier Tier = ItemQualityTier.Basic;
        public const ItemType Type = ItemType.DiamondSword;
        public const string Name = "Balloon Basher";

        public const int MaxDurability = 30;

        public const double Damage = 3.0;

        public const int PurchasePrice = 40;
        public const int SellPrice = 10;

        public readonly static IReadOnlyItem Item = new Item(
            Type,
            Name,
            MaxDurability,
            [
                $"Tier            {Tier.ToString()}",  // Quality Tier
                $"Damage          {Damage:F2}",
            ]);

        public static readonly int DefaultCount = MinecraftServerEngine.Items.Item.MinCount;

        //public static bool CanPurchase = true;


    }

}

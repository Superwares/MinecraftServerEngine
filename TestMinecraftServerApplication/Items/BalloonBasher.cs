

using MinecraftServerEngine;

namespace TestMinecraftServerApplication.Items
{

    public static class BalloonBasher
    {
        public const ItemQualityTier Tier = ItemQualityTier.Unique;
        public const ItemType Type = ItemType.DiamondSword;
        public const string Name = "Balloon Basher";

        public const int MaxDurability = 30;

        public const double Damage = 3.0;

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

        public static bool CanPurchase = true;

        public static ItemStack Create(int count = 1)
        {
            System.Diagnostics.Debug.Assert(Damage >= 0);
            System.Diagnostics.Debug.Assert(count >= Type.GetMinStackCount());

            return ItemStack.Create(Item, DefaultCount * count);
        }

        public static ItemStack CreateForShop(string username)
        {
            System.Diagnostics.Debug.Assert(Damage >= 0);

            if (username == null)
            {
                username = "없음";
            }

            return ItemStack.Create(
                Item,
                DefaultCount,
                [
                    $"",
                    // A lightweight yet powerful weapon that can send enemies flying with a single hit.
                    $"가볍지만 강력한 무기로 한 방에 적을 날려버릴 수 있습니다.",
                    $"",
                    // Left-click (Purchase)
                    $"왼클릭(구매)          {BalloonBasher.PurchasePrice} Coins",
                    // Right-click (Sell)
                    $"우클릭(판매)          {BalloonBasher.SellPrice} Coins",
                    $"구매자                {username}",
                ]);
        }

    }

}

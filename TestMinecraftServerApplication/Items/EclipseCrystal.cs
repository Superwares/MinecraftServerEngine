

using MinecraftServerEngine;

namespace TestMinecraftServerApplication.Items
{
    public static class EclipseCrystal
    {
        public const ItemQualityTier Tier = ItemQualityTier.Unique;
        public const ItemType Type = ItemType.EndCrystal;
        public const string Name = "Eclipse Crystal";

        public const int PurchasePrice = 30;
        public const int SellPrice = 5;

        public readonly static IReadOnlyItem Item = new Item(
            Type,
            Name,
            [
                $"Tier            {Tier.ToString()}",  // Quality Tier
            ]);

        public static readonly int DefaultCount = Item.Type.GetMinStackCount();

        public static bool CanPurchase = true;

        public static ItemStack Create(int count = 1)
        {
            System.Diagnostics.Debug.Assert(count >= Type.GetMinStackCount());

            return ItemStack.Create(Item, DefaultCount * count);
        }

        public static ItemStack CreateForShop(string username)
        {

            if (username == null)
            {
                username = "없음";
            }

            return ItemStack.Create(
                Item,
                DefaultCount,
                [
                    $"",
                    // It can obscure the world...
                    $"세상을 가릴 수 있습니다...",
                    $"",
                    // Left-click (Purchase)
                    $"왼클릭(구매)          {PurchasePrice} Coins",
                    // Right-click (Sell)
                    $"우클릭(판매)          {SellPrice} Coins",
                    $"구매자                {username}",
                ]);
        }
    }
}

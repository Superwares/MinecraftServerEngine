using MinecraftServerEngine;

namespace TestMinecraftServerApplication.Items
{
    public static class GamePanel
    {
        public const ItemType Type = ItemType.MusicDisc_C418_cat;
        public const string Name = "Game Panel";

        public const int MaxPurchaseCount = int.MaxValue;
        public const int PurchasePrice = 0;
        public const int SellPrice = 0;

        public readonly static IReadOnlyItem Item = new Item(
            Type,
            Name,
            [
                $"우클릭하여 게임의 자세한 정보를 확인할 수 있습니다!",
            ]);

        public static readonly int DefaultCount = Item.Type.GetMinStackCount();

        public static ItemStack Create(int count = 1)
        {
            System.Diagnostics.Debug.Assert(Name != null);
            System.Diagnostics.Debug.Assert(string.IsNullOrEmpty(Name) == false);
            System.Diagnostics.Debug.Assert(count >= Type.GetMinStackCount());

            return ItemStack.Create(Item, DefaultCount * count);
        }

        public static ItemStack CreateForShop(string[] descriptions)
        {
            System.Diagnostics.Debug.Assert(Name != null);
            System.Diagnostics.Debug.Assert(string.IsNullOrEmpty(Name) == false);
            return ItemStack.Create(
                Item,
                DefaultCount,
                [
                    ..descriptions,
                ]);
        }
    }
}

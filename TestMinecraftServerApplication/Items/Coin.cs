using MinecraftServerEngine;
using static System.Net.Mime.MediaTypeNames;


namespace TestMinecraftServerApplication.Items
{
    public static class Coin
    {
        public const ItemType Type = ItemType.GoldNugget;
        public const string Name = "COIN";

        public readonly static IReadOnlyItem Item = new Item(
            Type,
            Name,
            [
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
                DefaultCount * Type.GetMaxStackCount(),
                [
                    ..descriptions,
                ]);
        }

    }

}

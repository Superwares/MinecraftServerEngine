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

        public static ItemStack Create()
        {
            System.Diagnostics.Debug.Assert(Name != null);
            System.Diagnostics.Debug.Assert(string.IsNullOrEmpty(Name) == false);
            return new(Type,
                Name,
                DefaultCount,
                [
                ]);
        }

        public static ItemStack CreateShopItemStack(string[] descriptions)
        {
            System.Diagnostics.Debug.Assert(Name != null);
            System.Diagnostics.Debug.Assert(string.IsNullOrEmpty(Name) == false);
            return new(Type,
                Name,
                Type.GetMaxStackCount(),
                [
                    ..descriptions,
                ]);
        }

    }

}

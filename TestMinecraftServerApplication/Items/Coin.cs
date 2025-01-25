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
            Type.GetMinStackCount(),
            [
            ]);

        public static readonly int DefaultCount = Item.Type.GetMinStackCount();

        public static ItemStack Create()
        {
            System.Diagnostics.Debug.Assert(Name != null);
            System.Diagnostics.Debug.Assert(string.IsNullOrEmpty(Name) == false);
            return new(Type,
                Name,
                ItemType.DiamondSword.GetMinStackCount(),
                [
                ]);
        }

        public static ItemStack Create(string[] lore)
        {
            System.Diagnostics.Debug.Assert(Name != null);
            System.Diagnostics.Debug.Assert(string.IsNullOrEmpty(Name) == false);
            return new(Type,
                Name,
                ItemType.DiamondSword.GetMinStackCount(),
                [
                    ..lore,
                ]);
        }

    }

}

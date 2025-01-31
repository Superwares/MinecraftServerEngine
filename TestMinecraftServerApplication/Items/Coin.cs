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
                "게임의 기본 재화입니다.",
            ]);

        public static readonly int DefaultCount = Item.Type.GetMinStackCount();

        public static ItemStack Create()
        {
            System.Diagnostics.Debug.Assert(Name != null);
            System.Diagnostics.Debug.Assert(string.IsNullOrEmpty(Name) == false);
            System.Diagnostics.Debug.Assert(DefaultCount >= Type.GetMinStackCount());

            return ItemStack.Create(Item, DefaultCount);
        }

    }

}

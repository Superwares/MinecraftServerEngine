

using Common;

using MinecraftServerEngine;

namespace TestMinecraftServerApplication
{
    public sealed class ShopInventory : ItemInterfaceInventory
    {
        public override string Title => "Shop";

        public ShopInventory() : base(3)
        {

        }

        protected override void OnLeftClickSharedItem(
            UserId userId, AbstractPlayer player, PlayerInventory playerInventory, 
            int i, ItemType item, int count)
        {
            System.Diagnostics.Debug.Assert(count >= 0);

            MyConsole.Debug($"UserId: {userId}");
            MyConsole.Debug($"i: {i}, Item: {item}, count: {count}");
        }

        protected override void OnRightClickSharedItem(
            UserId userId, AbstractPlayer player, PlayerInventory playerInventory,
            int i, ItemType item, int count)
        {
            System.Diagnostics.Debug.Assert(count >= 0);

            MyConsole.Debug($"UserId: {userId}");
            MyConsole.Debug($"i: {i}, Item: {item}, count: {count}");

            playerInventory.GiveItem(new ItemStack(ItemType.DiamondSword));
        }
    }
}

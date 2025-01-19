

using Common;

using MinecraftServerEngine;

namespace TestMinecraftServerApplication
{
    public sealed class ShopInventory : ItemInterfaceInventory
    {
        protected override string Title => "Shop";

        public ShopInventory() : base(3)
        {

        }

        protected override void OnLeftClickSharedItem(
            UserId userId, PlayerInventory playerInventory, 
            int i, ItemType item, int count)
        {
            System.Diagnostics.Debug.Assert(count >= 0);

            MyConsole.Debug($"UserId: {userId}");
            MyConsole.Debug($"i: {i}, Item: {item}, count: {count}");
        }

        protected override void OnRightClickSharedItem(
            UserId userId, PlayerInventory playerInventory,
            int i, ItemType item, int count)
        {
            System.Diagnostics.Debug.Assert(count >= 0);

            MyConsole.Debug($"UserId: {userId}");
            MyConsole.Debug($"i: {i}, Item: {item}, count: {count}");
        }
    }
}

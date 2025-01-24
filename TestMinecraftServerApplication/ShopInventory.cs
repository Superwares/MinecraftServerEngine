

using Common;

using MinecraftPrimitives;

using MinecraftServerEngine;

namespace TestMinecraftServerApplication
{
    public sealed class ShopInventory : ItemInterfaceInventory
    {
        public override string Title => "Shop";

        public ShopInventory() : base(4)
        {
            ResetSlot(35, new ItemStack(ItemType.DiamondSword,
                "풍선망치",  // Balloon Basher
                [
                    "가볍지만 강력한 한 방으로 적을 날려버리세요!",
                    "",
                    "Price                         30 COINS",
                ]));
        }

        protected override void OnLeftClickSharedItem(
            UserId userId, AbstractPlayer player, PlayerInventory playerInventory,
            int i, ItemStack itemStack)
        {

            //MyConsole.Debug($"UserId: {userId}");
            //MyConsole.Debug($"i: {i}, ItemStack: {itemStack}");
        }

        protected override void OnRightClickSharedItem(
            UserId userId, AbstractPlayer player, PlayerInventory playerInventory,
            int i, ItemStack itemStack)
        {
            //MyConsole.Debug($"UserId: {userId}");
            //MyConsole.Debug($"i: {i}, ItemStack: {itemStack}");

            //playerInventory.GiveItem(new ItemStack(ItemType.DiamondSword, "Bad Stick!"));
            //playerInventory.GiveItem(new ItemStack(ItemType.DiamondSword, "Bad Stick!"));
            //playerInventory.GiveItem(new ItemStack(ItemType.DiamondSword, "Bad Stick!"));

            //ResetSlot(35, new ItemStack(ItemType.Stick, "Good hElllo", itemStack.Count - 1));

            //playerInventory.GiveItem(new ItemStack(ItemType.Stick, "Good hElllo", 1));


            //ItemStack[] itemStacks0 = playerInventory.TakeItemsInPrimary(ItemType.DiamondSword, "Bad Stick!", 2);
            //ItemStack[] itemStacks1 = playerInventory.TakeItemsInPrimary(ItemType.Stick, "Stick!", 2);
        }
    }
}

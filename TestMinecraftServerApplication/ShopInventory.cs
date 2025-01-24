

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
            ResetSlot(0, new ItemStack(ItemType.DiamondSword,
                "풍선망치",  // Balloon Basher
                ItemType.DiamondSword.GetMinCount(),
                [
                    "가볍지만 강력한 한 방으로 적을 날려버리세요!",
                    "",
                    "Price                         30 Coins",
                ]));

            ResetSlot(35, new ItemStack(ItemType.GoldNugget,
               "무료코인",  // Balloon Basher
               ItemType.DiamondSword.GetMaxCount(),
               [
                    "테스트용 무료로 코인입니다.",
                    "",
                    "왼클릭               지급",
                    "우클릭               차감",
               ]));
        }

        protected override void OnLeftClickSharedItem(
            UserId userId, AbstractPlayer player, PlayerInventory playerInventory,
            int i, ItemStack itemStack)
        {

            //MyConsole.Debug($"UserId: {userId}");
            //MyConsole.Debug($"i: {i}, ItemStack: {itemStack}");


            switch (i)
            {
                default:
                    System.Diagnostics.Debug.Assert(false);
                    break;
                case 35:
                    playerInventory.GiveItem(new ItemStack(ItemType.GoldNugget, "COIN"));
                    break;

            }
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


            switch (i)
            {
                default:
                    System.Diagnostics.Debug.Assert(false);
                    break;
                case 35:
                    playerInventory.TakeItemsInPrimary(
                        ItemType.GoldNugget, "COIN", ItemType.GoldNugget.GetMaxCount());
                    break;

            }
        }
    }
}

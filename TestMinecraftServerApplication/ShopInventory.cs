

using Common;

using MinecraftPrimitives;

using MinecraftServerEngine;

namespace TestMinecraftServerApplication
{
    public sealed class ShopInventory : ItemInterfaceInventory
    {
        public override string Title => "Shop";

        public ItemType Coin = ItemType.GoldNugget;
        public string CoinName = "COIN";

        public ShopInventory() : base(4)
        {
            ResetSlot(0, new ItemStack(ItemType.DiamondSword,
                "Balloon Basher",
                ItemType.DiamondSword.GetMinStackCount(),
                [
                    "가볍지만 강력한 한 방으로 적을 날려버리세요!",
                    "",
                    "데미지                             3.00",  // Damage
                    "내구도                              110",  // Durability
                    "",
                    "구매 상태                          0/10",  // Purchase Status
                    "",
                    "왼클릭(구매)                     30 코인",
                    "우클릭(판매)                      5 코인",
                ]));

            ResetSlot(35, new ItemStack(Coin,
               CoinName,
               Coin.GetMaxStackCount(),
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

            player.PlaySound("entity.item.pickup", 7, 1.0F, 2.0F);


            switch (i)
            {
                case 0:
                    {
                        int amount = 30;

                        System.Diagnostics.Debug.Assert(amount >= Coin.GetMinStackCount());
                        System.Diagnostics.Debug.Assert(amount <= Coin.GetMaxStackCount());

                        ItemStack[] coins = playerInventory.TakeItemsInPrimary(
                            Coin, CoinName, amount);

                        System.Diagnostics.Debug.Assert(coins != null);
                        if (coins.Length > 0)
                        {
                            System.Diagnostics.Debug.Assert(coins.Length == 1);
                            System.Diagnostics.Debug.Assert(coins[0].Count == amount);

                            playerInventory.GiveItem(new ItemStack(
                                itemStack.Type, itemStack.Name, itemStack.Count,
                                100, 9,
                                [
                                    "가볍지만 강력한 한 방으로 적을 날려버리세요!",
                                ]));

                        }

                    }
                    break;
                case 35:
                    playerInventory.GiveItem(new ItemStack(itemStack.Type, itemStack.Name, itemStack.Count));
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

            player.PlaySound("entity.item.pickup", 7, 1.0F, 2.0F);

            switch (i)
            {
                case 0:
                    {
                        int amount = 5;

                        System.Diagnostics.Debug.Assert(amount >= Coin.GetMinStackCount());
                        System.Diagnostics.Debug.Assert(amount <= Coin.GetMaxStackCount());

                        ItemStack[] taked = playerInventory.TakeItemsInPrimary(
                            itemStack.Type, itemStack.Name, itemStack.Count);

                        System.Diagnostics.Debug.Assert(taked != null);
                        if (taked.Length > 0)
                        {
                            System.Diagnostics.Debug.Assert(taked.Length == 1);
                            System.Diagnostics.Debug.Assert(taked[0].Count == itemStack.Count);

                            playerInventory.GiveItem(new ItemStack(Coin, CoinName, amount));
                        }

                    }
                    break;
                case 35:
                    playerInventory.TakeItemsInPrimary(
                        itemStack.Type, itemStack.Name, itemStack.Count);
                    break;

            }
        }
    }
}

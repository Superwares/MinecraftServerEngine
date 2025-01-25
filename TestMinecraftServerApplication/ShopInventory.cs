

using Common;

using MinecraftPrimitives;

using MinecraftServerEngine;
using TestMinecraftServerApplication.Items;

namespace TestMinecraftServerApplication
{
    public sealed class ShopInventory : ItemInterfaceInventory
    {
        public override string Title => "Shop";

        public ShopInventory() : base(4)
        {
            ResetSlot(0, BalloonBasher.Create([
                $"",
                $"가볍지만 강력한 한 방으로 적을 날려버리세요!",
                $"",
                $"왼클릭(구매)          30 코인",
                $"우클릭(판매)          5 코인",
                ]));

            ResetSlot(35, Coin.Create([
                $"",
                $"테스트용 무료 코인입니다.",
                $"",
                $"왼클릭          지급",
                $"우클릭          차감",
                ]));

        }

        protected override void OnLeftClickSharedItem(
            UserId userId, AbstractPlayer player, PlayerInventory playerInventory,
            int i, ItemStack itemStack)
        {

            //MyConsole.Debug($"UserId: {userId}");
            //MyConsole.Debug($"i: {i}, ItemStack: {itemStack}");

            bool success = false;

            ItemStack giveItem;

            switch (i)
            {
                case 0:
                    {
                        const int coinAmount = 30;

                        ItemStack[] coins = playerInventory.TakeItemStacksInPrimary(
                            Coin.Item, Coin.DefaultCount * coinAmount);

                        System.Diagnostics.Debug.Assert(coins != null);
                        if (coins.Length > 0)
                        {
                            System.Diagnostics.Debug.Assert(coins.Length == 1);
                            System.Diagnostics.Debug.Assert(coins[0].Count == coinAmount);

                            giveItem = ItemStack.Create(BalloonBasher.Item, BalloonBasher.DefaultCount);
                            playerInventory.GiveItem(giveItem);

                            success = true;
                        }

                    }
                    break;
                case 35:
                    {
                        const int coinAmount = 64;

                        giveItem = ItemStack.Create(Coin.Item, Coin.DefaultCount * coinAmount);
                        playerInventory.GiveItem(giveItem);
                    }
                    break;

            }

            if (success == true)
            {
                player.PlaySound("entity.item.pickup", 7, 1.0F, 2.0F);
            }
        }

        protected override void OnRightClickSharedItem(
            UserId userId, AbstractPlayer player, PlayerInventory playerInventory,
            int i, ItemStack itemStack)
        {
            bool success = false;

            switch (i)
            {
                case 0:
                    {
                        const int coinAmount = 5;

                        System.Diagnostics.Debug.Assert(coinAmount >= Coin.Type.GetMinStackCount());
                        System.Diagnostics.Debug.Assert(coinAmount <= Coin.Type.GetMaxStackCount());

                        ItemStack[] taked = playerInventory.TakeItemStacksInPrimary(
                            BalloonBasher.Item, BalloonBasher.DefaultCount);

                        System.Diagnostics.Debug.Assert(taked != null);
                        if (taked.Length > 0)
                        {
                            System.Diagnostics.Debug.Assert(taked.Length == 1);
                            System.Diagnostics.Debug.Assert(taked[0].Count == itemStack.Count);

                            ItemStack giveItem = ItemStack.Create(Coin.Item, Coin.DefaultCount * coinAmount);
                            playerInventory.GiveItem(giveItem);

                            success = true;
                        }

                    }
                    break;
                case 35:
                    {
                        const int coinAmount = 64;

                        playerInventory.TakeItemStacksInPrimary(Coin.Item, Coin.DefaultCount * coinAmount);
                    }
                    break;

            }

            if (success == true)
            {
                player.PlaySound("entity.item.pickup", 7, 1.0F, 2.0F);
            }
        }
    }
}

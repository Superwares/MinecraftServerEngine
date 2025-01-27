

using Common;

using MinecraftPrimitives;
using MinecraftServerEngine;

namespace TestMinecraftServerApplication
{
    using Items;

    public sealed class ShopInventory : ItemInterfaceInventory
    {
        public override string Title => "Shop";

        public ShopInventory() : base(MaxLineCount)
        {
            SetSlot(0, Coin.Create([
                $"",
                $"테스트용 무료 코인입니다.",
                $"",
                $"왼클릭          지급",
                $"우클릭          차감",
                ]));

            SetSlot(9 + 1, BalloonBasher.Create([
                $"",
                $"가볍지만 강력한 한 방으로 적을 날려버리세요!",
                $"",
                $"왼클릭(구매)          {BalloonBasher.PurchasePrice} 코인",
                $"우클릭(판매)          {BalloonBasher.SellPrice} 코인",
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
            ItemStack[] taked;

            switch (i)
            {
                case 0:
                    {
                        int coinAmount = Coin.Type.GetMaxStackCount();

                        giveItem = ItemStack.Create(Coin.Item, Coin.DefaultCount * coinAmount);
                        success = playerInventory.GiveItem(giveItem);

                    }
                    break;
                case 9 + 1:
                    {
                        const int coinAmount = BalloonBasher.PurchasePrice;

                        taked = playerInventory.TakeItemStacksInPrimary(
                           Coin.Item, Coin.DefaultCount * coinAmount);

                        System.Diagnostics.Debug.Assert(taked != null);
                        if (taked.Length > 0)
                        {
                            System.Diagnostics.Debug.Assert(taked.Length == 1);
                            System.Diagnostics.Debug.Assert(taked[0].Count == coinAmount);

                            giveItem = ItemStack.Create(BalloonBasher.Item, BalloonBasher.DefaultCount);
                            success = playerInventory.GiveItem(giveItem);

                            if (success == false)
                            {
                                giveItem = ItemStack.Create(Coin.Item, Coin.DefaultCount * coinAmount);
                                success = playerInventory.GiveItem(giveItem);

                                System.Diagnostics.Debug.Assert(success == true);

                                success = false;
                            }
                        }

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

            ItemStack giveItem;
            ItemStack[] taked;

            switch (i)
            {
                case 0:
                    {
                        int coinAmount = Coin.Type.GetMaxStackCount();

                        taked = playerInventory.TakeItemStacksInPrimary(Coin.Item, Coin.DefaultCount * coinAmount);

                        if (taked != null && taked.Length > 0)
                        {
                            success = true;
                        }
                    }
                    break;
                case 9 + 1:
                    {
                        const int coinAmount = BalloonBasher.SellPrice;

                        System.Diagnostics.Debug.Assert(coinAmount >= Coin.Type.GetMinStackCount());
                        System.Diagnostics.Debug.Assert(coinAmount <= Coin.Type.GetMaxStackCount());

                        taked = playerInventory.TakeItemStacksInPrimary(
                            BalloonBasher.Item, BalloonBasher.DefaultCount);

                        if (taked != null && taked.Length > 0)
                        {
                            System.Diagnostics.Debug.Assert(taked.Length == 1);
                            System.Diagnostics.Debug.Assert(taked[0].Count == itemStack.Count);

                            giveItem = ItemStack.Create(Coin.Item, Coin.DefaultCount * coinAmount);
                            success = playerInventory.GiveItem(giveItem);

                            if (success == false)
                            {
                                giveItem = ItemStack.Create(BalloonBasher.Item, BalloonBasher.DefaultCount);
                                success = playerInventory.GiveItem(giveItem);

                                System.Diagnostics.Debug.Assert(success == true);

                                success = false;
                            }
                        }

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

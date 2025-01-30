

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
            {
                int basicItemLineOffset = SlotCountPerLine * 0;

                SetSlot(basicItemLineOffset + 0, new ItemStack(
                    ItemType.GrayStainedGlassPane, "Basic Tier", 1));

                SetSlot(basicItemLineOffset + 1, WoodenSword.CreateShopItemStack([
                    $"",
                    $"평범한 나무검입니다!",
                    $"",
                    $"왼클릭(구매)          {WoodenSword.PurchasePrice} 코인",
                    $"우클릭(판매)          {WoodenSword.SellPrice} 코인",
                    ]));

                SetSlot(basicItemLineOffset + 2, BalloonBasher.CreateForShop([
                    $"",
                    $"가볍지만 강력한 한 방으로 적을 날려버리세요!",
                    $"",
                    $"왼클릭(구매)          {BalloonBasher.PurchasePrice} 코인",
                    $"우클릭(판매)          {BalloonBasher.SellPrice} 코인",
                    ]));
            }

            {
                int uniqueItemLineOffset = SlotCountPerLine * 1;
                SetSlot(uniqueItemLineOffset + 0, new ItemStack(
                    ItemType.PurpleStainedGlassPane, "Unique Tier", 1));
            }

            {
                int utilityItemLineOffset = SlotCountPerLine * 3;
                SetSlot(utilityItemLineOffset + 0, new ItemStack(
                    ItemType.WhiteStainedGlassPane, "Utility Tier", 1));
            }

            {
                int lastLineOffset = SlotCountPerLine * (MaxLineCount - 1);


                SetSlot(lastLineOffset + 0, Coin.CreateForShop([
                    $"",
                    $"게임 시작 전의 무료 코인입니다.",
                    $"",
                    $"왼클릭          지급",
                    $"우클릭          차감",
                    ]));

                SetSlot(lastLineOffset + 8, ShopItem.CreateForShop([
                    $"",
                    $"왼클릭          지급",
                    $"우클릭          차감",
                    ]));

                SetSlot(lastLineOffset + 7, GlobalChestItem.CreateForShop([
                    $"",
                    $"왼클릭          지급",
                    $"우클릭          차감",
                    ]));
            }
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

            switch (itemStack.Type)
            {
                case Coin.Type:
                    {
                        if (SuperWorld.GameContext.IsStarted == false)
                        {
                            giveItem = ItemStack.Create(Coin.Item, Coin.DefaultCount * itemStack.Count);
                            success = playerInventory.GiveItem(giveItem);
                        }

                    }
                    break;
                case ShopItem.Type:
                    {
                        giveItem = ItemStack.Create(ShopItem.Item, ShopItem.DefaultCount * itemStack.Count);
                        success = playerInventory.GiveItem(giveItem);
                    }
                    break;
                case GlobalChestItem.Type:
                    {
                        giveItem = ItemStack.Create(GlobalChestItem.Item, GlobalChestItem.DefaultCount * itemStack.Count);
                        success = playerInventory.GiveItem(giveItem);
                    }
                    break;
                case WoodenSword.Type:
                    {
                        const int coinAmount = WoodenSword.PurchasePrice;

                        taked = playerInventory.TakeItemStacksInPrimary(
                           Coin.Item, Coin.DefaultCount * coinAmount);

                        System.Diagnostics.Debug.Assert(taked != null);
                        if (taked.Length > 0)
                        {
                            System.Diagnostics.Debug.Assert(taked.Length == 1);
                            System.Diagnostics.Debug.Assert(taked[0].Count == coinAmount);

                            giveItem = ItemStack.Create(
                                WoodenSword.Item,
                                WoodenSword.DefaultCount * itemStack.Count);
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
                case BalloonBasher.Type:
                    {
                        const int coinAmount = BalloonBasher.PurchasePrice;

                        taked = playerInventory.TakeItemStacksInPrimary(
                           Coin.Item, Coin.DefaultCount * coinAmount);

                        System.Diagnostics.Debug.Assert(taked != null);
                        if (taked.Length > 0)
                        {
                            System.Diagnostics.Debug.Assert(taked.Length == 1);
                            System.Diagnostics.Debug.Assert(taked[0].Count == coinAmount);

                            giveItem = ItemStack.Create(
                                BalloonBasher.Item,
                                BalloonBasher.DefaultCount * itemStack.Count);
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

            switch (itemStack.Type)
            {
                case Coin.Type:
                    {
                        if (SuperWorld.GameContext.IsStarted == false)
                        {
                            taked = playerInventory.TakeItemStacksInPrimary(
                                Coin.Item, Coin.DefaultCount * itemStack.Count);

                            if (taked != null && taked.Length > 0)
                            {
                                success = true;
                            }
                        }

                    }
                    break;
                case ShopItem.Type:
                    {
                        taked = playerInventory.TakeItemStacksInPrimary(
                            ShopItem.Item, ShopItem.DefaultCount * itemStack.Count);

                        if (taked != null && taked.Length > 0)
                        {
                            success = true;
                        }
                    }
                    break;
                case GlobalChestItem.Type:
                    {
                        taked = playerInventory.TakeItemStacksInPrimary(
                            GlobalChestItem.Item, GlobalChestItem.DefaultCount * itemStack.Count);

                        if (taked != null && taked.Length > 0)
                        {
                            success = true;
                        }
                    }
                    break;
                case WoodenSword.Type:
                    {
                        const int coinAmount = WoodenSword.SellPrice;

                        System.Diagnostics.Debug.Assert(coinAmount >= Coin.Type.GetMinStackCount());
                        System.Diagnostics.Debug.Assert(coinAmount <= Coin.Type.GetMaxStackCount());

                        taked = playerInventory.TakeItemStacksInPrimary(
                            WoodenSword.Item, WoodenSword.DefaultCount);

                        if (taked != null && taked.Length > 0)
                        {
                            System.Diagnostics.Debug.Assert(taked.Length == 1);
                            System.Diagnostics.Debug.Assert(taked[0].Count == itemStack.Count);

                            giveItem = ItemStack.Create(Coin.Item, Coin.DefaultCount * coinAmount);
                            success = playerInventory.GiveItem(giveItem);

                            if (success == false)
                            {
                                giveItem = ItemStack.Create(
                                    WoodenSword.Item,
                                    WoodenSword.DefaultCount * itemStack.Count);
                                success = playerInventory.GiveItem(giveItem);

                                System.Diagnostics.Debug.Assert(success == true);

                                success = false;
                            }
                        }

                    }
                    break;
                case BalloonBasher.Type:
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
                                giveItem = ItemStack.Create(
                                    BalloonBasher.Item,
                                    BalloonBasher.DefaultCount * itemStack.Count);
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

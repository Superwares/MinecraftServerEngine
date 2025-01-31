

using Common;

using MinecraftPrimitives;
using MinecraftServerEngine;

namespace TestMinecraftServerApplication
{
    using Items;

    public sealed class ShopInventory : ItemInterfaceInventory
    {
        public override string Title => "Shop";


        public const int UniqueItemLineOffset = (SlotCountPerLine * 1) + 0;
        public const int BalloonBasherSlot = UniqueItemLineOffset + 1;


        public void ResetBalloonBasherSlot(string username)
        {
            if (username == null || string.IsNullOrEmpty(username) == true)
            {
                username = "없음";
            }

            SetSlot(BalloonBasherSlot, ItemStack.Create(BalloonBasher.Item, BalloonBasher.DefaultCount, [
                    // A lightweight yet powerful weapon that can send enemies flying with a single hit.
                    $"",
                    $"가볍지만 강력한 무기로 ",
                    $"한 방에 적을 날려버릴 수 있습니다.",
                    $"",
                    // Left-click (Purchase)
                    $"왼클릭(구매)          {BalloonBasher.PurchasePrice} Coins",
                    // Right-click (Sell)
                    $"우클릭(판매)          {BalloonBasher.SellPrice} Coins",
                    $"구매자                {username}",
                ]));
        }

        public ShopInventory() : base(MaxLineCount)
        {
            (bool, ItemStack)[] slots = new (bool, ItemStack)[GetTotalSlotCount()];

            // Basic items
            {
                int offset = SlotCountPerLine * 0;

                slots[offset + 0] = (
                    true,
                    new ItemStack(ItemType.GrayStainedGlassPane, "Basic Tier", 1)
                    );

                slots[offset + 1] = (
                    true,
                    ItemStack.Create(WoodenSword.Item, WoodenSword.DefaultCount, [
                        $"",
                        // A basic wooden sword, ideal for beginners.
                        $"초보자에게 이상적인 ",
                        $"기본 나무검입니다.",
                        $"",
                        // Left-click (Purchase)
                        $"왼클릭(구매)          {WoodenSword.PurchasePrice} Coins",
                        // Right-click (Sell)
                        $"우클릭(판매)          {WoodenSword.SellPrice} Coins",
                        ])
                    );


            }

            // Unique items
            {
                slots[UniqueItemLineOffset] = (
                    true,
                    new ItemStack(ItemType.PurpleStainedGlassPane, "Unique Tier", 1)
                    );

                ResetBalloonBasherSlot(null);
            }

            // Utility items
            {
                int offset = SlotCountPerLine * 3;
                slots[offset + 0] = (
                    true,
                    new ItemStack(ItemType.WhiteStainedGlassPane, "Utility Tier", 1)
                    );

                slots[offset + 1] = (
                    true,
                    ItemStack.Create(StoneOfSwiftness.Item, StoneOfSwiftness.DefaultCount, [
                        $"",
                        // A mystical stone that grants the bearer enhanced speed and agility.
                        $"사용자에게 향상된 속도와 민첩성을 ",
                        $"부여하는 신비한 돌입니다.",
                        $"",
                        // Left-click (Purchase)
                        $"왼클릭(구매)          {StoneOfSwiftness.PurchasePrice} Coins",
                        // Right-click (Sell)
                        $"우클릭(판매)          {StoneOfSwiftness.SellPrice} Coins",
                        ])
                    );
            }

            // Last line
            {
                int offset = SlotCountPerLine * (MaxLineCount - 1);

                slots[offset + 0] = (
                    true,
                    ItemStack.Create(Coin.Item, Coin.DefaultCount, [
                        $"",
                        $"게임 전에만 지급받을 수 있습니다!",
                        ])
                    );

                slots[offset + 8] = (
                    true,
                    ItemStack.Create(ShopItem.Item, ShopItem.DefaultCount)
                    );
                slots[offset + 7] = (
                    true,
                    ItemStack.Create(GlobalChestItem.Item, GlobalChestItem.DefaultCount)
                    );
            }

            SetSlots(slots);
        }

        protected override void OnLeftClickSharedItem(
            UserId userId, AbstractPlayer player, PlayerInventory playerInventory,
            int i, ItemStack itemStack)
        {

            //MyConsole.Debug($"UserId: {userId}");
            //MyConsole.Debug($"i: {i}, ItemStack: {itemStack}");

            bool success = false;
            ItemStack[] taked;

            switch (itemStack.Type)
            {
                case Coin.Type:
                    {
                        if (SuperWorld.GameContext.IsStarted == false)
                        {
                            success = playerInventory.GiveItemStacks(Coin.Item, itemStack.Count);
                        }

                    }
                    break;
                case ShopItem.Type:
                    {
                        success = playerInventory.GiveItemStacks(ShopItem.Item, itemStack.Count);
                    }
                    break;
                case GlobalChestItem.Type:
                    {
                        success = playerInventory.GiveItemStacks(ShopItem.Item, itemStack.Count);
                    }
                    break;
                case WoodenSword.Type:
                    {
                        taked = playerInventory.GiveAndTakeItemStacks(
                            WoodenSword.Item, WoodenSword.DefaultCount,
                            Coin.Item, Coin.DefaultCount * WoodenSword.PurchasePrice);

                        success = taked != null;
                    }
                    break;
                case BalloonBasher.Type:
                    {
                        if (BalloonBasher.CanPurchase == false)
                        {
                            break;
                        }

                        taked = playerInventory.GiveAndTakeItemStacks(
                           BalloonBasher.Item, BalloonBasher.DefaultCount,
                           Coin.Item, Coin.DefaultCount * BalloonBasher.PurchasePrice);

                        if (taked != null)
                        {
                            System.Diagnostics.Debug.Assert(player.Username != null);
                            System.Diagnostics.Debug.Assert(string.IsNullOrEmpty(player.Username) == false);
                            System.Diagnostics.Debug.Assert(i == BalloonBasherSlot);
                            ResetBalloonBasherSlot(player.Username);

                            BalloonBasher.CanPurchase = false;
                            success = true;
                        }
                        else
                        {
                            success = false;
                        }

                    }
                    break;
                case StoneOfSwiftness.Type:
                    {
                        taked = playerInventory.GiveAndTakeItemStacks(
                            StoneOfSwiftness.Item, StoneOfSwiftness.DefaultCount,
                            Coin.Item, Coin.DefaultCount * StoneOfSwiftness.PurchasePrice);

                        success = taked != null;

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

            ItemStack[] taked;

            switch (itemStack.Type)
            {
                case Coin.Type:
                    {
                        if (SuperWorld.GameContext.IsStarted == false)
                        {
                            taked = playerInventory.TakeItemStacksInPrimary(
                                itemStack, itemStack.Count);

                            if (taked != null)
                            {
                                System.Diagnostics.Debug.Assert(taked.Length > 0);
                                success = true;
                            }
                        }

                    }
                    break;
                case ShopItem.Type:
                    {
                        taked = playerInventory.TakeItemStacksInPrimary(
                            itemStack, itemStack.Count);

                        if (taked != null)
                        {
                            System.Diagnostics.Debug.Assert(taked.Length > 0);
                            success = true;
                        }
                    }
                    break;
                case GlobalChestItem.Type:
                    {
                        taked = playerInventory.TakeItemStacksInPrimary(
                            itemStack, itemStack.Count);

                        if (taked != null)
                        {
                            System.Diagnostics.Debug.Assert(taked.Length > 0);
                            success = true;
                        }
                    }
                    break;
                case WoodenSword.Type:
                    {
                        //const int coinAmount = WoodenSword.SellPrice;

                        //System.Diagnostics.Debug.Assert(coinAmount >= Coin.Type.GetMinStackCount());
                        //System.Diagnostics.Debug.Assert(coinAmount <= Coin.Type.GetMaxStackCount());

                        //taked = playerInventory.TakeItemStacksInPrimary(
                        //    WoodenSword.Item, WoodenSword.DefaultCount);

                        //if (taked != null && taked.Length > 0)
                        //{
                        //    System.Diagnostics.Debug.Assert(taked.Length == 1);
                        //    System.Diagnostics.Debug.Assert(taked[0].Count == itemStack.Count);

                        //    giveItem = ItemStack.Create(Coin.Item, Coin.DefaultCount * coinAmount);
                        //    success = playerInventory.GiveItemStack(giveItem);

                        //    if (success == false)
                        //    {
                        //        giveItem = ItemStack.Create(
                        //            WoodenSword.Item,
                        //            WoodenSword.DefaultCount * itemStack.Count);
                        //        success = playerInventory.GiveItemStack(giveItem);

                        //        System.Diagnostics.Debug.Assert(success == true);

                        //        success = false;
                        //    }
                        //}

                    }
                    break;
                case BalloonBasher.Type:
                    {
                        //if (BalloonBasher.CanPurchase == true)
                        //{
                        //    break;
                        //}

                        //const int coinAmount = BalloonBasher.SellPrice;

                        //System.Diagnostics.Debug.Assert(coinAmount >= Coin.Type.GetMinStackCount());
                        //System.Diagnostics.Debug.Assert(coinAmount <= Coin.Type.GetMaxStackCount());

                        //taked = playerInventory.TakeItemStacksInPrimary(
                        //    BalloonBasher.Item, BalloonBasher.DefaultCount);

                        //if (taked != null && taked.Length > 0)
                        //{

                        //    System.Diagnostics.Debug.Assert(Coin.DefaultCount * coinAmount <= Coin.Type.GetMaxStackCount());
                        //    giveItem = ItemStack.Create(Coin.Item, Coin.DefaultCount * coinAmount);
                        //    success = playerInventory.GiveItemStack(giveItem);

                        //    if (success == false)
                        //    {
                        //        giveItem = ItemStack.Create(
                        //            BalloonBasher.Item,
                        //            BalloonBasher.DefaultCount * itemStack.Count);
                        //        success = playerInventory.GiveItemStack(giveItem);

                        //        System.Diagnostics.Debug.Assert(success == true);

                        //        success = false;
                        //    }
                        //    else
                        //    {
                        //        System.Diagnostics.Debug.Assert(i == BalloonBasherSlot);
                        //        ResetBalloonBasherSlot(null);

                        //        BalloonBasher.CanPurchase = true;
                        //    }
                        //}

                    }
                    break;
                case StoneOfSwiftness.Type:
                    {
                        //const int coinAmount = StoneOfSwiftness.SellPrice;

                        //System.Diagnostics.Debug.Assert(coinAmount >= Coin.Type.GetMinStackCount());
                        //System.Diagnostics.Debug.Assert(coinAmount <= Coin.Type.GetMaxStackCount());

                        //taked = playerInventory.TakeItemStacksInPrimary(
                        //    StoneOfSwiftness.Item, StoneOfSwiftness.DefaultCount);

                        //if (taked != null && taked.Length > 0)
                        //{
                        //    System.Diagnostics.Debug.Assert(taked.Length == 1);
                        //    System.Diagnostics.Debug.Assert(taked[0].Count == itemStack.Count);

                        //    giveItem = ItemStack.Create(Coin.Item, Coin.DefaultCount * coinAmount);
                        //    success = playerInventory.GiveItemStack(giveItem);

                        //    if (success == false)
                        //    {
                        //        giveItem = ItemStack.Create(
                        //            StoneOfSwiftness.Item,
                        //            StoneOfSwiftness.DefaultCount * itemStack.Count);
                        //        success = playerInventory.GiveItemStack(giveItem);

                        //        System.Diagnostics.Debug.Assert(success == true);

                        //        success = false;
                        //    }
                        //}

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



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
            SetSlot(BalloonBasherSlot, BalloonBasher.CreateForShop(username));
        }

        public ShopInventory() : base(MaxLineCount)
        {
            // Basic items
            {
                int offset = SlotCountPerLine * 0;

                SetSlot(offset + 0, new ItemStack(
                    ItemType.GrayStainedGlassPane, "Basic Tier", 1));

                SetSlot(offset + 1, WoodenSword.CreateShopItemStack([
                    $"",
                    // A basic wooden sword, ideal for beginners.
                    $"초보자에게 이상적인 기본 나무검입니다.",
                    $"",
                    // Left-click (Purchase)
                    $"왼클릭(구매)          {WoodenSword.PurchasePrice} Coins",
                    // Right-click (Sell)
                    $"우클릭(판매)          {WoodenSword.SellPrice} Coins",
                    ]));


            }

            // Unique items
            {
                SetSlot(UniqueItemLineOffset, new ItemStack(
                    ItemType.PurpleStainedGlassPane, "Unique Tier", 1));

                ResetBalloonBasherSlot(null);
            }

            // Utility items
            {
                int offset = SlotCountPerLine * 3;
                SetSlot(offset + 0, new ItemStack(
                    ItemType.WhiteStainedGlassPane, "Utility Tier", 1));

                SetSlot(offset + 1, StoneOfSwiftness.CreateShopItemStack([
                    $"",
                    // A mystical stone that grants the bearer enhanced speed and agility.
                    $"사용자에게 향상된 속도와 민첩성을 부여하는 신비한 돌입니다.",
                    $"",
                    // Left-click (Purchase)
                    $"왼클릭(구매)          {StoneOfSwiftness.PurchasePrice} Coins",
                    // Right-click (Sell)
                    $"우클릭(판매)          {StoneOfSwiftness.SellPrice} Coins",
                    ]));
            }

            // Last line
            {
                int offset = SlotCountPerLine * (MaxLineCount - 1);

                SetSlot(offset + 0, Coin.CreateForShop([
                    $"",
                    $"게임 시작 전의 무료 코인입니다.",
                    $"",
                    $"왼클릭          지급",
                    $"우클릭          차감",
                    ]));

                SetSlot(offset + 8, ShopItem.CreateForShop([
                    $"",
                    $"왼클릭          지급",
                    $"우클릭          차감",
                    ]));

                SetSlot(offset + 7, GlobalChestItem.CreateForShop([
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
                        if (BalloonBasher.CanPurchase == false)
                        {
                            break;
                        }

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
                            else
                            {
                                System.Diagnostics.Debug.Assert(player.Username != null);
                                System.Diagnostics.Debug.Assert(string.IsNullOrEmpty(player.Username) == false);
                                System.Diagnostics.Debug.Assert(i == BalloonBasherSlot);
                                ResetBalloonBasherSlot(player.Username);

                                BalloonBasher.CanPurchase = false;
                            }
                        }

                    }
                    break;
                case StoneOfSwiftness.Type:
                    {
                        const int coinAmount = StoneOfSwiftness.PurchasePrice;

                        taked = playerInventory.TakeItemStacksInPrimary(
                           Coin.Item, Coin.DefaultCount * coinAmount);

                        System.Diagnostics.Debug.Assert(taked != null);
                        if (taked.Length > 0)
                        {
                            System.Diagnostics.Debug.Assert(taked.Length == 1);
                            System.Diagnostics.Debug.Assert(taked[0].Count == coinAmount);

                            giveItem = ItemStack.Create(
                                StoneOfSwiftness.Item,
                                StoneOfSwiftness.DefaultCount * itemStack.Count);
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
                        if (BalloonBasher.CanPurchase == true)
                        {
                            break;
                        }

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
                            else
                            {
                                System.Diagnostics.Debug.Assert(i == BalloonBasherSlot);
                                ResetBalloonBasherSlot(null);

                                BalloonBasher.CanPurchase = true;
                            }
                        }

                    }
                    break;
                case StoneOfSwiftness.Type:
                    {
                        const int coinAmount = StoneOfSwiftness.SellPrice;

                        System.Diagnostics.Debug.Assert(coinAmount >= Coin.Type.GetMinStackCount());
                        System.Diagnostics.Debug.Assert(coinAmount <= Coin.Type.GetMaxStackCount());

                        taked = playerInventory.TakeItemStacksInPrimary(
                            StoneOfSwiftness.Item, StoneOfSwiftness.DefaultCount);

                        if (taked != null && taked.Length > 0)
                        {
                            System.Diagnostics.Debug.Assert(taked.Length == 1);
                            System.Diagnostics.Debug.Assert(taked[0].Count == itemStack.Count);

                            giveItem = ItemStack.Create(Coin.Item, Coin.DefaultCount * coinAmount);
                            success = playerInventory.GiveItem(giveItem);

                            if (success == false)
                            {
                                giveItem = ItemStack.Create(
                                    StoneOfSwiftness.Item,
                                    StoneOfSwiftness.DefaultCount * itemStack.Count);
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

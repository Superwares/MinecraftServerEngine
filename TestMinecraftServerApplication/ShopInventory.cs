

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
        public const int EclipseCrystalSlot = UniqueItemLineOffset + 6;


        public void ResetBalloonBasherSlot(string username)
        {
            if (username == null || string.IsNullOrEmpty(username) == true)
            {
                username = "없음";
            }

            SetSlot(BalloonBasherSlot, ItemStack.Create(BalloonBasher.Item, BalloonBasher.DefaultCount, [
                    // A lightweight yet powerful weapon that
                    // can send enemies flying with a single hit.
                    $"",
                    $"가볍지만 강력한 무기로 ",
                    $"한 방에 적을 날려버릴 수 있습니다.",
                    $"",
                    // Left-click (Purchase)
                    $"왼클릭(구매)          {BalloonBasher.PurchasePrice * Coin.DefaultCount} Coins",
                    // Right-click (Sell)
                    $"우클릭(판매)          {BalloonBasher.SellPrice * Coin.DefaultCount} Coins",
                    $"구매자                {username}",
                ]));
        }

        public void ResetEclipseCrystalSlot(string username)
        {
            if (username == null || string.IsNullOrEmpty(username) == true)
            {
                username = "없음";
            }

            SetSlot(EclipseCrystalSlot, ItemStack.Create(EclipseCrystal.Item, EclipseCrystal.DefaultCount, [
                    // It can obscure the world...
                    $"",
                    $"세상을 가릴 수 있습니다...",
                    $"",
                    // Left-click (Purchase)
                    $"왼클릭(구매)          {EclipseCrystal.PurchasePrice * Coin.DefaultCount} Coins",
                    // Right-click (Sell)
                    $"우클릭(판매)          {EclipseCrystal.SellPrice * Coin.DefaultCount} Coins",
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
                        $"왼클릭(구매)          {Coin.DefaultCount * WoodenSword.PurchasePrice} Coins",
                        // Right-click (Sell)
                        $"우클릭(판매)          {Coin.DefaultCount * WoodenSword.SellPrice} Coins",
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
                ResetEclipseCrystalSlot(null);
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
                        $"왼클릭(구매)          {Coin.DefaultCount * StoneOfSwiftness.PurchasePrice} Coins",
                        // Right-click (Sell)
                        $"우클릭(판매)          {Coin.DefaultCount * StoneOfSwiftness.SellPrice} Coins",
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

            int purchaseCount = itemStack.Count;

            switch (itemStack.Type)
            {
                case Coin.Type:
                    {
                        if (SuperWorld.GameContext.IsStarted == false)
                        {
                            success = playerInventory.GiveItemStacks(Coin.Item, Coin.DefaultCount);
                        }

                    }
                    break;
                case ShopItem.Type:
                    {
                        success = playerInventory.GiveItemStacks(ShopItem.Item, ShopItem.DefaultCount);
                    }
                    break;
                case GlobalChestItem.Type:
                    {
                        success = playerInventory.GiveItemStacks(ShopItem.Item, ShopItem.DefaultCount);
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

                        if (taked != null && SuperWorld.GameContext.IsStarted == true)
                        {
                            System.Diagnostics.Debug.Assert(player.Username != null);
                            System.Diagnostics.Debug.Assert(string.IsNullOrEmpty(player.Username) == false);
                            System.Diagnostics.Debug.Assert(i == BalloonBasherSlot);
                            ResetBalloonBasherSlot(player.Username);

                            BalloonBasher.CanPurchase = false;
                        }

                        success = taked != null;

                    }
                    break;
                case EclipseCrystal.Type:
                    {
                        if (EclipseCrystal.CanPurchase == false)
                        {
                            break;
                        }

                        taked = playerInventory.GiveAndTakeItemStacks(
                           EclipseCrystal.Item, EclipseCrystal.DefaultCount,
                           Coin.Item, Coin.DefaultCount * EclipseCrystal.PurchasePrice);

                        if (taked != null && SuperWorld.GameContext.IsStarted == true)
                        {
                            System.Diagnostics.Debug.Assert(player.Username != null);
                            System.Diagnostics.Debug.Assert(string.IsNullOrEmpty(player.Username) == false);
                            System.Diagnostics.Debug.Assert(i == EclipseCrystalSlot);
                            ResetEclipseCrystalSlot(player.Username);

                            EclipseCrystal.CanPurchase = false;
                        }

                        success = taked != null;

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
                                Coin.Item, Coin.DefaultCount);

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
                            ShopItem.Item, ShopItem.DefaultCount);

                        if (taked != null)
                        {
                            success = true;
                        }
                    }
                    break;
                case GlobalChestItem.Type:
                    {
                        taked = playerInventory.TakeItemStacksInPrimary(
                            GlobalChestItem.Item, GlobalChestItem.DefaultCount);

                        if (taked != null)
                        {
                            success = true;
                        }
                    }
                    break;
                case WoodenSword.Type:
                    {
                        taked = playerInventory.GiveAndTakeItemStacks(
                            Coin.Item, Coin.DefaultCount * WoodenSword.SellPrice,
                            WoodenSword.Item, WoodenSword.DefaultCount);

                        success = taked != null;
                    }
                    break;
                case BalloonBasher.Type:
                    {
                        if (BalloonBasher.CanPurchase == true)
                        {
                            break;
                        }

                        taked = playerInventory.GiveAndTakeItemStacks(
                            Coin.Item, Coin.DefaultCount * BalloonBasher.SellPrice,
                            BalloonBasher.Item, BalloonBasher.DefaultCount);

                        if (taked != null && SuperWorld.GameContext.IsStarted == true)
                        {
                            System.Diagnostics.Debug.Assert(i == BalloonBasherSlot);
                            ResetBalloonBasherSlot(null);

                            BalloonBasher.CanPurchase = true;
                        }

                        success = taked != null;

                    }
                    break;
                case EclipseCrystal.Type:
                    {
                        if (EclipseCrystal.CanPurchase == true)
                        {
                            break;
                        }

                        taked = playerInventory.GiveAndTakeItemStacks(
                            Coin.Item, Coin.DefaultCount * EclipseCrystal.SellPrice,
                            EclipseCrystal.Item, EclipseCrystal.DefaultCount);

                        if (taked != null && SuperWorld.GameContext.IsStarted == true)
                        {
                            System.Diagnostics.Debug.Assert(i == EclipseCrystalSlot);
                            ResetEclipseCrystalSlot(null);

                            EclipseCrystal.CanPurchase = true;
                        }

                        success = taked != null;

                    }
                    break;
                case StoneOfSwiftness.Type:
                    {
                        taked = playerInventory.GiveAndTakeItemStacks(
                            Coin.Item, Coin.DefaultCount * StoneOfSwiftness.SellPrice,
                            StoneOfSwiftness.Item, StoneOfSwiftness.DefaultCount);

                        success = taked != null;

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

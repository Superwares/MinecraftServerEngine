﻿

using Common;

using MinecraftServerEngine;
using MinecraftServerEngine.Entities;
using MinecraftServerEngine.Items;
using MinecraftServerEngine.Inventories;
using MinecraftServerEngine.Protocols;

namespace TestMinecraftServerApplication
{
    using Items;


    public sealed class ShopInventory : ItemInterfaceInventory
    {
        public override string Title => "Shop";


        public const int UniqueItemLineOffset = (SlotCountPerLine * 1) + 0;
        //public const int BalloonBasherSlot = UniqueItemLineOffset + 1;
        public const int BlastCoreSlot = UniqueItemLineOffset + 3;
        public const int EclipseCrystalSlot = UniqueItemLineOffset + 5;
        public const int PhoenixFeatherSlot = UniqueItemLineOffset + 6;
        public const int HyperBeamSlot = UniqueItemLineOffset + 7;
        public const int DoombringerSlot = UniqueItemLineOffset + 8;


        //public void ResetBalloonBasherSlot(string username)
        //{
        //    if (username == null || string.IsNullOrEmpty(username) == true)
        //    {
        //        username = "없음";
        //    }

        //    SetSlot(BalloonBasherSlot, ItemStack.Create(BalloonBasher.Item, BalloonBasher.DefaultCount, [
        //            // A lightweight yet powerful weapon that
        //            // can send enemies flying with a single hit.
        //            $"",
        //            $"가볍지만 강력한 무기로 ",
        //            $"한 방에 적을 날려버릴 수 있습니다.",
        //            $"",
        //            // Left-click (Purchase)
        //            $"왼클릭(구매)          {BalloonBasher.PurchasePrice * Coin.DefaultCount} Coins",
        //            // Right-click (Sell)
        //            $"우클릭(판매)          {BalloonBasher.SellPrice * Coin.DefaultCount} Coins",
        //            $"구매자                {username}",
        //        ]));
        //}

        public void ResetBlastCoreSlot(string username)
        {
            if (username == null || string.IsNullOrEmpty(username) == true)
            {
                username = "없음";
            }

            SetSlot(BlastCoreSlot, ItemStack.Create(BlastCore.Item, BlastCore.DefaultCount, [
                    $"",
                    // A powerful core that explodes on use, knocking back all nearby players.
                    $"사용 시 폭발하여 주변의 모든 것을 ",
                    $"날려버리는 강력한 코어입니다.",
                    $"",
                    // Left-click (Purchase)
                    $"왼클릭(구매)          {BlastCore.PurchasePrice * Coin.DefaultCount} Coins",
                    // Right-click (Sell)
                    $"우클릭(판매)          {BlastCore.SellPrice * Coin.DefaultCount} Coins",
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
                    $"",
                    // It can obscure the world...
                    $"세상을 가릴 수 있습니다...",
                    $"",
                    // Left-click (Purchase)
                    $"왼클릭(구매)          {EclipseCrystal.PurchasePrice * Coin.DefaultCount} Coins",
                    // Right-click (Sell)
                    $"우클릭(판매)          {EclipseCrystal.SellPrice * Coin.DefaultCount} Coins",
                    $"구매자                {username}",
                ]));
        }

        public void ResetPhoenixFeatherSlot(string username)
        {
            if (username == null || string.IsNullOrEmpty(username) == true)
            {
                username = "없음";
            }

            SetSlot(PhoenixFeatherSlot, ItemStack.Create(PhoenixFeather.Item, PhoenixFeather.DefaultCount, [
                    $"",
                    // A mystical feather that can revive upon death.
                    $"죽음에서 부활시킬 수 있는 신비한 깃털입니다.",
                    $"",
                    // Left-click (Purchase)
                    $"왼클릭(구매)          {PhoenixFeather.PurchasePrice * Coin.DefaultCount} Coins",
                    // Right-click (Sell)
                    $"우클릭(판매)          {PhoenixFeather.SellPrice * Coin.DefaultCount} Coins",
                    $"구매자                {username}",
                ]));
        }
        
        public void ResetHyperBeamSlot(string username)
        {
            if (username == null || string.IsNullOrEmpty(username) == true)
            {
                username = "없음";
            }

            SetSlot(HyperBeamSlot, ItemStack.Create(HyperBeam.Item, HyperBeam.DefaultCount, [
                    $"",
                    // A devastating beam that can obliterate anything in its path.
                    $"모든 것을 파괴할 수 있는 파괴적인 빔.",
                    $"",
                    // Left-click (Purchase)
                    $"왼클릭(구매)          {HyperBeam.PurchasePrice * Coin.DefaultCount} Coins",
                    // Right-click (Sell)
                    $"우클릭(판매)          {HyperBeam.SellPrice * Coin.DefaultCount} Coins",
                    $"구매자                {username}",
                ]));
        }


        public void ResetDoombringerSlot(string username)
        {
            if (username == null || string.IsNullOrEmpty(username) == true)
            {
                username = "없음";
            }

            SetSlot(DoombringerSlot, ItemStack.Create(Doombringer.Item, Doombringer.DefaultCount, [
                    $"",
                    // A cursed relic that holds the power of instantaneous death.
                    $"즉각적인 죽음의 힘을 지닌 저주받은 유물.",
                    $"",
                    // Left-click (Purchase)
                    $"왼클릭(구매)          {Doombringer.PurchasePrice * Coin.DefaultCount} Coins",
                    // Right-click (Sell)
                    $"우클릭(판매)          {Doombringer.SellPrice * Coin.DefaultCount} Coins",
                    $"구매자                {username}",
                ]));
        }

        public ShopInventory() : base(MaxLines)
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
                        // A basic wooden sword,
                        // ideal for beginners.
                        $"초보자에게 이상적인 ",
                        $"기본 나무검입니다.",
                        $"",
                        // Left-click (Purchase)
                        $"왼클릭(구매)          {Coin.DefaultCount * WoodenSword.PurchasePrice} Coins",
                        // Right-click (Sell)
                        $"우클릭(판매)          {Coin.DefaultCount * WoodenSword.SellPrice} Coins",
                        ])
                    );

                slots[offset + 3] = (
                    true,
                    ItemStack.Create(BalloonBasher.Item, BalloonBasher.DefaultCount, [
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
                        ])
                    );

                slots[offset + 4] = (
                    true,
                    ItemStack.Create(Dash.Item, Dash.DefaultCount, [
                        $"",
                        // Left-click (Purchase)
                        $"왼클릭(구매)          {Dash.PurchasePrice * Coin.DefaultCount} Coins",
                        // Right-click (Sell)
                        $"우클릭(판매)          {Dash.SellPrice * Coin.DefaultCount} Coins",
                        ])
                    );
                slots[offset + 5] = (
                    true,
                    ItemStack.Create(Hint.Item, Hint.DefaultCount, [
                        $"",
                        // Left-click (Purchase)
                        $"왼클릭(구매)          {Hint.PurchasePrice * Coin.DefaultCount} Coins",
                        // Right-click (Sell)
                        $"우클릭(판매)          {Hint.SellPrice * Coin.DefaultCount} Coins",
                        ])
                    );

                slots[offset + 6] = (
                    true,
                    ItemStack.Create(ChaosSwap.Item, ChaosSwap.DefaultCount, [
                        $"",
                        // This item triggers a random swap of positions with another player.
                        $"이 아이템은 다른 플레이어와 위치를 무작위로 바꿉니다.",
                        $"",
                        // Left-click (Purchase)
                        $"왼클릭(구매)          {ChaosSwap.PurchasePrice * Coin.DefaultCount} Coins",
                        // Right-click (Sell)
                        $"우클릭(판매)          {ChaosSwap.SellPrice * Coin.DefaultCount} Coins",
                        ])
                    );

                slots[offset + 7] = (
                    true,
                    ItemStack.Create(EmergencyEscape.Item, EmergencyEscape.DefaultCount, [
                        $"",
                        // A powerful item that allows the user
                        // to escape from danger instantly.
                        $"위험에서 즉시 탈출할 수 있는 ",
                        $"강력한 아이템입니다.",
                        $"",
                        // Left-click (Purchase)
                        $"왼클릭(구매)          {Coin.DefaultCount * EmergencyEscape.PurchasePrice} Coins",
                        // Right-click (Sell)
                        $"우클릭(판매)          {Coin.DefaultCount * EmergencyEscape.SellPrice} Coins",
                        ])
                    );
                slots[offset + 8] = (
                    true,
                    ItemStack.Create(StoneOfSwiftness.Item, StoneOfSwiftness.DefaultCount, [
                        $"",
                        // A mystical stone that grants
                        // the bearer enhanced speed and agility.
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

            // Unique items
            {
                slots[UniqueItemLineOffset] = (
                    true,
                    new ItemStack(ItemType.PurpleStainedGlassPane, "Unique Tier", 1)
                    );

                //ResetBalloonBasherSlot(null);
                ResetBlastCoreSlot(null);
                ResetEclipseCrystalSlot(null);
                ResetPhoenixFeatherSlot(null);
                ResetHyperBeamSlot(null);
                ResetDoombringerSlot(null);
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
                    ItemStack.Create(GlobalChestItem.Item, GlobalChestItem.DefaultCount)
                    );

            }

            // Last line
            {
                int offset = SlotCountPerLine * (MaxLines - 1);

                slots[offset + 0] = (
                    true,
                    ItemStack.Create(Coin.Item, Coin.MaxCount)
                    );

                slots[offset + 8] = (
                    true,
                    ItemStack.Create(ShopItem.Item, ShopItem.DefaultCount)
                    );
            }

            SetSlots(slots);
        }

        protected override void OnLeftClickSharedItem(
            World world,
            UserId userId, 
            AbstractPlayer player, PlayerInventory playerInventory,
            int i, ItemStack itemStack)
        {
            System.Diagnostics.Debug.Assert(world != null);
            System.Diagnostics.Debug.Assert(userId != UserId.Null);
            System.Diagnostics.Debug.Assert(player != null);
            System.Diagnostics.Debug.Assert(playerInventory != null);

            bool success = false;
            ItemStack[] taked;

            int purchaseCount = itemStack.Count;

            switch (itemStack.Type)
            {


                case WoodenSword.Type:
                    {
                        taked = playerInventory.GiveAndTakeItemStacks(
                            WoodenSword.Item, WoodenSword.DefaultCount,
                            Coin.Item, Coin.DefaultCount * WoodenSword.PurchasePrice);

                        success = (taked != null);
                    }
                    break;
                case BalloonBasher.Type:
                    {
                        taked = playerInventory.GiveAndTakeItemStacks(
                            BalloonBasher.Item, BalloonBasher.DefaultCount,
                            Coin.Item, Coin.DefaultCount * BalloonBasher.PurchasePrice);

                        success = (taked != null);
                    }
                    break;
                case Dash.Type:
                    {
                        taked = playerInventory.GiveAndTakeItemStacks(
                            Dash.Item, Dash.DefaultCount,
                            Coin.Item, Coin.DefaultCount * Dash.PurchasePrice);

                        success = (taked != null);

                    }
                    break;
                case Hint.Type:
                    {
                        taked = playerInventory.GiveAndTakeItemStacks(
                            Hint.Item, Hint.DefaultCount,
                            Coin.Item, Coin.DefaultCount * Hint.PurchasePrice);

                        success = (taked != null);

                    }
                    break;
                case ChaosSwap.Type:
                    {
                        taked = playerInventory.GiveAndTakeItemStacks(
                            ChaosSwap.Item, ChaosSwap.DefaultCount,
                            Coin.Item, Coin.DefaultCount * ChaosSwap.PurchasePrice);

                        success = (taked != null);

                    }
                    break;
                case EmergencyEscape.Type:
                    {
                        taked = playerInventory.GiveAndTakeItemStacks(
                            EmergencyEscape.Item, EmergencyEscape.DefaultCount,
                            Coin.Item, Coin.DefaultCount * EmergencyEscape.PurchasePrice);

                        success = (taked != null);

                    }
                    break;
                case StoneOfSwiftness.Type:
                    {
                        taked = playerInventory.GiveAndTakeItemStacks(
                            StoneOfSwiftness.Item, StoneOfSwiftness.DefaultCount,
                            Coin.Item, Coin.DefaultCount * StoneOfSwiftness.PurchasePrice);

                        success = (taked != null);

                    }
                    break;

                case BlastCore.Type:
                    {
                        if (BlastCore.CanPurchase == false)
                        {
                            break;
                        }

                        taked = playerInventory.GiveAndTakeItemStacks(
                           BlastCore.Item, BlastCore.DefaultCount,
                           Coin.Item, Coin.DefaultCount * BlastCore.PurchasePrice);

                        if (taked != null && SuperWorld.GameContext.IsStarted == true)
                        {
                            System.Diagnostics.Debug.Assert(player.Username != null);
                            System.Diagnostics.Debug.Assert(string.IsNullOrEmpty(player.Username) == false);
                            System.Diagnostics.Debug.Assert(i == BlastCoreSlot);
                            ResetBlastCoreSlot(player.Username);

                            BlastCore.CanPurchase = false;
                        }

                        success = (taked != null);

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

                        success = (taked != null);

                    }
                    break;
                case PhoenixFeather.Type:
                    {
                        if (PhoenixFeather.CanPurchase == false)
                        {
                            break;
                        }

                        taked = playerInventory.GiveAndTakeItemStacks(
                           PhoenixFeather.Item, PhoenixFeather.DefaultCount,
                           Coin.Item, Coin.DefaultCount * PhoenixFeather.PurchasePrice);

                        if (taked != null && SuperWorld.GameContext.IsStarted == true)
                        {
                            System.Diagnostics.Debug.Assert(player.Username != null);
                            System.Diagnostics.Debug.Assert(string.IsNullOrEmpty(player.Username) == false);
                            System.Diagnostics.Debug.Assert(i == PhoenixFeatherSlot);
                            ResetPhoenixFeatherSlot(player.Username);

                            PhoenixFeather.CanPurchase = false;
                        }

                        success = (taked != null);

                    }
                    break;
                case HyperBeam.Type:
                    {
                        if (HyperBeam.CanPurchase == false)
                        {
                            break;
                        }

                        taked = playerInventory.GiveAndTakeItemStacks(
                           HyperBeam.Item, HyperBeam.DefaultCount,
                           Coin.Item, Coin.DefaultCount * HyperBeam.PurchasePrice);

                        if (taked != null && SuperWorld.GameContext.IsStarted == true)
                        {
                            System.Diagnostics.Debug.Assert(player.Username != null);
                            System.Diagnostics.Debug.Assert(string.IsNullOrEmpty(player.Username) == false);
                            System.Diagnostics.Debug.Assert(i == HyperBeamSlot);
                            ResetHyperBeamSlot(player.Username);

                            HyperBeam.CanPurchase = false;
                        }

                        success = (taked != null);

                    }
                    break;
                case Doombringer.Type:
                    {
                        if (Doombringer.CanPurchase == false)
                        {
                            break;
                        }

                        taked = playerInventory.GiveAndTakeItemStacks(
                           Doombringer.Item, Doombringer.DefaultCount,
                           Coin.Item, Coin.DefaultCount * Doombringer.PurchasePrice);

                        if (taked != null && SuperWorld.GameContext.IsStarted == true)
                        {
                            System.Diagnostics.Debug.Assert(player.Username != null);
                            System.Diagnostics.Debug.Assert(string.IsNullOrEmpty(player.Username) == false);
                            System.Diagnostics.Debug.Assert(i == DoombringerSlot);
                            ResetDoombringerSlot(player.Username);

                            Doombringer.CanPurchase = false;
                        }

                        success = (taked != null);

                    }
                    break;

                case GlobalChestItem.Type:
                    {
                        success = playerInventory.GiveItemStacks(
                            GlobalChestItem.Item, GlobalChestItem.DefaultCount);
                    }
                    break;

                case Coin.Type:
                    {
                        if (SuperWorld.GameContext.IsStarted == false)
                        {
                            success = playerInventory.GiveItemStacks(Coin.Item, Coin.MaxCount);
                        }

                    }
                    break;
                case ShopItem.Type:
                    {
                        success = playerInventory.GiveItemStacks(ShopItem.Item, ShopItem.DefaultCount);
                    }
                    break;

            }

            if (success == true)
            {
                player.PlaySound("entity.item.pickup", 7, 1.0F, 2.0F);
            }
        }

        protected override void OnRightClickSharedItem(
            World world,
            UserId userId, 
            AbstractPlayer player, PlayerInventory playerInventory,
            int i, ItemStack itemStack)
        {
            System.Diagnostics.Debug.Assert(world != null);
            System.Diagnostics.Debug.Assert(userId != UserId.Null);
            System.Diagnostics.Debug.Assert(player != null);
            System.Diagnostics.Debug.Assert(playerInventory != null);

            bool success = false;

            ItemStack[] taked;

            switch (itemStack.Type)
            {

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
                        taked = playerInventory.GiveAndTakeItemStacks(
                            Coin.Item, Coin.DefaultCount * BalloonBasher.SellPrice,
                            BalloonBasher.Item, BalloonBasher.DefaultCount);

                        success = taked != null;
                    }
                    break;
                case Dash.Type:
                    {
                        taked = playerInventory.GiveAndTakeItemStacks(
                            Coin.Item, Coin.DefaultCount * Dash.SellPrice,
                            Dash.Item, Dash.DefaultCount);

                        success = taked != null;

                    }
                    break;
                case Hint.Type:
                    {
                        taked = playerInventory.GiveAndTakeItemStacks(
                            Coin.Item, Coin.DefaultCount * Hint.SellPrice,
                            Hint.Item, Hint.DefaultCount);

                        success = taked != null;

                    }
                    break;
                case ChaosSwap.Type:
                    {
                        taked = playerInventory.GiveAndTakeItemStacks(
                            Coin.Item, Coin.DefaultCount * ChaosSwap.SellPrice,
                            ChaosSwap.Item, ChaosSwap.DefaultCount);

                        success = taked != null;

                    }
                    break;
                case EmergencyEscape.Type:
                    {
                        taked = playerInventory.GiveAndTakeItemStacks(
                            Coin.Item, Coin.DefaultCount * EmergencyEscape.SellPrice,
                            EmergencyEscape.Item, EmergencyEscape.DefaultCount);

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

                case BlastCore.Type:
                    {
                        taked = playerInventory.GiveAndTakeItemStacks(
                            Coin.Item, Coin.DefaultCount * BlastCore.SellPrice,
                            BlastCore.Item, BlastCore.DefaultCount);

                        if (taked != null && SuperWorld.GameContext.IsStarted == true)
                        {
                            System.Diagnostics.Debug.Assert(i == BlastCoreSlot);
                            ResetBlastCoreSlot(null);

                            BlastCore.CanPurchase = true;
                        }

                        success = taked != null;

                    }
                    break;
                case EclipseCrystal.Type:
                    {
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
                case PhoenixFeather.Type:
                    {
                        taked = playerInventory.GiveAndTakeItemStacks(
                            Coin.Item, Coin.DefaultCount * PhoenixFeather.SellPrice,
                            PhoenixFeather.Item, PhoenixFeather.DefaultCount);

                        if (taked != null && SuperWorld.GameContext.IsStarted == true)
                        {
                            System.Diagnostics.Debug.Assert(i == PhoenixFeatherSlot);
                            ResetPhoenixFeatherSlot(null);

                            PhoenixFeather.CanPurchase = true;
                        }

                        success = taked != null;

                    }
                    break;
                case HyperBeam.Type:
                    {
                        taked = playerInventory.GiveAndTakeItemStacks(
                            Coin.Item, Coin.DefaultCount * HyperBeam.SellPrice,
                            HyperBeam.Item, HyperBeam.DefaultCount);

                        if (taked != null && SuperWorld.GameContext.IsStarted == true)
                        {
                            System.Diagnostics.Debug.Assert(i == HyperBeamSlot);
                            ResetHyperBeamSlot(null);

                            HyperBeam.CanPurchase = true;
                        }

                        success = taked != null;

                    }
                    break;
                case Doombringer.Type:
                    {
                        taked = playerInventory.GiveAndTakeItemStacks(
                            Coin.Item, Coin.DefaultCount * Doombringer.SellPrice,
                            Doombringer.Item, Doombringer.DefaultCount);

                        if (taked != null && SuperWorld.GameContext.IsStarted == true)
                        {
                            System.Diagnostics.Debug.Assert(i == DoombringerSlot);
                            ResetDoombringerSlot(null);

                            Doombringer.CanPurchase = true;
                        }

                        success = taked != null;

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

                case Coin.Type:
                    {
                        if (SuperWorld.GameContext.IsStarted == false)
                        {
                            taked = playerInventory.TakeItemStacksInPrimary(
                                Coin.Item, Coin.MaxCount);

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

            }

            if (success == true)
            {
                player.PlaySound("entity.item.pickup", 7, 1.0F, 2.0F);
            }
        }


    }
}

﻿
using Common;
using Containers;
using Sync;

using MinecraftServerEngine;
using MinecraftServerEngine.Inventories;
using MinecraftServerEngine.Entities;
using MinecraftServerEngine.Items;
using MinecraftServerEngine.Protocols;

namespace TestMinecraftServerApplication
{
    using Items;


    public sealed class GameContextInventory : ItemInterfaceInventory
    {
        public const int GameContextInventoryMaxLineCount = 5;

        public const int PlayerSeatSlotOffset = (9 * 0) + 0;

        public const int GameSwitchSlot = (9 * 2) + 4;

        public const int ShopItemSlot = (9 * 2) + 8;

        public const int RoundIndicatorSlotOffset = (9 * 3) + 0;

        public const ItemType GameSwitchOnItemType = ItemType.JackOLantern;
        public const ItemType GameSwitchOffItemType = ItemType.Pumpkin;

        public const ItemType PlayerSeatItemType = ItemType.PlayerSkull;
        public const ItemType EmptySeatItemType = ItemType.RedWool;

        public const ItemType EnabledRoundItemType = ItemType.GreenStainedGlassPane;
        public const ItemType DisabledRoundItemType = ItemType.RedStainedGlassPane;
        public const ItemType EmptyRoundItemType = ItemType.GrayStainedGlassPane;

        public override string Title => "Game Context";

        private static ItemStack GetGameSwitchOnItemStack(
            int currentPlayers, int totalRounds, int currentRound)
        {
            System.Diagnostics.Debug.Assert(currentPlayers > 0);
            System.Diagnostics.Debug.Assert(totalRounds > 0);
            System.Diagnostics.Debug.Assert(currentRound > 0);
            System.Diagnostics.Debug.Assert(currentRound <= totalRounds);

            return new ItemStack(
                GameSwitchOnItemType,
                "게임 진행 중...", 1, [
                    $"참여 인원          {currentPlayers}",
                    $"",
                    $"라운드             {currentRound}/{totalRounds}",
                ]);
        }

        private static ItemStack GetGameSwitchOffItemStack(int minPlayers, int maxPlayers, int currentPlayers)
        {
            return new ItemStack(
                GameSwitchOffItemType, "게임 시작!", 1, [
                    $"클릭하여 게임을 시작합니다.",
                    $"",
                    $"최소/최대 인원     {minPlayers}/{maxPlayers}",
                    $"현재 인원          {currentPlayers}",
                ]);
        }

        private static void ResetPlayerScoreSlots(
            (bool, ItemStack)[] slots,
            List<SuperPlayer> players,
            IReadOnlyMap<UserId, ScoreboardPlayerRow> scoreboard)
        {
            System.Diagnostics.Debug.Assert(slots != null);
            System.Diagnostics.Debug.Assert(slots.Length > 0);
            System.Diagnostics.Debug.Assert(scoreboard != null);

            int i = 0;

            if (players != null)
            {
                System.Diagnostics.Debug.Assert(players.Length <= GameContext.MaxPlayers);
                foreach (SuperPlayer player in players)
                {
                    int slot = i++ + PlayerSeatSlotOffset;

                    ScoreboardPlayerRow row = scoreboard.Lookup(player.UserId);

                    slots[slot] = (true, new ItemStack(
                        PlayerSeatItemType, player.Username, 1, [
                            $"현재 참여한 플레이어입니다.",
                            $"",
                            $"킬/데스         {row.Kills}/{row.Deaths}",
                            $"생존            {row.Surviving}",
                            $"총 포인트       {row.TotalPoints}",
                        ]));
                }
            }


            for (; i < GameContext.MaxRounds; ++i)
            {
                int slot = i + PlayerSeatSlotOffset;

                slots[slot] = (true, new ItemStack(
                    EmptySeatItemType, "빈자리", 1, [
                        "진행 중에는 게임을 탈퇴할 수 없습니다.",
                    ]));
            }
        }

        private static void ResetPlayerSeatSlots((bool, ItemStack)[] slots, List<SuperPlayer> players)
        {
            System.Diagnostics.Debug.Assert(slots != null);
            System.Diagnostics.Debug.Assert(slots.Length > 0);

            //if (players == null || totalRounds == 0)
            //{
            //    return;
            //}

            int i = 0;

            if (players != null)
            {
                System.Diagnostics.Debug.Assert(players.Length <= GameContext.MaxPlayers);
                foreach (SuperPlayer player in players)
                {
                    int slot = i++ + PlayerSeatSlotOffset;

                    slots[slot] = (true, new ItemStack(
                        PlayerSeatItemType, player.Username, 1, [
                            "현재 참여한 플레이어입니다.",
                        ]));
                }
            }


            for (; i < GameContext.MaxRounds; ++i)
            {
                int slot = i + PlayerSeatSlotOffset;

                slots[slot] = (true, new ItemStack(
                    EmptySeatItemType, "빈자리", 1, [
                        "클릭하여 게임에 참여 또는 탈퇴합니다.",
                    ]));
            }
        }

        public void Reset(
            List<SuperPlayer> players,
            int minPlayers, int maxPlayers,
            int maxRounds)
        {
            (bool, ItemStack)[] slots = new (bool, ItemStack)[GetTotalSlotCount()];

            ResetPlayerSeatSlots(slots, players);

            int currentPlayers = players == null ? 0 : players.Length;

            System.Diagnostics.Debug.Assert(GameSwitchSlot < GameContextInventoryMaxLineCount * SlotCountPerLine);
            slots[GameSwitchSlot] = (true, GetGameSwitchOffItemStack(
                minPlayers, maxPlayers,
                currentPlayers));

            slots[ShopItemSlot] = (true, ItemStack.Create(ShopItem.Item, ShopItem.DefaultCount));

            System.Diagnostics.Debug.Assert(maxRounds % SlotCountPerLine == 0);
            for (int i = 0; i < maxRounds; ++i)
            {
                int slot = i + RoundIndicatorSlotOffset;
                System.Diagnostics.Debug.Assert(slot < GameContextInventoryMaxLineCount * SlotCountPerLine);

                slots[slot] = (true, new ItemStack(
                    EmptyRoundItemType, "빈 라운드", 1, [
                        "플레이어 수에 맞춰 라운드가 활성화됩니다!",
                    ]));
            }

            SetSlots(slots);
        }

        public GameContextInventory(
            int minPlayers, int maxPlayers,
            int maxRounds) : base(GameContextInventoryMaxLineCount)
        {
            Reset(null, minPlayers, maxPlayers, maxRounds);
        }

        protected override void OnLeftClickSharedItem(
            World world,
            UserId userId,
            AbstractPlayer _player, PlayerInventory playerInventory,
            int i, ItemStack itemStack)
        {
            if (_player is SuperPlayer player)
            {
                bool success = false;

                ItemStack droppedItemStack = null;
                //ItemStack giveItem;

                switch (i)
                {
                    case int _playerSeat when (
                        _playerSeat >= PlayerSeatSlotOffset &&
                        _playerSeat <= PlayerSeatSlotOffset + GameContext.MaxPlayers):
                        {
                            int playerSeat = _playerSeat - PlayerSeatSlotOffset;


                            System.Diagnostics.Debug.Assert(SuperWorld.GameContext != null);
                            bool f = SuperWorld.GameContext.AddPlayer(player);
                            if (f == false)
                            {
                                System.Diagnostics.Debug.Assert(SuperWorld.GameContext != null);
                                f = SuperWorld.GameContext.RemovePlayer(player.UserId);
                            }

                            if (f == false)
                            {
                                System.Diagnostics.Debug.Assert(SuperWorld.GameContext != null);
                                SuperWorld.GameContext.TeleportTo(player, playerSeat);
                            }

                            success = true;


                        }
                        break;
                    case GameSwitchSlot:
                        {
                            success = SuperWorld.GameContext.Ready();

                            if (success == true)   
                            {
                                droppedItemStack = player.CloseInventory();
                            }
                        }
                        break;
                }

                switch (itemStack.Type)
                {
                    case ShopItem.Type:
                        {
                            //giveItem = ItemStack.Create(ShopItem.Item, ShopItem.DefaultCount * itemStack.Count);
                            //success = playerInventory.GiveItemStack(giveItem);
                        }
                        break;
                }

                if (success == true)
                {
                    _player.PlaySound("entity.item.pickup", 7, 1.0F, 2.0F);
                }

                if (droppedItemStack != null)
                {
                    world.SpawnObject(new ItemEntity(droppedItemStack, player.Position));
                }
            }
        }


        protected override void OnRightClickSharedItem(
            World world,
            UserId userId, 
            AbstractPlayer player, PlayerInventory playerInventory,
            int i, ItemStack itemStack)
        {
            bool success = false;

            //ItemStack giveItem;
            ItemStack[] taked;


            switch (itemStack.Type)
            {
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
            }

            if (success == true)
            {
                player.PlaySound("entity.item.pickup", 7, 1.0F, 2.0F);
            }
        }

        public void ResetPlayerSeatsBeforeGame(
            List<SuperPlayer> players,
            int minPlayers, int maxPlayers)
        {
            (bool, ItemStack)[] slots = new (bool, ItemStack)[GetTotalSlotCount()];

            ResetPlayerSeatSlots(slots, players);

            System.Diagnostics.Debug.Assert(GameSwitchSlot < GameContextInventoryMaxLineCount * SlotCountPerLine);
            slots[GameSwitchSlot] = (true, GetGameSwitchOffItemStack(
                minPlayers, maxPlayers,
                players.Length));

            SetSlots(slots);
        }

        public void StartGame(
            List<SuperPlayer> players,
            int totalRounds,
            IReadOnlyMap<UserId, ScoreboardPlayerRow> scoreboard)
        {
            System.Diagnostics.Debug.Assert(players != null);
            System.Diagnostics.Debug.Assert(totalRounds > 0);
            System.Diagnostics.Debug.Assert(scoreboard != null);

            (bool, ItemStack)[] slots = new (bool, ItemStack)[GetTotalSlotCount()];

            ResetPlayerScoreSlots(slots, players, scoreboard);

            slots[GameSwitchSlot] = (true, GetGameSwitchOnItemStack(players.Length, totalRounds, 1));

            for (int i = 0; i < totalRounds; ++i)
            {
                int slot = i + RoundIndicatorSlotOffset;
                System.Diagnostics.Debug.Assert(slot < GameContextInventoryMaxLineCount * SlotCountPerLine);

                slots[slot] = (true, new ItemStack(
                    DisabledRoundItemType, "시작하지 않은 라운드", 1, [
                    ]));
            }

            SetSlots(slots);
        }

        public void StartRound(List<SuperPlayer> players, int totalRounds, int currentRoundIndex)
        {
            System.Diagnostics.Debug.Assert(currentRoundIndex >= 0);
            System.Diagnostics.Debug.Assert(currentRoundIndex < totalRounds);

            (bool, ItemStack)[] slots = new (bool, ItemStack)[GetTotalSlotCount()];

            slots[GameSwitchSlot] = (true,
                GetGameSwitchOnItemStack(players.Length, totalRounds, currentRoundIndex + 1));

            int slot = currentRoundIndex + RoundIndicatorSlotOffset;
            System.Diagnostics.Debug.Assert(slot < GameContextInventoryMaxLineCount * SlotCountPerLine);

            slots[slot] = (true, new ItemStack(
                EnabledRoundItemType, "현재 라운드", 1, [
                ]));

            SetSlots(slots);
        }

        public void UpdatePlayerScores(
            List<SuperPlayer> players,
            IReadOnlyMap<UserId, ScoreboardPlayerRow> scoreboard)
        {
            System.Diagnostics.Debug.Assert(players != null);
            System.Diagnostics.Debug.Assert(scoreboard != null);

            (bool, ItemStack)[] slots = new (bool, ItemStack)[GetTotalSlotCount()];

            ResetPlayerScoreSlots(slots, players, scoreboard);

            SetSlots(slots);
        }

        public void EndRound(List<SuperPlayer> players, int totalRounds, int currentRoundIndex)
        {
            System.Diagnostics.Debug.Assert(currentRoundIndex >= 0);
            System.Diagnostics.Debug.Assert(currentRoundIndex < totalRounds);

            (bool, ItemStack)[] slots = new (bool, ItemStack)[GetTotalSlotCount()];

            int slot = currentRoundIndex + RoundIndicatorSlotOffset;
            System.Diagnostics.Debug.Assert(slot < GameContextInventoryMaxLineCount * SlotCountPerLine);

            slots[slot] = (true, new ItemStack(
                EnabledRoundItemType, "종료된 라운드", 1, [
                ]));

            SetSlots(slots);
        }

    }
}

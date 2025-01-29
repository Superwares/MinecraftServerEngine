
using Common;
using Containers;
using Sync;

using MinecraftPrimitives;
using MinecraftServerEngine;

namespace TestMinecraftServerApplication
{
    using Items;

    public sealed class GameContextInventory : ItemInterfaceInventory
    {
        public const int GameContextInventoryMaxLineCount = 5;

        public const int PlayerSeatSlotOffset = (9 * 0) + 0;

        public const int GameSwitchSlot = (9 * 2) + 4;

        public const int RoundIndicatorSlotOffset = (9 * 3) + 0;

        private const ItemType GameSwitchOnItemType = ItemType.JackOLantern;
        private const ItemType GameSwitchOffItemType = ItemType.Pumpkin;

        private const ItemType PlayerSeatItemType = ItemType.PlayerSkull;
        private const ItemType EmptySeatItemType = ItemType.RedWool;

        private const ItemType EnabledRoundItemType = ItemType.GreenStainedGlassPane;
        private const ItemType DisabledRoundItemType = ItemType.RedStainedGlassPane;
        private const ItemType EmptyRoundItemType = ItemType.GrayStainedGlassPane;

        public override string Title => "Game Context";

        private ItemStack GetGameSwitchOnItemStack(
            int currentPlayers, int totalRounds)
        {
            return new ItemStack(
                GameSwitchOnItemType,
                "게임 진행 중...", 1, [
                    $"참여 인원          {currentPlayers}",
                    $"총 라운드          {totalRounds}",
                ]);
        }

        private ItemStack GetGameSwitchOffItemStack(int minPlayers, int maxPlayers, int currentPlayers)
        {
            return new ItemStack(
                GameSwitchOffItemType, "게임 시작!", 1, [
                    $"클릭하여 게임을 시작합니다.",
                    $"",
                    $"최소/최대 인원     {minPlayers}/{maxPlayers}",
                    $"현재 인원          {currentPlayers}",
                ]);
        }

        private void ResetPlayerSeatSlots((bool, ItemStack)[] slots, List<SuperPlayer> players)
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
            UserId userId,
            AbstractPlayer _player, PlayerInventory playerInventory,
            int i, ItemStack itemStack)
        {
            bool success = false;

            switch (i)
            {
                case int playerSeat when (
                    playerSeat >= PlayerSeatSlotOffset &&
                    playerSeat <= PlayerSeatSlotOffset + GameContext.MaxPlayers):
                    {
                        if (_player is SuperPlayer player)
                        {
                            bool f = SuperWorld.GameContext.Add(player);
                            if (f == false)
                            {
                                SuperWorld.GameContext.Remove(player.UserId);
                            }

                            success = true;
                        }

                    }
                    break;
                case GameSwitchSlot:
                    {
                        success = SuperWorld.GameContext.Start();
                    }
                    break;
            }

            if (success == true)
            {
                _player.PlaySound("entity.item.pickup", 7, 1.0F, 2.0F);
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

        public void StartGame(List<SuperPlayer> players, int totalRounds)
        {
            (bool, ItemStack)[] slots = new (bool, ItemStack)[GetTotalSlotCount()];

            slots[GameSwitchSlot] = (true, GetGameSwitchOnItemStack(players.Length, totalRounds));

            System.Diagnostics.Debug.Assert(GameContext.MaxPlayers % SlotCountPerLine == 0);
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

    }
}

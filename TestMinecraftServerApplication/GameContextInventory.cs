
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

        public override string Title => "Game Context";



        private ItemStack GetGameSwitchOffItemStack(int minPlayers, int maxPlayers, int currentPlayers)
        {
            return new ItemStack(
                GameSwitchOffItemType, "게임 시작!", 1, [
                    $"클릭하여 게임을 시작합니다.",
                    $"",
                    $"최소/최대 인원 수     {minPlayers}/{maxPlayers}",
                    $"현재 인원 수          {currentPlayers}",
                ]);
        }

        private void ResetPlayerSeatSlots((bool, ItemStack)[] slots, List<SuperPlayer> players)
        {
            System.Diagnostics.Debug.Assert(slots != null);
            System.Diagnostics.Debug.Assert(slots.Length > 0);

            //if (players == null || players.Length == 0)
            //{
            //    return;
            //}

            int i = 0;

            if (players != null)
            {
                System.Diagnostics.Debug.Assert(players.Count <= GameContext.MaxPlayers);
                foreach (SuperPlayer player in players)
                {
                    int slot = i++ + PlayerSeatSlotOffset;

                    slots[slot] = (true, new ItemStack(
                        PlayerSeatItemType, player.Username, 1, [
                            "현재 참여한 플레이어입니다.",
                        ]));
                }
            }


            for (; i < GameContext.MaxPlayers; ++i)
            {
                int slot = i + PlayerSeatSlotOffset;

                slots[slot] = (true, new ItemStack(
                    EmptySeatItemType, "빈자리", 1, [
                        "클릭하여 게임에 참여 또는 탈퇴합니다.",
                    ]));
            }
        }

        public GameContextInventory() : base(GameContextInventoryMaxLineCount)
        {
            (bool, ItemStack)[] slots = new (bool, ItemStack)[GetTotalSlotCount()];

            //SetSlot((9 * 1) + 1, new ItemStack(
            //    ItemType.PlayerSkull, "welcomehyunseo", 1, [
            //        "Click to leave the game",
            //    ]));o

            ResetPlayerSeatSlots(slots, null);

            System.Diagnostics.Debug.Assert(GameSwitchSlot < GameContextInventoryMaxLineCount * SlotCountPerLine);
            slots[GameSwitchSlot] = (true, GetGameSwitchOffItemStack(
                GameContext.MinPlayers, GameContext.MaxPlayers,
                TestWorld.GameContext.CurrentPlayers));

            System.Diagnostics.Debug.Assert(GameContext.MaxPlayers % SlotCountPerLine == 0);
            for (int i = 0; i < GameContext.MaxPlayers; ++i)
            {
                int slot = i + RoundIndicatorSlotOffset;
                System.Diagnostics.Debug.Assert(slot < GameContextInventoryMaxLineCount * SlotCountPerLine);

                slots[slot] = (true, new ItemStack(
                    ItemType.GrayStainedGlassPane, "빈 라운드", 1, [
                        "플레이어 수에 맞춰 라운드가 활성화됩니다!",
                    ]));
            }

            SetSlots(slots);
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
                            bool f = TestWorld.GameContext.Add(player);
                            if (f == false)
                            {
                                TestWorld.GameContext.Remove(player.UserId);
                            }

                            success = true;
                        }

                    }
                    break;
                case GameSwitchSlot:
                    {
                        if (TestWorld.GameContext.CanStart == true)
                        {
                            success = TestWorld.GameContext.Start();
                        }
                    }
                    break;
            }

            if (success == true)
            {
                _player.PlaySound("entity.item.pickup", 7, 1.0F, 2.0F);
            }
        }

        public void ResetPlayerSeats(List<SuperPlayer> players)
        {
            (bool, ItemStack)[] slots = new (bool, ItemStack)[GetTotalSlotCount()];

            ResetPlayerSeatSlots(slots, players);

            System.Diagnostics.Debug.Assert(GameSwitchSlot < GameContextInventoryMaxLineCount * SlotCountPerLine);
            slots[GameSwitchSlot] = (true, GetGameSwitchOffItemStack(
                GameContext.MinPlayers, GameContext.MaxPlayers,
                TestWorld.GameContext.CurrentPlayers));

            SetSlots(slots);
        }

        public void StartGame(List<SuperPlayer> players)
        {
            (bool, ItemStack)[] slots = new (bool, ItemStack)[GetTotalSlotCount()];

            slots[GameSwitchSlot] = (true, new ItemStack(
                GameSwitchOnItemType,
                "게임 진행 중...",  // Game in progress...
                1, [
                ]));

            System.Diagnostics.Debug.Assert(GameContext.MaxPlayers % SlotCountPerLine == 0);
            System.Diagnostics.Debug.Assert(players.Count >= GameContext.MinPlayers);
            System.Diagnostics.Debug.Assert(players.Count <= GameContext.MaxPlayers);
            for (int i = 0; i < players.Count; ++i)
            {
                int slot = i + RoundIndicatorSlotOffset;
                System.Diagnostics.Debug.Assert(slot < GameContextInventoryMaxLineCount * SlotCountPerLine);

                slots[slot] = (true, new ItemStack(
                    ItemType.RedStainedGlassPane, "시작하지 않은 라운드", 1, [
                    ]));
            }

            SetSlots(slots);
        }

    }
}

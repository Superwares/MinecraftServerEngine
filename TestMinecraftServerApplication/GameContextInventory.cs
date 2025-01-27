
using Common;

using MinecraftPrimitives;
using MinecraftServerEngine;

namespace TestMinecraftServerApplication
{
    using Items;
    using Sync;

    public sealed class GameContextInventory : ItemInterfaceInventory
    {
        public const int PLAYER_SEAT_SLOT_OFFSET = (9 * 1) + 1;
        public readonly int[] PLAYER_SEAT_SLOTS = [
            (9 * 1) + 1,
            (9 * 1) + 2,
            (9 * 1) + 3,
            (9 * 1) + 4,
            (9 * 1) + 5,
            (9 * 1) + 6,
            (9 * 1) + 7,
            (9 * 2) + 1,
            (9 * 2) + 2,
            (9 * 2) + 3,
            (9 * 2) + 4,
            (9 * 2) + 5,
            (9 * 2) + 6,
            (9 * 2) + 7,
            ];

        public const int START_GAME_SLOT = (9 * 4) + 0;

        public const int FIRST_ROUND_SLOT = (9 * 4) + 1;
        public const int SECOND_ROUND_SLOT = (9 * 4) + 2;
        public const int THIRD_ROUND_SLOT = (9 * 4) + 3;
        public const int FOURTH_ROUND_SLOT = (9 * 4) + 4;
        public const int FIFTH_ROUND_SLOT = (9 * 4) + 5;
        public const int SIXTH_ROUND_SLOT = (9 * 4) + 6;
        public const int FINAL_ROUND_SLOT = (9 * 4) + 7;


        public override string Title => "Game Context";

        public GameContextInventory() : base(MaxLineCount)
        {
            //SetSlot((9 * 1) + 1, new ItemStack(
            //    ItemType.PlayerSkull, "welcomehyunseo", 1, [
            //        "Click to leave the game",
            //    ]));

            foreach (int slot in PLAYER_SEAT_SLOTS) {
                SetSlot(slot, new ItemStack(
                    ItemType.RedWool, "Empty Seat", 1, [
                        "Click to join the game",
                    ]));
            }
            
            SetSlot(START_GAME_SLOT, new ItemStack(
                ItemType.GrayStainedGlassPane, "Start Game!", 1, [
                    "Click to start the game",
                ]));
            SetSlot(FIRST_ROUND_SLOT, new ItemStack(
                ItemType.RedStainedGlassPane, "First Round", 1, [
                ]));
            SetSlot(SECOND_ROUND_SLOT, new ItemStack(
                ItemType.RedStainedGlassPane, "Second Round", 1, [
                ]));
            SetSlot(THIRD_ROUND_SLOT, new ItemStack(
                ItemType.RedStainedGlassPane, "Third Round", 1, [
                ]));
            SetSlot(FOURTH_ROUND_SLOT, new ItemStack(
                ItemType.RedStainedGlassPane, "Fourth Round", 1, [
                ]));
            SetSlot(FIFTH_ROUND_SLOT, new ItemStack(
                ItemType.RedStainedGlassPane, "Fifth Round", 1, [
                ]));
            SetSlot(SIXTH_ROUND_SLOT, new ItemStack(
                ItemType.RedStainedGlassPane, "Sixth Round", 1, [
                ]));
            SetSlot(FINAL_ROUND_SLOT, new ItemStack(
                ItemType.RedStainedGlassPane, "Final Round", 1, [
                ]));

        }

        protected override void OnLeftClickSharedItem(
            UserId userId, 
            AbstractPlayer player, PlayerInventory playerInventory, 
            int i, ItemStack itemStack)
        {
            bool success = false;

            switch (i)
            {
                case START_GAME_SLOT:
                    {
                        SetSlot(START_GAME_SLOT, new ItemStack(
                            ItemType.GreenStainedGlassPane, "Game in progress...", 1, [
                            ]));
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

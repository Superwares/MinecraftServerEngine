
using Common;

using MinecraftPrimitives;
using MinecraftServerEngine;

namespace TestMinecraftServerApplication
{
    using Items;

    public sealed class GameContextInventory : ItemInterfaceInventory
    {
        public override string Title => "Game Context";

        public GameContextInventory() : base(MaxLineCount)
        {
            SetSlot((9 * 1) + 1, new ItemStack(
                ItemType.PlayerSkull, "welcomehyunseo", 1, [
                    "Click to leave the game",
                ]));
            SetSlot((9 * 1) + 2, new ItemStack(
                ItemType.RedWool, "Empty Seat", 1, [
                    "Click to join the game",
                ]));
            SetSlot((9 * 1) + 3, new ItemStack(
                ItemType.RedWool, "Empty Seat", 1, [
                    "Click to join the game",
                ]));
            SetSlot((9 * 1) + 4, new ItemStack(
                ItemType.RedWool, "Empty Seat", 1, [
                    "Click to join the game",
                ]));
            SetSlot((9 * 1) + 5, new ItemStack(
                ItemType.RedWool, "Empty Seat", 1, [
                    "Click to join the game",
                ]));
            SetSlot((9 * 1) + 6, new ItemStack(
                ItemType.RedWool, "Empty Seat", 1, [
                    "Click to join the game",
                ]));
            SetSlot((9 * 1) + 7, new ItemStack(
                ItemType.RedWool, "Empty Seat", 1, [
                    "Click to join the game",
                ]));
            SetSlot((9 * 2) + 1, new ItemStack(
                ItemType.RedWool, "Empty Seat", 1, [
                    "Click to join the game",
                ]));
            SetSlot((9 * 2) + 2, new ItemStack(
                ItemType.RedWool, "Empty Seat", 1, [
                    "Click to join the game",
                ]));
            SetSlot((9 * 2) + 3, new ItemStack(
                ItemType.RedWool, "Empty Seat", 1, [
                    "Click to join the game",
                ]));
            SetSlot((9 * 2) + 4, new ItemStack(
                ItemType.RedWool, "Empty Seat", 1, [
                    "Click to join the game",
                ]));
            SetSlot((9 * 2) + 5, new ItemStack(
                ItemType.RedWool, "Empty Seat", 1, [
                    "Click to join the game",
                ]));
            SetSlot((9 * 2) + 6, new ItemStack(
                ItemType.RedWool, "Empty Seat", 1, [
                    "Click to join the game",
                ]));
            SetSlot((9 * 2) + 7, new ItemStack(
                ItemType.RedWool, "Empty Seat", 1, [
                    "Click to join the game",
                ]));

            SetSlot((9 * 4) + 0, new ItemStack(
                ItemType.GrayStainedGlassPane, "Start Game!", 1, [
                    "Click to start the game",
                ]));
            SetSlot((9 * 4) + 1, new ItemStack(
                ItemType.RedStainedGlassPane, "First Round", 1, [
                ]));
            SetSlot((9 * 4) + 2, new ItemStack(
                ItemType.RedStainedGlassPane, "Second Round", 1, [
                ]));
            SetSlot((9 * 4) + 3, new ItemStack(
                ItemType.RedStainedGlassPane, "Third Round", 1, [
                ]));
            SetSlot((9 * 4) + 4, new ItemStack(
                ItemType.RedStainedGlassPane, "Fourth Round", 1, [
                ]));
            SetSlot((9 * 4) + 5, new ItemStack(
                ItemType.RedStainedGlassPane, "Fifth Round", 1, [
                ]));
            SetSlot((9 * 4) + 6, new ItemStack(
                ItemType.RedStainedGlassPane, "Sixth Round", 1, [
                ]));
            SetSlot((9 * 4) + 7, new ItemStack(
                ItemType.RedStainedGlassPane, "Final Round", 1, [
                ]));

        }





    }
}

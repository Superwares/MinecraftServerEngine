
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

        }





    }
}

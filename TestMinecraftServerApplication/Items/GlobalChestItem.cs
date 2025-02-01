using MinecraftServerEngine;

namespace TestMinecraftServerApplication.Items
{
    public static class GlobalChestItem
    {
        public const ItemQualityTier Tier = ItemQualityTier.Utility;

        public const ItemType Type = ItemType.MusicDisc_C418_blocks;
        public const string Name = "Global Chest";

        public const int InventoryLines = 3;

        public const int PurchasePrice = 0;
        public const int SellPrice = 0;

        public readonly static IReadOnlyItem Item = new Item(
            Type,
            Name,
            [
                $"Tier            {Tier.ToString()}",  // Quality Tier
                $"Total slots     {InventoryLines * SharedInventory.SlotCountPerLine}",
            ]);

        public static readonly int DefaultCount = MinecraftServerEngine.Item.MinCount;

        static GlobalChestItem()
        {
            System.Diagnostics.Debug.Assert(InventoryLines > 0);
            System.Diagnostics.Debug.Assert(InventoryLines <= SharedInventory.MaxLines);
        }
    }
}

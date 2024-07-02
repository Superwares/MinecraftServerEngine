

namespace MinecraftServerEngine
{
    internal sealed class InventorySlot
    {
        private ItemStack _stack = new(ItemType.Air, ItemStack.MinCount);

        public InventorySlot()
        {

        }



        public override string ToString()
        {
            System.Diagnostics.Debug.Assert(_stack != null);
            return $"{_stack}";
        }
    }
}


using Common;

using MinecraftPrimitives;
using MinecraftServerEngine;

namespace TestMinecraftServerApplication
{
    using Items;

    internal class GameContextInventory : ItemInterfaceInventory
    {
        public override string Title => "Game Context";

        public GameContextInventory() : base(MaxLineCount)
        {
            SetSlot(0, new ItemStack(ItemType.PlayerSkull, "welcomehyunseo", 1));

        }

    }
}

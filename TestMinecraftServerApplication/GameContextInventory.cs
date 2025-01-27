
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
            ResetSlot(0, new ItemStack(ItemType.PlayerSkull, "welcomehyunseo"));

            ResetSlot(9 + 1, BalloonBasher.Create([
                $"",
                $"가볍지만 강력한 한 방으로 적을 날려버리세요!",
                $"",
                $"왼클릭(구매)          {BalloonBasher.PurchasePrice} 코인",
                $"우클릭(판매)          {BalloonBasher.SellPrice} 코인",
                ]));
        }

    }
}

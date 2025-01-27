
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
            ResetSlot(0, Coin.Create([
                $"",
                $"테스트용 무료 코인입니다.",
                $"",
                $"왼클릭          지급",
                $"우클릭          차감",
                ]));

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

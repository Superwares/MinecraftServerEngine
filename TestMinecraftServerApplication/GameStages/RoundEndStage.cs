

using Common;
using Containers;

using MinecraftServerEngine;
using MinecraftServerEngine.ProgressBars;
using MinecraftServerEngine.Text;

namespace TestMinecraftServerApplication.GameStages
{

    public sealed class RoundEndStage : IGameStage
    {
        //public readonly static Time Duration = Time.FromSeconds(30);
        public readonly static Time Duration = Time.FromSeconds(5);  // for debug

        private readonly Time _StartTime = Time.Now();

        public readonly TextComponent[] Message0 = [
            new TextComponent($"============SUPERDEK=============\n", TextColor.DarkGray),
            new TextComponent($"\n", TextColor.White),
            new TextComponent($"게임",  TextColor.Red),
            new TextComponent($"에 대한 자세한 정보는 게임 패널",  TextColor.White),
            new TextComponent($"에서 확인하실 수 있습니다. \n", TextColor.White),
            new TextComponent($"해당 창은 게임 패널 ", TextColor.White),
            new TextComponent($"아이템", TextColor.Purple),
            new TextComponent($"을 우클릭하여 ", TextColor.White),
            new TextComponent($"열람 가능합니다. \n", TextColor.White),
            new TextComponent($"\n", TextColor.White),
            new TextComponent($"게임의 중요한 정보를 놓치지 마세요! \n", TextColor.Pink),
            new TextComponent($"\n", TextColor.White),
            new TextComponent($"=================================", TextColor.DarkGray),
        ];
        private bool printMessage0 = false;

        public RoundEndStage()
        {

        }

        public IGameStage CreateNextNode(GameContext ctx)
        {
            System.Diagnostics.Debug.Assert(ctx != null);

            if (ctx.IsFinalRound == true)
            {
                return new WinnerStage();
            }
            else
            {
                return new RoundStartStage();
            }

        }

        public bool StartRoutine(GameContext ctx, SuperWorld world)
        {
            System.Diagnostics.Debug.Assert(ctx != null);
            System.Diagnostics.Debug.Assert(world != null);

            Time elapsedTime = Time.Now() - _StartTime;

            if (elapsedTime < Duration)
            {
                if (ctx.IsBeforeFirstRound == true && printMessage0 == false)
                {
                    world.WriteMessageInChatBox(Message0);

                    printMessage0 = true;
                }
            }
            else
            {
                ctx.EndRound();

                world.WriteMessageInChatBox([
                    new TextComponent("라운드 종료", TextColor.Gray),
                    ]);

                return true;
            }

            return false;
        }
    }

}



using Common;
using Containers;

using MinecraftServerEngine;
using MinecraftServerEngine.ProgressBars;
using MinecraftServerEngine.Text;

namespace TestMinecraftServerApplication.GameStages
{
    // The seeker closes their eyes and counts to a number.
    public sealed class SeekerCountStage : IGameStage
    {
        public readonly static Time Duration = Time.FromSeconds(30);
        //public readonly static Time Duration = Time.FromSeconds(5);  // for debug

        private readonly Time _StartTime = Time.Now();


        public readonly TextComponent[] Message0 = [
            new TextComponent($"============SUPERDEK=============\n", TextColor.DarkGray),
            new TextComponent($"\n", TextColor.White),
            new TextComponent($"쉬프트 키를 누르면 자신이 서 있는 아래 \n",  TextColor.White),
            new TextComponent($"블록", TextColor.White, true, false, true, false, false),
            new TextComponent($"으로 변신합니다.\n", TextColor.White),
            new TextComponent($"자신에게는 여전히 플레이어 형태로 보이지만, \n", TextColor.White),
            new TextComponent($"다른 플레이어에게는 ", TextColor.White),
            new TextComponent($"블록", TextColor.White, true, false, true, false, false),
            new TextComponent($"으로 위장됩니다.\n", TextColor.White),
            new TextComponent($"\n", TextColor.White),
            new TextComponent($"=================================", TextColor.DarkGray),
        ];
        private bool printMessage0 = false;

        public readonly TextComponent[] Message1 = [
            new TextComponent($"============SUPERDEK=============\n", TextColor.DarkGray),
            new TextComponent($"\n", TextColor.White),
            new TextComponent($"상점 아이템",  TextColor.Yellow),
            new TextComponent($"을 우클릭하면 상점을 \n",  TextColor.White),
            new TextComponent($"이용할 수 있습니다.\n", TextColor.White),
            new TextComponent($"게임에서 사용되는 공통 재화는 ", TextColor.White),
            new TextComponent($"코인", TextColor.Gold),
            new TextComponent($"이며, \n", TextColor.White),
            new TextComponent($"이는 아이템 형태로 제공됩니다.\n", TextColor.White),
            new TextComponent($"\n", TextColor.White),
            new TextComponent($"기본 코인:      ${GameContext.DefaultCoinAmount} Coins\n", TextColor.Gray),
            new TextComponent($"\n", TextColor.White),
            new TextComponent($"=================================", TextColor.DarkGray),
        ];
        private bool printMessage1 = false;


        private System.Guid _progressBarId = System.Guid.Empty;

        private bool _init = false;


        public SeekerCountStage()
        {
        }

        public IGameStage CreateNextStage(GameContext ctx)
        {
            System.Diagnostics.Debug.Assert(ctx != null);

            return new FindHidersStage();
        }

        public bool StartRoutine(GameContext ctx, SuperWorld world)
        {
            System.Diagnostics.Debug.Assert(ctx != null);
            System.Diagnostics.Debug.Assert(world != null);

            Time elapsedTime = Time.Now() - _StartTime;

            if (ctx.IsBeforeFirstRound == true && printMessage0 == false && elapsedTime > Time.FromSeconds(2))
            {
                world.WriteMessageInChatBox(Message0);

                printMessage0 = true;
            }

            if (ctx.IsBeforeFirstRound == true && printMessage1 == false && elapsedTime > Time.FromSeconds(4))
            {
                world.WriteMessageInChatBox(Message1);

                printMessage1 = true;
            }


            double progressBar;

            if (_init == false)
            {
                ctx.StartSeekerCount();

                progressBar = 1.0 - elapsedTime.Amount / (double)Duration.Amount;
                System.Diagnostics.Debug.Assert(progressBar >= 0.0);
                System.Diagnostics.Debug.Assert(progressBar <= 1.0);

                System.Diagnostics.Debug.Assert(_progressBarId == System.Guid.Empty);
                _progressBarId = world.OpenProgressBar(
                    [
                        new TextComponent($"[SUPERDEK] 술래카운트 ", TextColor.Gold),
                        new TextComponent($"(술래: {ctx.CurrentSeeker.Username})", TextColor.Red),
                    ],
                    progressBar,
                    ProgressBarColor.Red,
                    ProgressBarDivision.Notches_20);

                _init = true;
            }

            if (elapsedTime < Duration)
            {
                progressBar = 1.0 - elapsedTime.Amount / (double)Duration.Amount;
                System.Diagnostics.Debug.Assert(progressBar >= 0.0);
                System.Diagnostics.Debug.Assert(progressBar <= 1.0);

                System.Diagnostics.Debug.Assert(_progressBarId != System.Guid.Empty);
                world.UpdateProgressBarHealth(
                    _progressBarId,
                    progressBar);
            }
            else
            {
                ctx.EndSeekerCount();

                world.DisplayTitle(
                    Time.Zero, Time.FromSeconds(1), Time.FromSeconds(1),
                    new TextComponent($"주의! 술래가 출발합니다!", TextColor.Red));

                System.Diagnostics.Debug.Assert(_progressBarId != System.Guid.Empty);
                world.CloseProgressBar(_progressBarId);

                ctx.PlaySound("entity.player.levelup", 0, 0.5, 1.0);

                return true;
            }

            return false;
        }
    }

}

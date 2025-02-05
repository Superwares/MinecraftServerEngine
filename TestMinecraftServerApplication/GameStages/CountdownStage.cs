

using Common;
using Containers;

using MinecraftServerEngine;
using MinecraftServerEngine.ProgressBars;
using MinecraftServerEngine.Text;

namespace TestMinecraftServerApplication.GameStages
{
    public sealed class CountdownStage : IGameStage
    {
        private readonly static Time INTERVAL_TIME = Time.FromSeconds(1);

        public readonly TextComponent[] Message = [
            new TextComponent($"============SUPERDEK=============\n", TextColor.DarkGray),
            new TextComponent($"\n", TextColor.White),
            new TextComponent($"**최종 우승 조건** \n", TextColor.Gold),
            new TextComponent($"최종 포인트", TextColor.Yellow, true, false, true, false, false),
            new TextComponent($"가 가장 높은 플레이어가 ", TextColor.White),
            new TextComponent($"우승자", TextColor.Gold),
            new TextComponent($"가 됩니다!\n", TextColor.White),
            new TextComponent($"\n", TextColor.White),
            new TextComponent($"킬      +포인트/코인:    {ScoreboardPlayerRow.PointsPerKill}/{GameContext.KillCoins}\n",  TextColor.Gray),
            new TextComponent($"데스    +포인트:         {ScoreboardPlayerRow.PointsPerDeath}\n", TextColor.Gray),
            new TextComponent($"생존    +포인트/코인:     {ScoreboardPlayerRow.PoinsPerSurviving}/{GameContext.RoundSurvivingCoins}\n", TextColor.Gray),
            new TextComponent($"술래승리 +포인트/코인:    {GameContext.RoundSeekerWinAdditionalPoints}/{GameContext.RoundSeekerWinCoins}\n", TextColor.Gray),
            new TextComponent($"생존승리 +포인트/코인:    {GameContext.RoundHiderWinAdditionalPoints}/{GameContext.RoundHiderWinCoins}\n", TextColor.Gray),
            new TextComponent($"\n", TextColor.White),
            new TextComponent($"* 라운드는 참여한 플레이어 수만큼 진행됩니다.\n", TextColor.Gray),
            new TextComponent($"\n", TextColor.White),
            new TextComponent($"=================================", TextColor.DarkGray),
        ];
        private bool _printMessage = false;


        private Time _prevDisplayTime = Time.Now() - INTERVAL_TIME;
        private int _count = 5;




        public IGameStage CreateNextStage(GameContext ctx)
        {
            System.Diagnostics.Debug.Assert(ctx != null);

             return new GameStartStage();
        }

        public bool StartRoutine(GameContext ctx, SuperWorld world)
        {
            System.Diagnostics.Debug.Assert(ctx != null);
            System.Diagnostics.Debug.Assert(world != null);

            //MyConsole.Debug($"_count: {_count}");

            if (ctx.IsBeforeFirstRound == true && _printMessage == false)
            {
                world.WriteMessageInChatBox(Message);

                _printMessage = true;
            }

            if (_count > 0)
            {
                Time elapsedTime = Time.Now() - _prevDisplayTime;

                if (elapsedTime >= INTERVAL_TIME)
                {
                    // $"The game will start in {_count} seconds!"
                    world.DisplayTitle(
                        Time.Zero, INTERVAL_TIME, Time.FromMilliseconds(500),
                        new TextComponent($"{_count}초 후에 게임을 시작합니다...", TextColor.Red));

                    --_count;

                    _prevDisplayTime = Time.Now();

                    ctx.PlaySound("entity.llama.chest", 0, 0.5, 1.0);
                }

                return false;
            }



            return true;
        }
    }

}

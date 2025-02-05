

using Common;
using Containers;

using MinecraftServerEngine;
using MinecraftServerEngine.ProgressBars;
using MinecraftServerEngine.Text;
using TestMinecraftServerApplication.Configs;

namespace TestMinecraftServerApplication.GameStages
{
    public sealed class FindHidersStage : IGameStage
    {
        public const int DefaultNormalTimeInSeconds = 200;
        public const int DefaultBurningTimeInSeconds = 60;
        public readonly static Time NormalDuration;
        public readonly static Time BurningDuration;

        public readonly static Time CoinGiveInterval = Time.FromSeconds(1);

        //public readonly static Time NormalTimeDuration = Time.FromSeconds(30);  // for debug
        //public readonly static Time BurningTimeDuration = Time.FromSeconds(5);  // for debug

        private readonly Time _StartTime = Time.Now();


        public readonly TextComponent[] Message0 = [
            new TextComponent($"============SUPERDEK=============\n", TextColor.DarkGray),
            new TextComponent($"\n", TextColor.White),
            new TextComponent($"버닝 타임 시작! \n",  TextColor.Red),
            new TextComponent($"\n", TextColor.White),
            new TextComponent($"이제 잘못된 ", TextColor.White),
            new TextComponent($"공격", TextColor.DarkRed),
            new TextComponent($"으로 인해 체력이 감소하지 \n", TextColor.White),
            new TextComponent($"않으며, 술래의 ", TextColor.White),
            new TextComponent($"속도", TextColor.Cyan),
            new TextComponent($"가 더욱 빨라집니다! \n", TextColor.White),
            new TextComponent($"\n", TextColor.White),
            new TextComponent($"=================================", TextColor.DarkGray),
        ];
        private bool printMessage0 = false;


        private System.Guid _progressBarId = System.Guid.Empty;

        private bool _initNormal = false, _initBurning = false;


        private Time _lastCoinGiveTime = Time.Now();

        static FindHidersStage() 
        {
            int normalTimeInSeconds;
            int burningTimeInSeconds;

            IConfigGameRound config = ConfigXml.Config.Game?.Round;

            if (config == null)
            {
                MyConsole.Warn($"Config.Game.Round is null. Using defaults: " +
                    $"Config.Game.Round.NormalTimeInSeconds={DefaultNormalTimeInSeconds}, " +
                    $"Config.Game.Round.BurningTimeInSeconds={DefaultBurningTimeInSeconds}");

                System.Diagnostics.Debug.Assert(DefaultNormalTimeInSeconds > 0);
                System.Diagnostics.Debug.Assert(DefaultBurningTimeInSeconds > 0);
                config = new ConfigGameRound()
                {
                    NormalTimeInSeconds = DefaultNormalTimeInSeconds,

                    BurningTimeInSeconds = DefaultBurningTimeInSeconds,

                };
            }

            normalTimeInSeconds = config.NormalTimeInSeconds;
            burningTimeInSeconds = config.BurningTimeInSeconds;

            if (config.NormalTimeInSeconds <= 0)
            {
                MyConsole.Warn($"Config.Game.Round.NormalTimeInSeconds <= 0. Using defaults: " +
                    $"Config.Game.Round.NormalTimeInSeconds={DefaultNormalTimeInSeconds}");

                System.Diagnostics.Debug.Assert(DefaultNormalTimeInSeconds > 0);
                normalTimeInSeconds = DefaultNormalTimeInSeconds;
            }

            if (config.BurningTimeInSeconds <= 0)
            {
                MyConsole.Warn($"Config.Game.Round.BurningTimeInSeconds <= 0. Using defaults: " +
                    $"Config.Game.Round.BurningTimeInSeconds={DefaultBurningTimeInSeconds}");


                System.Diagnostics.Debug.Assert(DefaultBurningTimeInSeconds > 0);
                burningTimeInSeconds = DefaultBurningTimeInSeconds;
            }

            NormalDuration = Time.FromSeconds(normalTimeInSeconds);
            BurningDuration = Time.FromSeconds(burningTimeInSeconds);
        }

        public FindHidersStage()
        {
        }

        public IGameStage CreateNextStage(GameContext ctx)
        {
            System.Diagnostics.Debug.Assert(ctx != null);

            return new RoundEndStage();
        }

        public bool StartRoutine(GameContext ctx, SuperWorld world)
        {
            System.Diagnostics.Debug.Assert(ctx != null);
            System.Diagnostics.Debug.Assert(world != null);

            if (ctx.IsHiderWin == true)
            {
                if (_progressBarId != System.Guid.Empty)
                {
                    System.Diagnostics.Debug.Assert(_progressBarId != System.Guid.Empty);
                    System.Diagnostics.Debug.Assert(world != null);
                    world.CloseProgressBar(_progressBarId);
                }

                System.Diagnostics.Debug.Assert(world != null);
                world.DisplayTitle(
                    Time.Zero, Time.FromSeconds(1), Time.Zero,
                    new TextComponent($"술래가 사망하였습니다!", TextColor.BrightGreen));

                ctx.PlaySound("entity.player.levelup", 0, 0.5, 1.0);
                ctx.PlaySound("entity.illusion_illager.ambient", 0, 0.5, 1.0);

                //world.WriteMessageInChatBox([
                //    new TextComponent($"생존승리! (+{GameContext.HIDER_ROUND_WIN_ADDITIONAL_POINTS}포인트, +{GameContext.HIDER_ROUND_WIN_COINS}코인)", TextColor.Gray),
                //    ]);

                return true;
            }

            if (ctx.IsSeekerWin == true)
            {
                if (_progressBarId != System.Guid.Empty)
                {
                    System.Diagnostics.Debug.Assert(_progressBarId != System.Guid.Empty);
                    System.Diagnostics.Debug.Assert(world != null);
                    world.CloseProgressBar(_progressBarId);
                }

                System.Diagnostics.Debug.Assert(world != null);
                world.DisplayTitle(
                    Time.Zero, Time.FromSeconds(1), Time.Zero,
                    new TextComponent($"도망자 모두가 사망하였습니다!", TextColor.BrightGreen));

                ctx.PlaySound("entity.player.levelup", 0, 0.5, 1.0);
                ctx.PlaySound("entity.illusion_illager.ambient", 0, 0.5, 1.0);

                //world.WriteMessageInChatBox([
                //    new TextComponent($"술래승리! (+{GameContext.SEEKER_ROUND_WIN_ADDITIONAL_POINTS}포인트, +{GameContext.SEEKER_ROUND_WIN_COINS}코인)", TextColor.Gray),
                //    ]);

                return true;
            }

            {
                Time elapsedGiveCoinTime = Time.Now() - _lastCoinGiveTime;
                if (elapsedGiveCoinTime > CoinGiveInterval)
                {
                    System.Diagnostics.Debug.Assert(ctx != null);
                    ctx.GiveCoins();

                    _lastCoinGiveTime = Time.Now();
                }
            }

            Time elapsedTime = Time.Now() - _StartTime;
            double progressBar;

            if (elapsedTime < NormalDuration)
            {
                progressBar = 1.0 - elapsedTime.Amount / (double)NormalDuration.Amount;
                System.Diagnostics.Debug.Assert(progressBar >= 0.0);
                System.Diagnostics.Debug.Assert(progressBar <= 1.0);


                if (_initNormal == false)
                {
                    System.Diagnostics.Debug.Assert(_progressBarId == System.Guid.Empty);
                    _progressBarId = world.OpenProgressBar(
                        [
                        new TextComponent($"[SUPERDEK] 도망치기 ", TextColor.Gold),
                        new TextComponent($"(술래: {ctx.CurrentSeeker.Username})", TextColor.Red),
                        ],
                        progressBar,
                        ProgressBarColor.Red,
                        ProgressBarDivision.Notches_20);

                    ctx.PlaySound("entity.player.levelup", 0, 0.5, 1.0);

                    _initNormal = true;
                }
                else
                {
                    System.Diagnostics.Debug.Assert(_progressBarId != System.Guid.Empty);
                    world.UpdateProgressBarHealth(
                        _progressBarId,
                        progressBar);
                }

            }
            else if (elapsedTime - NormalDuration < BurningDuration)
            {
                if (ctx.IsBeforeFirstRound == true && printMessage0 == false)
                {
                    world.WriteMessageInChatBox(Message0);

                    printMessage0 = true;
                }

                progressBar = 1.0 - (elapsedTime - NormalDuration).Amount / (double)BurningDuration.Amount;
                System.Diagnostics.Debug.Assert(progressBar >= 0.0);
                System.Diagnostics.Debug.Assert(progressBar <= 1.0);

                if (_initBurning == false)
                {
                    System.Diagnostics.Debug.Assert(_progressBarId != System.Guid.Empty);
                    world.CloseProgressBar(_progressBarId);

                    _progressBarId = world.OpenProgressBar(
                        [
                        new TextComponent($"[SUPERDEK] ", TextColor.Gold),
                        new TextComponent($"Burning Time! ", TextColor.Yellow),
                        new TextComponent($"(술래: {ctx.CurrentSeeker.Username})", TextColor.Red),
                        ],
                        progressBar,
                        ProgressBarColor.Yellow,
                        ProgressBarDivision.Notches_20);

                    world.DisplayTitle(
                        Time.Zero, Time.FromSeconds(1), Time.Zero,
                        new TextComponent($"Burning Time!", TextColor.Gold));

                    ctx.StartBuringTime();

                    ctx.PlaySound("entity.player.levelup", 0, 0.5, 1.0);

                    _initBurning = true;
                }
                else
                {
                    System.Diagnostics.Debug.Assert(_progressBarId != System.Guid.Empty);
                    world.UpdateProgressBarHealth(
                        _progressBarId,
                        progressBar);
                }


            }
            else
            {
                world.DisplayTitle(
                    Time.Zero, Time.FromSeconds(1), Time.FromSeconds(1),
                    new TextComponent($"종료!", TextColor.Gold));

                System.Diagnostics.Debug.Assert(_progressBarId != System.Guid.Empty);
                world.CloseProgressBar(_progressBarId);

                ctx.PlaySound("entity.player.levelup", 0, 0.5, 1.0);

                return true;
            }

            return false;
        }
    }

}

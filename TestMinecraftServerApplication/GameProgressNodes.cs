

using Common;
using Containers;

using MinecraftServerEngine;
using MinecraftPrimitives;

namespace TestMinecraftServerApplication
{
    public interface IGameProgressNode
    {
        public IGameProgressNode CreateNextNode(GameContext ctx);

        public bool StartRoutine(GameContext ctx, SuperWorld world);

    }

    public sealed class LobbyNode : IGameProgressNode
    {
        public IGameProgressNode CreateNextNode(GameContext ctx)
        {
            System.Diagnostics.Debug.Assert(ctx != null);

            return new CountdownNode();
        }

        public bool StartRoutine(GameContext ctx, SuperWorld world)
        {
            System.Diagnostics.Debug.Assert(ctx != null);
            System.Diagnostics.Debug.Assert(world != null);

            return ctx.IsReady == true;
        }
    }

    public sealed class CountdownNode : IGameProgressNode
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
            new TextComponent($"킬 당 포인트:       {ScoreboardPlayerRow.PointsPerKill}\n",  TextColor.Gray),
            new TextComponent($"데스 당 포인트:     {ScoreboardPlayerRow.PointsPerDeath}\n", TextColor.Gray),
            new TextComponent($"\n", TextColor.White),
            new TextComponent($"* 라운드는 참여한 플레이어 수만큼 진행됩니다.\n", TextColor.Gray),
            new TextComponent($"\n", TextColor.White),
            new TextComponent($"=================================", TextColor.DarkGray),
        ];
        private bool _printMessage = false;


        private Time _prevDisplayTime = Time.Now() - INTERVAL_TIME;
        private int _count = 5;




        public IGameProgressNode CreateNextNode(GameContext ctx)
        {
            System.Diagnostics.Debug.Assert(ctx != null);

            return new GameStartNode();
        }

        public bool StartRoutine(GameContext ctx, SuperWorld world)
        {
            System.Diagnostics.Debug.Assert(ctx != null);
            System.Diagnostics.Debug.Assert(world != null);

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

                    ctx.PlaySound("entity.llama.chest", 0, 1.0, 1.5);
                }

                return false;
            }



            return true;
        }
    }

    public sealed class GameStartNode : IGameProgressNode
    {
        public GameStartNode()
        {
        }

        public IGameProgressNode CreateNextNode(GameContext ctx)
        {
            System.Diagnostics.Debug.Assert(ctx != null);

            return new RoundStartNode();
        }

        public bool StartRoutine(GameContext ctx, SuperWorld world)
        {
            System.Diagnostics.Debug.Assert(ctx != null);
            System.Diagnostics.Debug.Assert(world != null);

            ctx.Start();

            return true;
        }
    }

    public sealed class RoundStartNode : IGameProgressNode
    {

        public RoundStartNode()
        {
        }

        public IGameProgressNode CreateNextNode(GameContext ctx)
        {
            System.Diagnostics.Debug.Assert(ctx != null);

            List<int> nonSeekerIndexList = new List<int>();
            int length = ctx.MakeNonSeekerIndexList(nonSeekerIndexList);

            return new RandomSeekerNode(nonSeekerIndexList);
        }

        public bool StartRoutine(GameContext ctx, SuperWorld world)
        {
            System.Diagnostics.Debug.Assert(ctx != null);
            System.Diagnostics.Debug.Assert(world != null);

            ctx.StartRound();

            world.ChangeWorldTimeOfDay(SuperWorld.DefaultWorldTime, Time.Zero);
            world.ChangeWorldBorderSize(SuperWorld.DefaultWorldBorderRadiusInMeters, Time.Zero);

            return true;
        }
    }

    public sealed class RandomSeekerNode : IGameProgressNode
    {
        private readonly System.Random _Random = new();

        private readonly List<int> _NonSeekerIndexList;

        public readonly TextComponent[] Message = [
            new TextComponent($"============SUPERDEK=============\n", TextColor.DarkGray),
            new TextComponent($"\n", TextColor.White),
            new TextComponent($"모든 플레이어는 무작위로 술래로 지정됩니다.\n",  TextColor.White),
            new TextComponent($"단, 한 번 술래가 된 플레이어는 다음 라운드\n", TextColor.White),
            new TextComponent($"에서는 술래로 선정되지 않습니다.\n", TextColor.White),
            new TextComponent($"\n", TextColor.White),
            new TextComponent($"=================================", TextColor.DarkGray),
        ];
        private bool printMessage = false;

        private Time _intervalTime = Time.FromMilliseconds(250);
        private Time _time;

        private int _repeat = 3;
        private int i = 0;


        public RandomSeekerNode(List<int> nonSeekerIndexList)
        {
            System.Diagnostics.Debug.Assert(nonSeekerIndexList != null);
            _NonSeekerIndexList = nonSeekerIndexList;

            _intervalTime /= nonSeekerIndexList.Length;
            _time = Time.Now() - _intervalTime;

        }

        public IGameProgressNode CreateNextNode(GameContext ctx)
        {
            System.Diagnostics.Debug.Assert(ctx != null);

            System.Diagnostics.Debug.Assert(_NonSeekerIndexList != null);
            _NonSeekerIndexList.Flush();
            _NonSeekerIndexList.Dispose();

            return new SeekerCountNode();
        }

        public bool StartRoutine(GameContext ctx, SuperWorld world)
        {
            System.Diagnostics.Debug.Assert(ctx != null);
            System.Diagnostics.Debug.Assert(world != null);

            if (ctx.IsBeforeFirstRound == true && printMessage == false)
            {
                world.WriteMessageInChatBox(Message);

                printMessage = true;
            }

            System.Diagnostics.Debug.Assert(ctx.Players != null);
            IReadOnlyList<SuperPlayer> players = ctx.Players;

            if (_NonSeekerIndexList.Length == 1)
            {
                int j = _NonSeekerIndexList[0];

                SuperPlayer seeker = players[j];

                world.DisplayTitle(
                    Time.Zero, Time.FromSeconds(1), Time.FromSeconds(1),
                    new TextComponent($"{seeker.Username}", TextColor.DarkGreen));

                ctx.SeletSeeker(seeker);

                ctx.PlaySound("entity.player.levelup", 0, 1.0, 1.5);

                return true;
            }

            if (i >= _NonSeekerIndexList.Length * _repeat)
            {
                int j = _Random.Next(_NonSeekerIndexList.Length);
                int k = _NonSeekerIndexList[j];

                _NonSeekerIndexList.Extract(_j => _j == j, -1);

                //SuperPlayer player = players[k];

                _intervalTime += Time.FromMilliseconds(100);

                //_repeat += 1;
                i = 0;

                ctx.PlaySound("entity.item.pickup", 0, 1.0, 1.5);
            }
            else
            {
                Time time = Time.Now() - _time;

                if (time > _intervalTime)
                {
                    int j = i % _NonSeekerIndexList.Length;
                    int k = _NonSeekerIndexList[j];

                    //MyConsole.Debug($"j: {j}");

                    SuperPlayer player = players[k];

                    world.DisplayTitle(
                        Time.Zero, _intervalTime, Time.Zero,
                        new TextComponent($"{player.Username}", TextColor.Gray));

                    ++i;

                    _time = Time.Now();

                    ctx.PlaySound("entity.item.pickup", 0, 1.0, 1.5);
                }

            }

            return false;
        }
    }

    // The seeker closes their eyes and counts to a number.
    public sealed class SeekerCountNode : IGameProgressNode
    {
        //public readonly static Time Duration = Time.FromSeconds(30);
        public readonly static Time Duration = Time.FromSeconds(5);  // for debug

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


        public SeekerCountNode()
        {
        }

        public IGameProgressNode CreateNextNode(GameContext ctx)
        {
            System.Diagnostics.Debug.Assert(ctx != null);

            return new FindHidersNode();
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

                progressBar = 1.0 - ((double)elapsedTime.Amount / (double)Duration.Amount);
                System.Diagnostics.Debug.Assert(progressBar >= 0.0);
                System.Diagnostics.Debug.Assert(progressBar <= 1.0);

                System.Diagnostics.Debug.Assert(_progressBarId == System.Guid.Empty);
                _progressBarId = world.OpenProgressBar(
                    [
                        new TextComponent($"[SUPERDEK] 술래카운트 ", TextColor.Gold),
                        new TextComponent($"(술래: {ctx.CurrentSeeker.Username})", TextColor.Red),
                    ],
                    progressBar,
                    BossBarColor.Red,
                    BossBarDivision.Notches_20);

                _init = true;
            }

            if (elapsedTime < Duration)
            {
                progressBar = 1.0 - ((double)elapsedTime.Amount / (double)Duration.Amount);
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

                ctx.PlaySound("entity.player.levelup", 0, 1.0, 1.5);

                return true;
            }

            return false;
        }
    }

    public sealed class FindHidersNode : IGameProgressNode
    {
        //public readonly static Time NormalTimeDuration = Time.FromMinutes(2);
        //public readonly static Time BurningTimeDuration = Time.FromMinutes(1);

        public readonly static Time NormalTimeDuration = Time.FromSeconds(5);  // for debug
        public readonly static Time BurningTimeDuration = Time.FromSeconds(5);  // for debug

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

        public FindHidersNode()
        {
        }

        public IGameProgressNode CreateNextNode(GameContext ctx)
        {
            System.Diagnostics.Debug.Assert(ctx != null);

            return new RoundEndNode();
        }

        public bool StartRoutine(GameContext ctx, SuperWorld world)
        {
            System.Diagnostics.Debug.Assert(ctx != null);
            System.Diagnostics.Debug.Assert(world != null);

            if (ctx.IsSeekerDeath == true)
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
                    new TextComponent($"술래가 사망하였습니다...", TextColor.DarkGray));

                ctx.PlaySound("entity.illusion_illager.ambient", 0, 1.0, 1.5);

                return true;
            }

            Time elapsedTime = Time.Now() - _StartTime;
            double progressBar;

            if (elapsedTime < NormalTimeDuration)
            {
                progressBar = 1.0 - ((double)elapsedTime.Amount / (double)NormalTimeDuration.Amount);
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
                        BossBarColor.Red,
                        BossBarDivision.Notches_20);

                    ctx.PlaySound("entity.player.levelup", 0, 1.0, 1.5);

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
            else if ((elapsedTime - NormalTimeDuration) < BurningTimeDuration)
            {
                if (ctx.IsBeforeFirstRound == true && printMessage0 == false)
                {
                    world.WriteMessageInChatBox(Message0);

                    printMessage0 = true;
                }

                progressBar = 1.0 - ((double)(elapsedTime - NormalTimeDuration).Amount / (double)BurningTimeDuration.Amount);
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
                        BossBarColor.Yellow,
                        BossBarDivision.Notches_20);

                    world.DisplayTitle(
                        Time.Zero, Time.FromSeconds(1), Time.Zero,
                        new TextComponent($"Burning Time!", TextColor.Gold));

                    ctx.StartBuringTime();

                    ctx.PlaySound("entity.player.levelup", 0, 1.0, 1.5);

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

                ctx.PlaySound("entity.player.levelup", 0, 1.0, 1.5);

                return true;
            }

            return false;
        }
    }

    public sealed class RoundEndNode : IGameProgressNode
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

        public RoundEndNode()
        {

        }

        public IGameProgressNode CreateNextNode(GameContext ctx)
        {
            System.Diagnostics.Debug.Assert(ctx != null);

            if (ctx.IsFinalRound == true)
            {
                return new WinnerNode();
            }
            else
            {
                return new RoundStartNode();
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


                return true;
            }

            return false;
        }
    }

    public sealed class WinnerNode : IGameProgressNode
    {
        public readonly static Time WinnerDisplayDuration = Time.FromSeconds(5);
        public readonly static Time FireworkLaunchTime_0 = Time.FromMilliseconds(100);
        public readonly static Time FireworkLaunchTime_1 = Time.FromMilliseconds(310);
        public readonly static Time FireworkLaunchTime_2 = Time.FromMilliseconds(790);
        public readonly static Time FireworkLaunchTime_3 = Time.FromMilliseconds(950);
        public readonly static Time FireworkLaunchTime_4 = Time.FromMilliseconds(1140);
        public readonly static Time FireworkLaunchTime_5 = Time.FromMilliseconds(1590);
        public readonly static Time FireworkLaunchTime_6 = Time.FromMilliseconds(1890);
        public readonly static Time FireworkLaunchTime_7 = Time.FromMilliseconds(2190);
        public readonly static Time FireworkLaunchTime_8 = Time.FromMilliseconds(2590);
        public readonly static Time FireworkLaunchTime_9 = Time.FromMilliseconds(3310);

        public readonly static Time FireworkBlastTime_0 = Time.FromMilliseconds(610);
        public readonly static Time FireworkBlastTime_1 = Time.FromMilliseconds(1040);
        public readonly static Time FireworkBlastTime_2 = Time.FromMilliseconds(1370);
        public readonly static Time FireworkBlastTime_3 = Time.FromMilliseconds(1610);
        public readonly static Time FireworkBlastTime_4 = Time.FromMilliseconds(1820);
        public readonly static Time FireworkBlastTime_5 = Time.FromMilliseconds(2390);
        public readonly static Time FireworkBlastTime_6 = Time.FromMilliseconds(2790);
        public readonly static Time FireworkBlastTime_7 = Time.FromMilliseconds(3110);
        public readonly static Time FireworkBlastTime_8 = Time.FromMilliseconds(3500);
        public readonly static Time FireworkBlastTime_9 = Time.FromMilliseconds(3720);

        public readonly static Time FireworkTwinkleTime_0 = Time.FromMilliseconds(740);
        public readonly static Time FireworkTwinkleTime_1 = Time.FromMilliseconds(1140);
        public readonly static Time FireworkTwinkleTime_2 = Time.FromMilliseconds(1570);
        public readonly static Time FireworkTwinkleTime_3 = Time.FromMilliseconds(1710);
        public readonly static Time FireworkTwinkleTime_4 = Time.FromMilliseconds(1920);
        public readonly static Time FireworkTwinkleTime_5 = Time.FromMilliseconds(2590);
        public readonly static Time FireworkTwinkleTime_6 = Time.FromMilliseconds(2900);
        public readonly static Time FireworkTwinkleTime_7 = Time.FromMilliseconds(3210);
        public readonly static Time FireworkTwinkleTime_8 = Time.FromMilliseconds(3600);
        public readonly static Time FireworkTwinkleTime_9 = Time.FromMilliseconds(3820);

        private readonly System.Random _Random = new();

        private Time _startTime = Time.Now();


        private readonly List<SuperPlayer> _Winners = new();
        private bool _displayWinner = false;
        private SuperPlayer _winner = null;

        private bool _init = false;


        private Time _intervalTime = Time.FromMilliseconds(250);
        private Time _time;

        private int _repeat = 3;
        private int i = 0;


        private bool _fireworkLaunch_0 = false;
        private bool _fireworkLaunch_1 = false;
        private bool _fireworkLaunch_2 = false;
        private bool _fireworkLaunch_3 = false;
        private bool _fireworkLaunch_4 = false;
        private bool _fireworkLaunch_5 = false;
        private bool _fireworkLaunch_6 = false;
        private bool _fireworkLaunch_7 = false;
        private bool _fireworkLaunch_8 = false;
        private bool _fireworkLaunch_9 = false;

        private bool _fireworkBlast_0 = false;
        private bool _fireworkBlast_1 = false;
        private bool _fireworkBlast_2 = false;
        private bool _fireworkBlast_3 = false;
        private bool _fireworkBlast_4 = false;
        private bool _fireworkBlast_5 = false;
        private bool _fireworkBlast_6 = false;
        private bool _fireworkBlast_7 = false;
        private bool _fireworkBlast_8 = false;
        private bool _fireworkBlast_9 = false;

        private bool _fireworkTwinkle_0 = false;
        private bool _fireworkTwinkle_1 = false;
        private bool _fireworkTwinkle_2 = false;
        private bool _fireworkTwinkle_3 = false;
        private bool _fireworkTwinkle_4 = false;
        private bool _fireworkTwinkle_5 = false;
        private bool _fireworkTwinkle_6 = false;
        private bool _fireworkTwinkle_7 = false;
        private bool _fireworkTwinkle_8 = false;
        private bool _fireworkTwinkle_9 = false;


        public IGameProgressNode CreateNextNode(GameContext ctx)
        {
            System.Diagnostics.Debug.Assert(ctx != null);

            _Winners.Dispose();

            return new GameEndNode();
        }

        public bool StartRoutine(GameContext ctx, SuperWorld world)
        {
            System.Diagnostics.Debug.Assert(ctx != null);
            System.Diagnostics.Debug.Assert(world != null);

            Time elapsedTime = Time.Now() - _startTime;

            if (_winner != null)
            {
                if (_displayWinner == false)
                {
                    System.Diagnostics.Debug.Assert(_winner != null);
                    world.DisplayTitle(
                        Time.Zero, Time.FromSeconds(5), Time.FromSeconds(5),
                        new TextComponent($"! {_winner.Username} !", TextColor.BrightGreen));

                    ctx.PlaySound("entity.player.levelup", 0, 1.0, 1.5);

                    _displayWinner = true;
                }

                if (_fireworkLaunch_0 == false && elapsedTime > FireworkLaunchTime_0)
                {
                    _fireworkLaunch_0 = true;

                    ctx.PlaySound("entity.firework.launch", 0, 1.0, 2.0);
                }
                else if (_fireworkLaunch_1 == false && elapsedTime > FireworkLaunchTime_1)
                {
                    _fireworkLaunch_1 = true;

                    ctx.PlaySound("entity.firework.launch", 0, 1.0, 2.0);
                }
                else if (_fireworkLaunch_2 == false && elapsedTime > FireworkLaunchTime_2)
                {
                    _fireworkLaunch_2 = true;

                    ctx.PlaySound("entity.firework.launch", 0, 1.0, 2.0);
                }
                else if (_fireworkLaunch_3 == false && elapsedTime > FireworkLaunchTime_3)
                {
                    _fireworkLaunch_3 = true;

                    ctx.PlaySound("entity.firework.launch", 0, 1.0, 2.0);
                    ctx.PlaySound("entity.firework.blast", 0, 1.0, 2.0);
                }
                else if (_fireworkLaunch_4 == false && elapsedTime > FireworkLaunchTime_4)
                {
                    _fireworkLaunch_4 = true;

                    ctx.PlaySound("entity.firework.launch", 0, 1.0, 2.0);
                    ctx.PlaySound("entity.firework.blast_far", 0, 1.0, 2.0);
                }
                else if (_fireworkLaunch_5 == false && elapsedTime > FireworkLaunchTime_5)
                {
                    _fireworkLaunch_5 = true;

                    ctx.PlaySound("entity.firework.launch", 0, 1.0, 2.0);
                    ctx.PlaySound("entity.firework.blast", 0, 1.0, 2.0);
                }
                else if (_fireworkLaunch_6 == false && elapsedTime > FireworkLaunchTime_6)
                {
                    _fireworkLaunch_6 = true;

                    ctx.PlaySound("entity.firework.launch", 0, 1.0, 2.0);
                    ctx.PlaySound("entity.firework.large_blast", 0, 1.0, 2.0);
                }
                else if (_fireworkLaunch_7 == false && elapsedTime > FireworkLaunchTime_7)
                {
                    _fireworkLaunch_7 = true;

                    ctx.PlaySound("entity.firework.launch", 0, 1.0, 2.0);
                    ctx.PlaySound("entity.firework.blast", 0, 1.0, 2.0);
                }
                else if (_fireworkLaunch_8 == false && elapsedTime > FireworkLaunchTime_8)
                {
                    _fireworkLaunch_8 = true;

                    ctx.PlaySound("entity.firework.launch", 0, 1.0, 2.0);
                    ctx.PlaySound("entity.firework.large_blast", 0, 1.0, 2.0);
                }
                else if (_fireworkLaunch_9 == false && elapsedTime > FireworkLaunchTime_9)
                {
                    _fireworkLaunch_9 = true;

                    ctx.PlaySound("entity.firework.launch", 0, 1.0, 2.0);
                    ctx.PlaySound("entity.firework.large_blast", 0, 1.0, 2.0);
                }

                if (_fireworkBlast_0 == false && elapsedTime > FireworkBlastTime_0)
                {
                    _fireworkBlast_0 = true;

                    ctx.PlaySound("entity.firework.launch", 0, 1.0, 2.0);
                    ctx.PlaySound("entity.firework.large_blast_far", 0, 1.0, 2.0);
                }
                else if (_fireworkBlast_1 == false && elapsedTime > FireworkBlastTime_1)
                {
                    _fireworkBlast_1 = true;

                    ctx.PlaySound("entity.firework.launch", 0, 1.0, 2.0);
                    ctx.PlaySound("entity.firework.blast_far", 0, 1.0, 2.0);
                }
                else if (_fireworkBlast_2 == false && elapsedTime > FireworkBlastTime_2)
                {
                    _fireworkBlast_2 = true;

                    ctx.PlaySound("entity.firework.blast", 0, 1.0, 2.0);
                    ctx.PlaySound("entity.firework.large_blast_far", 0, 1.0, 2.0);
                }
                else if (_fireworkBlast_3 == false && elapsedTime > FireworkBlastTime_3)
                {
                    _fireworkBlast_3 = true;

                    ctx.PlaySound("entity.firework.launch", 0, 1.0, 2.0);
                    ctx.PlaySound("entity.firework.blast", 0, 1.0, 2.0);
                }
                else if (_fireworkBlast_4 == false && elapsedTime > FireworkBlastTime_4)
                {
                    _fireworkBlast_4 = true;

                    ctx.PlaySound("entity.firework.launch", 0, 1.0, 2.0);
                    //ctx.PlaySound("entity.firework.blast", 0, 1.0, 2.0);
                    ctx.PlaySound("entity.firework.blast_far", 0, 1.0, 2.0);
                }
                else if (_fireworkBlast_5 == false && elapsedTime > FireworkBlastTime_5)
                {
                    _fireworkBlast_5 = true;

                    ctx.PlaySound("entity.firework.launch", 0, 1.0, 2.0);
                    //ctx.PlaySound("entity.firework.blast", 0, 1.0, 2.0);
                    ctx.PlaySound("entity.firework.large_blast_far", 0, 1.0, 2.0);
                }
                else if (_fireworkBlast_6 == false && elapsedTime > FireworkBlastTime_6)
                {
                    _fireworkBlast_6 = true;

                    ctx.PlaySound("entity.firework.launch", 0, 1.0, 2.0);
                    ctx.PlaySound("entity.firework.blast_far", 0, 1.0, 2.0);
                    //ctx.PlaySound("entity.firework.large_blast", 0, 1.0, 2.0);
                }
                else if (_fireworkBlast_7 == false && elapsedTime > FireworkBlastTime_7)
                {
                    _fireworkBlast_7 = true;

                    ctx.PlaySound("entity.firework.blast", 0, 1.0, 2.0);
                    ctx.PlaySound("entity.firework.large_blast_far", 0, 1.0, 2.0);
                    //ctx.PlaySound("entity.firework.blast_far", 0, 1.0, 2.0);
                }
                else if (_fireworkBlast_8 == false && elapsedTime > FireworkBlastTime_8)
                {
                    _fireworkBlast_8 = true;

                    //ctx.PlaySound("entity.firework.large_blast", 0, 1.0, 2.0);
                    ctx.PlaySound("entity.firework.blast_far", 0, 1.0, 2.0);
                }
                else if (_fireworkBlast_9 == false && elapsedTime > FireworkBlastTime_9)
                {
                    _fireworkBlast_9 = true;

                    ctx.PlaySound("entity.firework.large_blast", 0, 1.0, 2.0);
                    //ctx.PlaySound("entity.firework.blast_far", 0, 1.0, 2.0);
                }

                if (_fireworkTwinkle_0 == false && elapsedTime > FireworkTwinkleTime_0)
                {
                    _fireworkTwinkle_0 = true;

                    ctx.PlaySound("entity.firework.twinkle", 0, 1.0, 2.0);
                    ctx.PlaySound("entity.firework.large_blast_far", 0, 1.0, 2.0);
                }
                else if (_fireworkTwinkle_1 == false && elapsedTime > FireworkTwinkleTime_1)
                {
                    _fireworkTwinkle_1 = true;

                    ctx.PlaySound("entity.firework.twinkle", 0, 1.0, 2.0);
                    ctx.PlaySound("entity.firework.twinkle_far", 0, 1.0, 2.0);
                    ctx.PlaySound("entity.firework.blast_far", 0, 1.0, 2.0);
                }
                else if (_fireworkTwinkle_2 == false && elapsedTime > FireworkTwinkleTime_2)
                {
                    _fireworkTwinkle_2 = true;

                    ctx.PlaySound("entity.firework.blast", 0, 1.0, 2.0);
                    ctx.PlaySound("entity.firework.twinkle", 0, 1.0, 2.0);
                    ctx.PlaySound("entity.firework.large_blast_far", 0, 1.0, 2.0);
                }
                else if (_fireworkTwinkle_3 == false && elapsedTime > FireworkTwinkleTime_3)
                {
                    _fireworkTwinkle_3 = true;

                    ctx.PlaySound("entity.firework.twinkle", 0, 1.0, 2.0);
                    ctx.PlaySound("entity.firework.blast", 0, 1.0, 2.0);
                }
                else if (_fireworkTwinkle_4 == false && elapsedTime > FireworkTwinkleTime_4)
                {
                    _fireworkTwinkle_4 = true;

                    ctx.PlaySound("entity.firework.blast", 0, 1.0, 2.0);
                    ctx.PlaySound("entity.firework.twinkle_far", 0, 1.0, 2.0);
                    ctx.PlaySound("entity.firework.blast_far", 0, 1.0, 2.0);
                }
                else if (_fireworkTwinkle_5 == false && elapsedTime > FireworkTwinkleTime_5)
                {
                    _fireworkTwinkle_5 = true;

                    ctx.PlaySound("entity.firework.twinkle", 0, 1.0, 2.0);
                    ctx.PlaySound("entity.firework.twinkle_far", 0, 1.0, 2.0);
                    ctx.PlaySound("entity.firework.blast", 0, 1.0, 2.0);
                    ctx.PlaySound("entity.firework.large_blast_far", 0, 1.0, 2.0);
                }
                else if (_fireworkTwinkle_6 == false && elapsedTime > FireworkTwinkleTime_6)
                {
                    _fireworkTwinkle_6 = true;

                    ctx.PlaySound("entity.firework.twinkle_far", 0, 1.0, 2.0);
                    ctx.PlaySound("entity.firework.blast_far", 0, 1.0, 2.0);
                    ctx.PlaySound("entity.firework.large_blast", 0, 1.0, 2.0);
                }
                else if (_fireworkTwinkle_7 == false && elapsedTime > FireworkTwinkleTime_7)
                {
                    _fireworkTwinkle_7 = true;

                    ctx.PlaySound("entity.firework.twinkle_far", 0, 1.0, 2.0);
                    ctx.PlaySound("entity.firework.large_blast_far", 0, 1.0, 2.0);
                    ctx.PlaySound("entity.firework.blast_far", 0, 1.0, 2.0);
                }
                else if (_fireworkTwinkle_8 == false && elapsedTime > FireworkTwinkleTime_8)
                {
                    _fireworkTwinkle_8 = true;

                    ctx.PlaySound("entity.firework.large_blast", 0, 1.0, 2.0);
                    ctx.PlaySound("entity.firework.blast_far", 0, 1.0, 2.0);
                    ctx.PlaySound("entity.firework.twinkle", 0, 1.0, 2.0);
                    ctx.PlaySound("entity.firework.twinkle_far", 0, 1.0, 2.0);
                }
                else if (_fireworkTwinkle_9 == false && elapsedTime > FireworkTwinkleTime_9)
                {
                    _fireworkTwinkle_9 = true;

                    ctx.PlaySound("entity.firework.large_blast", 0, 1.0, 2.0);
                    ctx.PlaySound("entity.firework.blast_far", 0, 1.0, 2.0);
                    ctx.PlaySound("entity.firework.twinkle", 0, 1.0, 2.0);
                    ctx.PlaySound("entity.firework.twinkle_far", 0, 1.0, 2.0);
                }

                if (elapsedTime > WinnerDisplayDuration)
                {
                    return true;
                }

                return false;
            }


            if (_Winners.Length > 0)
            {
                if (_Winners.Length == 1)
                {
                    _winner = _Winners.Shift(null);

                    System.Diagnostics.Debug.Assert(_winner != null);

                    _startTime = Time.Now();

                    return false;
                }

                if (i >= _Winners.Length * _repeat)
                {
                    int j = _Random.Next(_Winners.Length);

                    SuperPlayer extracted = _Winners.Extract(j, null);

                    System.Diagnostics.Debug.Assert(extracted != null);

                    _intervalTime += Time.FromMilliseconds(100);

                    //_repeat += 1;
                    i = 0;

                    ctx.PlaySound("entity.item.pickup", 0, 1.0, 1.5);
                }
                else
                {
                    Time time = Time.Now() - _time;

                    if (time > _intervalTime)
                    {
                        int j = i % _Winners.Length;
                        SuperPlayer player = _Winners[j];

                        //MyConsole.Debug($"j: {j}");

                        world.DisplayTitle(
                            Time.Zero, _intervalTime, Time.Zero,
                            new TextComponent($"{player.Username}", TextColor.Gray));

                        ++i;

                        _time = Time.Now();

                        ctx.PlaySound("entity.item.pickup", 0, 1.0, 1.5);
                    }

                }

                return false;
            }



            if (_init == false)
            {
                world.DisplayTitle(
                    Time.Zero, Time.FromSeconds(4), Time.FromSeconds(1),
                    new TextComponent($"우승자는!", TextColor.Gold));

                ctx.PlaySound("block.note.pling", 0, 1.0, 1.5);

                _init = true;
            }

            if (elapsedTime > Time.FromSeconds(5))
            {
                System.Diagnostics.Debug.Assert(_Winners.Length == 0);
                ctx.DetermineWinners(_Winners);

                System.Diagnostics.Debug.Assert(_Winners.Length > 0);

                _intervalTime /= _Winners.Length;
                _time = Time.Now() - _intervalTime;

                ctx.PlaySound("block.note.pling", 0, 1.0, 1.5);
            }

            return false;
        }
    }

    public sealed class GameEndNode : IGameProgressNode
    {
        //public readonly static Time Duration = Time.FromSeconds(30);
        public readonly static Time Duration = Time.FromSeconds(5);  // for debug

        private readonly Time _StartTime = Time.Now();

        public IGameProgressNode CreateNextNode(GameContext ctx)
        {
            System.Diagnostics.Debug.Assert(ctx != null);

            return new LobbyNode();
        }

        public bool StartRoutine(GameContext ctx, SuperWorld world)
        {
            System.Diagnostics.Debug.Assert(ctx != null);
            System.Diagnostics.Debug.Assert(world != null);

            Time elapsedTime = Time.Now() - _StartTime;

            if (elapsedTime < Duration)
            {

            }
            else
            {
                ctx.End();

                return true;
            }

            return false;
        }
    }

}

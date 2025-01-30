

using Common;
using Containers;

using MinecraftServerEngine;
using MinecraftPrimitives;
using System;

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

                    _displayWinner = true;
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
                    }

                }

                return false;
            }



            if (_init == false)
            {
                world.DisplayTitle(
                    Time.Zero, Time.FromSeconds(4), Time.FromSeconds(1),
                    new TextComponent($"우승자는!", TextColor.Gold));

                _init = true;
            }

            if (elapsedTime > Time.FromSeconds(5))
            {
                System.Diagnostics.Debug.Assert(_Winners.Length == 0);
                ctx.DetermineWinners(_Winners);

                System.Diagnostics.Debug.Assert(_Winners.Length > 0);

                _intervalTime /= _Winners.Length;
                _time = Time.Now() - _intervalTime;

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

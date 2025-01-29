﻿

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

            return ctx.IsStarted == true;
        }
    }

    public sealed class CountdownNode : IGameProgressNode
    {
        private readonly static Time INTERVAL_TIME = Time.FromSeconds(1);

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

            foreach (SuperPlayer player in ctx.Players)
            {
                player.FlushItems();

                SuperPlayer.GiveDefaultItems(player);
            }

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
                        new TextComponent($"[Superdek] 술래카운트 ", TextColor.Gold),
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
                        new TextComponent($"[Superdek] 도망치기 ", TextColor.Gold),
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
                progressBar = 1.0 - ((double)(elapsedTime - NormalTimeDuration).Amount / (double)BurningTimeDuration.Amount);
                System.Diagnostics.Debug.Assert(progressBar >= 0.0);
                System.Diagnostics.Debug.Assert(progressBar <= 1.0);

                if (_initBurning == false)
                {
                    System.Diagnostics.Debug.Assert(_progressBarId != System.Guid.Empty);
                    world.CloseProgressBar(_progressBarId);

                    _progressBarId = world.OpenProgressBar(
                        [
                        new TextComponent($"[Superdek] ", TextColor.Gold),
                        new TextComponent($"Burning Time! ", TextColor.Yellow),
                        new TextComponent($"(술래: {ctx.CurrentSeeker.Username})", TextColor.Red),
                        ],
                        progressBar,
                        BossBarColor.Yellow,
                        BossBarDivision.Notches_20);

                    world.DisplayTitle(
                        Time.Zero, Time.FromSeconds(1), Time.Zero,
                        new TextComponent($"Burning Time!", TextColor.Gold));

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

        public RoundEndNode()
        {

        }

        public IGameProgressNode CreateNextNode(GameContext ctx)
        {
            System.Diagnostics.Debug.Assert(ctx != null);

            if (ctx.IsFinalRound == true)
            {
                return new GameEndNode();
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

            }
            else
            {
                ctx.EndRound();

                return true;
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

            throw new System.NotImplementedException();
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

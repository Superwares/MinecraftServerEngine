

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

            return ctx.IsStarted;
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
                        new TextComponent($"{_count}초 후에 게임을 시작합니다!", TextColor.Red));

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
        private int _currentRoundIndex = -1;

        public GameStartNode()
        {
        }

        public IGameProgressNode CreateNextNode(GameContext ctx)
        {
            System.Diagnostics.Debug.Assert(ctx != null);

            List<SuperPlayer> prevSeekers = new();
            return new RoundStartNode(_currentRoundIndex, prevSeekers);
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
        private int _currentRoundIndex;

        private readonly List<SuperPlayer> _PrevSeekers;

        public RoundStartNode(int currentRoundIndex, List<SuperPlayer> prevSeekers)
        {
            _currentRoundIndex = currentRoundIndex;

            System.Diagnostics.Debug.Assert(prevSeekers != null);
            _PrevSeekers = prevSeekers;
        }

        public IGameProgressNode CreateNextNode(GameContext ctx)
        {
            System.Diagnostics.Debug.Assert(ctx != null);

            System.Diagnostics.Debug.Assert(_currentRoundIndex >= 0);
            System.Diagnostics.Debug.Assert(_currentRoundIndex < ctx.TotalRounds);
            System.Diagnostics.Debug.Assert(_PrevSeekers != null);
            return new RandomSeekerNode(ctx, _currentRoundIndex, _PrevSeekers);
        }

        public bool StartRoutine(GameContext ctx, SuperWorld world)
        {
            System.Diagnostics.Debug.Assert(ctx != null);
            System.Diagnostics.Debug.Assert(world != null);

            ++_currentRoundIndex;

            return true;
        }
    }

    public sealed class RandomSeekerNode : IGameProgressNode
    {
        private int _currentRoundIndex;

        private readonly List<SuperPlayer> _PrevSeekers;

        private List<int> _playerIndices;

        private Time _intervalTime = Time.FromMilliseconds(250);
        private Time _time;

        private readonly System.Random _Random = new();

        private SuperPlayer _seeker = null;

        private int _repeat = 3;
        private int i = 0;

        public RandomSeekerNode(GameContext ctx, int currentRoundIndex, List<SuperPlayer> prevSeekers)
        {
            System.Diagnostics.Debug.Assert(ctx != null);

            int length = ctx.Players.Length - prevSeekers.Length;
            _playerIndices = new List<int>(length);

            bool prevSeeker = false;
            for (int i = 0; i < ctx.Players.Length; ++i)
            {
                for (int j = 0; j < prevSeekers.Length; ++j)
                {
                    if (object.ReferenceEquals(prevSeekers[j], ctx.Players[i]) == true)
                    {
                        prevSeeker = true;
                        break;
                    }


                }

                if (prevSeeker == true)
                {
                    continue;
                }

                _playerIndices[i] = i;
            }

            _currentRoundIndex = currentRoundIndex;

            _intervalTime /= length;
            _time = Time.Now() - _intervalTime;

            System.Diagnostics.Debug.Assert(prevSeekers != null);
            _PrevSeekers = prevSeekers;
        }

        public IGameProgressNode CreateNextNode(GameContext ctx)
        {
            System.Diagnostics.Debug.Assert(ctx != null);

            System.Diagnostics.Debug.Assert(_playerIndices != null);
            _playerIndices.Dispose();

            System.Diagnostics.Debug.Assert(_seeker != null);
            System.Diagnostics.Debug.Assert(_currentRoundIndex >= 0);
            System.Diagnostics.Debug.Assert(_currentRoundIndex < ctx.TotalRounds);
            System.Diagnostics.Debug.Assert(_PrevSeekers != null);
            return new SeekerCountNode(_seeker, _currentRoundIndex, _PrevSeekers);
        }

        public bool StartRoutine(GameContext ctx, SuperWorld world)
        {
            System.Diagnostics.Debug.Assert(ctx != null);
            System.Diagnostics.Debug.Assert(world != null);


            System.Diagnostics.Debug.Assert(ctx.Players != null);
            IReadOnlyList<SuperPlayer> players = ctx.Players;

            if (_playerIndices.Length == 1)
            {
                int j = _playerIndices[0];

                _seeker = players[j];

                world.DisplayTitle(
                    Time.Zero, Time.FromSeconds(1), Time.FromSeconds(1),
                    new TextComponent($"{_seeker.Username}", TextColor.DarkGreen));

                return true;
            }

            if (i >= _playerIndices.Length * _repeat)
            {
                int j = _Random.Next(_playerIndices.Length);
                int k = _playerIndices[j];

                _playerIndices.Extract(_j => _j == j, -1);

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
                    int j = i % _playerIndices.Length;
                    int k = _playerIndices[j];

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
        public readonly static Time Duration = Time.FromSeconds(30);

        private readonly Time _StartTime = Time.Now();

        private int _currentRoundIndex;

        private readonly List<SuperPlayer> _PrevSeekers;

        private readonly SuperPlayer _Seeker;


        public SeekerCountNode(SuperPlayer player, int currentRoundIndex, List<SuperPlayer> prevSeekers)
        {
            System.Diagnostics.Debug.Assert(player != null);
            _Seeker = player;

            System.Diagnostics.Debug.Assert(currentRoundIndex >= 0);
            _currentRoundIndex = currentRoundIndex;

            System.Diagnostics.Debug.Assert(prevSeekers != null);
            _PrevSeekers = prevSeekers;
        }

        public IGameProgressNode CreateNextNode(GameContext ctx)
        {
            System.Diagnostics.Debug.Assert(ctx != null);

            System.Diagnostics.Debug.Assert(_Seeker != null);
            System.Diagnostics.Debug.Assert(_currentRoundIndex >= 0);
            System.Diagnostics.Debug.Assert(_currentRoundIndex < ctx.TotalRounds);
            System.Diagnostics.Debug.Assert(_PrevSeekers != null);
            return new FindHidersNode(_Seeker, _currentRoundIndex, _PrevSeekers);
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
                world.DisplayTitle(
                    Time.Zero, Time.FromSeconds(1), Time.Zero,
                    new TextComponent($"주의! 술래가 출발합니다!", TextColor.Red));

                return true;
            }

            return false;
        }
    }

    public sealed class FindHidersNode : IGameProgressNode
    {
        public readonly static Time NormalTimeDuration = Time.FromMinutes(2);
        public readonly static Time BurningTimeDuration = Time.FromMinutes(1);

        private readonly Time _StartTime = Time.Now();

        private int _currentRoundIndex;

        private readonly List<SuperPlayer> _PrevSeekers;

        private readonly SuperPlayer _Seeker;

        private bool _alertBurningTime = false;

        public FindHidersNode(SuperPlayer player, int currentRoundIndex, List<SuperPlayer> prevSeekers)
        {
            System.Diagnostics.Debug.Assert(player != null);
            _Seeker = player;

            System.Diagnostics.Debug.Assert(currentRoundIndex >= 0);
            _currentRoundIndex = currentRoundIndex;

            System.Diagnostics.Debug.Assert(prevSeekers != null);
            _PrevSeekers = prevSeekers;
        }

        public IGameProgressNode CreateNextNode(GameContext ctx)
        {
            System.Diagnostics.Debug.Assert(ctx != null);

            System.Diagnostics.Debug.Assert(_Seeker != null);
            System.Diagnostics.Debug.Assert(_currentRoundIndex >= 0);
            System.Diagnostics.Debug.Assert(_currentRoundIndex < ctx.TotalRounds);
            System.Diagnostics.Debug.Assert(_PrevSeekers != null);
            return new RoundEndNode(_Seeker, _currentRoundIndex, _PrevSeekers);
        }

        public bool StartRoutine(GameContext ctx, SuperWorld world)
        {
            System.Diagnostics.Debug.Assert(ctx != null);
            System.Diagnostics.Debug.Assert(world != null);

            Time elapsedTime = Time.Now() - _StartTime;

            if (elapsedTime < NormalTimeDuration)
            {

            }
            else if (elapsedTime < BurningTimeDuration)
            {
                if (_alertBurningTime == false)
                {
                    world.DisplayTitle(
                        Time.Zero, Time.FromSeconds(1), Time.Zero,
                        new TextComponent($"Burning Time 시작!", TextColor.Red));

                    _alertBurningTime = true;
                }
            }
            else
            {
                return true;
            }

            return false;
        }
    }

    public sealed class RoundEndNode : IGameProgressNode
    {
        private int _currentRoundIndex;

        private readonly List<SuperPlayer> _PrevSeekers;

        private readonly SuperPlayer Seeker;

        public RoundEndNode(SuperPlayer player, int currentRoundIndex, List<SuperPlayer> prevSeekers)
        {
            System.Diagnostics.Debug.Assert(player != null);
            Seeker = player;

            System.Diagnostics.Debug.Assert(currentRoundIndex >= 0);
            _currentRoundIndex = currentRoundIndex;

            System.Diagnostics.Debug.Assert(prevSeekers != null);
            _PrevSeekers = prevSeekers;
        }

        public IGameProgressNode CreateNextNode(GameContext ctx)
        {
            System.Diagnostics.Debug.Assert(ctx != null);

            System.Diagnostics.Debug.Assert(_currentRoundIndex >= 0);
            System.Diagnostics.Debug.Assert(_currentRoundIndex < ctx.TotalRounds);

            int finalRound = ctx.TotalRounds - 1;

            _PrevSeekers.Append(Seeker);

            System.Diagnostics.Debug.Assert(_currentRoundIndex >= 0);
            System.Diagnostics.Debug.Assert(_currentRoundIndex < ctx.TotalRounds);
            System.Diagnostics.Debug.Assert(_PrevSeekers != null);
            if (_currentRoundIndex < finalRound)
            {
                return new RoundStartNode(_currentRoundIndex, _PrevSeekers);
            }
            else
            {
                throw new System.NotImplementedException();
            }

        }

        public bool StartRoutine(GameContext ctx, SuperWorld world)
        {
            System.Diagnostics.Debug.Assert(ctx != null);
            System.Diagnostics.Debug.Assert(world != null);

            return true;
        }
    }

}

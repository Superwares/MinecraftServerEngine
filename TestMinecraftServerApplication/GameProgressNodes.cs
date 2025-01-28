

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

            return new RoundStartNode(_currentRoundIndex);
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

        public RoundStartNode(int currentRoundIndex)
        {
            _currentRoundIndex = currentRoundIndex;
        }

        public IGameProgressNode CreateNextNode(GameContext ctx)
        {
            System.Diagnostics.Debug.Assert(ctx != null);

            System.Diagnostics.Debug.Assert(_currentRoundIndex >= 0);
            return new RandomSeekerNode(ctx, _currentRoundIndex);
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

        private List<int> _playerIndices;

        private Time _intervalTime = Time.FromMilliseconds(250);
        private Time _time;

        private readonly System.Random _Random = new();

        private SuperPlayer _player = null;

        private int _repeat = 3;
        private int i = 0;

        public RandomSeekerNode(GameContext ctx, int currentRoundIndex)
        {
            System.Diagnostics.Debug.Assert(ctx != null);

            int length = ctx.Players.Length;
            _playerIndices = new List<int>(length);
            for (int i = 0; i < length; ++i)
            {
                _playerIndices[i] = i;
            }

            _currentRoundIndex = currentRoundIndex;

            _intervalTime /= length;
            _time = Time.Now() - _intervalTime;
        }

        public IGameProgressNode CreateNextNode(GameContext ctx)
        {
            System.Diagnostics.Debug.Assert(ctx != null);

            System.Diagnostics.Debug.Assert(_playerIndices != null);
            _playerIndices.Dispose();

            System.Diagnostics.Debug.Assert(_currentRoundIndex >= 0);
            System.Diagnostics.Debug.Assert(_currentRoundIndex < ctx.TotalRounds);
            System.Diagnostics.Debug.Assert(_player != null);
            return new SeekerCountNode(_player, _currentRoundIndex);
        }

        public bool StartRoutine(GameContext ctx, SuperWorld world)
        {
            System.Diagnostics.Debug.Assert(ctx != null);
            System.Diagnostics.Debug.Assert(world != null);


            System.Diagnostics.Debug.Assert(ctx.Players != null);
            IReadOnlyList<SuperPlayer> players = ctx.Players;

            SuperPlayer player;

            if (i >= _playerIndices.Length * _repeat)
            {
                int j = _Random.Next(_playerIndices.Length);
                int k = _playerIndices[j];

                _playerIndices.Extract(_j => _j == j, -1);

                //SuperPlayer player = players[k];

                _intervalTime += Time.FromMilliseconds(100);

                //_repeat += 1;
                i = 0;

                if (_playerIndices.Length == 1)
                {
                    j = _playerIndices[0];

                    _player = players[j];

                    world.DisplayTitle(
                        Time.Zero, Time.FromSeconds(1), Time.FromSeconds(1),
                        new TextComponent($"{_player.Username}", TextColor.DarkGreen));

                    return true;
                }
            }
            else
            {
                Time time = Time.Now() - _time;

                if (time > _intervalTime)
                {
                    int j = i % _playerIndices.Length;
                    int k = _playerIndices[j];

                    MyConsole.Debug($"j: {j}");

                    player = players[k];

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

        private readonly SuperPlayer Seeker;


        public SeekerCountNode(SuperPlayer player, int currentRoundIndex)
        {
            Seeker = player;

            System.Diagnostics.Debug.Assert(currentRoundIndex >= 0);
            _currentRoundIndex = currentRoundIndex;
        }

        public IGameProgressNode CreateNextNode(GameContext ctx)
        {
            System.Diagnostics.Debug.Assert(ctx != null);

            System.Diagnostics.Debug.Assert(_currentRoundIndex >= 0);
            System.Diagnostics.Debug.Assert(_currentRoundIndex < ctx.TotalRounds);
            return new FindHidersNode(_currentRoundIndex);
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

        private bool _alertBurningTime = false;

        public FindHidersNode(int currentRoundIndex)
        {
            System.Diagnostics.Debug.Assert(currentRoundIndex >= 0);

            _currentRoundIndex = currentRoundIndex;
        }

        public IGameProgressNode CreateNextNode(GameContext ctx)
        {
            System.Diagnostics.Debug.Assert(ctx != null);

            System.Diagnostics.Debug.Assert(_currentRoundIndex >= 0);
            System.Diagnostics.Debug.Assert(_currentRoundIndex < ctx.TotalRounds);
            return new RoundEndNode(_currentRoundIndex);
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

        public RoundEndNode(int currentRoundIndex)
        {
            System.Diagnostics.Debug.Assert(currentRoundIndex >= 0);

            _currentRoundIndex = currentRoundIndex;
        }

        public IGameProgressNode CreateNextNode(GameContext ctx)
        {
            System.Diagnostics.Debug.Assert(ctx != null);

            System.Diagnostics.Debug.Assert(_currentRoundIndex >= 0);
            System.Diagnostics.Debug.Assert(_currentRoundIndex < ctx.TotalRounds);

            int finalRound = ctx.TotalRounds - 1;

            if (_currentRoundIndex < finalRound)
            {
                return new RoundStartNode(_currentRoundIndex);
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

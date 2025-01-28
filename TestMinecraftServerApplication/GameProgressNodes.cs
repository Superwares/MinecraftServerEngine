

using Common;
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
            return new SeekerCountNode(_currentRoundIndex);
        }

        public bool StartRoutine(GameContext ctx, SuperWorld world)
        {
            System.Diagnostics.Debug.Assert(ctx != null);
            System.Diagnostics.Debug.Assert(world != null);

            ++_currentRoundIndex;

            return true;
        }
    }

    // The seeker closes their eyes and counts to a number.
    public sealed class SeekerCountNode : IGameProgressNode
    {
        public readonly static Time Duration = Time.FromSeconds(30);

        private readonly Time _StartTime = Time.Now();

        private int _currentRoundIndex;


        public SeekerCountNode(int currentRoundIndex)
        {
            System.Diagnostics.Debug.Assert(currentRoundIndex >= 0);

            _currentRoundIndex = currentRoundIndex;
        }

        public IGameProgressNode CreateNextNode(GameContext ctx)
        {
            System.Diagnostics.Debug.Assert(ctx != null);

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

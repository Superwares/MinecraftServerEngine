

using Common;
using MinecraftServerEngine;
using MinecraftPrimitives;

namespace TestMinecraftServerApplication
{
    public interface IGameProgressNode
    {
        public IGameProgressNode NextNode();

        public bool StartRoutine(GameContext ctx, TestWorld world);

    }

    public sealed class LobbyNode : IGameProgressNode
    {
        public IGameProgressNode NextNode()
        {
            return new CountdownNode();
        }

        public bool StartRoutine(GameContext ctx, TestWorld world)
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

        public IGameProgressNode NextNode()
        {
            throw new System.NotImplementedException();
        }

        public bool StartRoutine(GameContext ctx, TestWorld world)
        {
            System.Diagnostics.Debug.Assert(ctx != null);
            System.Diagnostics.Debug.Assert(world != null);

            if (_count > 0)
            {
                Time elapsedTime = Time.Now() - _prevDisplayTime;

                if (elapsedTime >= INTERVAL_TIME)
                {
                    world.DisplayTitle(
                        Time.Zero, INTERVAL_TIME, Time.Zero, 
                        new TextComponent($"{_count}", TextColor.Red));

                    ++_count;
                }

                return false;
            }

            return true;
        }
    }
}



using Common;
using Containers;

using MinecraftServerEngine;
using MinecraftServerEngine.ProgressBars;
using MinecraftServerEngine.Text;

namespace TestMinecraftServerApplication.GameStages
{
    public sealed class GameEndStage : IGameStage
    {
        //public readonly static Time Duration = Time.FromSeconds(30);
        public readonly static Time Duration = Time.FromSeconds(5);  // for debug

        private readonly Time _StartTime = Time.Now();

        public IGameStage CreateNextNode(GameContext ctx)
        {
            System.Diagnostics.Debug.Assert(ctx != null);

            return new LobbyStage();
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

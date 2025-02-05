

using Common;
using Containers;

using MinecraftServerEngine;
using MinecraftServerEngine.ProgressBars;
using MinecraftServerEngine.Text;

namespace TestMinecraftServerApplication.GameStages
{
    public sealed class GameStartStage : IGameStage
    {
        public GameStartStage()
        {
        }

        public IGameStage CreateNextStage(GameContext ctx)
        {
            System.Diagnostics.Debug.Assert(ctx != null);

            return new RoundStartStage();
        }

        public bool StartRoutine(GameContext ctx, SuperWorld world)
        {
            System.Diagnostics.Debug.Assert(ctx != null);
            System.Diagnostics.Debug.Assert(world != null);

            ctx.Start();

            world.ChangeWorldTimeOfDay(SuperWorld.DefaultWorldTime, Time.Zero);
            world.ChangeWorldBorderSize(world.DefaultWorldBorderRadiusInMeters, Time.Zero);

            return true;
        }
    }

}

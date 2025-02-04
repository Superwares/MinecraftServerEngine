

using Common;
using Containers;

using MinecraftServerEngine;
using MinecraftServerEngine.ProgressBars;
using MinecraftServerEngine.Text;

namespace TestMinecraftServerApplication.GameStages
{
    public sealed class RoundStartStage : IGameStage
    {

        public RoundStartStage()
        {
        }

        public IGameStage CreateNextNode(GameContext ctx)
        {
            System.Diagnostics.Debug.Assert(ctx != null);

            List<int> nonSeekerIndexList = new List<int>();
            int length = ctx.MakeNonSeekerIndexList(nonSeekerIndexList);

            return new RandomSeekerStage(nonSeekerIndexList);
        }

        public bool StartRoutine(GameContext ctx, SuperWorld world)
        {
            System.Diagnostics.Debug.Assert(ctx != null);
            System.Diagnostics.Debug.Assert(world != null);

            ctx.StartRound();

            world.ChangeWorldTimeOfDay(SuperWorld.DefaultWorldTime, Time.Zero);
            world.ChangeWorldBorderSize(world.DefaultWorldBorderRadiusInMeters, Time.Zero);

            return true;
        }
    }
}

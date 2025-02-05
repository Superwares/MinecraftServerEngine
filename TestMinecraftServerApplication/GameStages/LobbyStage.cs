

namespace TestMinecraftServerApplication.GameStages
{
    public sealed class LobbyStage : IGameStage
    {
        public IGameStage CreateNextStage(GameContext ctx)
        {
            System.Diagnostics.Debug.Assert(ctx != null);

            return new CountdownStage();
        }

        public bool StartRoutine(GameContext ctx, SuperWorld world)
        {
            System.Diagnostics.Debug.Assert(ctx != null);
            System.Diagnostics.Debug.Assert(world != null);

            return ctx.IsReady == true;
        }
    }
}

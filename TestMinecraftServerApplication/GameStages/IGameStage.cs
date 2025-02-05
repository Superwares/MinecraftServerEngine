
namespace TestMinecraftServerApplication.GameStages
{
    public interface IGameStage
    {
        public IGameStage CreateNextStage(GameContext ctx);

        public bool StartRoutine(GameContext ctx, SuperWorld world);

    }
}

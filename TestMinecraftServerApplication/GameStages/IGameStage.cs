
namespace TestMinecraftServerApplication.GameStages
{
    public interface IGameStage
    {
        public IGameStage CreateNextNode(GameContext ctx);

        public bool StartRoutine(GameContext ctx, SuperWorld world);

    }
}

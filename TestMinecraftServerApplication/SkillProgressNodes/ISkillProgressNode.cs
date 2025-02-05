


namespace TestMinecraftServerApplication.SkillProgressNodes
{
    internal interface ISkillProgressNode
    {
        string Name { get; }

        ISkillProgressNode CreateNextNode();

        bool Start(SuperWorld world, SuperPlayer player);
        void Close(SuperPlayer player);
    }
}

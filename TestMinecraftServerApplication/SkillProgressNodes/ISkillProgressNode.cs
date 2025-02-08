

using MinecraftServerEngine.Physics;

namespace TestMinecraftServerApplication.SkillProgressNodes
{
    internal interface ISkillProgressNode
    {
        string Name { get; }

        ISkillProgressNode CreateNextNode();

        bool Start(SuperWorld world, PhysicsObject obj);
        void Close(PhysicsObject obj);
    }
}

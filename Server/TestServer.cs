using Protocol;
using Framework;

namespace TestServer
{
    public static class TestServer
    {
        
        public static void Main()
        {
            System.Console.WriteLine("Hello, World!");

            using World world = new SuperWorld();

            using MinecraftServerFramework framework = new(world);
            framework.Run();
        }

    }

}


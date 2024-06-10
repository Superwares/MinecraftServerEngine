using MinecraftServerEngine;

namespace TestServer
{
    public static class TestServerApplication
    {
        public static void Main()
        {
            System.Console.WriteLine("Hello, World!");

            using World world = new SuperWorld();

            using ServerFramework framework = new(world);
            framework.Run();

            
        }

    }

}


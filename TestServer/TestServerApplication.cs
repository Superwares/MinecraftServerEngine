using MinecraftServerEngine;

namespace TestServer
{
    public class TestServerApplication : ServerFramework
    {
        public TestServerApplication() : base(new SuperWorld())
        {
        }

        public static void Main()
        {
            System.Console.WriteLine("Hello, World!");

            using World world = new SuperWorld();

            using ServerFramework framework = new(world);
            framework.Run();

            
        }

    }

}


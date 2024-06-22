using Common;
using MinecraftServerEngine;
using TestMinecraftServerApplication;

Console.Printl("Hello, World!");

using World world = new Lobby();

using ServerFramework framework = new(world);
framework.Run();

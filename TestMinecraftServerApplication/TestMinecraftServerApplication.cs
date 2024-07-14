using Common;
using MinecraftServerEngine;
using TestMinecraftServerApplication;

Console.Printl("Hello, World!");

const ushort port = 25565;

using World world = new Lobby();

using ServerFramework framework = new(world);
framework.Run(port);

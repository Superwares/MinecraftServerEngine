using System.IO;

using Common;

using MinecraftPrimitives;
using MinecraftServerEngine;
using TestMinecraftServerApplication;


Console.Printl("Hello, World!");

//const ushort port = 25565;

//using World world = new Lobby();

//using ServerFramework framework = new(world);
//framework.Run(port);


FileInfo fileInfo = new("C:\\Users\\Peach\\Documents\\Superwares\\MinecraftServerEngine\\Testing\\r.0.0.mca");

NBTTagCompound tag = NBTTagRootCompoundLoader.Load(fileInfo, 1, 1);

if (tag != null)
{
    Console.Printl(tag.ToString());
}

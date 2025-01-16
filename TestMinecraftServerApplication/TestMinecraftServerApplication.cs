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

int chunkX = 4;
int chunkZ = 4;

FileInfo fileInfo = new("C:\\Users\\Peach\\Documents\\Superwares\\MinecraftServerEngine\\Testing\\r.0.0.mca");

NBTTagCompound tag = NBTTagRootCompoundLoader.Load(fileInfo, chunkX, chunkZ);

if (tag != null)
{
    Console.Printl(tag.ToString());
}

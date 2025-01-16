using System.IO;

using Common;

using MinecraftPrimitives;
using MinecraftServerEngine;
using TestMinecraftServerApplication;

//Console.Debug("Hello, World!");
//Console.Info("Hello, World!");
//Console.Warn("Hello, World!");

Console.Info("Hello, World!");

//const ushort port = 25565;

//using World world = new Lobby();

//using ServerFramework framework = new(world);
//framework.Run(port);

int chunkX = 0;
int chunkZ = 0;

FileInfo fileInfo = new("region\\r.0.0.mca");

NBTTagCompound tag = NBTTagRootCompoundLoader.Load(fileInfo, chunkX, chunkZ);

if (tag != null)
{
    Console.Printl(tag.ToString());

    NBTTagList<NBTTagCompound> list =
        tag.GetNBTTag<NBTTagCompound>("Level")
        .GetNBTTag<NBTTagList<NBTTagCompound>>("Sections");

    foreach (NBTTagCompound section in list.Data)
    {
        //Console.Printl(section.ToString());

        byte[] blocks = section.GetNBTTag<NBTTagByteArray>("Blocks").Data;
        byte[] skyLights = section.GetNBTTag<NBTTagByteArray>("SkyLight").Data;
        int y = section.GetNBTTag<NBTTagByte>("Y").Value;
        byte[] blockLight = section.GetNBTTag<NBTTagByteArray>("BlockLight").Data;
        byte[] data = section.GetNBTTag<NBTTagByteArray>("Data").Data;

    }

}

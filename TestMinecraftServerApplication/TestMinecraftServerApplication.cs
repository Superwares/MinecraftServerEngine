
using Common;

using MinecraftServerEngine;
using MinecraftServerEngine.Entities;
using MinecraftServerEngine.Physics;
using TestMinecraftServerApplication;
using TestMinecraftServerApplication.Configs;



MyConsole.Info("Hello, World!");



ConfigXml.Deserialize("Config.xml");

ConfigServer configServer = ConfigXml.GetConfig().Server;

ConfigWorld configWorld = ConfigXml.GetConfig().World;



ushort port = configServer.Port;

double worldCenterX = configWorld.CenterX;
double worldCenterZ = configWorld.CenterZ;
double defaultWorldBorderRadiusInMeters = configWorld.DefaultWorldBorderRadiusInMeters;
Vector respawningPos = new(configWorld.RespawningX, configWorld.RespawningY, configWorld.RespawningZ);
EntityAngles respawningLook = new(configWorld.RespawningYaw, configWorld.RespawningPitch) ;

if (configWorld.DefaultWorldBorderRadiusInMeters <= 0)
{
    throw new System.InvalidOperationException($"The value for \"{nameof(configWorld.DefaultWorldBorderRadiusInMeters)}\" must be > 0");
}



using World world = new SuperWorld(
    worldCenterX, worldCenterZ,
    defaultWorldBorderRadiusInMeters,

    respawningPos, respawningLook
    );

using MinecraftServerFramework framework = new(world);

framework.Run(port);


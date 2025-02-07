
using Common;

using MinecraftServerEngine;
using MinecraftServerEngine.Entities;
using MinecraftServerEngine.Physics;
using TestMinecraftServerApplication;
using TestMinecraftServerApplication.Configs;


const ushort port = 25565;


MyConsole.Info("Hello, World!");

ConfigXml.Deserialize("Config.xml");

ConfigWorld config = ConfigXml.GetConfig().World;

double worldCenterX = config.CenterX;
double worldCenterZ = config.CenterZ;
double defaultWorldBorderRadiusInMeters = config.DefaultWorldBorderRadiusInMeters;
Vector respawningPos = new(config.RespawningX, config.RespawningY, config.RespawningZ);
EntityAngles respawningLook = new(config.RespawningYaw, config.RespawningPitch) ;

if (config.DefaultWorldBorderRadiusInMeters <= 0)
{
    throw new System.InvalidOperationException($"The value for \"{nameof(config.DefaultWorldBorderRadiusInMeters)}\" must be > 0");
}

using World world = new SuperWorld(
    worldCenterX, worldCenterZ,
    defaultWorldBorderRadiusInMeters,

    respawningPos, respawningLook
    );

using MinecraftServerFramework framework = new(world);
framework.Run(port);


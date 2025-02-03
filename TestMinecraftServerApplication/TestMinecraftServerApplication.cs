
using Common;

using MinecraftPrimitives;
using MinecraftServerEngine;
using MinecraftServerEngine.PhysicsEngine;
using TestMinecraftServerApplication;


const ushort port = 25565;


MyConsole.Info("Hello, World!");

Config.Deserialize("Config.xml");

double worldCenterX, worldCenterZ;
double defaultWorldBorderRadiusInMeters;
Vector respawningPos;
Angles respawningLook;

{
    IConfigWorld config = Config.Instance.World;

    if (config == null)
    {
        MyConsole.Warn("Config.World is null");

        config = new ConfigWorld()
        {
            CenterX = 0.0,
            CenterZ = 0.0,
            DefaultWorldBorderRadiusInMeters = World.MaxWorldBorderRadiusInMeters,

            RespawningX = 0.0,
            RespawningY = 0.0,
            RespawningZ = 0.0,
            RespawningYaw = 0.0,
            RespawningPitch = 0.0,
        };
    }

    worldCenterX = config.CenterX;
    worldCenterZ = config.CenterZ;

    respawningPos = new Vector(
        config.RespawningX,
        config.RespawningY,
        config.RespawningZ
        );
    respawningLook = new Angles(
        config.RespawningYaw,
        config.RespawningPitch
        );

    if (config.DefaultWorldBorderRadiusInMeters <= 0)
    {
        MyConsole.Warn(
            $"Config.World.DefaultWorldBorderRadiusInMeters must be greater than 0: " +
            $"{config.DefaultWorldBorderRadiusInMeters}");

        defaultWorldBorderRadiusInMeters = World.MaxWorldBorderRadiusInMeters;
    }
    else
    {
        defaultWorldBorderRadiusInMeters = config.DefaultWorldBorderRadiusInMeters;
    }

}



using World world = new SuperWorld(
    worldCenterX, worldCenterZ,
    defaultWorldBorderRadiusInMeters,

    respawningPos, respawningLook
    );

using MinecraftServerFramework framework = new(world);
framework.Run(port);


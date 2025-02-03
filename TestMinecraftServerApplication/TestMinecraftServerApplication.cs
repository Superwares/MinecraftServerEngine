
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
    IConfigWorld configWorld = Config.Instance.World;

    if (configWorld == null)
    {
        MyConsole.Warn("Config.World is null");

        configWorld = new ConfigWorld()
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

    worldCenterX = configWorld.CenterX;
    worldCenterZ = configWorld.CenterZ;

    respawningPos = new Vector(
        configWorld.RespawningX,
        configWorld.RespawningY,
        configWorld.RespawningZ
        );
    respawningLook = new Angles(
        configWorld.RespawningYaw,
        configWorld.RespawningPitch
        );

    if (configWorld.DefaultWorldBorderRadiusInMeters <= 0)
    {
        MyConsole.Warn(
            $"Config.World.DefaultWorldBorderRadiusInMeters must be greater than 0: " +
            $"{configWorld.DefaultWorldBorderRadiusInMeters}");

        defaultWorldBorderRadiusInMeters = World.MaxWorldBorderRadiusInMeters;
    }
    else
    {
        defaultWorldBorderRadiusInMeters = configWorld.DefaultWorldBorderRadiusInMeters;
    }

}



using World world = new SuperWorld(
    worldCenterX, worldCenterZ,
    defaultWorldBorderRadiusInMeters,

    respawningPos, respawningLook
    );

using ServerFramework framework = new(world);
framework.Run(port);


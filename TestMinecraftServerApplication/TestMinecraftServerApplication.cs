
using Common;

using MinecraftPrimitives;
using MinecraftServerEngine;
using MinecraftServerEngine.PhysicsEngine;
using TestMinecraftServerApplication;

MyConsole.Info("Hello, World!");

Config.Deserialize("Config.xml");

IConfigWorld worldConfig = Config.Instance.World;

if (worldConfig == null)
{
    throw new System.InvalidOperationException("Config.World is null");
}

if (worldConfig.DefaultWorldBorderRadiusInMeters <= 0)
{
    throw new System.InvalidOperationException(
        $"Config.World.DefaultWorldBorderRadiusInMeters must be greater than 0: " +
        $"{worldConfig.DefaultWorldBorderRadiusInMeters}");
}

const ushort port = 25565;

using World world = new SuperWorld(
    worldConfig.CenterX,
    worldConfig.CenterZ,
    worldConfig.DefaultWorldBorderRadiusInMeters,

    new Vector(
        worldConfig.RespawningX,
        worldConfig.RespawningY,
        worldConfig.RespawningZ),
    new Angles(
        worldConfig.RespawningYaw,
        worldConfig.RespawningPitch)
    );

using ServerFramework framework = new(world);
framework.Run(port);


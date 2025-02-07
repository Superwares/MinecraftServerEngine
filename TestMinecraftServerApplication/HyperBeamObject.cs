using Common;
using Containers;

using MinecraftServerEngine.Physics;
using MinecraftServerEngine.Physics.BoundingVolumes;
using MinecraftServerEngine.Particles;
using MinecraftServerEngine.ShapeObjects;


namespace TestMinecraftServerApplication
{
    internal sealed class HyperBeamObject : CylinderObject
    {
        public HyperBeamObject() 
            : base(
                  new Vector(151.5, 15.0, 214.5),
                  new Vector(1.0, 1.0, 1.0), 
                  Angles.CreateByDegrees(0.0, 45.0, 0.0)
                  )
        {
        }

        public override void StartRoutine(PhysicsWorld world)
        {
            System.Diagnostics.Debug.Assert(world != null);

            using Tree<PhysicsObject> objs = new();

            world.SearchObjects(objs, BoundingVolume, true);

            MyConsole.Debug($"objs: {objs.Count}");

            if (BoundingVolume is OrientedBoundingBox obb) 
            {
                foreach (Vector vertex in obb.Vertices)
                {
                    EmitParticles(Particle.Reddust, vertex, 0.0000001, 1, 0.0, 0.0, 0.0);
                }
            }
        }
    }
}

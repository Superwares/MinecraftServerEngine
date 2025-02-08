

using Common;

using MinecraftServerEngine.Particles;
using MinecraftServerEngine.Physics;
using MinecraftServerEngine.Physics.BoundingVolumes;

using TestMinecraftServerApplication.Items;

namespace TestMinecraftServerApplication.SkillProgressNodes
{
    internal sealed class HyperBeamActualDamagingSkillNode : ISkillProgressNode
    {
        public string Name => $"{HyperBeam.Name}'s Actual Damaging Skill Node";

        private Time _startTime = Time.Now() - HyperBeam.ChargingInterval;

        public ISkillProgressNode CreateNextNode()
        {
            return null;
        }

        public bool Start(SuperWorld world, PhysicsObject _obj)
        {
            System.Diagnostics.Debug.Assert(world != null);
            System.Diagnostics.Debug.Assert(_obj != null);
            
            if (_obj is HyperBeamObject obj && obj.BoundingVolume is OrientedBoundingBox obb)
            {
                Time elapsedTime = Time.Now() - _startTime;

                if (elapsedTime >= HyperBeam.ChargingInterval)
                {
                    
                }

                return false;
            }

            throw new System.InvalidOperationException("The provided object is not a HyperBeamObject");
        }

        public void Close(PhysicsObject _obj)
        {
            System.Diagnostics.Debug.Assert(_obj != null);

            if (_obj is HyperBeamObject obj)
            {

            }
        }
    }
}

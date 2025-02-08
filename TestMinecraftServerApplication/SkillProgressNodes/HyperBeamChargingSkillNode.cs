

using Common;

using MinecraftServerEngine.Particles;
using MinecraftServerEngine.Physics;
using MinecraftServerEngine.Physics.BoundingVolumes;
using TestMinecraftServerApplication.Items;

namespace TestMinecraftServerApplication.SkillProgressNodes
{
    internal sealed class HyperBeamChargingSkillNode : ISkillProgressNode
    {
        public string Name => $"{HyperBeam.Name}'s Charging Skill Node";

        private Time _startTime = Time.Now() - HyperBeam.ChargingInterval;

        private int _chargingIndex = 0;

        public ISkillProgressNode CreateNextNode()
        {
            return new HyperBeamDamagingSkillNode();
        }

        public bool Start(SuperWorld world, PhysicsObject _obj)
        {
            System.Diagnostics.Debug.Assert(world != null);
            System.Diagnostics.Debug.Assert(_obj != null);

            const double ParticleInterval = HyperBeam.ChargingParticleInterval;
            const int ParticlePlanes = (int)(HyperBeam.Length / ParticleInterval) + 1;

            //const double ChargingLength = HyperBeam.Length + 1;

            Time elapsedTime = Time.Now() - _startTime;

            System.Diagnostics.Debug.Assert(_chargingIndex <= ParticlePlanes);
            if (_chargingIndex == ParticlePlanes)
            {
                if (elapsedTime > Time.FromSeconds(1))
                {
                    return true;
                }

                return false;
            }

            if (_obj is HyperBeamObject obj && obj.BoundingVolume is OrientedBoundingBox obb)
            {
                

                if (elapsedTime >= HyperBeam.ChargingInterval)
                {

                    Vector u = new(1.0, 0.0, 0.0);
                    u = u.Rotate(obb.Angles);

                    Vector d = u * ((_chargingIndex++ % ParticlePlanes) * ParticleInterval);
                    Vector o = d + obb.Center - (u * obb.Extents.X);

                    const double CircleRadius = HyperBeam.Radius;
                    const int CirclePoints = 35;

                    double angleIncrement = 2 * System.Math.PI / CirclePoints;

                    Vector[] vertices = new Vector[CirclePoints];

                    for (int i = 0; i < CirclePoints; i++)
                    {
                        double angle = i * angleIncrement;
                        double x = CircleRadius * System.Math.Cos(angle);
                        double y = CircleRadius * System.Math.Sin(angle);

                        vertices[i] = new Vector(0, x, y);
                    }

                    Vector.Rotate(obb.Angles, vertices);

                    for (int i = 0; i < CirclePoints; i++)
                    {
                        Vector v = vertices[i] + o;
                        obj.EmitRgbParticle(v, 0.0, 0.0, 0.0);  // Black
                    }

                    world.PlaySound("entity.experience_orb.pickup", 0, o, 1.0, 2.0);

                    _startTime = _startTime + HyperBeam.ChargingInterval;
                    
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

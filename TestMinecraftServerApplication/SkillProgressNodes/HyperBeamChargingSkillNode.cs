

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
            throw new System.NotImplementedException();
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
                    Vector u = obb.Angles.ToUnitVector();
                    Vector d = u * (_chargingIndex++ % (HyperBeam.Length + 1));
                    Vector o = d + obb.Center;

                    const double Radius = HyperBeam.Radius;
                    const int NumberOfPoints = 10;

                    double angleIncrement = 2 * System.Math.PI / NumberOfPoints;

                    Vector[] vertices = new Vector[NumberOfPoints];

                    for (int i = 0; i < NumberOfPoints; i++)
                    {
                        double angle = i * angleIncrement;
                        double x = Radius * System.Math.Cos(angle);
                        double y = Radius * System.Math.Sin(angle);

                        vertices[i] = new Vector(0, x, y);
                    }

                    Vector.Rotate(obb.Angles, vertices);

                    for (int i = 0; i < NumberOfPoints; i++)
                    {
                        obj.EmitParticles(Particle.Reddust, (vertices[i] + o), 0.1, 1, 0.0, 0.0, 0.0);
                    }

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



using Common;
using Containers;

using MinecraftServerEngine.Particles;
using MinecraftServerEngine.Entities;
using MinecraftServerEngine.Physics;
using MinecraftServerEngine.Physics.BoundingVolumes;

using TestMinecraftServerApplication.Items;

namespace TestMinecraftServerApplication.SkillProgressNodes
{
    internal sealed class HyperBeamDamagingSkillNode : ISkillProgressNode
    {
        public string Name => $"{HyperBeam.Name}'s Actual Damaging Skill Node";

        private readonly static Time _SleepTime = Time.FromMilliseconds(1000);

        private Time _startTime = Time.Now() - HyperBeam.DamagingInterval;

        private int _planeIndex = 0;
        private int _damagingIndex = 0;
        private bool _damaged = false;

        public ISkillProgressNode CreateNextNode()
        {
            return null;
        }

        public bool Start(SuperWorld world, PhysicsObject _obj)
        {
            System.Diagnostics.Debug.Assert(world != null);
            System.Diagnostics.Debug.Assert(_obj != null);

            const double ParticleInterval = HyperBeam.ParticleInterval;
            const int ParticlePlanes = (int)(HyperBeam.Length / ParticleInterval) + 1;

            const double CircleRadius = HyperBeam.Radius;
            const int CirclePoints = 35;

            Time elapsedTime = Time.Now() - _startTime;

            System.Diagnostics.Debug.Assert(_planeIndex <= ParticlePlanes);
            if (_planeIndex == ParticlePlanes)
            {
                // Damage!
                if (_damaged == false) {
                    using Tree<PhysicsObject> objs = new();

                    world.SearchObjects(objs, _obj.BoundingVolume, true);

                    foreach (PhysicsObject __obj in objs.GetKeys())
                    {
                        if (__obj is LivingEntity livingEntity)
                        {
                            livingEntity.Damage(HyperBeam.Damage);
                        }
                    }

                    _damaged = true;
                }

                System.Diagnostics.Debug.Assert(_damagingIndex <= HyperBeam.DamagingEmitCount);
                if (_damagingIndex == HyperBeam.DamagingEmitCount)
                {
                    return true;
                }

                if (elapsedTime > _SleepTime)
                {
                    _planeIndex = 0;
                    ++_damagingIndex;
                    _startTime += _SleepTime;

                    _damaged = false;

                    return false;
                }

                return false;
            }

            if (_obj is HyperBeamObject obj && obj.BoundingVolume is OrientedBoundingBox obb)
            {

                if (elapsedTime >= HyperBeam.DamagingInterval)
                {
               

                    Vector u = new(1.0, 0.0, 0.0);
                    u = u.Rotate(obb.Angles);

                    Vector d = u * ((_planeIndex++ % ParticlePlanes) * ParticleInterval);
                    Vector o = d + obb.Center - (u * obb.Extents.X);

                    
                    double angleIncrement = 2 * System.Math.PI / CirclePoints;

                    Vector[] vertices = new Vector[CirclePoints];

                    for (int i = 0; i < CirclePoints; ++i)
                    {
                        double angle = i * angleIncrement;
                        double x = CircleRadius * System.Math.Cos(angle);
                        double y = CircleRadius * System.Math.Sin(angle);

                        vertices[i] = new Vector(0, x, y);
                    }

                    Vector.Rotate(obb.Angles, vertices);

                    for (int i = 0; i < CirclePoints; ++i)
                    {
                        Vector v = vertices[i] + o;

                        obj.EmitRgbParticle(v, 1.0, 0.0, 0.0);  // Red
                    }

                    if (_planeIndex % 10 == 0)
                    {
                        world.PlaySound("entity.enderdragon.growl", 0, o, 1.0, 1.5);
                    }

                    _startTime += HyperBeam.DamagingInterval;

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

using Common;
using Sync;
using Containers;

using MinecraftServerEngine;
using MinecraftServerEngine.Entities;
using MinecraftServerEngine.ShapeObjects;
using MinecraftServerEngine.Particles;
using MinecraftServerEngine.Physics;
using MinecraftServerEngine.Physics.BoundingVolumes;

using TestMinecraftServerApplication.SkillProgressNodes;
using TestMinecraftServerApplication.Items;
using System.Runtime.Intrinsics;

namespace TestMinecraftServerApplication
{
    internal sealed class HyperBeamObject : CylinderObject
    {


        private bool _disposed = false;

        private readonly Queue<ISkillProgressNode> _SkillQueue = new();

        private SuperPlayer _caster = null;
        internal SuperPlayer Caster => _caster;

        private static Angles GenerateAngles(EntityAngles _angles)
        {
            Angles offset = Angles.CreateByDegrees(0.0, -90.0, 0.0);
            //Angles angles = Angles.CreateByDegrees(0, -1 * _angles.Yaw, _angles.Pitch) + offset;
            Angles angles = Angles.CreateByDegrees(0.0, -1 * _angles.Yaw, 0.0) + offset;

            return angles;

        }

        private static Vector CalculateCenter(Vector v, EntityAngles _angles)
        {
            Vector u = new(0, 0, 1);

            Angles angles = Angles.CreateByDegrees(0.0, -1 * _angles.Yaw, 0.0);

            Vector d = u.Rotate(angles) * HyperBeam.HalfLength;

            return v + d;
        }

        public HyperBeamObject(SuperPlayer player)
            : base(
                  CalculateCenter(player.Position + new Vector(0.0, player.GetEyeHeight(), 0.0), player.Look),
                  new Vector(HyperBeam.HalfLength, HyperBeam.Radius, HyperBeam.Radius),
                  GenerateAngles(player.Look)
                  )
        {
            System.Diagnostics.Debug.Assert(_SkillQueue != null);
            _SkillQueue.Enqueue(new HyperBeamChargingSkillNode());

            player.DisableDespawning();
            _caster = player;
        }

        protected override void OnDespawn(PhysicsWorld world)
        {
            System.Diagnostics.Debug.Assert(_caster != null);
            _caster.EnableDespawning();
            _caster = null;
        }

        protected override bool HandleDespawning()
        {
            System.Diagnostics.Debug.Assert(_disposed == false);

            System.Diagnostics.Debug.Assert(_SkillQueue != null);
            return _SkillQueue.Empty == true;
        }
    
        public override void StartRoutine(PhysicsWorld _world)
        {
            System.Diagnostics.Debug.Assert(_world != null);

            //using Tree<PhysicsObject> objs = new();

            //_world.SearchObjects(objs, BoundingVolume, true);

            //if (objs.Count > 0)
            //{
            //    MyConsole.Debug($"Detected!: {objs.Count}");
            //}

            //if (BoundingVolume is OrientedBoundingBox obb)
            //{
            //    EmitParticles(Particle.Reddust, obb.Center, 0.0000001, 1, 0.0, 0.0, 0.0);

            //    foreach (Vector vertex in obb.Vertices)
            //    {
            //        EmitParticles(Particle.Reddust, vertex, 0.0000001, 1, 0.0, 0.0, 0.0);
            //    }
            //}

            if (_world is SuperWorld world)
            {
                System.Diagnostics.Debug.Assert(_SkillQueue != null);
                if (_SkillQueue.Empty == false)
                {
                    int currentSkill = 0;
                    ISkillProgressNode skillNode;

                    System.Diagnostics.Debug.Assert(_SkillQueue != null);
                    while (
                        currentSkill++ < _SkillQueue.Length &&
                        _SkillQueue.Dequeue(out skillNode) == true
                        )
                    {
                        System.Diagnostics.Debug.Assert(currentSkill >= 0);

                        System.Diagnostics.Debug.Assert(skillNode != null);
                        if (skillNode.Start(world, this) == true)
                        {
                            //MyConsole.Debug("Close skill!");
                            skillNode.Close(this);

                            skillNode = skillNode.CreateNextNode();

                            
                        }

                        if (skillNode != null)
                        {
                            _SkillQueue.Enqueue(skillNode);
                        }
                    }
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if (_disposed == false)
            {
                System.Diagnostics.Debug.Assert(_caster == null);

                // If disposing equals true, dispose all managed
                // and unmanaged resources.
                if (disposing == true)
                {
                    // Dispose managed resources.
                    _SkillQueue.Dispose();
                }

                // Call the appropriate methods to clean up
                // unmanaged resources here.
                // If disposing is false,
                // only the following code is executed.
                //CloseHandle(handle);
                //handle = IntPtr.Zero;

                // Note disposing has been done.
                _disposed = true;
            }

            base.Dispose(disposing);
        }

    }
}

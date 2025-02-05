
using Common;

using MinecraftServerEngine;
using MinecraftServerEngine.Physics;
using System;
using TestMinecraftServerApplication.Items;

namespace TestMinecraftServerApplication.SkillProgressNodes
{
    internal sealed class PhoenixFeatherSkillNode : ISkillProgressNode
    {
        public static readonly Vector[] RandomPointsInSphere = new Vector[]
        {
            new Vector(0.0, 0.0, 1.0),
            new Vector(0.0, 0.0, -1.0),
            new Vector(0.0, 1.0, 0.0),
            new Vector(0.0, -1.0, 0.0),
            new Vector(1.0, 0.0, 0.0),
            new Vector(-1.0, 0.0, 0.0),
            new Vector(0.707, 0.707, 0.0),
            new Vector(0.707, -0.707, 0.0),
            new Vector(-0.707, 0.707, 0.0),
            new Vector(-0.707, -0.707, 0.0),
            new Vector(0.707, 0.0, 0.707),
            new Vector(0.707, 0.0, -0.707),
            new Vector(-0.707, 0.0, 0.707),
            new Vector(-0.707, 0.0, -0.707),
            new Vector(0.0, 0.707, 0.707),
            new Vector(0.0, 0.707, -0.707),
            new Vector(0.0, -0.707, 0.707),
            new Vector(0.0, -0.707, -0.707),
            new Vector(0.577, 0.577, 0.577),
            new Vector(0.577, 0.577, -0.577),
            new Vector(0.577, -0.577, 0.577),
            new Vector(0.577, -0.577, -0.577),
            new Vector(-0.577, 0.577, 0.577),
            new Vector(-0.577, 0.577, -0.577),
            new Vector(-0.577, -0.577, 0.577),
            new Vector(-0.577, -0.577, -0.577),
            new Vector(0.866, 0.5, 0.0),
            new Vector(0.866, -0.5, 0.0),
            new Vector(-0.866, 0.5, 0.0),
            new Vector(-0.866, -0.5, 0.0),
            new Vector(0.5, 0.866, 0.0),
            new Vector(0.5, -0.866, 0.0),
            new Vector(-0.5, 0.866, 0.0),
            new Vector(-0.5, -0.866, 0.0),
            new Vector(0.866, 0.0, 0.5),
            new Vector(0.866, 0.0, -0.5),
            new Vector(-0.866, 0.0, 0.5),
            new Vector(-0.866, 0.0, -0.5),
            new Vector(0.0, 0.866, 0.5),
            new Vector(0.0, 0.866, -0.5),
            new Vector(0.0, -0.866, 0.5),
            new Vector(0.0, -0.866, -0.5),
            new Vector(0.707, 0.707, 0.707),
            new Vector(0.707, 0.707, -0.707),
            new Vector(0.707, -0.707, 0.707),
            new Vector(0.707, -0.707, -0.707),
            new Vector(-0.707, 0.707, 0.707),
            new Vector(-0.707, 0.707, -0.707),
            new Vector(-0.707, -0.707, 0.707),
            new Vector(-0.707, -0.707, -0.707),
        };

        public string Name => $"{PhoenixFeather.Name}'s Skill";

        private bool _init = false;

        private Time _lastEmitTime = Time.Zero;
        private int _currentEmit = 0;

        public ISkillProgressNode CreateNextNode()
        {
            return null;
        }

        public bool Start(SuperWorld world, SuperPlayer player)
        {
            System.Diagnostics.Debug.Assert(world != null);
            System.Diagnostics.Debug.Assert(player != null);

            if (_init == false)
            {
                _lastEmitTime = Time.Now() - PhoenixFeather.EmitInterval;

                System.Diagnostics.Debug.Assert(player != null);
                System.Diagnostics.Debug.Assert(PhoenixFeather.MovementSpeedIncrease >= 0.0);
                player.AddMovementSpeed(PhoenixFeather.MovementSpeedIncrease);

                player.EmitParticles(PhoenixFeather.PhoenixParticle, 0.8, 1_000);

                player.HealFully();

                world.PlaySound("block.anvil.land", 4, player.Position, 1.0, 2.0);

                _init = true;
            }

            Time elapsedTime = Time.Now() - _lastEmitTime;
            if (elapsedTime >= PhoenixFeather.EmitInterval)
            {
                int emits = (int)System.Math.Floor(
                    (double)elapsedTime.Amount / (double)PhoenixFeather.EmitInterval.Amount
                    );

                if (_currentEmit + emits > PhoenixFeather.MaxEmits)
                {
                    emits = PhoenixFeather.MaxEmits - _currentEmit;
                }

                for (int i = 0; i < emits; ++i)
                {
                    System.Diagnostics.Debug.Assert(player != null);
                    player.AddAdditionalHealth(PhoenixFeather.AdditionalHeartsIncrease);

                    if (i % 2 == 0)
                    {
                        world.PlaySound("block.note.bell", 4, player.Position, 1.0, 2.0);
                    }
                    else
                    {
                        world.PlaySound("block.note.chime", 4, player.Position, 1.0, 2.0);
                    }

                    for (int k = 0; k < PhoenixFeather.HealParticleCountInOneEmit; ++k)
                    {
                        int j = new System.Random().Next(RandomPointsInSphere.Length);
                        player.EmitParticles(
                            PhoenixFeather.HealParticle,
                            new Vector(0.0, player.GetEyeHeight(), 0.0) + RandomPointsInSphere[j],
                            0.4,
                            1
                        );
                    }

                    
                }

                System.Diagnostics.Debug.Assert(emits > 0);
                _currentEmit += emits;

                _lastEmitTime = Time.Now();
            }

            System.Diagnostics.Debug.Assert(_currentEmit >= 0);
            System.Diagnostics.Debug.Assert(_currentEmit <= PhoenixFeather.MaxEmits);
            return _currentEmit == PhoenixFeather.MaxEmits;
        }

        public void Close(SuperPlayer player)
        {
            System.Diagnostics.Debug.Assert(player != null);

            System.Diagnostics.Debug.Assert(PhoenixFeather.MovementSpeedIncrease >= 0.0);
            player.RemoveMovementSpeed(PhoenixFeather.MovementSpeedIncrease);

            PhoenixFeather.CanPurchase = true;
        }
    }
}

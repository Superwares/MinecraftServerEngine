

using Common;

using MinecraftServerEngine;
using MinecraftServerEngine.Physics;

using TestMinecraftServerApplication.Items;

namespace TestMinecraftServerApplication.SkillProgressNodes
{
    internal sealed class StoneOfSwiftnessSkill : ISkillProgressNode
    {
        public string Name => $"{StoneOfSwiftness.Name}'s Skill";


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
                _lastEmitTime = Time.Now() - StoneOfSwiftness.EmitParticleInterval;

                System.Diagnostics.Debug.Assert(player != null);
                System.Diagnostics.Debug.Assert(StoneOfSwiftness.MovementSpeedIncrease >= 0.0);
                player.AddMovementSpeed(StoneOfSwiftness.MovementSpeedIncrease);

                world.PlaySound("block.anvil.land", 4, player.Position, 0.8, 1.5);

                _init = true;
            }

            Time elapsedTime = Time.Now() - _lastEmitTime;
            if (elapsedTime >= StoneOfSwiftness.EmitParticleInterval)
            {
                int emits = (int)System.Math.Floor(
                    (double)elapsedTime.Amount / (double)StoneOfSwiftness.EmitParticleInterval.Amount
                    );

                if (_currentEmit + emits > StoneOfSwiftness.MaxParticleEmits)
                {
                    emits = StoneOfSwiftness.MaxParticleEmits - _currentEmit;
                }

                for (int i = 0; i < _currentEmit; ++i)
                {
                    System.Diagnostics.Debug.Assert(player != null);
                    player.EmitParticles(Particle.Spell, new Vector(0.0, 0.1, 0.0), 0.1, 1);
                }

                System.Diagnostics.Debug.Assert(emits > 0);
                _currentEmit += emits;

                _lastEmitTime = Time.Now();
            }

            System.Diagnostics.Debug.Assert(_currentEmit >= 0);
            System.Diagnostics.Debug.Assert(_currentEmit <= StoneOfSwiftness.MaxParticleEmits);
            return _currentEmit == StoneOfSwiftness.MaxParticleEmits;
        }

        public void Close(SuperPlayer player)
        {
            System.Diagnostics.Debug.Assert(player != null);

            System.Diagnostics.Debug.Assert(StoneOfSwiftness.MovementSpeedIncrease >= 0.0);
            player.RemoveMovementSpeed(StoneOfSwiftness.MovementSpeedIncrease);
        }

    }

}

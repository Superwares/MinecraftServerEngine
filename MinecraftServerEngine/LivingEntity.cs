using Sync;

namespace MinecraftServerEngine
{
    using PhysicsEngine;

    public abstract class LivingEntity : Entity
    {
        private bool _disposed = false;

        protected readonly Locker LockerHealth = new();
        protected float _maxHealth = 20.0F;
        protected float _health;
        public float MaxHealth => _maxHealth;
        public float Health => _health;

        private protected LivingEntity(
            System.Guid uniqueId,
            Vector p, Look look,
            bool noGravity,
            Hitbox hitbox,
            double m, double maxStepLevel)
            : base(uniqueId, p, look, noGravity, hitbox, m, maxStepLevel)
        {
            _health = _maxHealth;
        }

        ~LivingEntity()
        {
            System.Diagnostics.Debug.Assert(false);

            //Dispose(false);
        }

        protected internal override bool IsDead()
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            return _health <= 0.0D;
        }

        public virtual void Damage(float amount)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(amount >= 0.0D);
            System.Diagnostics.Debug.Assert(_health > 0.0D);
            System.Diagnostics.Debug.Assert(_health <= _maxHealth);

            LockerHealth.Hold();

            _health -= amount;

            SetEntityStatus(2);

            if (_health <= 0.0D)
            {
                SetEntityStatus(3);
            }

            LockerHealth.Release();
        }

        public override void Dispose()
        {
            // Assertions.
            System.Diagnostics.Debug.Assert(!_disposed);

            // Release resources.
            LockerHealth.Dispose();

            // Finish.
            base.Dispose();
            _disposed = true;
        }

    }

}

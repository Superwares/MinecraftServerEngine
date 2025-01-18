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

            Dispose(false);
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

        protected override void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if (_disposed == false)
            {

                // If disposing equals true, dispose all managed
                // and unmanaged resources.
                if (disposing == true)
                {
                    // Dispose managed resources.
                    LockerHealth.Dispose();
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

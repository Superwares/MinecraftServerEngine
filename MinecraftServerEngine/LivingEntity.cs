using Common;
using Sync;

namespace MinecraftServerEngine
{
    using PhysicsEngine;

    public abstract class LivingEntity : Entity
    {
        public const double DefaultMovementSpeed = 0.099999988079071;

        private bool _disposed = false;

        /**
         * The main health is _health.
         * If the additional health is greater than 0 and main health is zero, 
         * the living entity is dead.
         * 
         * The additional health cannot be self-healed, only damaged.
         */
        protected readonly Locker LockerHealths = new();
        protected double _additionalHealth = 0.0;
        protected double _maxHealth = 20.0;
        protected double _health;
        public double AdditionalHealth
        {
            get
            {
                if (_disposed == true)
                {
                    throw new System.ObjectDisposedException(GetType().Name);
                }

                System.Diagnostics.Debug.Assert(_additionalHealth >= 0.0);
                return _additionalHealth;
            }
        }
        public double MaxHealth
        {
            get
            {
                if (_disposed == true)
                {
                    throw new System.ObjectDisposedException(GetType().Name);
                }

                System.Diagnostics.Debug.Assert(_maxHealth >= 0.0);
                System.Diagnostics.Debug.Assert(_maxHealth <= 1024.0);
                return _maxHealth;
            }
        }
        public double Health
        {
            get
            {
                if (_disposed == true)
                {
                    throw new System.ObjectDisposedException(GetType().Name);
                }

                System.Diagnostics.Debug.Assert(_health >= 0.0);
                System.Diagnostics.Debug.Assert(_health <= 1024.0);
                System.Diagnostics.Debug.Assert(_health <= _maxHealth);
                return _health;
            }
        }

        protected readonly Locker LockerMovementSpeed = new();
        protected double _movementSpeed = DefaultMovementSpeed;
        public double MovementSpeed
        {
            get
            {
                if (_disposed == true)
                {
                    throw new System.ObjectDisposedException(GetType().Name);
                }

                System.Diagnostics.Debug.Assert(_movementSpeed >= 0.0);
                System.Diagnostics.Debug.Assert(_movementSpeed <= 1024.0);
                return _movementSpeed;
            }
        }


        private Time _lastAttackTime = Time.Now();

        private protected LivingEntity(
            System.Guid uniqueId,
            Vector p, Angles look,
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

        public abstract double GetEyeHeight();

        public Vector GetEyeOrigin()
        {
            double eyeHeight = GetEyeHeight();

            double x = Position.X,
                y = Position.Y + eyeHeight,
                z = Position.Z;

            return new Vector(x, y, z);
        }

        protected internal override bool HandleDeath()
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(_health >= 0.0D);
            return _health == 0.0D;
        }

        protected internal virtual void OnAttack(World world, double attackCharge) { }
        protected internal virtual void OnAttack(World world, ItemStack stack, double attackCharge) { }
        protected internal virtual void OnItemBreak(World world, ItemStack stack) { }
        protected internal virtual void OnUseItem(World world, ItemStack stack) { }
        protected internal virtual void OnUseEntity(World world, Entity entity) { }

        private double CalculateAttackCharge()
        {
            Time currentTime = Time.Now();
            Time MaxTime = Time.FromMilliseconds(250);
            //Time MaxTime = Time.FromSeconds(1);

            Time elapsedTime = currentTime - _lastAttackTime;

            //MyConsole.Debug($"elapsedTime: {elapsedTime:F2}");
            double normalized = (double)elapsedTime.Amount / (double)MaxTime.Amount;

            //MyConsole.Debug($"normalized: {normalized:F2}");
            return System.Math.Clamp(normalized, 0.0, 1.0); // Clamp to [0, 1]
        }


        internal virtual void _Attack(World world)
        {
            System.Diagnostics.Debug.Assert(world != null);

            double attackCharge = CalculateAttackCharge();

            OnAttack(world, attackCharge);

            if (_noRendering)
            {
                System.Diagnostics.Debug.Assert(Renderers.Empty);
                return;
            }

            System.Diagnostics.Debug.Assert(Renderers != null);
            foreach (EntityRenderer renderer in Renderers.GetKeys())
            {
                System.Diagnostics.Debug.Assert(renderer != null);
                renderer.Animate(Id, EntityAnimation.SwingMainArm);
                //renderer.Animate(Id, EntityAnimation.TakeDamage);
            }

            _lastAttackTime = Time.Now();
        }

        internal virtual void _Attack(World world, ItemStack stack)
        {
            System.Diagnostics.Debug.Assert(world != null);
            System.Diagnostics.Debug.Assert(stack != null);

            double attackCharge = CalculateAttackCharge();

            OnAttack(world, stack, attackCharge);

            if (_noRendering == true)
            {
                System.Diagnostics.Debug.Assert(Renderers.Empty);
                return;
            }

            System.Diagnostics.Debug.Assert(Renderers != null);
            foreach (EntityRenderer renderer in Renderers.GetKeys())
            {
                System.Diagnostics.Debug.Assert(renderer != null);
                renderer.Animate(Id, EntityAnimation.SwingMainArm);
            }

            _lastAttackTime = Time.Now();
        }

        internal virtual void _ItemBreak(World world, ItemStack stack)
        {
            System.Diagnostics.Debug.Assert(world != null);

            System.Diagnostics.Debug.Assert(stack != null);

            OnItemBreak(world, stack);
        }

        protected virtual (bool, double) _Damage(double amount)
        {
            System.Diagnostics.Debug.Assert(amount >= 0.0D);

            System.Diagnostics.Debug.Assert(_disposed == false);

            System.Diagnostics.Debug.Assert(LockerHealths != null);
            LockerHealths.Hold();

            try
            {

                if (amount == 0.0D)
                {
                    return (false, _health);
                }

                System.Diagnostics.Debug.Assert(_health >= 0.0);
                if (_health == 0.0)
                {
                    return (false, 0.0);
                }

                System.Diagnostics.Debug.Assert(_additionalHealth >= 0.0);
                if (_additionalHealth > 0.0)
                {
                    System.Diagnostics.Debug.Assert(amount >= 0.0);
                    _additionalHealth -= amount;

                    if (_additionalHealth <= 0.0)
                    {
                        amount = -_additionalHealth;
                        _additionalHealth = 0.0;
                    }
                    else
                    {
                        amount = 0.0;
                    }
                }

                System.Diagnostics.Debug.Assert(_health >= 0.0D);
                System.Diagnostics.Debug.Assert(_health <= _maxHealth);
                System.Diagnostics.Debug.Assert(amount >= 0.0);
                _health -= amount;

                SetEntityStatus(2);

                if (_health <= 0.0)
                {
                    _health = 0.0;

                    //using Buffer buffer = new();
                    //buffer.WriteShort(-1);

                    //byte[] data = buffer.ReadData();

                    //UpdateEntityEquipmentsData((data, data));
                    //SetEntityStatus(3);

                }

                System.Diagnostics.Debug.Assert(_health >= 0.0D);
                System.Diagnostics.Debug.Assert(_health <= _maxHealth);
                return (true, _health);
            }
            finally
            {
                System.Diagnostics.Debug.Assert(LockerHealths != null);
                LockerHealths.Release();

            }
        }

        public (bool, double) Damage(double amount)
        {
            if (amount < 0.0)
            {
                throw new System.ArgumentOutOfRangeException(nameof(amount), "Amount cannot be negative.");
            }

            if (_disposed == true)
            {
                throw new System.ObjectDisposedException(GetType().Name);
            }

            return _Damage(amount);
        }

        protected virtual double _Heal(double amount)
        {
            System.Diagnostics.Debug.Assert(amount >= 0.0D);

            System.Diagnostics.Debug.Assert(_disposed == false);

            System.Diagnostics.Debug.Assert(LockerHealths != null);
            LockerHealths.Hold();

            try
            {

                if (amount == 0.0)
                {
                    return _health;
                }

                System.Diagnostics.Debug.Assert(_health >= 0.0D);
                System.Diagnostics.Debug.Assert(_health <= _maxHealth);
                System.Diagnostics.Debug.Assert(amount >= 0.0);
                _health += amount;

                System.Diagnostics.Debug.Assert(_maxHealth > 0.0D);
                if (_health >= _maxHealth)
                {
                    _health = _maxHealth;
                }

                return _health;
            }
            finally
            {
                System.Diagnostics.Debug.Assert(LockerHealths != null);
                LockerHealths.Release();

            }
        }

        protected virtual void _SetMaxHealth(double amount)
        {
            System.Diagnostics.Debug.Assert(amount > 0.0D);

            System.Diagnostics.Debug.Assert(_disposed == false);

            System.Diagnostics.Debug.Assert(LockerHealths != null);
            LockerHealths.Hold();

            try
            {

                System.Diagnostics.Debug.Assert(amount > 0.0);
                _maxHealth = amount;

                if (_health > _maxHealth)
                {
                    _health = _maxHealth;
                }

            }
            finally
            {
                System.Diagnostics.Debug.Assert(LockerHealths != null);
                LockerHealths.Release();
            }
        }

        protected virtual void _SetAdditionalHealth(double amount)
        {
            System.Diagnostics.Debug.Assert(amount >= 0.0D);

            System.Diagnostics.Debug.Assert(_disposed == false);

            System.Diagnostics.Debug.Assert(LockerHealths != null);
            LockerHealths.Hold();

            try
            {
                System.Diagnostics.Debug.Assert(amount >= 0.0);
                _additionalHealth = amount;

            }
            finally
            {
                System.Diagnostics.Debug.Assert(LockerHealths != null);
                LockerHealths.Release();
            }
        }

        protected virtual void _SetMovementSpeed(double amount)
        {
            System.Diagnostics.Debug.Assert(amount >= 0.0);
            System.Diagnostics.Debug.Assert(amount <= 1024.0);

            System.Diagnostics.Debug.Assert(_disposed == false);

            System.Diagnostics.Debug.Assert(LockerMovementSpeed != null);
            LockerMovementSpeed.Hold();

            try
            {
                System.Diagnostics.Debug.Assert(amount >= 0.0);
                System.Diagnostics.Debug.Assert(amount <= 1024.0);
                _movementSpeed = amount;

            }
            finally
            {
                System.Diagnostics.Debug.Assert(LockerMovementSpeed != null);
                LockerMovementSpeed.Release();
            }
        }

        public void Heal(double amount)
        {
            if (amount < 0.0)
            {
                throw new System.ArgumentOutOfRangeException(nameof(amount), "Amount cannot be negative.");
            }

            if (_disposed == true)
            {
                throw new System.ObjectDisposedException(GetType().Name);
            }

            System.Diagnostics.Debug.Assert(amount >= 0.0D);

            System.Diagnostics.Debug.Assert(_disposed == false);

            _Heal(amount);
        }

        public void SetMaxHealth(double amount)
        {
            if (amount <= 0.0)
            {
                throw new System.ArgumentOutOfRangeException(nameof(amount), "Amount cannot be negative or zero.");
            }

            if (_disposed == true)
            {
                throw new System.ObjectDisposedException(GetType().Name);
            }

            System.Diagnostics.Debug.Assert(amount > 0.0D);

            System.Diagnostics.Debug.Assert(_disposed == false);

            _SetMaxHealth(amount);
        }

        public void SetAdditionalHealth(double amount)
        {
            if (amount < 0.0)
            {
                throw new System.ArgumentOutOfRangeException(nameof(amount), "Amount cannot be negative.");
            }

            if (_disposed == true)
            {
                throw new System.ObjectDisposedException(GetType().Name);
            }

            System.Diagnostics.Debug.Assert(amount >= 0.0D);

            System.Diagnostics.Debug.Assert(_disposed == false);

            _SetAdditionalHealth(amount);
        }

        public void SetMovementSpeed(double amount)
        {
            if (amount < 0.0)
            {
                throw new System.ArgumentOutOfRangeException(nameof(amount), "Amount cannot be negative.");
            }
            if (amount > 1024.0)
            {
                throw new System.ArgumentOutOfRangeException(nameof(amount), "Amount cannot be greater than 1024.0.");
            }

            if (_disposed == true)
            {
                throw new System.ObjectDisposedException(GetType().Name);
            }

            System.Diagnostics.Debug.Assert(amount >= 0.0);
            System.Diagnostics.Debug.Assert(amount <= 1024.0);

            System.Diagnostics.Debug.Assert(_disposed == false);

            _SetMovementSpeed(amount);
        }

        public void HealFully()
        {
            System.Diagnostics.Debug.Assert(_disposed == false);

            _Heal(MaxHealth);
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
                    LockerHealths.Dispose();
                    LockerMovementSpeed.Dispose();
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

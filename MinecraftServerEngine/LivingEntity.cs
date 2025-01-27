using Common;
using Sync;

namespace MinecraftServerEngine
{
    using PhysicsEngine;

    public abstract class LivingEntity : Entity
    {
        private bool _disposed = false;

        protected readonly Locker LockerHealth = new();
        protected double _maxHealth = 20.0F;
        protected double _health;
        public double MaxHealth => _maxHealth;
        public double Health => _health;

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

            return _health <= 0.0D;
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

        public virtual (bool, double) Damage(double amount)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(amount >= 0.0D);
            System.Diagnostics.Debug.Assert(_health >= 0.0D);
            System.Diagnostics.Debug.Assert(_health <= _maxHealth);

            LockerHealth.Hold();

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

                _health -= amount;

                SetEntityStatus(2);

                if (_health <= 0.0D)
                {
                    _health = 0.0F;

                    //using Buffer buffer = new();
                    //buffer.WriteShort(-1);

                    //byte[] data = buffer.ReadData();

                    //UpdateEntityEquipmentsData((data, data));
                    //SetEntityStatus(3);

                }

                return (true, _health);
            }
            finally
            {
                LockerHealth.Release();

            }
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

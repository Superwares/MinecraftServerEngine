using Sync;

using MinecraftServerEngine.Items;
using MinecraftServerEngine.Renderers;
using MinecraftServerEngine.Physics;

namespace MinecraftServerEngine.Entities
{
    public sealed class ItemEntity : Entity
    {
        private static readonly Hitbox DefaultHitbox = new(0.25D, 0.25D);
        public const double DefaultMass = 1.0D;
        public const double DefaultMaxStepLevel = 0.0D;


        private bool _disposed = false;

        private readonly Locker DefaultLocker = new();
        private ItemStack _stack;


        public ItemEntity(ItemStack stack, Vector p)
            : base(
                  System.Guid.NewGuid(), p, new EntityAngles(0.0F, 0.0F), false,
                  DefaultHitbox,
                  DefaultMass, DefaultMaxStepLevel)
        {
            System.Diagnostics.Debug.Assert(stack != null);
            _stack = stack;
        }

        ~ItemEntity()
        {
            System.Diagnostics.Debug.Assert(false);

            Dispose(false);
        }

        private protected override Hitbox GetHitbox()
        {
            System.Diagnostics.Debug.Assert(_disposed == false);

            return DefaultHitbox;
        }

        private protected override void RenderSpawning(EntityRenderer renderer)
        {
            System.Diagnostics.Debug.Assert(renderer != null);

            System.Diagnostics.Debug.Assert(_disposed == false);

            renderer.SpawnItemEntity(
                Id, UniqueId,
                Position, Look,
                _stack);
        }

        public override void StartRoutine(PhysicsWorld world)
        {
            System.Diagnostics.Debug.Assert(_disposed == false);

            System.Diagnostics.Debug.Assert(world != null);
        }

        protected internal override bool HandleDespawning()
        {
            System.Diagnostics.Debug.Assert(_disposed == false);

            return _stack == null;
        }

        public void PickUp(AbstractPlayer player)
        {
            if (_disposed == true)
            {
                throw new System.ObjectDisposedException(GetType().Name);
            }

            if (player == null)
            {
                return;
            }

            System.Diagnostics.Debug.Assert(player != null);

            System.Diagnostics.Debug.Assert(_disposed == false);



            System.Diagnostics.Debug.Assert(DefaultLocker != null);
            DefaultLocker.Hold();

            try
            {
                if (_stack == null)
                {
                    return;
                }

                System.Diagnostics.Debug.Assert(_stack.Count >= Item.MinCount);
                int pickupCount = _stack.Count;

                System.Diagnostics.Debug.Assert(player != null);
                if (player.GiveItemStack(ref _stack) == true)
                {
                    System.Diagnostics.Debug.Assert(Renderers != null);
                    if (Renderers.Empty == false)
                    {
                        foreach (EntityRenderer renderer in Renderers.GetKeys())
                        {
                            System.Diagnostics.Debug.Assert(renderer != null);
                            renderer.CollectItemEntity(Id, player.Id, pickupCount);
                        }
                    }
                }
            }
            finally
            {
                System.Diagnostics.Debug.Assert(DefaultLocker != null);
                DefaultLocker.Release();
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
                    DefaultLocker.Dispose();
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

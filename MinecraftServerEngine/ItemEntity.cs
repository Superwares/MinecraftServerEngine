
namespace MinecraftServerEngine
{
    using PhysicsEngine;

    public sealed class ItemEntity : Entity
    {
        private static readonly Hitbox DefaultHitbox = new(0.25D, 0.25D);
        public const double DefaultMass = 1.0D;
        public const double DefaultMaxStepLevel = 0.0D;


        private bool _disposed = false;


        private readonly ItemStack Stack;


        public ItemEntity(ItemStack stack, Vector p)
            : base(System.Guid.NewGuid(), p, new Angles(0.0F, 0.0F), false,
                  DefaultHitbox,
                  DefaultMass, DefaultMaxStepLevel)
        {
            System.Diagnostics.Debug.Assert(stack != null);

            Stack = stack;
        }

        ~ItemEntity()
        {
            System.Diagnostics.Debug.Assert(false);

            Dispose(false);
        }

        private protected override Hitbox GetHitbox()
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            return DefaultHitbox;
        }

        private protected override void RenderSpawning(EntityRenderer renderer)
        {
            System.Diagnostics.Debug.Assert(renderer != null);

            System.Diagnostics.Debug.Assert(!_disposed);

            renderer.SpawnItemEntity(
                Id, UniqueId,
                Position, Look,
                Stack);
        }

        public override void StartRoutine(PhysicsWorld world)
        {
            System.Diagnostics.Debug.Assert(world != null);
        }

        public void PickUp()
        {
            throw new System.NotImplementedException();
        }


        protected override void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if (_disposed == false)
            {
                System.Diagnostics.Debug.Assert(Stack != null);

                // If disposing equals true, dispose all managed
                // and unmanaged resources.
                if (disposing == true)
                {
                    // Dispose managed resources.

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

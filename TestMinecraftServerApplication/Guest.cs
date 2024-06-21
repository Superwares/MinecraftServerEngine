using MinecraftServerEngine;
using MinecraftPhysicsEngine;

namespace TestServerApplication
{
    public sealed class Guest : Player
    {
        private bool _disposed = false;

        public Guest(System.Guid userId, Vector p, Look look) 
            : base(userId, p, look) { }

        ~Guest() => System.Diagnostics.Debug.Assert(false);

        public override void StartRoutine(long serverTicks, World world)
        {
            System.Diagnostics.Debug.Assert(!_disposed);
            
        }

        public override void Dispose()
        {
            // Assertions.
            System.Diagnostics.Debug.Assert(!_disposed);

            // Release reousrces.

            // Finish.
            base.Dispose();
            _disposed = true;
        }
    }
}

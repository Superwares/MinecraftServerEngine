using MinecraftServerEngine;
using MinecraftPhysicsEngine;
using Common;

namespace TestServerApplication
{
    internal sealed class Lobby : World
    {
        private bool _disposed = false;

        private readonly Vector PosSpawning = new(0.0D, 101.0D, 0.0D);
        private readonly Look LookSpawning = new(0.0F, 0.0F);

        public Lobby() : base() { }

        ~Lobby() => System.Diagnostics.Debug.Assert(false);

        public override bool CanJoinWorld()
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            return true;
        }

        protected override bool DetermineToDespawnPlayerOnDisconnect()
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            return false;
        }

        protected override Player CreatePlayer(System.Guid userId)
        {
            return new Guest(userId, PosSpawning, LookSpawning);
        }

        public override void Dispose()
        {
            // Assertions.
            System.Diagnostics.Debug.Assert(!_disposed);

            // Release resources.

            // Finish.
            base.Dispose();
            _disposed = true;
        }

    }
}

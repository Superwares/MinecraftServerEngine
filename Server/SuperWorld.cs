using Protocol;

namespace Server
{
    internal sealed class SuperWorld : World
    {
        private bool _disposed = false;

        private bool _gameInProgress = false;

        public SuperWorld() : base(new(0, 70, 0), new(0,0)) { }

        ~SuperWorld() => System.Diagnostics.Debug.Assert(false);

        protected override bool DetermineNewPlayerCanJoinWorld()
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            return true;
        }

        protected override bool DetermineToDespawnPlayerOnDisconnect()
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            return false;
        }

        protected override void StartPlayerRoutine(long serverTicks, Player player)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            // If player is dead, spawn to the spanwpoint of their team.
        }

        protected override void StartSubRoutine(long serverTicks)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            // check player is dead. if dead, player must be respanwed.
        }

        public override void Dispose()
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            // Assertion.

            // Release resources.

            base.Dispose();
            _disposed = true;
        }

    }
}

using Protocol;

namespace Server
{
    internal sealed class SuperWorld : World
    {
        private bool _disposed = false;

        private bool _gameInProgress = false;

        public SuperWorld() : base(new(0, 70, 0), new(0,0)) { }

        ~SuperWorld() => System.Diagnostics.Debug.Assert(false);

        protected override bool CanJoinWorld()
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            return true;
        }

        protected override bool DetermineToDespawnPlayerOnDisconnect()
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            return false;
        }

        public override void Dispose()
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            // Assertion.

            // Release resources.

            // Finish.
            base.Dispose();
            _disposed = true;
        }

    }
}

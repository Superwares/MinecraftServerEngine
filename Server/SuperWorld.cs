using Protocol;

namespace Server
{
    internal class SuperWorld : World
    {
        private bool _disposed = false;

        public SuperWorld() : base(new(0, 61, 0), new(0,0)) { }

        protected override bool CanJoinWorld()
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            return base.CanJoinWorld();
        }

        protected override void StartPlayerRoutine(long serverTicks, Player player)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            // If player is dead, spawn to the spanwpoint of their team.
        }

        public override void StartRoutine(long serverTicks)
        {
            System.Diagnostics.Debug.Assert(!_disposed);
        }

        protected override void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                // Assertion.

                if (disposing == true)
                {
                    // Release managed resources.
                }

                // Release unmanaged resources.

                _disposed = true;
            }

            base.Dispose(disposing);
        }

    }
}

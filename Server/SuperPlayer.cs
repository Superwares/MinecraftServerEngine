using Protocol;

namespace Server
{
    public sealed class SuperPlayer : Player
    {
        private bool _disposed = false;

        public SuperPlayer() : base() { }

        ~SuperPlayer() => System.Diagnostics.Debug.Assert(false);

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

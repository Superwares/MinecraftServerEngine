using MinecraftServerEngine;
using MinecraftServerEngine.PhysicsEngine;
using Common;

namespace TestMinecraftServerApplication
{
    internal sealed class Lobby : World
    {
        private static readonly Vector PosSpawning = new(0.0D, 3.0D, 0.0D);
        private static readonly Angles LookSpawning = new(0.0F, 0.0F);

        private bool _disposed = false;

        public Lobby() : base() { }

        ~Lobby()
        {
            System.Diagnostics.Debug.Assert(false);

            Dispose(false);
        }

        public override bool CanJoinWorld()
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            return true;
        }

        protected override bool DetermineToDespawnPlayerOnDisconnect()
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            return true;
        }

        protected override void StartRoutine()
        {
            System.Diagnostics.Debug.Assert(!_disposed);


        }

        protected override AbstractPlayer CreatePlayer(UserId id)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(id != UserId.Null);

            return new Guest(id, PosSpawning, LookSpawning);
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

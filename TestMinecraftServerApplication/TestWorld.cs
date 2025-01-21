
using Common;

using MinecraftPrimitives;
using MinecraftServerEngine;
using MinecraftServerEngine.PhysicsEngine;

namespace TestMinecraftServerApplication
{
    internal sealed class Lobby : World
    {
        private static readonly Vector PosSpawning = new(0.0D, 3.0D, 0.0D);
        private static readonly Angles LookSpawning = new(0.0F, 0.0F);

        private bool _disposed = false;

        private static Time _INTERVAL = Time.FromSeconds(10);
        private Time _start = Time.Now();

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

            Time end = Time.Now();

            if (end - _start >= _INTERVAL)
            {
                _start = end;
                return true;
            }

            return false;
        }

        protected override void StartRoutine()
        {
            System.Diagnostics.Debug.Assert(!_disposed);


        }

        protected override AbstractPlayer CreatePlayer(UserId userId, string username)
        {
            System.Diagnostics.Debug.Assert(_disposed == false);

            System.Diagnostics.Debug.Assert(userId != UserId.Null);
            System.Diagnostics.Debug.Assert(username != null && string.IsNullOrEmpty(username) == false);

            return new Guest(userId, username, PosSpawning, LookSpawning);
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

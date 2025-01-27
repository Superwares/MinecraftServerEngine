
using Containers;

namespace TestMinecraftServerApplication
{
    public sealed class GameContext : System.IDisposable
    {
        private bool _disposed = false;

        public const int MinPlayers = 2;

        private readonly Map<int, SuperPlayer> _playersById = new();
        private readonly Map<string, SuperPlayer> _playersByUsername = new();

        private bool _started = false;

        public GameContext()
        {
        }

        ~GameContext()
        {
            System.Diagnostics.Debug.Assert(false);

            Dispose(false);
        }

        public void Add(SuperPlayer player)
        {
            System.Diagnostics.Debug.Assert(_disposed == false);

            throw new System.NotImplementedException();
        }

        public void Remove()
        {
            System.Diagnostics.Debug.Assert(_disposed == false);

            throw new System.NotImplementedException();
        }

        public void Start()
        {
            System.Diagnostics.Debug.Assert(_disposed == false);

            System.Diagnostics.Debug.Assert(_playersById.Count >= MinPlayers);
            System.Diagnostics.Debug.Assert(_playersByUsername.Count >= MinPlayers);

            System.Diagnostics.Debug.Assert(_started == false);
            _started = true;
        }

        public void Stop()
        {
            System.Diagnostics.Debug.Assert(_disposed == false);

            System.Diagnostics.Debug.Assert(_started == true);
            _started = false;

            _playersById.Flush();
            _playersByUsername.Flush();
        }

        public void Dispose()
        {
            Dispose(true);
            System.GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if (_disposed == false)
            {
                System.Diagnostics.Debug.Assert(_started == false);

                // If disposing equals true, dispose all managed
                // and unmanaged resources.
                if (disposing == true)
                {
                    // Dispose managed resources.
                    _playersByUsername.Dispose();
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

        }

    }
}

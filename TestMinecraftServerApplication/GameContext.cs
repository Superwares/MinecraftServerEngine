
using Common;
using Containers;
using Sync;

namespace TestMinecraftServerApplication
{
    public sealed class GameContext : System.IDisposable
    {
        private bool _disposed = false;

        public const int MinPlayers = 2;
        public const int MaxPlayers = 18;

        private readonly Locker LockerPlayers = new();
        private readonly List<SuperPlayer> _players = new();

        public int CurrentPlayers
        {
            get
            {
                System.Diagnostics.Debug.Assert(LockerPlayers != null);
                LockerPlayers.Hold();

                try
                {
                    return _players.Count;
                }
                finally
                {
                    System.Diagnostics.Debug.Assert(LockerPlayers != null);
                    LockerPlayers.Release();
                }
            }
        }

        public bool CanStart
        {
            get
            {
                System.Diagnostics.Debug.Assert(LockerPlayers != null);
                LockerPlayers.Hold();

                try
                {
                    return _players.Count >= MinPlayers;
                }
                finally
                {
                    System.Diagnostics.Debug.Assert(LockerPlayers != null);
                    LockerPlayers.Release();
                }
            }
        }

        //public readonly Time RoundInterval;
        //public readonly int RoundCount;

        private bool _started = false;
        public bool IsStarted => _started;
        //private Time _startTime = Time.Zero;

        //public Time StartTime
        //{
        //    get
        //    {
        //        System.Diagnostics.Debug.Assert(_started == true);
        //        System.Diagnostics.Debug.Assert(_startTime > Time.Zero);
        //        return _startTime;
        //    }
        //}

        //public Time ElapsedTime
        //{
        //    get
        //    {
        //        System.Diagnostics.Debug.Assert(_started == true);
        //        System.Diagnostics.Debug.Assert(_startTime > Time.Zero);
        //        return Time.Now() - _startTime;
        //    }
        //}

        //public GameContext(Time roundInterval, int roundCount)
        //{
        //    System.Diagnostics.Debug.Assert(roundInterval > Time.Zero);
        //    RoundInterval = roundInterval;

        //    System.Diagnostics.Debug.Assert(roundCount > 0);
        //    RoundCount = roundCount;
        //}

        public GameContext()
        {
        }

        ~GameContext()
        {
            System.Diagnostics.Debug.Assert(false);

            Dispose(false);
        }

        public bool Add(SuperPlayer player)
        {
            System.Diagnostics.Debug.Assert(player != null);

            System.Diagnostics.Debug.Assert(_disposed == false);

            if (_started == true)
            {
                return false;
            }

            System.Diagnostics.Debug.Assert(_started == false);

            System.Diagnostics.Debug.Assert(LockerPlayers != null);
            LockerPlayers.Hold();

            try
            {
                System.Diagnostics.Debug.Assert(_players != null);
                _players.Append(player);

                //TestWorld.GameContextInventory.ResetPlayerSeats();
            }
            catch (DuplicateKeyException)
            {
                return false;
            }
            finally
            {
                System.Diagnostics.Debug.Assert(LockerPlayers != null);
                LockerPlayers.Release();
            }

            return true;
        }

        public void Remove(int entityId)
        {
            System.Diagnostics.Debug.Assert(_disposed == false);

            if (_started == true)
            {
                return;
            }

            System.Diagnostics.Debug.Assert(_started == false);

            System.Diagnostics.Debug.Assert(LockerPlayers != null);
            LockerPlayers.Hold();

            try
            {
                System.Diagnostics.Debug.Assert(_players != null);
                _players.Extract(player => player.Id == entityId, null);

                
            }
            catch (KeyNotFoundException)
            {
            }
            finally
            {
                System.Diagnostics.Debug.Assert(LockerPlayers != null);
                LockerPlayers.Release();
            }

        }

        public void Start()
        {
            System.Diagnostics.Debug.Assert(_disposed == false);

            System.Diagnostics.Debug.Assert(_players.Count >= MinPlayers);

            System.Diagnostics.Debug.Assert(_started == false);
            _started = true;

            //_startTime = Time.Now();
        }

        public void Stop()
        {
            System.Diagnostics.Debug.Assert(_disposed == false);

            System.Diagnostics.Debug.Assert(_started == true);
            _started = false;

            _players.Flush();

            //System.Diagnostics.Debug.Assert(_startTime > Time.Zero);
            //_startTime = Time.Zero;
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
                    LockerPlayers.Dispose();
                    _players.Dispose();
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

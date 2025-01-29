
using Common;
using Containers;
using Sync;

using MinecraftPrimitives;

namespace TestMinecraftServerApplication
{
    public sealed class GameContext : System.IDisposable
    {
        public const int DefaultCoinAmount = 10;

        public const int MinPlayers = 2;
        public const int MaxPlayers = 18;
        public const int MaxRounds = MaxPlayers;


        public readonly static GameContextInventory Inventory = new(MinPlayers, MaxPlayers, MaxRounds);


        private bool _disposed = false;

        private readonly Locker _LockerPlayers = new();

        private readonly List<SuperPlayer> _players = new();



        public bool IsStarted => _started;
        private bool _started = false;


        public IReadOnlyList<SuperPlayer> Players
        {
            get
            {
                System.Diagnostics.Debug.Assert(_disposed == false);

                return _players;
            }
        }

        public int TotalRounds
        {
            get
            {
                System.Diagnostics.Debug.Assert(_disposed == false);

                return _players.Length;
            }
        }


        private List<SuperPlayer> _prevSeekers = new();
        private SuperPlayer _currentSeeker = null;
        private int _currentRoundIndex = -1;
        public bool IsBeforeFirstRound
        {
            get
            {
                System.Diagnostics.Debug.Assert(_disposed == false);

                System.Diagnostics.Debug.Assert(_started == true);

                return _currentRoundIndex <= 0;
            }
        }
        public bool IsFinalRound
        {
            get
            {
                System.Diagnostics.Debug.Assert(_disposed == false);

                System.Diagnostics.Debug.Assert(_started == true);

                return _currentRoundIndex == TotalRounds - 1;
            }
        }

        public SuperPlayer CurrentSeeker
        {
            get
            {
                System.Diagnostics.Debug.Assert(_disposed == false);

                System.Diagnostics.Debug.Assert(_started == true);

                System.Diagnostics.Debug.Assert(_currentSeeker != null);
                return _currentSeeker;
            }
        }

        private readonly Locker _LockerScoreboard = new();
        private readonly Map<UserId, ScoreboardPlayerRow> _ScoreboardByUserId = new();
        private readonly Map<string, ScoreboardPlayerRow> _ScoreboardByUsername = new();

        public GameContext()
        {
        }

        ~GameContext()
        {
            System.Diagnostics.Debug.Assert(false);

            Dispose(false);
        }

        public bool AddPlayer(SuperPlayer player)
        {
            System.Diagnostics.Debug.Assert(player != null);

            System.Diagnostics.Debug.Assert(_disposed == false);

            System.Diagnostics.Debug.Assert(_LockerPlayers != null);
            _LockerPlayers.Hold();

            try
            {
                if (_started == true)
                {
                    return false;
                }

                if (_players.Length > MaxPlayers)
                {
                    return false;
                }

                bool exists = _players.Find(_player => _player.UserId == player.UserId, null) != null;

                if (exists == false)
                {
                    System.Diagnostics.Debug.Assert(_players != null);
                    _players.Append(player);

                    Inventory.ResetPlayerSeatsBeforeGame(_players, MinPlayers, MaxPlayers);
                }

                return exists == false;
            }
            catch (DuplicateKeyException)
            {
                return false;
            }
            finally
            {
                System.Diagnostics.Debug.Assert(_LockerPlayers != null);
                _LockerPlayers.Release();
            }

        }

        public void RemovePlayer(UserId userId)
        {
            System.Diagnostics.Debug.Assert(_disposed == false);

            System.Diagnostics.Debug.Assert(_LockerPlayers != null);
            _LockerPlayers.Hold();

            try
            {
                if (_started == true)
                {
                    return;
                }

                System.Diagnostics.Debug.Assert(_players != null);
                _players.Extract(player => player.UserId == userId, null);

                Inventory.ResetPlayerSeatsBeforeGame(_players, MinPlayers, MaxPlayers);
            }
            catch (KeyNotFoundException)
            {
            }
            finally
            {
                System.Diagnostics.Debug.Assert(_LockerPlayers != null);
                _LockerPlayers.Release();
            }

        }

        public bool Start()
        {
            System.Diagnostics.Debug.Assert(_disposed == false);

            System.Diagnostics.Debug.Assert(_LockerPlayers != null);
            _LockerPlayers.Hold();
            System.Diagnostics.Debug.Assert(_LockerScoreboard != null);
            _LockerScoreboard.Hold();

            try
            {
                if (_started == true)
                {
                    return false;
                }

                if (_players.Length < MinPlayers)
                {
                    return false;
                }

                System.Diagnostics.Debug.Assert(_players.Length >= MinPlayers);
                System.Diagnostics.Debug.Assert(_players.Length <= MaxPlayers);

                System.Diagnostics.Debug.Assert(_started == false);
                _started = true;

                System.Diagnostics.Debug.Assert(_prevSeekers != null);
                System.Diagnostics.Debug.Assert(_prevSeekers.Length == 0);
                System.Diagnostics.Debug.Assert(_currentSeeker == null);
                System.Diagnostics.Debug.Assert(_currentRoundIndex < 0);

                System.Diagnostics.Debug.Assert(_ScoreboardByUserId != null);
                System.Diagnostics.Debug.Assert(_ScoreboardByUserId.Count == 0);
                System.Diagnostics.Debug.Assert(_ScoreboardByUsername != null);
                System.Diagnostics.Debug.Assert(_ScoreboardByUsername.Count == 0);
                foreach (SuperPlayer player in _players)
                {
                    ScoreboardPlayerRow row = new(player.UserId, player.Username);

                    _ScoreboardByUserId.Insert(row.UserId, row);
                    _ScoreboardByUsername.Insert(row.Username, row);
                }

                Inventory.StartGame(_players, TotalRounds, _ScoreboardByUserId);

                return true;
            }
            finally
            {
                System.Diagnostics.Debug.Assert(_LockerScoreboard != null);
                _LockerScoreboard.Release();
                System.Diagnostics.Debug.Assert(_LockerPlayers != null);
                _LockerPlayers.Release();
            }

        }

        public void StartRound()
        {
            System.Diagnostics.Debug.Assert(_disposed == false);

            System.Diagnostics.Debug.Assert(_started == true);

            System.Diagnostics.Debug.Assert(_prevSeekers != null);
            System.Diagnostics.Debug.Assert(_currentSeeker == null);
            ++_currentRoundIndex;

            System.Diagnostics.Debug.Assert(Inventory != null);
            Inventory.StartRound(_players, TotalRounds, _currentRoundIndex);
        }

        public int MakeNonSeekerIndexList(List<int> indices)
        {
            System.Diagnostics.Debug.Assert(indices != null);

            System.Diagnostics.Debug.Assert(_disposed == false);

            System.Diagnostics.Debug.Assert(_started == true);

            bool prevSeeker = false;
            for (int i = 0; i < _players.Length; ++i)
            {
                for (int j = 0; j < _prevSeekers.Length; ++j)
                {
                    if (object.ReferenceEquals(_prevSeekers[j], _players[i]) == true)
                    {
                        prevSeeker = true;
                        break;
                    }


                }

                if (prevSeeker == true)
                {
                    prevSeeker = false;
                    continue;
                }

                indices.Append(i);
            }

            return _players.Length - _prevSeekers.Length;
        }

        public void SeletSeeker(SuperPlayer player)
        {
            System.Diagnostics.Debug.Assert(player != null);

            System.Diagnostics.Debug.Assert(_disposed == false);

            System.Diagnostics.Debug.Assert(_started == true);

            System.Diagnostics.Debug.Assert(_currentSeeker == null);
            _currentSeeker = player;
        }

        public void StartSeekerCount()
        {
            System.Diagnostics.Debug.Assert(_disposed == false);

            System.Diagnostics.Debug.Assert(_started == true);

            System.Diagnostics.Debug.Assert(_currentSeeker != null);
            _currentSeeker.ApplyBilndness(true);

        }

        public void EndSeekerCount()
        {

            System.Diagnostics.Debug.Assert(_disposed == false);

            System.Diagnostics.Debug.Assert(_started == true);

            System.Diagnostics.Debug.Assert(_currentSeeker != null);
            _currentSeeker.ApplyBilndness(false);


        }

        public void IncreaseKillPoint(UserId userId)
        {
            System.Diagnostics.Debug.Assert(_disposed == false);

            System.Diagnostics.Debug.Assert(_started == true);

            System.Diagnostics.Debug.Assert(_LockerScoreboard != null);
            _LockerScoreboard.Hold();

            try
            {
                System.Diagnostics.Debug.Assert(_ScoreboardByUserId != null);
                ScoreboardPlayerRow row = _ScoreboardByUserId.Lookup(userId);

                ++row.Kills;

                System.Diagnostics.Debug.Assert(_players != null);
                System.Diagnostics.Debug.Assert(_ScoreboardByUserId != null);
                Inventory.UpdatePlayerScores(_players, _ScoreboardByUserId);
            }
            finally
            {
                System.Diagnostics.Debug.Assert(_LockerScoreboard != null);
                _LockerScoreboard.Release();
            }

        }

        public void IncreaseDeathPoint(UserId userId)
        {
            System.Diagnostics.Debug.Assert(_disposed == false);

            System.Diagnostics.Debug.Assert(_started == true);

            System.Diagnostics.Debug.Assert(_LockerScoreboard != null);
            _LockerScoreboard.Hold();

            try
            {
                System.Diagnostics.Debug.Assert(_ScoreboardByUserId != null);
                ScoreboardPlayerRow row = _ScoreboardByUserId.Lookup(userId);

                ++row.Deaths;

                System.Diagnostics.Debug.Assert(_players != null);
                System.Diagnostics.Debug.Assert(_ScoreboardByUserId != null);
                Inventory.UpdatePlayerScores(_players, _ScoreboardByUserId);
            }
            finally
            {
                System.Diagnostics.Debug.Assert(_LockerScoreboard != null);
                _LockerScoreboard.Release();
            }
        }

        public void EndRound()
        {
            System.Diagnostics.Debug.Assert(_disposed == false);

            System.Diagnostics.Debug.Assert(_started == true);

            System.Diagnostics.Debug.Assert(_prevSeekers != null);
            System.Diagnostics.Debug.Assert(_currentSeeker != null);
            _prevSeekers.Append(_currentSeeker);

            _currentSeeker = null;

            System.Diagnostics.Debug.Assert(_currentRoundIndex >= 0);

            System.Diagnostics.Debug.Assert(Inventory != null);
            Inventory.EndRound(_players, TotalRounds, _currentRoundIndex);
        }

        public void End()
        {
            System.Diagnostics.Debug.Assert(_disposed == false);

            System.Diagnostics.Debug.Assert(_LockerPlayers != null);
            _LockerPlayers.Hold();
            System.Diagnostics.Debug.Assert(_LockerScoreboard != null);
            _LockerScoreboard.Hold();

            try
            {

                System.Diagnostics.Debug.Assert(_started == true);
                _started = false;



                _prevSeekers.Flush();
                _currentSeeker = null;
                _currentRoundIndex = -1;

                System.Diagnostics.Debug.Assert(_ScoreboardByUserId != null);
                System.Diagnostics.Debug.Assert(_ScoreboardByUserId.Count >= MinPlayers);
                System.Diagnostics.Debug.Assert(_ScoreboardByUserId.Count <= MaxPlayers);
                System.Diagnostics.Debug.Assert(_ScoreboardByUsername != null);
                System.Diagnostics.Debug.Assert(_ScoreboardByUsername.Count >= MinPlayers);
                System.Diagnostics.Debug.Assert(_ScoreboardByUsername.Count <= MaxPlayers);
                _ScoreboardByUserId.Flush();
                _ScoreboardByUsername.Flush();

                System.Diagnostics.Debug.Assert(Inventory != null);
                Inventory.Reset(_players, MinPlayers, MaxPlayers, MaxRounds);
            }
            finally
            {
                System.Diagnostics.Debug.Assert(_LockerPlayers != null);
                _LockerPlayers.Release();
                System.Diagnostics.Debug.Assert(_LockerScoreboard != null);
                _LockerScoreboard.Release();
            }
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
                    _LockerPlayers.Dispose();
                    _players.Dispose();

                    _prevSeekers.Dispose();

                    _LockerScoreboard.Dispose();
                    _ScoreboardByUserId.Dispose();
                    _ScoreboardByUsername.Dispose();
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

﻿
using Common;
using Containers;
using Sync;

using MinecraftPrimitives;

namespace TestMinecraftServerApplication
{
    public sealed class GameContext : System.IDisposable
    {

        public const int MinPlayers = 2;
        public const int MaxPlayers = 18;
        public const int MaxRounds = MaxPlayers;

        public readonly static GameContextInventory Inventory = new(MinPlayers, MaxPlayers, MaxRounds);
        
        private bool _disposed = false;

        private readonly Locker Locker = new();

        private readonly List<SuperPlayer> _players = new();

        public bool CanStart
        {
            get
            {
                System.Diagnostics.Debug.Assert(Locker != null);
                Locker.Hold();

                try
                {
                    return _players.Length >= MinPlayers;
                }
                finally
                {
                    System.Diagnostics.Debug.Assert(Locker != null);
                    Locker.Release();
                }
            }
        }


        private bool _started = false;
        public bool IsStarted => _started;


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

            System.Diagnostics.Debug.Assert(Locker != null);
            Locker.Hold();

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

                    SuperWorld.GameContextInventory.ResetPlayerSeatsBeforeGame(_players, MinPlayers, MaxPlayers);
                }

                return exists == false;
            }
            catch (DuplicateKeyException)
            {
                return false;
            }
            finally
            {
                System.Diagnostics.Debug.Assert(Locker != null);
                Locker.Release();
            }

        }

        public void Remove(UserId userId)
        {
            System.Diagnostics.Debug.Assert(_disposed == false);

            System.Diagnostics.Debug.Assert(Locker != null);
            Locker.Hold();

            try
            {
                if (_started == true)
                {
                    return;
                }

                System.Diagnostics.Debug.Assert(_players != null);
                _players.Extract(player => player.UserId == userId, null);

                SuperWorld.GameContextInventory.ResetPlayerSeatsBeforeGame(_players, MinPlayers, MaxPlayers);
            }
            catch (KeyNotFoundException)
            {
            }
            finally
            {
                System.Diagnostics.Debug.Assert(Locker != null);
                Locker.Release();
            }

        }

        public bool Start()
        {
            System.Diagnostics.Debug.Assert(_disposed == false);

            System.Diagnostics.Debug.Assert(Locker != null);
            Locker.Hold();

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

                SuperWorld.GameContextInventory.StartGame(_players, TotalRounds);

                return true;
            }
            finally
            {
                System.Diagnostics.Debug.Assert(Locker != null);
                Locker.Release();
            }

        }

        public void Stop()
        {
            System.Diagnostics.Debug.Assert(_disposed == false);

            System.Diagnostics.Debug.Assert(Locker != null);
            Locker.Hold();

            try
            {

                System.Diagnostics.Debug.Assert(_started == true);
                _started = false;

                SuperWorld.GameContextInventory.Reset(_players, MinPlayers, MaxPlayers, MaxRounds);

            }
            finally
            {
                System.Diagnostics.Debug.Assert(Locker != null);
                Locker.Release();
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
                    Locker.Dispose();
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

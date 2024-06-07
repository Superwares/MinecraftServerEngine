

using Containers;
using Threading;

namespace MinecraftServerEngine
{
    internal class PlayerList : System.IDisposable
    {
        private class Info
        {
            public readonly System.Guid UserId;
            public readonly string Username;
            private long _laytency = -1;
            public long Laytency => _laytency;

            public Info(System.Guid uniqueId, string username)
            {
                UserId = uniqueId;
                Username = username;
            }

            public void Connect()
            {
                System.Diagnostics.Debug.Assert(_laytency == -1);
                _laytency = 0;
            }

            public void UpdateLaytency(long ticks)
            {
                System.Diagnostics.Debug.Assert(ticks > 0);
                System.Diagnostics.Debug.Assert(_laytency >= 0);
                _laytency = ticks;
            }

            public void Disconnect()
            {
                System.Diagnostics.Debug.Assert(_laytency == 0);
                _laytency = -1;
            }

        }

        private bool _disposed = false;

        private readonly Mutex _MUTEX = new();  // Disposable
        private readonly ConcurrentTable<System.Guid, Info> _INFORS = new();  // Disposable
        private readonly PlayerListRendererManager _MANAGER = new();  // Disposable

        internal PlayerList() { }

        ~PlayerList() => System.Diagnostics.Debug.Assert(false);

        private bool IsDisconnected(System.Guid userId)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            return !_MANAGER.Contains(userId);
        }

        public void Add(System.Guid userId, string username)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(IsDisconnected(userId));

            Info info = new(userId, username);
            System.Diagnostics.Debug.Assert(!_INFORS.Contains(userId));
            _INFORS.Insert(userId, info);

            _MANAGER.AddPlayerWithLaytency(userId, username, info.Laytency);
        }

        public void Connect(System.Guid userId, PlayerListRenderer renderer)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            _MUTEX.Lock();

            foreach (Info info in _INFORS.GetValues())
            {
                renderer.AddPlayerWithLaytency(info.UserId, info.Username, info.Laytency);
            }

            System.Diagnostics.Debug.Assert(_INFORS.Contains(userId));
            Info infoSelf = _INFORS.Lookup(userId);

            System.Diagnostics.Debug.Assert(userId == infoSelf.UserId);
            infoSelf.Connect();

            System.Diagnostics.Debug.Assert(IsDisconnected(userId));
            _MANAGER.Apply(userId, renderer);

            _MANAGER.UpdatePlayerLatency(infoSelf.UserId, infoSelf.Laytency);

            _MUTEX.Unlock();
        }

        public void UpdateLaytency(System.Guid userId, long ticks)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(_INFORS.Contains(userId));
            Info info = _INFORS.Lookup(userId);
            info.UpdateLaytency(ticks);

            System.Diagnostics.Debug.Assert(userId == info.UserId);
            _MANAGER.UpdatePlayerLatency(info.UserId, ticks);
        }

        public void Disconnect(System.Guid userId)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            _MUTEX.Lock();

            System.Diagnostics.Debug.Assert(!IsDisconnected(userId));
            _MANAGER.Cancel(userId);

            System.Diagnostics.Debug.Assert(_INFORS.Contains(userId));
            Info info = _INFORS.Lookup(userId);
            info.Disconnect();

            System.Diagnostics.Debug.Assert(userId == info.UserId);
            _MANAGER.UpdatePlayerLatency(info.UserId, info.Laytency);

            _MUTEX.Unlock();
        }

        public void Remove(System.Guid userId)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(IsDisconnected(userId));

            Info info = _INFORS.Extract(userId);
            System.Diagnostics.Debug.Assert(info.UserId == userId);

            _MANAGER.RemovePlayer(userId);
        }

        public void Dispose()
        {
            // Assertion.
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(_INFORS.Empty);

            // Release resources.
            _MUTEX.Dispose();
            _INFORS.Dispose();
            _MANAGER.Dispose();

            // Finish.
            System.GC.SuppressFinalize(this);
            _disposed = true;
        }


    }

}

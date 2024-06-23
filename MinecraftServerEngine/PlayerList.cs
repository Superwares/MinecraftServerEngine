

using Containers;
using Sync;

namespace MinecraftServerEngine
{
    internal class PlayerList : System.IDisposable
    {
        private class RendererManager : System.IDisposable
        {
            private bool _disposed = false;

            private readonly Table<System.Guid, PlayerListRenderer> Renderers = new();

            public RendererManager() { }

            ~RendererManager() => System.Diagnostics.Debug.Assert(false);

            public void Apply(System.Guid userId, PlayerListRenderer renderer)
            {
                System.Diagnostics.Debug.Assert(userId != System.Guid.Empty);
                System.Diagnostics.Debug.Assert(renderer != null);

                System.Diagnostics.Debug.Assert(!_disposed);

                System.Diagnostics.Debug.Assert(!Renderers.Contains(userId));
                Renderers.Insert(userId, renderer);
            }

            public void Cancel(System.Guid userId)
            {
                System.Diagnostics.Debug.Assert(userId != System.Guid.Empty);

                System.Diagnostics.Debug.Assert(!_disposed);

                System.Diagnostics.Debug.Assert(Renderers.Contains(userId));
                Renderers.Extract(userId);
            }

            public bool Contains(System.Guid userId)
            {
                System.Diagnostics.Debug.Assert(userId != System.Guid.Empty);

                System.Diagnostics.Debug.Assert(!_disposed);

                return Renderers.Contains(userId);
            }

            public void AddPlayerWithLaytency(System.Guid userId, string username, long ticks)
            {
                System.Diagnostics.Debug.Assert(userId != System.Guid.Empty);
                System.Diagnostics.Debug.Assert(username != null && !string.IsNullOrEmpty(username));

                System.Diagnostics.Debug.Assert(!_disposed);

                foreach (PlayerListRenderer renderer in Renderers.GetValues())
                {
                    renderer.AddPlayerWithLaytency(userId, username, ticks);
                }
            }

            public void RemovePlayer(System.Guid userId)
            {
                System.Diagnostics.Debug.Assert(userId != System.Guid.Empty);

                System.Diagnostics.Debug.Assert(!_disposed);

                foreach (PlayerListRenderer renderer in Renderers.GetValues())
                {
                    renderer.RemovePlayer(userId);
                }
            }

            public void UpdatePlayerLatency(System.Guid userId, long ticks)
            {
                System.Diagnostics.Debug.Assert(userId != System.Guid.Empty);

                System.Diagnostics.Debug.Assert(!_disposed);

                foreach (PlayerListRenderer renderer in Renderers.GetValues())
                {
                    renderer.UpdatePlayerLatency(userId, ticks);
                }
            }

            public virtual void Dispose()
            {
                // Assertion.
                System.Diagnostics.Debug.Assert(!_disposed);

                System.Diagnostics.Debug.Assert(Renderers.Empty);

                // Release resources.
                Renderers.Dispose();

                // Finish.
                System.GC.SuppressFinalize(this);
                _disposed = true;
            }
        }

        private class Info
        {
            public readonly System.Guid UserId;
            public readonly string Username;
            private long _laytency = -1;
            public long Laytency => _laytency;

            public Info(System.Guid userId, string username)
            {
                System.Diagnostics.Debug.Assert(userId != System.Guid.Empty);
                System.Diagnostics.Debug.Assert(username != null && !string.IsNullOrEmpty(username));

                UserId = userId;
                Username = username;
            }

            public void Connect()
            {
                System.Diagnostics.Debug.Assert(_laytency == -1);
                _laytency = 0;
            }

            public void UpdateLaytency(long ticks)
            {
                System.Diagnostics.Debug.Assert(ticks >= 0);
                System.Diagnostics.Debug.Assert(_laytency >= 0);
                _laytency = ticks;
            }

            public void Disconnect()
            {
                System.Diagnostics.Debug.Assert(_laytency >= 0);
                _laytency = -1;
            }

        }

        private bool _disposed = false;

        private readonly Locker Locker = new();  // Disposable
        private readonly ConcurrentTable<System.Guid, Info> Infos = new();  // Disposable
        private readonly RendererManager Manager = new();  // Disposable

        internal PlayerList() { }

        ~PlayerList() => System.Diagnostics.Debug.Assert(false);

        private bool IsDisconnected(System.Guid userId)
        {
            System.Diagnostics.Debug.Assert(userId != System.Guid.Empty);

            System.Diagnostics.Debug.Assert(!_disposed);

            return !Manager.Contains(userId);
        }

        public void Add(System.Guid userId, string username)
        {
            System.Diagnostics.Debug.Assert(userId != System.Guid.Empty);
            System.Diagnostics.Debug.Assert(username != null && !string.IsNullOrEmpty(username));

            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(IsDisconnected(userId));

            Info info = new(userId, username);
            System.Diagnostics.Debug.Assert(!Infos.Contains(userId));
            Infos.Insert(userId, info);

            Manager.AddPlayerWithLaytency(userId, username, info.Laytency);
        }

        public void Connect(System.Guid userId, PlayerListRenderer renderer)
        {
            System.Diagnostics.Debug.Assert(renderer != null);
            System.Diagnostics.Debug.Assert(userId != System.Guid.Empty);

            System.Diagnostics.Debug.Assert(!_disposed);

            Locker.Hold();

            foreach (Info info in Infos.GetValues())
            {
                renderer.AddPlayerWithLaytency(info.UserId, info.Username, info.Laytency);
            }

            System.Diagnostics.Debug.Assert(Infos.Contains(userId));
            Info infoSelf = Infos.Lookup(userId);

            System.Diagnostics.Debug.Assert(userId == infoSelf.UserId);
            infoSelf.Connect();

            System.Diagnostics.Debug.Assert(IsDisconnected(userId));
            Manager.Apply(userId, renderer);

            Manager.UpdatePlayerLatency(infoSelf.UserId, infoSelf.Laytency);

            Locker.Release();
        }

        public void UpdateLaytency(System.Guid userId, long ticks)
        {
            System.Diagnostics.Debug.Assert(userId != System.Guid.Empty);
            System.Diagnostics.Debug.Assert(ticks >= 0);

            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(Infos.Contains(userId));
            Info info = Infos.Lookup(userId);
            info.UpdateLaytency(ticks);

            System.Diagnostics.Debug.Assert(userId == info.UserId);
            Manager.UpdatePlayerLatency(info.UserId, ticks);
        }

        public void Disconnect(System.Guid userId)
        {
            System.Diagnostics.Debug.Assert(userId != System.Guid.Empty);

            System.Diagnostics.Debug.Assert(!_disposed);

            Locker.Hold();

            System.Diagnostics.Debug.Assert(!IsDisconnected(userId));
            Manager.Cancel(userId);

            System.Diagnostics.Debug.Assert(Infos.Contains(userId));
            Info info = Infos.Lookup(userId);
            info.Disconnect();

            System.Diagnostics.Debug.Assert(userId == info.UserId);
            Manager.UpdatePlayerLatency(info.UserId, info.Laytency);

            Locker.Release();
        }

        public void Remove(System.Guid userId)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(IsDisconnected(userId));

            System.Diagnostics.Debug.Assert(Infos.Contains(userId));
            Info info = Infos.Extract(userId);
            System.Diagnostics.Debug.Assert(info.UserId == userId);

            Manager.RemovePlayer(userId);
        }

        public void Dispose()
        {
            // Assertion.
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(Infos.Empty);

            // Release resources.
            Locker.Dispose();
            Infos.Dispose();
            Manager.Dispose();

            // Finish.
            System.GC.SuppressFinalize(this);
            _disposed = true;
        }


    }

}

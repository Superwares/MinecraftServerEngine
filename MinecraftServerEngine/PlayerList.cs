

using Common;
using Containers;
using Sync;

namespace MinecraftServerEngine
{
    internal class PlayerList : System.IDisposable
    {
        private class RendererManager : System.IDisposable
        {
            private bool _disposed = false;

            private readonly Table<UserId, PlayerListRenderer> Renderers = new();

            internal RendererManager() { }

            ~RendererManager() => System.Diagnostics.Debug.Assert(false);

            internal void Apply(UserId id, PlayerListRenderer renderer)
            {
                System.Diagnostics.Debug.Assert(id != UserId.Null);
                System.Diagnostics.Debug.Assert(renderer != null);

                System.Diagnostics.Debug.Assert(!_disposed);

                System.Diagnostics.Debug.Assert(!Renderers.Contains(id));
                Renderers.Insert(id, renderer);
            }

            internal void Cancel(UserId id)
            {
                System.Diagnostics.Debug.Assert(id != UserId.Null);

                System.Diagnostics.Debug.Assert(!_disposed);

                System.Diagnostics.Debug.Assert(Renderers.Contains(id));
                Renderers.Extract(id);
            }

            internal bool Contains(UserId id)
            {
                System.Diagnostics.Debug.Assert(id != UserId.Null);

                System.Diagnostics.Debug.Assert(!_disposed);

                return Renderers.Contains(id);
            }

            internal void AddPlayerWithLaytency(UserId id, string username, long ticks)
            {
                System.Diagnostics.Debug.Assert(id != UserId.Null);
                System.Diagnostics.Debug.Assert(username != null && !string.IsNullOrEmpty(username));

                System.Diagnostics.Debug.Assert(!_disposed);

                long ms = ticks * 50;
                System.Diagnostics.Debug.Assert(ms >= int.MinValue);
                System.Diagnostics.Debug.Assert(ms <= int.MaxValue);
                PlayerListItemAddPacket pck = new(id.Data, username, (int)ms);

                foreach (PlayerListRenderer renderer in Renderers.GetValues())
                {
                    renderer.AddPlayerWithLaytency(pck);
                }
            }

            internal void RemovePlayer(UserId id)
            {
                System.Diagnostics.Debug.Assert(id != UserId.Null);

                System.Diagnostics.Debug.Assert(!_disposed);

                PlayerListItemRemovePacket pck = new(id.Data);
                foreach (PlayerListRenderer renderer in Renderers.GetValues())
                {
                    renderer.RemovePlayer(pck);
                }
            }

            internal void UpdatePlayerLatency(UserId id, long ticks)
            {
                System.Diagnostics.Debug.Assert(id != UserId.Null);

                System.Diagnostics.Debug.Assert(!_disposed);

                long ms = ticks * 50;
                System.Diagnostics.Debug.Assert(ms >= int.MinValue);
                System.Diagnostics.Debug.Assert(ms <= int.MaxValue);
                PlayerListItemUpdateLatencyPacket pck = new(id.Data, (int)ms);
                foreach (PlayerListRenderer renderer in Renderers.GetValues())
                {
                    renderer.UpdatePlayerLatency(pck);
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
            public readonly UserId Id;
            public readonly string Username;
            private long _laytency = -1;
            public long Laytency => _laytency;

            public Info(UserId id, string username)
            {
                System.Diagnostics.Debug.Assert(id != UserId.Null);
                System.Diagnostics.Debug.Assert(username != null && !string.IsNullOrEmpty(username));

                Id = id;
                Username = username;
            }

            internal void Connect()
            {
                System.Diagnostics.Debug.Assert(_laytency == -1);
                _laytency = 0;
            }

            internal void UpdateLaytency(long ticks)
            {
                System.Diagnostics.Debug.Assert(ticks >= 0);
                System.Diagnostics.Debug.Assert(_laytency >= 0);
                _laytency = ticks;
            }

            internal void Disconnect()
            {
                System.Diagnostics.Debug.Assert(_laytency >= 0);
                _laytency = -1;
            }

        }

        private bool _disposed = false;

        private readonly Locker Locker = new();  // Disposable
        private readonly ConcurrentTable<UserId, Info> Infos = new();  // Disposable
        private readonly RendererManager Manager = new();  // Disposable

        internal PlayerList() { }

        ~PlayerList() => System.Diagnostics.Debug.Assert(false);

        private bool IsDisconnected(UserId id)
        {
            System.Diagnostics.Debug.Assert(id != UserId.Null);

            System.Diagnostics.Debug.Assert(!_disposed);

            return !Manager.Contains(id);
        }

        internal void Add(UserId id, string username)
        {
            System.Diagnostics.Debug.Assert(id != UserId.Null);
            System.Diagnostics.Debug.Assert(username != null && !string.IsNullOrEmpty(username));

            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(IsDisconnected(id));

            Info info = new(id, username);
            System.Diagnostics.Debug.Assert(!Infos.Contains(id));
            Console.Printl("PlayerList::Add 1!");
            Infos.Insert(id, info);
            Console.Printl("PlayerList::Add 2!");
            Manager.AddPlayerWithLaytency(id, username, info.Laytency);
        }

        internal void Connect(UserId id, PlayerListRenderer renderer)
        {
            System.Diagnostics.Debug.Assert(renderer != null);
            System.Diagnostics.Debug.Assert(id != UserId.Null);

            System.Diagnostics.Debug.Assert(!_disposed);

            Locker.Hold();

            foreach (Info info in Infos.GetValues())
            {
                renderer.AddPlayerWithLaytency(info.Id, info.Username, info.Laytency);
            }

            System.Diagnostics.Debug.Assert(Infos.Contains(id));
            Info infoSelf = Infos.Lookup(id);

            System.Diagnostics.Debug.Assert(id == infoSelf.Id);
            infoSelf.Connect();

            System.Diagnostics.Debug.Assert(IsDisconnected(id));
            Manager.Apply(id, renderer);

            Manager.UpdatePlayerLatency(infoSelf.Id, infoSelf.Laytency);

            Locker.Release();
        }

        internal void UpdateLaytency(UserId id, long ticks)
        {
            System.Diagnostics.Debug.Assert(id != UserId.Null);
            System.Diagnostics.Debug.Assert(ticks >= 0);

            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(Infos.Contains(id));
            Info info = Infos.Lookup(id);
            info.UpdateLaytency(ticks);

            System.Diagnostics.Debug.Assert(id == info.Id);
            Manager.UpdatePlayerLatency(info.Id, ticks);
        }

        internal void Disconnect(UserId id)
        {
            System.Diagnostics.Debug.Assert(id != UserId.Null);

            System.Diagnostics.Debug.Assert(!_disposed);

            Locker.Hold();

            System.Diagnostics.Debug.Assert(!IsDisconnected(id));
            Manager.Cancel(id);

            System.Diagnostics.Debug.Assert(Infos.Contains(id));
            Info info = Infos.Lookup(id);
            info.Disconnect();

            System.Diagnostics.Debug.Assert(id == info.Id);
            Manager.UpdatePlayerLatency(info.Id, info.Laytency);

            Locker.Release();
        }

        internal void Remove(UserId id)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(IsDisconnected(id));

            System.Diagnostics.Debug.Assert(Infos.Contains(id));
            Info info = Infos.Extract(id);
            System.Diagnostics.Debug.Assert(info.Id == id);

            Manager.RemovePlayer(id);
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



using Containers;
using System.Diagnostics;
using System.Xml;

namespace Protocol
{
    internal class PlayerList : System.IDisposable
    {
        private class Item
        {
            public readonly System.Guid UniqueId;
            public readonly string Username;
            public int laytencyInTicks = -1;

            /*
             *  -1: Not started.
             * >= 0: In progress.
             */
            private long timestampInTicks = -1;

            public Item(System.Guid uniqueId, string username)
            {
                UniqueId = uniqueId;
                Username = username;
            }

            public void Connect()
            {
                timestampInTicks = -1;
            }

            public void Disconnect()
            {
                laytencyInTicks = -1;
            }

            public bool IsInProgress()
            {
                return timestampInTicks >= 0;
            }

            public void Start(long serverTicks)
            {
                System.Diagnostics.Debug.Assert(timestampInTicks == -1);
                timestampInTicks = serverTicks;
            }

            public void Done(long serverTicks)
            {
                System.Diagnostics.Debug.Assert(timestampInTicks > -1);

                System.Diagnostics.Debug.Assert(serverTicks > timestampInTicks);
                laytencyInTicks = (int)(serverTicks - timestampInTicks);

                timestampInTicks = -1;  // reset
            }

        }

        private bool _disposed = false;

        private readonly Table<System.Guid, Item> _items = new();
        private readonly Table<System.Guid, Queue<ClientboundPlayingPacket>> _renderers = new();

        internal PlayerList() { }

        ~PlayerList() => System.Diagnostics.Debug.Assert(false);

        private void RenderToInit(Queue<ClientboundPlayingPacket> renderer)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            foreach (Item item in _items.GetValues())
            {
                int laytencyInMilliseconds = item.laytencyInTicks * 50;
                renderer.Enqueue(new AddPlayerListItemPacket(
                    item.UniqueId, item.Username, laytencyInMilliseconds));
            }
        }

        private void RenderToAdd(Item item)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            int laytencyInMilliseconds = item.laytencyInTicks * 50;

            foreach (var renderer in _renderers.GetValues())
                renderer.Enqueue(new AddPlayerListItemPacket(
                    item.UniqueId, item.Username, laytencyInMilliseconds));
        }

        private void RenderToRemove(System.Guid uniqueId)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            foreach (var renderer in _renderers.GetValues())
                renderer.Enqueue(new RemovePlayerListItemPacket(uniqueId));
        }

        private void RenderToUpdateLatency(Item item)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            int laytencyInMilliseconds = item.laytencyInTicks * 50;
            foreach (var outPackets in _renderers.GetValues())
                outPackets.Enqueue(new UpdatePlayerListItemLatencyPacket(
                    item.UniqueId, laytencyInMilliseconds));
        }

        private bool IsDisconnected(System.Guid uniqueId)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            return !_renderers.Contains(uniqueId);
        }

        internal void InitPlayer(System.Guid uniqueId, string username)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            Debug.Assert(IsDisconnected(uniqueId));

            Item item = new(uniqueId, username);
            _items.Insert(uniqueId, item);

            /*System.Console.WriteLine($"UniqueId: {uniqueId} in InitPlayer");*/

            RenderToAdd(item);
        }

        internal void ClosePlayer(System.Guid uniqueId)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            Debug.Assert(IsDisconnected(uniqueId));

            _items.Extract(uniqueId);

            RenderToRemove(uniqueId);
        }

        internal void Connect(
            System.Guid uniqueId,
            Queue<ClientboundPlayingPacket> renderer)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            /*System.Console.WriteLine($"UniqueId: {uniqueId} in Connect");*/

            Debug.Assert(IsDisconnected(uniqueId));
            _renderers.Insert(uniqueId, renderer);

            Item item = _items.Lookup(uniqueId);
            item.Connect();

            RenderToInit(renderer);
        }

        internal void Disconnect(System.Guid uniqueId)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            Debug.Assert(!IsDisconnected(uniqueId));
            _renderers.Extract(uniqueId);

            Item item = _items.Lookup(uniqueId);
            item.Disconnect();

            RenderToUpdateLatency(item);
        }

        public void KeepAlive(long serverTicks, System.Guid uniqueId)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            Item item = _items.Lookup(uniqueId);
            item.Done(serverTicks);
        }

        internal void StartRoutine(long serverTicks)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            if (serverTicks % 20 != 0) return;

            foreach (Item item in _items.GetValues())
            {
                if (IsDisconnected(item.UniqueId)) continue;

                if (item.IsInProgress()) continue;

                RenderToUpdateLatency(item);

                Queue<ClientboundPlayingPacket> outPacket = _renderers.Lookup(item.UniqueId);
                long payload = new System.Random().NextInt64();
                outPacket.Enqueue(new RequestKeepAlivePacket(payload));

                item.Start(serverTicks);
            }
        }

        private void Dispose(bool disposing)
        {
            if (_disposed) return;

            // Assertion.
            Debug.Assert(_items.Empty);
            Debug.Assert(_renderers.Empty);

            if (disposing == true)
            {
                // Release managed resources.
                _items.Dispose();
                _renderers.Dispose();
            }

            // Release unmanaged resources.

            _disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            System.GC.SuppressFinalize(this);
        }

        public void Close() => Dispose();

    }

}

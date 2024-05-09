

using Containers;

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

        private readonly Table<System.Guid, Item> _ITEMS = new();
        private readonly Table<System.Guid, Queue<ClientboundPlayingPacket>> _RENDERERS = new();

        internal PlayerList() { }

        ~PlayerList() => System.Diagnostics.Debug.Assert(false);

        private void RenderToInit(Queue<ClientboundPlayingPacket> renderer)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            foreach (Item item in _ITEMS.GetValues())
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

            foreach (var renderer in _RENDERERS.GetValues())
            {
                renderer.Enqueue(new AddPlayerListItemPacket(
                    item.UniqueId, item.Username, laytencyInMilliseconds));
            }
        }

        private void RenderToRemove(System.Guid uniqueId)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            foreach (var renderer in _RENDERERS.GetValues())
            {
                renderer.Enqueue(new RemovePlayerListItemPacket(uniqueId));
            }
        }

        private void RenderToUpdateLatency(Item item)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            int laytencyInMilliseconds = item.laytencyInTicks * 50;
            foreach (var outPackets in _RENDERERS.GetValues())
            {
                outPackets.Enqueue(new UpdatePlayerListItemLatencyPacket(
                    item.UniqueId, laytencyInMilliseconds));
            }
        }

        private bool IsDisconnected(System.Guid uniqueId)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            return !_RENDERERS.Contains(uniqueId);
        }

        internal void InitPlayer(System.Guid uniqueId, string username)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(IsDisconnected(uniqueId));

            Item item = new(uniqueId, username);
            _ITEMS.Insert(uniqueId, item);

            /*System.Console.WriteLine($"UniqueId: {uniqueId} in InitPlayer");*/

            RenderToAdd(item);
        }

        internal void ClosePlayer(System.Guid uniqueId)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(IsDisconnected(uniqueId));

            _ITEMS.Extract(uniqueId);

            RenderToRemove(uniqueId);
        }

        internal void Connect(
            System.Guid uniqueId,
            Queue<ClientboundPlayingPacket> renderer)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            /*System.Console.WriteLine($"UniqueId: {uniqueId} in Connect");*/

            System.Diagnostics.Debug.Assert(IsDisconnected(uniqueId));
            _RENDERERS.Insert(uniqueId, renderer);

            Item item = _ITEMS.Lookup(uniqueId);
            item.Connect();

            RenderToInit(renderer);
        }

        internal void Disconnect(System.Guid uniqueId)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(!IsDisconnected(uniqueId));
            _RENDERERS.Extract(uniqueId);

            Item item = _ITEMS.Lookup(uniqueId);
            item.Disconnect();

            RenderToUpdateLatency(item);
        }

        public void KeepAlive(long serverTicks, System.Guid uniqueId)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            Item item = _ITEMS.Lookup(uniqueId);
            item.Done(serverTicks);
        }

        internal void StartRoutine(long serverTicks)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            if (serverTicks % 20 != 0) return;

            foreach (Item item in _ITEMS.GetValues())
            {
                if (IsDisconnected(item.UniqueId)) continue;

                if (item.IsInProgress()) continue;

                RenderToUpdateLatency(item);

                Queue<ClientboundPlayingPacket> outPacket = _RENDERERS.Lookup(item.UniqueId);
                long payload = new System.Random().NextInt64();
                outPacket.Enqueue(new RequestKeepAlivePacket(payload));

                item.Start(serverTicks);
            }
        }

        public void Dispose()
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            // Assertion.
            System.Diagnostics.Debug.Assert(_ITEMS.Empty);
            System.Diagnostics.Debug.Assert(_RENDERERS.Empty);

            // Release resources.
            _ITEMS.Dispose();
            _RENDERERS.Dispose();

            // Finish.
            System.GC.SuppressFinalize(this);
            _disposed = true;
        }


    }

}

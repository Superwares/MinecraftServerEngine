using Containers;

namespace MinecraftServerFramework
{
    internal sealed class PublicInventoryRenderer : System.IDisposable
    {
        private bool _disposed = false;

        private readonly 
            Table<int, (int, Queue<ClientboundPlayingPacket>)> _Table = new();  // Disposable

        public PublicInventoryRenderer() { }

        ~PublicInventoryRenderer() => System.Diagnostics.Debug.Assert(false);

        public void Add(
            int id,
            int windowId, Queue<ClientboundPlayingPacket> outPackets)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(!_Table.Contains(id));
            _Table.Insert(id, (windowId, outPackets));
        }

        public int Remove(int id)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            (int windowId, var _) = _Table.Extract(id);

            return windowId;
        }

        public void RenderToSet(int index, Item item)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            foreach ((int windowId, var outPackets) in _Table.GetValues())
            {
                System.Diagnostics.Debug.Assert(windowId > 0);

                SlotData slotData = item.ConventToPacketFormat();

                System.Diagnostics.Debug.Assert(windowId >= sbyte.MinValue);
                System.Diagnostics.Debug.Assert(windowId <= sbyte.MaxValue);
                System.Diagnostics.Debug.Assert(index >= short.MinValue);
                System.Diagnostics.Debug.Assert(index <= short.MaxValue);
                outPackets.Enqueue(new SetSlotPacket(
                    (sbyte)windowId, (short)index, slotData));
            }
        }

        public void RenderToEmpty(int index)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            foreach ((int windowId, var outPackets) in _Table.GetValues())
            {
                System.Diagnostics.Debug.Assert(windowId > 0);

                SlotData slotData = new();

                System.Diagnostics.Debug.Assert(windowId >= sbyte.MinValue);
                System.Diagnostics.Debug.Assert(windowId <= sbyte.MaxValue);
                System.Diagnostics.Debug.Assert(index >= short.MinValue);
                System.Diagnostics.Debug.Assert(index <= short.MaxValue);
                outPackets.Enqueue(new SetSlotPacket(
                    (sbyte)windowId, (short)index, slotData));
            }
        }

        public int[] CloseForciblyAndFlush()
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            // TODO: Release Resources for no garbage.
            var arr = _Table.Flush();

            int[] ids = new int[arr.Length];

            for (int i = 0; i < arr.Length; ++i)
            {
                (int id, (int windowId, var outPackets)) = arr[i];

                System.Diagnostics.Debug.Assert(windowId > 0);

                System.Diagnostics.Debug.Assert(windowId >= byte.MinValue);
                System.Diagnostics.Debug.Assert(windowId <= byte.MaxValue);
                outPackets.Enqueue(new ClientboundCloseWindowPacket((byte)windowId));

                ids[i] = id;
            }

            return ids;
        }

        private void Dispose(bool disposing)
        {
            if (_disposed) return;

            System.Diagnostics.Debug.Assert(_Table.Empty);

            if (disposing == true)
            {
                // Release managed resources.
                _Table.Dispose();
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


using System;
using System.Diagnostics;
using Containers;

namespace Protocol
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

        public (int, Queue<ClientboundPlayingPacket>) Remove(int id)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            return _Table.Extract(id);
        }

        public void RenderToSet(int index, Item item)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            foreach ((int windowId, var outPackets) in _Table.GetValues())
            {
                Debug.Assert(windowId > 0);

                Debug.Assert(item.Id >= short.MinValue);
                Debug.Assert(item.Id <= short.MaxValue);
                Debug.Assert(item.Count >= byte.MinValue);
                Debug.Assert(item.Count <= byte.MaxValue);
                SlotData slotData = new((short)item.Id, (byte)item.Count);

                Debug.Assert(windowId >= sbyte.MinValue);
                Debug.Assert(windowId <= sbyte.MaxValue);
                Debug.Assert(index >= short.MinValue);
                Debug.Assert(index <= short.MaxValue);
                outPackets.Enqueue(new SetSlotPacket(
                    (sbyte)windowId, (short)index, slotData));
            }
        }

        public void RenderToEmpty(int index)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            foreach ((int windowId, var outPackets) in _Table.GetValues())
            {
                Debug.Assert(windowId > 0);

                SlotData slotData = new();

                Debug.Assert(windowId >= sbyte.MinValue);
                Debug.Assert(windowId <= sbyte.MaxValue);
                Debug.Assert(index >= short.MinValue);
                Debug.Assert(index <= short.MaxValue);
                outPackets.Enqueue(new SetSlotPacket(
                    (sbyte)windowId, (short)index, slotData));
            }
        }

        public int[] Flush()
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            // TODO: Release Resources for no garbage.
            var arr = _Table.Flush();

            int[] ids = new int[arr.Length];

            for (int i = 0; i < arr.Length; ++i)
            {
                (int id, (int windowId, var outPackets)) = arr[i];

                Debug.Assert(windowId > 0);

                Debug.Assert(windowId >= byte.MinValue);
                Debug.Assert(windowId <= byte.MaxValue);
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

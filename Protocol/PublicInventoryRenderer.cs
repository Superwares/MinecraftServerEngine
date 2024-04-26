
using System.Diagnostics;
using Containers;

namespace Protocol
{
    internal sealed class PublicInventoryRenderer
    {
        public readonly int Id;

        private readonly int _WindowId;
        private readonly Queue<ClientboundPlayingPacket> _outPackets;

        public PublicInventoryRenderer(
            int id, 
            int windowId, Queue<ClientboundPlayingPacket> outPackets)
        {
            Id = id;

            _WindowId = windowId;
            _outPackets = outPackets;
        }

        private void Enqueue(ClientboundPlayingPacket packet)
        {
            _outPackets.Enqueue(packet);
        }

        public void Set(int index, Item item)
        {
            Debug.Assert(_WindowId > 0);

            Debug.Assert(item.Id >= short.MinValue);
            Debug.Assert(item.Id <= short.MaxValue);
            Debug.Assert(item.Count >= byte.MinValue);
            Debug.Assert(item.Count <= byte.MaxValue);
            SlotData slotData = new((short)item.Id, (byte)item.Count);

            Debug.Assert(_WindowId >= sbyte.MinValue);
            Debug.Assert(_WindowId <= sbyte.MaxValue);
            Debug.Assert(index >= short.MinValue);
            Debug.Assert(index <= short.MaxValue);
            Enqueue(new SetSlotPacket(
                (sbyte)_WindowId, (short)index, slotData));
        }

        public void Empty(int index)
        {
            Debug.Assert(_WindowId > 0);

            SlotData slotData = new();

            Debug.Assert(_WindowId >= sbyte.MinValue);
            Debug.Assert(_WindowId <= sbyte.MaxValue);
            Debug.Assert(index >= short.MinValue);
            Debug.Assert(index <= short.MaxValue);
            Enqueue(new SetSlotPacket(
                (sbyte)_WindowId, (short)index, slotData));
        }

        public void Close()
        {
            Debug.Assert(_WindowId >= byte.MinValue);
            Debug.Assert(_WindowId <= byte.MaxValue);
            Enqueue(new CloseWindowPacket((byte)_WindowId);
        }

    }

}

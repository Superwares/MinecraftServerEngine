
using System;
using System.Diagnostics;
using Containers;

namespace Protocol
{
    internal abstract class InventoryRenderer
    {
        public abstract int SlotCount { get; }

        protected readonly int _WindowId;
        private readonly Queue<ClientboundPlayingPacket> _outPackets;

        public InventoryRenderer(int windowId, Queue<ClientboundPlayingPacket> outPackets)
        {
            _WindowId = windowId;
            _outPackets = outPackets;
        }

        protected void Enqueue(ClientboundPlayingPacket packet)
        {
            _outPackets.Enqueue(packet);
        }

        /*public abstract void Init(Item?[] items);*/

        public abstract void Set(int index, Item item);

        public abstract void Empty(int index);

    }

    internal abstract class PlayerInventoryRenderer : InventoryRenderer
    {
        public PlayerInventoryRenderer(
            int windowId, Queue<ClientboundPlayingPacket> outPackets) 
            : base(windowId, outPackets) { }
    }

    internal sealed class CompleteSelfPlayerInventoryRenderer : PlayerInventoryRenderer
    {
        public override int SlotCount => 46;

        public CompleteSelfPlayerInventoryRenderer(
            int windowId, Queue<ClientboundPlayingPacket> outPackets) 
            : base(windowId, outPackets) { }

        /*public override void Init(Item?[] items)
        {
            Debug.Assert(items.Length == PlayerInventory.TotalCount);
            SlotData[] arr = new SlotData[PlayerInventory.TotalCount];

            for (int i = 0; i < PlayerInventory.TotalCount; ++i)
            {
                Item? item = items[i];
                if (item == null)
                {
                    arr[i] = new();
                    continue;
                }

                Debug.Assert(item.Id >= short.MinValue);
                Debug.Assert(item.Id <= short.MaxValue);
                Debug.Assert(item.Count >= byte.MinValue);
                Debug.Assert(item.Count <= byte.MaxValue);
                arr[i] = new((short)item.Id, (byte)item.Count);
            }

            Debug.Assert(_WindowId >= byte.MinValue);
            Debug.Assert(_WindowId <= byte.MaxValue);
            Enqueue(new SetWindowItemsPacket((byte)_WindowId, arr));
        }*/

        public override void Set(int index, Item item)
        {
            Debug.Assert(_WindowId == 0);

            Debug.Assert(index >= 0);
            Debug.Assert(index < 46);

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

        public override void Empty(int index)
        {
            Debug.Assert(_WindowId == 0);

            SlotData slotData = new();

            Debug.Assert(_WindowId >= sbyte.MinValue);
            Debug.Assert(_WindowId <= sbyte.MaxValue);
            Debug.Assert(index >= short.MinValue);
            Debug.Assert(index <= short.MaxValue);
            Enqueue(new SetSlotPacket(
                (sbyte)_WindowId, (short)index, slotData));
        }
    }

    internal sealed class SelfPlayerInventoryRenderer : PlayerInventoryRenderer
    {
        public override int SlotCount => 36;

        private readonly int _offset;

        public SelfPlayerInventoryRenderer(
            int windowId, Queue<ClientboundPlayingPacket> outPackets, int offset)
            : base(windowId, outPackets)
        {
            Debug.Assert(offset >= 0);
            _offset = offset;
        }

        /*public override void Init(Item?[] items)
        {
            Debug.Assert(items.Length == PlayerInventory.TotalCount);
            SlotData[] arr = new SlotData[PlayerInventory.TotalCount];

            for (int i = 0; i < PlayerInventory.TotalCount; ++i)
            {
                Item? item = items[i];
                if (item == null)
                {
                    arr[i] = new();
                    continue;
                }

                Debug.Assert(item.Id >= short.MinValue);
                Debug.Assert(item.Id <= short.MaxValue);
                Debug.Assert(item.Count >= byte.MinValue);
                Debug.Assert(item.Count <= byte.MaxValue);
                arr[i] = new((short)item.Id, (byte)item.Count);
            }

            Debug.Assert(_WindowId >= byte.MinValue);
            Debug.Assert(_WindowId <= byte.MaxValue);
            Enqueue(new SetWindowItemsPacket((byte)_WindowId, arr));
        }*/

        public override void Set(int index, Item item)
        {
            Debug.Assert(_WindowId > 0);

            if (index >= 0 && index <= 8)
                return;
            if (index == 45)
                return;

            Debug.Assert(index >= 9);
            Debug.Assert(index < 45);

            int indexNormalized = index - 9 + _offset;

            Debug.Assert(item.Id >= short.MinValue);
            Debug.Assert(item.Id <= short.MaxValue);
            Debug.Assert(item.Count >= byte.MinValue);
            Debug.Assert(item.Count <= byte.MaxValue);
            SlotData slotData = new((short)item.Id, (byte)item.Count);

            Debug.Assert(_WindowId >= sbyte.MinValue);
            Debug.Assert(_WindowId <= sbyte.MaxValue);
            Debug.Assert(indexNormalized >= short.MinValue);
            Debug.Assert(indexNormalized <= short.MaxValue);
            Enqueue(new SetSlotPacket(
                (sbyte)_WindowId, (short)indexNormalized, slotData));
        }

        public override void Empty(int index)
        {
            Debug.Assert(_WindowId > 0);

            if (index >= 0 && index <= 8)
                return;
            if (index == 45)
                return;

            Debug.Assert(index >= 9);
            Debug.Assert(index < 45);

            int indexNormalized = index - 9 + _offset;
            SlotData slotData = new();

            Debug.Assert(_WindowId >= sbyte.MinValue);
            Debug.Assert(_WindowId <= sbyte.MaxValue);
            Debug.Assert(indexNormalized >= short.MinValue);
            Debug.Assert(indexNormalized <= short.MaxValue);
            Enqueue(new SetSlotPacket(
                (sbyte)_WindowId, (short)indexNormalized, slotData));
        }

    }

    internal sealed class OtherPlayerInventoryRenderer : PlayerInventoryRenderer
    {
        public override int SlotCount => 36;

        public OtherPlayerInventoryRenderer(
            int windowId, Queue<ClientboundPlayingPacket> outPackets)
            : base(windowId, outPackets) { }

        public override void Set(int index, Item item)
        {
            Debug.Assert(_WindowId > 0);

            if (index >= 0 && index <= 8)
                return;
            if (index == 45)
                return;

            Debug.Assert(index >= 9);
            Debug.Assert(index < 45);

            int indexNormalized = index - 9;

            Debug.Assert(item.Id >= short.MinValue);
            Debug.Assert(item.Id <= short.MaxValue);
            Debug.Assert(item.Count >= byte.MinValue);
            Debug.Assert(item.Count <= byte.MaxValue);
            SlotData slotData = new((short)item.Id, (byte)item.Count);

            Debug.Assert(_WindowId >= sbyte.MinValue);
            Debug.Assert(_WindowId <= sbyte.MaxValue);
            Debug.Assert(indexNormalized >= short.MinValue);
            Debug.Assert(indexNormalized <= short.MaxValue);
            Enqueue(new SetSlotPacket(
                (sbyte)_WindowId, (short)indexNormalized, slotData));
        }

        public override void Empty(int index)
        {
            Debug.Assert(_WindowId > 0);

            if (index >= 0 && index <= 8)
                return;
            if (index == 45)
                return;

            Debug.Assert(index >= 9);
            Debug.Assert(index < 45);

            int indexNormalized = index - 9;
            SlotData slotData = new();

            Debug.Assert(_WindowId >= sbyte.MinValue);
            Debug.Assert(_WindowId <= sbyte.MaxValue);
            Debug.Assert(indexNormalized >= short.MinValue);
            Debug.Assert(indexNormalized <= short.MaxValue);
            Enqueue(new SetSlotPacket(
                (sbyte)_WindowId, (short)indexNormalized, slotData));
        }

    }

    internal sealed class ChestInventoryRenderer : InventoryRenderer
    {
        public override int SlotCount => 27;

        public ChestInventoryRenderer(
            int windowId, Queue<ClientboundPlayingPacket> outPackets)
            : base(windowId, outPackets) { }

        public override void Set(int index, Item item)
        {
            Debug.Assert(_WindowId > 0);

            Debug.Assert(index >= 0 && index < 27);

            int indexNormalized = index;

            Debug.Assert(item.Id >= short.MinValue);
            Debug.Assert(item.Id <= short.MaxValue);
            Debug.Assert(item.Count >= byte.MinValue);
            Debug.Assert(item.Count <= byte.MaxValue);
            SlotData slotData = new((short)item.Id, (byte)item.Count);

            Debug.Assert(_WindowId >= sbyte.MinValue);
            Debug.Assert(_WindowId <= sbyte.MaxValue);
            Debug.Assert(indexNormalized >= short.MinValue);
            Debug.Assert(indexNormalized <= short.MaxValue);
            Enqueue(new SetSlotPacket(
                (sbyte)_WindowId, (short)indexNormalized, slotData));
        }

        public override void Empty(int index)
        {
            Debug.Assert(_WindowId > 0);

            Debug.Assert(index >= 0 && index < 27);

            int indexNormalized = index;
            SlotData slotData = new();

            Debug.Assert(_WindowId >= sbyte.MinValue);
            Debug.Assert(_WindowId <= sbyte.MaxValue);
            Debug.Assert(indexNormalized >= short.MinValue);
            Debug.Assert(indexNormalized <= short.MaxValue);
            Enqueue(new SetSlotPacket(
                (sbyte)_WindowId, (short)indexNormalized, slotData));
        }

    }
}

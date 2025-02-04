using Common;
using Containers;

using MinecraftPrimitives;

using MinecraftServerEngine.Inventories;

namespace MinecraftServerEngine.Renderers
{

    internal sealed class WindowRenderer : Renderer
    {

        // TODO: Remove offset parameter.
        // offset value is only for to render private inventory when _id == -1;
        internal void Update(
            SharedInventory sharedInventory,
            PlayerInventory playerInventory, InventorySlot cursor)
        {
            System.Diagnostics.Debug.Assert(playerInventory != null);
            System.Diagnostics.Debug.Assert(cursor != null);

            using MinecraftProtocolDataStream buffer = new();

            if (sharedInventory == null)
            {
                int windowId = 0;

                foreach (InventorySlot slot in playerInventory.Slots)
                {
                    System.Diagnostics.Debug.Assert(slot != null);
                    slot.WriteData(buffer);
                }

                int totalSlots = playerInventory.GetTotalSlotCount();
                System.Diagnostics.Debug.Assert(totalSlots > 0);

                System.Diagnostics.Debug.Assert(windowId >= byte.MinValue);
                System.Diagnostics.Debug.Assert(windowId <= byte.MaxValue);
                System.Diagnostics.Debug.Assert(totalSlots > 0);
                Render(new WindowItemsPacket(
                    (byte)windowId, totalSlots, buffer.ReadData()));
            }
            else
            {
                int windowId = 1;

                System.Diagnostics.Debug.Assert(sharedInventory.Slots != null);
                foreach (InventorySlot slot in sharedInventory.Slots)
                {
                    System.Diagnostics.Debug.Assert(slot != null);
                    slot.WriteData(buffer);
                }

                foreach (InventorySlot slot in playerInventory.GetPrimarySlots())
                {
                    System.Diagnostics.Debug.Assert(slot != null);
                    slot.WriteData(buffer);
                }

                int totalSlots = sharedInventory.GetTotalSlotCount() + PlayerInventory.PrimarySlotCount;
                byte[] data = buffer.ReadData();

                System.Diagnostics.Debug.Assert(windowId >= byte.MinValue);
                System.Diagnostics.Debug.Assert(windowId <= byte.MaxValue);
                System.Diagnostics.Debug.Assert(totalSlots > 0);
                Render(new WindowItemsPacket(
                    (byte)windowId, totalSlots, data));

            }

            cursor.WriteData(buffer);

            Render(new SetSlotPacket(-1, 0, buffer.ReadData()));
        }



        public WindowRenderer(ConcurrentQueue<ClientboundPlayingPacket> outPackets,
            PlayerInventory invPrivate, InventorySlot cursor)
            : base(outPackets)
        {
            System.Diagnostics.Debug.Assert(outPackets != null);
            System.Diagnostics.Debug.Assert(invPrivate != null);
            System.Diagnostics.Debug.Assert(cursor != null);

            Update(null, invPrivate, cursor);
        }

        internal void Open(
            SharedInventory sharedInventory,
            PlayerInventory playerInventory, InventorySlot cursor)
        {
            System.Diagnostics.Debug.Assert(sharedInventory != null);
            System.Diagnostics.Debug.Assert(playerInventory != null);
            System.Diagnostics.Debug.Assert(cursor != null);

            int windowId = 1;
            string title = sharedInventory.Title;
            int totalSharedSlots = sharedInventory.GetTotalSlotCount();

            System.Diagnostics.Debug.Assert(totalSharedSlots > 0);

            System.Diagnostics.Debug.Assert(windowId >= byte.MinValue);
            System.Diagnostics.Debug.Assert(windowId <= byte.MaxValue);
            Render(new OpenWindowPacket((byte)windowId, "minecraft:chest", title, (byte)totalSharedSlots));

            Update(sharedInventory, playerInventory, cursor);
        }

        internal void Reset(PlayerInventory playerInventory, InventorySlot cursor)
        {
            System.Diagnostics.Debug.Assert(cursor.Empty);

            Update(null, playerInventory, cursor);
        }

        internal bool HandleMainHandSlot(PlayerInventory playerInventory)
        {
            System.Diagnostics.Debug.Assert(playerInventory != null);

            using MinecraftProtocolDataStream buffer = new();

            InventorySlot mainSlot = playerInventory.HandleMainHandSlot2();

            mainSlot.WriteData(buffer);

            int slot = PlayerInventory.HotbarSlotsOffset + playerInventory.ActiveMainHandIndex;

            int windowId = 0;
            System.Diagnostics.Debug.Assert(windowId >= sbyte.MinValue);
            System.Diagnostics.Debug.Assert(windowId <= sbyte.MaxValue);
            System.Diagnostics.Debug.Assert(slot >= short.MinValue);
            System.Diagnostics.Debug.Assert(slot <= short.MaxValue);
            Render(new SetSlotPacket((sbyte)windowId, (short)slot, buffer.ReadData()));

            return mainSlot.Empty;
        }

    }

}

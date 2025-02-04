using Common;
using Containers;

using MinecraftPrimitives;

namespace MinecraftServerEngine.Renderers
{
    internal sealed class PublicInventoryRenderer : Renderer
    {
        private const byte WindowId = 1;

        internal readonly UserId UserId;

        internal PublicInventoryRenderer(
            UserId userId,
            ConcurrentQueue<ClientboundPlayingPacket> outPackets)
            : base(outPackets)
        {
            System.Diagnostics.Debug.Assert(outPackets != null);

            UserId = userId;
        }

        //public void Open(string title, int totalSharedSlots, int totalSlots, byte[] data)
        //{
        //    System.Diagnostics.Debug.Assert(title != null);
        //    System.Diagnostics.Debug.Assert(totalSharedSlots > 0);
        //    Render(new OpenWindowPacket(WindowId, "minecraft:chest", title, (byte)totalSharedSlots));


        //    System.Diagnostics.Debug.Assert(totalSlots > 0);
        //    System.Diagnostics.Debug.Assert(data != null);
        //    Render(new WindowItemsPacket(WindowId, totalSlots, data));
        //}

        internal void Update(int count, byte[] data)
        {
            System.Diagnostics.Debug.Assert(count > 0);
            System.Diagnostics.Debug.Assert(data != null);

            Render(new WindowItemsPacket(WindowId, count, data));
        }
    }
}

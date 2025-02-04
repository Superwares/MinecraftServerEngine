using Common;
using Containers;
using MinecraftServerEngine.Protocols;

namespace MinecraftServerEngine.Renderers
{
    internal abstract class Renderer
    {
        private protected readonly ConcurrentQueue<ClientboundPlayingPacket> OutPackets;

        public Renderer(ConcurrentQueue<ClientboundPlayingPacket> outPackets)
        {
            OutPackets = outPackets;
        }

        protected void Render(ClientboundPlayingPacket packet)
        {
            OutPackets.Enqueue(packet);
        }

        protected void Render(params ClientboundPlayingPacket[] packets)
        {
            if (packets == null || packets.Length == 0)
            {
                return;
            }

            foreach (ClientboundPlayingPacket p in packets)
            {
                OutPackets.Enqueue(p);
            }

        }
    }
}

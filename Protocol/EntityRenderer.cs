using Containers;

namespace Protocol
{
    internal class EntityRenderer
    {
        private bool _init = false;
        private readonly Queue<ClientboundPlayingPacket> _outPackets;

        public EntityRenderer(Queue<ClientboundPlayingPacket> outPackets)
        {
            _outPackets = outPackets;
        }

        public void Render(ClientboundPlayingPacket packet)
        {
            if (!_init)
                // TODO: send spawn packet
        }

    }
}

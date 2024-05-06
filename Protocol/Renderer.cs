using Common;
using Containers;
using System.Runtime.CompilerServices;

namespace Protocol
{
    internal abstract class Renderer
    {
        private readonly int _CONN_ID;

        private readonly Queue<ClientboundPlayingPacket> _OUT_PACKETS;

        public Renderer(int connId, Queue<ClientboundPlayingPacket> outPackets)
        {
            _CONN_ID = connId;
            _OUT_PACKETS = outPackets;
        }

        protected void Render(ClientboundPlayingPacket packet)
        {
            _OUT_PACKETS.Enqueue(packet);
        }

    }

    internal sealed class EntityRenderer : Renderer
    {
        public EntityRenderer(int connId, Queue<ClientboundPlayingPacket> outPackets)
            : base(connId, outPackets) { }
        
        public void MoveAndRotate(
            int entityId, 
            Entity.Vector posNew, Entity.Vector pos, Entity.Angles look, bool onGround)
        {
            double dx = (posNew.X - pos.X) * (32 * 128),
                dy = (posNew.Y - pos.Y) * (32 * 128),
                dz = (posNew.Z - pos.Z) * (32 * 128);
            System.Diagnostics.Debug.Assert(Comparing.IsGreaterThanOrEqualTo(dx, short.MinValue));
            System.Diagnostics.Debug.Assert(Comparing.IsLessThanOrEqualTo(dx, short.MaxValue));
            System.Diagnostics.Debug.Assert(Comparing.IsGreaterThanOrEqualTo(dy, short.MinValue));
            System.Diagnostics.Debug.Assert(Comparing.IsLessThanOrEqualTo(dy, short.MaxValue));
            System.Diagnostics.Debug.Assert(Comparing.IsGreaterThanOrEqualTo(dz, short.MinValue));
            System.Diagnostics.Debug.Assert(Comparing.IsLessThanOrEqualTo(dz, short.MaxValue));

            (byte x, byte y) = look.ConvertToPacketFormat();
            Render(new EntityLookAndRelMovePacket(
                entityId,
                (short)dx, (short)dy, (short)dz,
                x, y,
                onGround));
            Render(new EntityHeadLookPacket(entityId, x));
        }

        public void Move(int entityId, Entity.Vector posNew, Entity.Vector pos, bool onGround)
        {
            double dx = (posNew.X - pos.X) * (32 * 128), 
                dy = (posNew.Y - pos.Y) * (32 * 128), 
                dz = (posNew.Z - pos.Z) * (32 * 128);
            System.Diagnostics.Debug.Assert(Comparing.IsGreaterThanOrEqualTo(dx, short.MinValue));
            System.Diagnostics.Debug.Assert(Comparing.IsLessThanOrEqualTo(dx, short.MaxValue));
            System.Diagnostics.Debug.Assert(Comparing.IsGreaterThanOrEqualTo(dy, short.MinValue));
            System.Diagnostics.Debug.Assert(Comparing.IsLessThanOrEqualTo(dy, short.MaxValue));
            System.Diagnostics.Debug.Assert(Comparing.IsGreaterThanOrEqualTo(dz, short.MinValue));
            System.Diagnostics.Debug.Assert(Comparing.IsLessThanOrEqualTo(dz, short.MaxValue));

            Render(new EntityRelMovePacket(
                entityId,
                (short)dx, (short)dy, (short)dz,
                onGround));
        }

        public void Rotate(int entityId, Entity.Angles look, bool onGround)
        {
            (byte x, byte y) = look.ConvertToPacketFormat();
            Render(new EntityLookPacket(entityId, x, y, onGround));
            Render(new EntityHeadLookPacket(entityId, x));
        }

        public void Stand(int entityId)
        {
            Render(new EntityPacket(entityId));
        }

        public void Teleport(
            int entityId, Entity.Vector pos, Entity.Angles look, bool onGround)
        {
            Render(new EntityTeleportPacket(
                    entityId,
                    pos.X, pos.Y, pos.Z,
                    look.Yaw, look.Pitch,
                    onGround));
        }

        public void ChangeForms(int entityId, bool sneaking, bool sprinting)
        {
            byte flags = 0x00;

            if (sneaking)
                flags |= 0x02;
            if (sprinting)
                flags |= 0x08;

            using EntityMetadata metadata = new();
            metadata.AddByte(0, flags);

            Render(new EntityMetadataPacket(entityId, metadata.WriteData()));
        }

    }

    /*internal sealed class SelfPlayerRenderer : Renderer
    {
        private readonly Client _CLIENT;

        public EntityRenderer(
            int connId, Queue<ClientboundPlayingPacket> outPackets, 
            Client client)
            : base(connId, outPackets) 
        {
            _CLIENT = client;
        }

        public void AddForce()
        {
            _CLIENT.Send();
        }

    }
*/
}

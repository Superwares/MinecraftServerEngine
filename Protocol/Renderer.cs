using Common;
using Containers;

namespace Protocol
{
    internal abstract class Renderer : System.IDisposable
    {
        private bool _disposed = false;

        public readonly int Id;

        private readonly Queue<ClientboundPlayingPacket> _OUT_PACKETS;

        public Renderer(int id, Queue<ClientboundPlayingPacket> outPackets)
        {
            Id = id;
            _OUT_PACKETS = outPackets;
        }

        ~Renderer() => System.Diagnostics.Debug.Assert(false);

        protected void Render(ClientboundPlayingPacket packet)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            _OUT_PACKETS.Enqueue(packet);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;

            // Assertion

            if (disposing == true)
            {
                // managed objects

            }

            // unmanaged objects

            _disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            System.GC.SuppressFinalize(this);
        }

    }

    internal sealed class EntityRenderer : Renderer
    {
        private bool _disposed = false;

        private bool _disconnected = false;
        public bool IsDisconnected => _disconnected;

        private Chunk.Vector _p;

        private int _d;

        public EntityRenderer(
            int id, Queue<ClientboundPlayingPacket> outPackets,
            Chunk.Vector p, int d) : base(id, outPackets) 
        {
            System.Diagnostics.Debug.Assert(d > 0);
            _p = p;
            _d = d;
        }

        ~EntityRenderer() => System.Diagnostics.Debug.Assert(false);

        public void Disconnect()
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(!_disconnected);

            _disconnected = true;
        }

        public void Update(Chunk.Vector p)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(!_disconnected);

            _p = p;
        }

        public void Update(int d)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(!_disconnected);
            System.Diagnostics.Debug.Assert(d > 0);

            _d = d;
        }

        public bool CanRender(Entity.Vector p, BoundingBox boundingBox)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(!_disconnected);
            System.Diagnostics.Debug.Assert(_d > 0);

            Chunk.Grid gridEntity = Chunk.Grid.Generate(p, boundingBox);
            Chunk.Grid gridRender = Chunk.Grid.Generate(_p, _d);

            bool overlap = (Chunk.Grid.Generate(gridEntity, gridRender) != null);
            return overlap;
        }

        public void MoveAndRotate(
            int entityId, 
            Entity.Vector posNew, Entity.Vector pos, Entity.Angles look, bool onGround)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(!_disconnected);

            double dx = (posNew.X - pos.X) * (32 * 128),
                dy = (posNew.Y - pos.Y) * (32 * 128),
                dz = (posNew.Z - pos.Z) * (32 * 128);
            System.Diagnostics.Debug.Assert(Comparing.IsInRange(dx, short.MinValue, short.MinValue));
            System.Diagnostics.Debug.Assert(Comparing.IsInRange(dy, short.MinValue, short.MinValue));
            System.Diagnostics.Debug.Assert(Comparing.IsInRange(dz, short.MinValue, short.MinValue));

            (byte x, byte y) = look.ConvertToPacketFormat();
            Render(new EntityLookAndRelMovePacket(
                entityId,
                (short)dx, (short)dy, (short)dz,
                x, y,
                onGround));
            Render(new EntityHeadLookPacket(entityId, x));

        }

        public void Move(
            int entityId, 
            Entity.Vector posNew, Entity.Vector pos, bool onGround)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(!_disconnected);

            double dx = (posNew.X - pos.X) * (32 * 128), 
                dy = (posNew.Y - pos.Y) * (32 * 128), 
                dz = (posNew.Z - pos.Z) * (32 * 128);
            System.Diagnostics.Debug.Assert(Comparing.IsInRange(dx, short.MinValue, short.MinValue));
            System.Diagnostics.Debug.Assert(Comparing.IsInRange(dy, short.MinValue, short.MinValue));
            System.Diagnostics.Debug.Assert(Comparing.IsInRange(dz, short.MinValue, short.MinValue));

            Render(new EntityRelMovePacket(
                entityId,
                (short)dx, (short)dy, (short)dz,
                onGround));

        }

        public void Rotate(int entityId, Entity.Angles look, bool onGround)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(!_disconnected);

            (byte x, byte y) = look.ConvertToPacketFormat();
            Render(new EntityLookPacket(entityId, x, y, onGround));
            Render(new EntityHeadLookPacket(entityId, x));
        }

        public void Stand(int entityId)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(!_disconnected);

            Render(new EntityPacket(entityId));
        }

        public void Teleport(
            int entityId, Entity.Vector pos, Entity.Angles look, bool onGround)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            Render(new EntityTeleportPacket(
                entityId,
                pos.X, pos.Y, pos.Z,
                look.Yaw, look.Pitch,
                onGround));
        }

        public void ChangeForms(int entityId, bool sneaking, bool sprinting)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            byte flags = 0x00;

            if (sneaking)
                flags |= 0x02;
            if (sprinting)
                flags |= 0x08;

            using EntityMetadata metadata = new();
            metadata.AddByte(0, flags);

            Render(new EntityMetadataPacket(entityId, metadata.WriteData()));
        }

        public void DestroyEntity(int entityId)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            Render(new DestroyEntitiesPacket([entityId]));
        }

        public void Flush(int entityId)
        {
            DestroyEntity(entityId);
        }

        protected override void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                // Assertion.

                if (disposing == true)
                {
                    // Release managed resources.
                }

                // Release unmanaged resources.

                _disposed = true;
            }

            base.Dispose(disposing);
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

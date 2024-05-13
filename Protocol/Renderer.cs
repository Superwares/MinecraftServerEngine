using Common;
using Containers;

namespace Protocol
{
    internal abstract class Renderer : System.IDisposable
    {
        private bool _disposed = false;

        private readonly Queue<ClientboundPlayingPacket> _OUT_PACKETS;

        public Renderer(Queue<ClientboundPlayingPacket> outPackets)
        {
            _OUT_PACKETS = outPackets;
        }

        ~Renderer() => System.Diagnostics.Debug.Assert(false);

        protected void Render(ClientboundPlayingPacket packet)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            _OUT_PACKETS.Enqueue(packet);
        }

        public virtual void Dispose()
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            // Assertion

            // Release Resources.

            // Finish.
            System.GC.SuppressFinalize(this);
            _disposed = true;
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
            Queue<ClientboundPlayingPacket> outPackets,
            Chunk.Vector p, int d) : base(outPackets) 
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

        public bool CanRender(Entity.Vector p, Entity.BoundingBox bb)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(!_disconnected);
            System.Diagnostics.Debug.Assert(_d > 0);

            /*System.Console.WriteLine($"d: {_d}");*/
            /*System.Console.WriteLine();*/
            Chunk.Grid gridEntity = Chunk.Grid.Generate(p, bb);
            /*System.Console.WriteLine($"gridEntity: {gridEntity}");*/
            Chunk.Grid gridRender = Chunk.Grid.Generate(_p, _d);
            /*System.Console.WriteLine($"gridRender: {gridRender}");*/

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
            System.Diagnostics.Debug.Assert(Comparing.IsInRange(dx, short.MinValue, short.MaxValue));
            System.Diagnostics.Debug.Assert(Comparing.IsInRange(dy, short.MinValue, short.MaxValue));
            System.Diagnostics.Debug.Assert(Comparing.IsInRange(dz, short.MinValue, short.MaxValue));

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
            System.Diagnostics.Debug.Assert(Comparing.IsInRange(dx, short.MinValue, short.MaxValue));
            System.Diagnostics.Debug.Assert(Comparing.IsInRange(dy, short.MinValue, short.MaxValue));
            System.Diagnostics.Debug.Assert(Comparing.IsInRange(dz, short.MinValue, short.MaxValue));

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

        public override void Dispose()
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            // Assertion.

            // Release resources.

            base.Dispose();
            _disposed = true;
        }

    }

    internal sealed class SelfPlayerRenderer : Renderer
    {
        private bool _disposed = false;

        private readonly Client _CLIENT;

        public SelfPlayerRenderer(
            Queue<ClientboundPlayingPacket> outPackets,
            Client client) : base(outPackets)
        {
            _CLIENT = client;
        }

        public void Init(
            int entityId,
            Entity.Vector v, Entity.Vector p, Entity.Angles look)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            {
                int payload = new System.Random().Next();
                Render(new TeleportSelfPlayerPacket(
                    p.X, p.Y, p.Z,
                    look.Yaw, look.Pitch,
                    false, false, false, false, false,
                    payload));
            }

            {
                Render(new SetPlayerAbilitiesPacket(
                    false, false, false, false, 0f, 0));
            }

            {
                Render(new EntityVelocityPacket(
                    entityId,
                    Conversions.ToShort(v.X * 8000),
                    Conversions.ToShort(v.Y * 8000),
                    Conversions.ToShort(v.Z * 8000)));
            }


        }

        /*public void Teleport(Entity.Vector p, Entity.Angles look)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            int payload = new System.Random().Next();
            Render(new TeleportSelfPlayerPacket(
                p.X, p.Y, p.Z,
                look.Yaw, look.Pitch,
                false, false, false, false, false,
                payload));
        }*/

        public void ApplyVelocity(int entityId, Entity.Vector v)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            using Buffer buffer = new();

            var packet = new EntityVelocityPacket(
                entityId,
                Conversions.ToShort(v.X * 8000),
                Conversions.ToShort(v.Y * 8000),
                Conversions.ToShort(v.Z * 8000));
            packet.Write(buffer);
            _CLIENT.Send(buffer);
        }

        public override void Dispose()
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            // Assertion.

            // Release resources.

            base.Dispose();
            _disposed = true;
        }

    }

}

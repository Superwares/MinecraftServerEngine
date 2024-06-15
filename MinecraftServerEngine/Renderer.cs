using Common;
using Containers;
using PhysicsEngine;

namespace MinecraftServerEngine
{
    internal abstract class Renderer
    {
        private readonly ConcurrentQueue<ClientboundPlayingPacket> _OUT_PACKETS;

        public Renderer(ConcurrentQueue<ClientboundPlayingPacket> outPackets)
        {
            _OUT_PACKETS = outPackets;
        }

        protected void Render(ClientboundPlayingPacket packet)
        {
            _OUT_PACKETS.Enqueue(packet);
        }
        
    }

    internal sealed class PlayerListRenderer : Renderer
    {
        public PlayerListRenderer(
            ConcurrentQueue<ClientboundPlayingPacket> outPackets)
            : base(outPackets) { }

        public void AddPlayerWithLaytency(
            System.Guid userId, string username, long ticks)
        {
            System.Diagnostics.Debug.Assert(ticks <= int.MaxValue);
            System.Diagnostics.Debug.Assert(ticks >= int.MinValue);
            
            int ms = (int)ticks;

            Render(new AddPlayerListItemPacket(userId, username, ms));
        }

        public void RemovePlayer(System.Guid userId)
        {
            Render(new RemovePlayerListItemPacket(userId));
        }

        public void UpdatePlayerLatency(System.Guid userId, long ticks)
        {
            System.Diagnostics.Debug.Assert(ticks <= int.MaxValue);
            System.Diagnostics.Debug.Assert(ticks >= int.MinValue);

            int ms = (int)ticks;

            Render(new UpdatePlayerListItemLatencyPacket(userId, (int)ms));
        }
    }

    /*internal sealed class InventoryRenderer : Renderer
    {

    }

    internal sealed class ChunkRenderer : Renderer
    {

    }*/

    internal abstract class WorldRenderer : Renderer
    {
        public WorldRenderer(
            ConcurrentQueue<ClientboundPlayingPacket> outPackets) 
            : base(outPackets) { }

        // particles
    }

    internal sealed class EntityRenderer : WorldRenderer
    {
        private bool _disconnected = false;
        public bool IsDisconnected => _disconnected;


        private readonly int _id;
        public int Id => _id;

        private ChunkLocation _loc;

        private int _dEntityRendering = -1;


        public EntityRenderer(
            ConcurrentQueue<ClientboundPlayingPacket> outPackets,
            int id,
            ChunkLocation loc, int dEntityRendering) 
            : base(outPackets) 
        {
            _id = id;

            _loc = loc;

            _dEntityRendering = dEntityRendering;
        }

        public void Disconnect()
        {
            System.Diagnostics.Debug.Assert(!_disconnected);

            _disconnected = true;
        }

        public void Update(ChunkLocation loc)
        {
            System.Diagnostics.Debug.Assert(!_disconnected);

            _loc = loc;
        }

        public void Update(int d)
        {
            System.Diagnostics.Debug.Assert(!_disconnected);
            System.Diagnostics.Debug.Assert(d > 0);

            _dEntityRendering = d;
        }

        public bool CanRender(Vector p)
        {
            System.Diagnostics.Debug.Assert(!_disconnected);
            System.Diagnostics.Debug.Assert(_dEntityRendering > 0);

            ChunkGrid grid = ChunkGrid.Generate(_loc, _dEntityRendering);
            ChunkLocation loc = ChunkLocation.Generate(p);
            return grid.Contains(loc);
        }

        public void MoveAndRotate(
            int id, 
            Vector p, Vector pPrev, Entity.Angles look, bool onGround)
        {
            System.Diagnostics.Debug.Assert(id != Id);

            System.Diagnostics.Debug.Assert(!_disconnected);

            double dx = (p.X - pPrev.X) * (32 * 128),
                dy = (p.Y - pPrev.Y) * (32 * 128),
                dz = (p.Z - pPrev.Z) * (32 * 128);
            System.Diagnostics.Debug.Assert(dx >= short.MinValue && dx <= short.MaxValue);
            System.Diagnostics.Debug.Assert(dy >= short.MinValue && dy <= short.MaxValue);
            System.Diagnostics.Debug.Assert(dz >= short.MinValue && dz <= short.MaxValue);

            (byte x, byte y) = look.ConvertToPacketFormat();
            Render(new EntityLookAndRelMovePacket(
                id,
                (short)dx, (short)dy, (short)dz,
                x, y,
                onGround));
            Render(new EntityHeadLookPacket(id, x));
        
        }

        public void Move(int id, Vector p, Vector pPrev, bool onGround)
        {
            System.Diagnostics.Debug.Assert(id != Id);

            System.Diagnostics.Debug.Assert(!_disconnected);

            double dx = (p.X - pPrev.X) * (32 * 128), 
                dy = (p.Y - pPrev.Y) * (32 * 128), 
                dz = (p.Z - pPrev.Z) * (32 * 128);
            System.Diagnostics.Debug.Assert((dx >= short.MinValue) && (dx <= short.MaxValue));
            System.Diagnostics.Debug.Assert((dy >= short.MinValue) && (dy <= short.MaxValue));
            System.Diagnostics.Debug.Assert((dz >= short.MinValue) && (dz <= short.MaxValue));

            Render(new EntityRelMovePacket(
                id,
                (short)dx, (short)dy, (short)dz,
                onGround));

        }

        public void Rotate(int id, Entity.Angles look, bool onGround)
        {
            System.Diagnostics.Debug.Assert(id != Id);

            System.Diagnostics.Debug.Assert(!_disconnected);

            (byte x, byte y) = look.ConvertToPacketFormat();
            Render(new EntityLookPacket(id, x, y, onGround));
            Render(new EntityHeadLookPacket(id, x));
        }

        public void Stand(int id)
        {
            System.Diagnostics.Debug.Assert(id != Id);

            System.Diagnostics.Debug.Assert(!_disconnected);

            Render(new EntityPacket(id));
        }

        /*public void Teleport(
            int entityId, Entity.Vector pos, Entity.Angles look, bool onGround)
        {
            
            Render(new EntityTeleportPacket(
                entityId,
                pos.X, pos.Y, pos.Z,
                look.Yaw, look.Pitch,
                onGround));
        }*/

        public void ChangeForms(int id, bool sneaking, bool sprinting)
        {
            System.Diagnostics.Debug.Assert(id != Id);

            System.Diagnostics.Debug.Assert(!_disconnected);

            byte flags = 0x00;

            if (sneaking)
            {
                flags |= 0x02;
            }
            if (sprinting)
            {
                flags |= 0x08;
            }

            using EntityMetadata metadata = new();
            metadata.AddByte(0, flags);

            Render(new EntityMetadataPacket(id, metadata.WriteData()));
        }

        public void SpawnPlayer(
            int id, System.Guid uniqueId, 
            Vector p, Entity.Angles look, 
            bool sneaking, bool sprinting)
        {
            System.Diagnostics.Debug.Assert(id != Id);

            System.Diagnostics.Debug.Assert(!_disconnected);

            byte flags = 0x00;

            if (sneaking)
            {
                flags |= 0x02;
            }
            if (sprinting)
            {
                flags |= 0x08;
            }

            using EntityMetadata metadata = new();
            metadata.AddByte(0, flags);

            (byte x, byte y) = look.ConvertToPacketFormat();
            Render(new SpawnNamedEntityPacket(
                id, uniqueId,
                p.X, p.Y, p.Z,
                x, y,
                metadata.WriteData()));
        }

        public void SpawnItemEntity(int id)
        {
            System.Diagnostics.Debug.Assert(id != Id);

            System.Diagnostics.Debug.Assert(!_disconnected);

            throw new System.NotImplementedException();
        }

        public void DestroyEntity(int id)
        {
            System.Diagnostics.Debug.Assert(id != Id);

            System.Diagnostics.Debug.Assert(!_disconnected);

            Render(new DestroyEntitiesPacket(id));
        }

    }

    internal sealed class SelfPlayerRenderer : WorldRenderer
    {
        private readonly Client _CLIENT;

        public SelfPlayerRenderer(
            ConcurrentQueue<ClientboundPlayingPacket> outPackets, Client client) 
            : base(outPackets)
        {
            _CLIENT = client;
        }

        public void Init(int entityId, Vector p, Entity.Angles look)
        {
            

            /*{
                Render(new EntityVelocityPacket(
                    entityId,
                    Conversions.ToShort(v.X * 8000),
                    Conversions.ToShort(v.Y * 8000),
                    Conversions.ToShort(v.Z * 8000)));
            }*/

        }

        /*public void Teleport(Entity.Vector p, Entity.Angles look)
        {
            int payload = new System.Random().Next();
            Render(new TeleportSelfPlayerPacket(
                p.X, p.Y, p.Z,
                look.Yaw, look.Pitch,
                false, false, false, false, false,
                payload));
        }*/

        public void ApplyVelocity(int entityId, Vector v)
        {
            using Buffer buffer = new();

            var packet = new EntityVelocityPacket(
                entityId,
                (short)(v.X * 8000),
                (short)(v.Y * 8000),
                (short)(v.Z * 8000));
            packet.Write(buffer);
            _CLIENT.Send(buffer);
        }
        
    }

}

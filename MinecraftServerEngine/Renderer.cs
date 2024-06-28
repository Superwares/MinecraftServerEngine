using Common;
using Containers;
using MinecraftServerEngine.PhysicsEngine;
using static System.Reflection.Metadata.BlobBuilder;

namespace MinecraftServerEngine
{
    internal abstract class Renderer
    {
        private readonly ConcurrentQueue<ClientboundPlayingPacket> OutPackets;

        public Renderer(ConcurrentQueue<ClientboundPlayingPacket> outPackets)
        {
            OutPackets = outPackets;
        }

        protected void Render(ClientboundPlayingPacket packet)
        {
            OutPackets.Enqueue(packet);
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

    internal sealed class InventoryRenderer : Renderer
    {
        private readonly int WindowId;

        public InventoryRenderer(
            int idWindow, ConcurrentQueue<ClientboundPlayingPacket> outPackets) 
            : base(outPackets) 
        {
            System.Diagnostics.Debug.Assert(idWindow >= 0);
            WindowId = idWindow;
        }

        public void SetSlots(ItemSlot[] slots)
        {
            System.Diagnostics.Debug.Assert(slots != null);

            int n = slots.Length;

            int i = 0;
            var arr = new SlotData[n];

            foreach (ItemSlot slot in slots)
            {
                if (slot == null)
                {
                    arr[i++] = new SlotData();
                    continue;
                }

                arr[i++] = slot.ConventToProtocolFormat();
            }
            System.Diagnostics.Debug.Assert(i == n);

            System.Diagnostics.Debug.Assert(WindowId >= byte.MinValue);
            System.Diagnostics.Debug.Assert(WindowId <= byte.MaxValue);
            Render(new SetWindowItemsPacket((byte)WindowId, arr));
        }

        public void SetCursorSlot(ItemSlot slot)
        {
            if (slot == null)
            {
                Render(new SetSlotPacket(-1, 0, new SlotData()));
            }
            else
            {
                Render(new SetSlotPacket(-1, 0, slot.ConventToProtocolFormat()));
            }
            
        }

        public void EmptyCursorSlot()
        {
            Render(new SetSlotPacket(-1, 0, new SlotData()));
        }

        public void OpenWindow(int countSlot)
        {
            System.Diagnostics.Debug.Assert(WindowId > 0);
            System.Diagnostics.Debug.Assert(countSlot >= 0);

            System.Diagnostics.Debug.Assert(WindowId >= byte.MinValue);
            System.Diagnostics.Debug.Assert(WindowId <= byte.MaxValue);
            System.Diagnostics.Debug.Assert(countSlot >= byte.MinValue);
            System.Diagnostics.Debug.Assert(countSlot <= byte.MaxValue);
            Render(new OpenWindowPacket(
                (byte)WindowId, "minecraft:chest", "EmptyTItle!", (byte)countSlot));
        }
    }

    /*internal sealed class ChunkRenderer : Renderer
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
        public bool Disconnected => _disconnected;


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
            Vector p, Vector pPrev, Look look, bool onGround)
        {
            System.Diagnostics.Debug.Assert(id != Id);

            System.Diagnostics.Debug.Assert(!_disconnected);

            double dx = (p.X - pPrev.X) * (32 * 128),
                dy = (p.Y - pPrev.Y) * (32 * 128),
                dz = (p.Z - pPrev.Z) * (32 * 128);
            System.Diagnostics.Debug.Assert(dx >= short.MinValue && dx <= short.MaxValue);
            System.Diagnostics.Debug.Assert(dy >= short.MinValue && dy <= short.MaxValue);
            System.Diagnostics.Debug.Assert(dz >= short.MinValue && dz <= short.MaxValue);

            (byte x, byte y) = look.ConvertToProtocolFormat();
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

        public void Rotate(int id, Look look, bool onGround)
        {
            System.Diagnostics.Debug.Assert(id != Id);

            System.Diagnostics.Debug.Assert(!_disconnected);

            (byte x, byte y) = look.ConvertToProtocolFormat();
            Render(new EntityLookPacket(id, x, y, onGround));
            Render(new EntityHeadLookPacket(id, x));
        }

        public void Stand(int id)
        {
            System.Diagnostics.Debug.Assert(id != Id);

            System.Diagnostics.Debug.Assert(!_disconnected);

            Render(new EntityPacket(id));
        }

        public void Teleport(int id, Vector p, Look look, bool onGround)
        {
            System.Diagnostics.Debug.Assert(id != Id);

            System.Diagnostics.Debug.Assert(!_disconnected);

            Render(new EntityTeleportPacket(
                id,
                p.X, p.Y, p.Z,
                look.Yaw, look.Pitch,
                onGround));
        }

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
            Vector p, Look look, 
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

            (byte x, byte y) = look.ConvertToProtocolFormat();
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

}

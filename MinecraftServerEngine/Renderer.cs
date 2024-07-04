using Common;
using Containers;
using MinecraftServerEngine.PhysicsEngine;

namespace MinecraftServerEngine
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
        
    }

    internal sealed class PlayerListRenderer : Renderer
    {
        public PlayerListRenderer(
            ConcurrentQueue<ClientboundPlayingPacket> outPackets)
            : base(outPackets) { }

        public void AddPlayerWithLaytency(
            UserId id, string username, long ticks)
        {
            System.Diagnostics.Debug.Assert(ticks <= int.MaxValue);
            System.Diagnostics.Debug.Assert(ticks >= int.MinValue);
            
            int ms = (int)ticks;

            Render(new PlayerListItemAddPacket(id.Data, username, ms));
        }

        public void RemovePlayer(UserId id)
        {
            Render(new PlayerListItemRemovePacket(id.Data));
        }

        public void UpdatePlayerLatency(UserId id, long ticks)
        {
            System.Diagnostics.Debug.Assert(ticks <= int.MaxValue);
            System.Diagnostics.Debug.Assert(ticks >= int.MinValue);

            int ms = (int)ticks;

            Render(new PlayerListItemUpdateLatencyPacket(id.Data, (int)ms));
        }
    }

    internal sealed class PublicInventoryRenderer : Renderer
    {
        private const byte WindowId = 1;

        internal PublicInventoryRenderer(ConcurrentQueue<ClientboundPlayingPacket> outPackets)
            : base(outPackets)
        {
            System.Diagnostics.Debug.Assert(outPackets != null);
        }

        public void Open(string title, Slot[] slots)
        {
            System.Diagnostics.Debug.Assert(title != null);
            System.Diagnostics.Debug.Assert(slots != null);

            System.Diagnostics.Debug.Assert(slots.Length >= byte.MinValue);
            System.Diagnostics.Debug.Assert(slots.Length <= byte.MaxValue);
            Render(new OpenWindowPacket(WindowId, "minecraft:chest", title, (byte)slots.Length));

            using Buffer buffer = new();

            foreach (Slot slot in slots)
            {
                System.Diagnostics.Debug.Assert(slot != null);
                slot.WriteData(buffer);
            }

            System.Diagnostics.Debug.Assert(slots.Length > 0);
            Render(new WindowItemsPacket(WindowId, slots.Length, buffer.ReadData()));
        }

        internal void Update(int count, byte[] data)
        {
            System.Diagnostics.Debug.Assert(count > 0);
            System.Diagnostics.Debug.Assert(data != null);

            Render(new WindowItemsPacket(WindowId, count, data));
        }
    }

    internal sealed class WindowRenderer : Renderer
    {
        private int _id;

        // TODO: Remove offset parameter. offset value is only for to render private inventory when _id == -1;
        public void Update(
            int id, PlayerInventory invPrivate, Slot cursor, int offset)
        {
            System.Diagnostics.Debug.Assert(id >= 0);
            System.Diagnostics.Debug.Assert(invPrivate != null);
            System.Diagnostics.Debug.Assert(cursor != null);
            System.Diagnostics.Debug.Assert(offset >= 0);

            using Buffer buffer = new();

            if (id == 0)
            {
                System.Diagnostics.Debug.Assert(offset == 0);

                foreach (Slot slot in invPrivate.Slots)
                {
                    System.Diagnostics.Debug.Assert(slot != null);
                    slot.WriteData(buffer);
                }

                System.Diagnostics.Debug.Assert(id >= byte.MinValue);
                System.Diagnostics.Debug.Assert(id <= byte.MaxValue);
                System.Diagnostics.Debug.Assert(invPrivate.TotalSlotCount > 0);
                Render(new WindowItemsPacket(
                    (byte)id, invPrivate.TotalSlotCount, buffer.ReadData()));
            }
            else
            {
                System.Diagnostics.Debug.Assert(offset > 0);

                System.Diagnostics.Debug.Assert(id == 1);

                for (int i = 0; i < invPrivate.PrimarySlotCount; ++i)
                {
                    Slot slot = invPrivate.GetPrimarySlot(i);
                    System.Diagnostics.Debug.Assert(slot != null);

                    System.Diagnostics.Debug.Assert(id >= sbyte.MinValue);
                    System.Diagnostics.Debug.Assert(id <= sbyte.MaxValue);
                    int j = i + offset;
                    System.Diagnostics.Debug.Assert(j >= short.MinValue);
                    System.Diagnostics.Debug.Assert(j <= short.MaxValue);
                    Render(new SetSlotPacket(
                        (sbyte)id, j, buffer.ReadData()));
                }

            }

            cursor.WriteData(buffer);

            Render(new SetSlotPacket(-1, 0, buffer.ReadData()));
        }

        public WindowRenderer(ConcurrentQueue<ClientboundPlayingPacket> outPackets, 
            PlayerInventory invPrivate, Slot cursor)
            : base(outPackets)
        {
            System.Diagnostics.Debug.Assert(outPackets != null);
            System.Diagnostics.Debug.Assert(invPrivate != null);
            System.Diagnostics.Debug.Assert(cursor != null);

            _id = 0;

            Update(_id, invPrivate, cursor, 0);
        }

        internal void Open(PlayerInventory invPrivate, Slot cursor, int offset)
        {
            System.Diagnostics.Debug.Assert(invPrivate != null);
            System.Diagnostics.Debug.Assert(cursor != null);
            System.Diagnostics.Debug.Assert(offset >= 0);

            _id = 1;

            Update(_id, invPrivate, cursor, offset);
        }

        internal void Reset(PlayerInventory invPrivate, Slot cursor)
        {
            System.Diagnostics.Debug.Assert(cursor.Empty);

            using Buffer buffer = new();

            _id = 0;

            Update(invPrivate, cursor, 0);
        }

        public void Update(PlayerInventory invPrivate, Slot cursor, int offset)
        {
            System.Diagnostics.Debug.Assert(invPrivate != null);
            System.Diagnostics.Debug.Assert(cursor != null);
            System.Diagnostics.Debug.Assert(offset >= 0);

            Update(_id, invPrivate, cursor, offset);
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

        public void RelMoveAndRotate(
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
            Render(new EntityRelMoveLookPacket(
                id,
                (short)dx, (short)dy, (short)dz,
                x, y,
                onGround));
            Render(new EntityHeadLookPacket(id, x));
        
        }

        public void RelMove(int id, Vector p, Vector pPrev, bool onGround)
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

        public void SetMainHand(int id, byte[] data)
        {
            System.Diagnostics.Debug.Assert(data != null);

            System.Diagnostics.Debug.Assert(id != Id);

            System.Diagnostics.Debug.Assert(!_disconnected);

            Render(new EntityEquipmentPacket(id, 0, data));
        }

        public void SetOffHand(int id, byte[] data)
        {
            System.Diagnostics.Debug.Assert(data != null);

            System.Diagnostics.Debug.Assert(id != Id);

            System.Diagnostics.Debug.Assert(!_disconnected);

            Render(new EntityEquipmentPacket(id, 1, data));
        }

        public void SpawnPlayer(
            int id, System.Guid uniqueId, 
            Vector p, Look look, 
            bool sneaking, bool sprinting,
            byte[] mainHand, byte[] offHand)
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

            SetMainHand(id, mainHand);
            SetMainHand(id, offHand);
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

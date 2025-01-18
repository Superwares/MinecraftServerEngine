using Common;
using Containers;

namespace MinecraftServerEngine
{
    using PhysicsEngine;
    using System.Security.Cryptography;

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
        internal PlayerListRenderer(
            ConcurrentQueue<ClientboundPlayingPacket> outPackets)
            : base(outPackets) { }

        internal void AddPlayerWithLaytency(UserId id, string username, long ticks)
        {
            System.Diagnostics.Debug.Assert(id != UserId.Null);
            System.Diagnostics.Debug.Assert(username != "");
            System.Diagnostics.Debug.Assert(ticks <= int.MaxValue);
            System.Diagnostics.Debug.Assert(ticks >= int.MinValue);

            long ms = ticks * 50;
            System.Diagnostics.Debug.Assert(ms >= int.MinValue);
            System.Diagnostics.Debug.Assert(ms <= int.MaxValue);
            Render(new PlayerListItemAddPacket(id.Data, username, (int)ms));
        }

        internal void RemovePlayer(UserId id)
        {
            System.Diagnostics.Debug.Assert(id != UserId.Null);

            Render(new PlayerListItemRemovePacket(id.Data));
        }

        internal void UpdatePlayerLatency(UserId id, long ticks)
        {
            System.Diagnostics.Debug.Assert(id != UserId.Null);
            System.Diagnostics.Debug.Assert(ticks <= int.MaxValue);
            System.Diagnostics.Debug.Assert(ticks >= int.MinValue);

            long ms = ticks * 50;
            System.Diagnostics.Debug.Assert(ms >= int.MinValue);
            System.Diagnostics.Debug.Assert(ms <= int.MaxValue);
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

        public void Open(string title, InventorySlot[] slots)
        {
            System.Diagnostics.Debug.Assert(title != null);
            System.Diagnostics.Debug.Assert(slots != null);

            System.Diagnostics.Debug.Assert(slots.Length >= byte.MinValue);
            System.Diagnostics.Debug.Assert(slots.Length <= byte.MaxValue);
            Render(new OpenWindowPacket(WindowId, "minecraft:chest", title, (byte)slots.Length));

            using Buffer buffer = new();

            foreach (InventorySlot slot in slots)
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

        // TODO: Remove offset parameter.
        // offset value is only for to render private inventory when _id == -1;
        public void Update(
            int id, PlayerInventory invPlayer, InventorySlot cursor, int offset)
        {
            System.Diagnostics.Debug.Assert(id >= 0);
            System.Diagnostics.Debug.Assert(invPlayer != null);
            System.Diagnostics.Debug.Assert(cursor != null);
            System.Diagnostics.Debug.Assert(offset >= 0);

            using Buffer buffer = new();

            if (id == 0)
            {
                System.Diagnostics.Debug.Assert(offset == 0);

                foreach (InventorySlot slot in invPlayer.Slots)
                {
                    System.Diagnostics.Debug.Assert(slot != null);
                    slot.WriteData(buffer);
                }

                System.Diagnostics.Debug.Assert(id >= byte.MinValue);
                System.Diagnostics.Debug.Assert(id <= byte.MaxValue);
                System.Diagnostics.Debug.Assert(invPlayer.TotalSlotCount > 0);
                Render(new WindowItemsPacket(
                    (byte)id, invPlayer.TotalSlotCount, buffer.ReadData()));
            }
            else
            {
                System.Diagnostics.Debug.Assert(offset > 0);

                System.Diagnostics.Debug.Assert(id == 1);

                for (int i = 0; i < PlayerInventory.PrimarySlotCount; ++i)
                {
                    InventorySlot slot = invPlayer.GetPrimarySlot(i);
                    System.Diagnostics.Debug.Assert(slot != null);

                    System.Diagnostics.Debug.Assert(id >= sbyte.MinValue);
                    System.Diagnostics.Debug.Assert(id <= sbyte.MaxValue);
                    int j = i + offset;
                    System.Diagnostics.Debug.Assert(j >= short.MinValue);
                    System.Diagnostics.Debug.Assert(j <= short.MaxValue);
                    Render(new SetSlotPacket(
                        (sbyte)id, (short)j, buffer.ReadData()));
                }

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

            _id = 0;

            Update(_id, invPrivate, cursor, 0);
        }

        internal void Open(PlayerInventory invPrivate, InventorySlot cursor, int offset)
        {
            System.Diagnostics.Debug.Assert(invPrivate != null);
            System.Diagnostics.Debug.Assert(cursor != null);
            System.Diagnostics.Debug.Assert(offset >= 0);

            _id = 1;

            Update(_id, invPrivate, cursor, offset);
        }

        internal void Reset(PlayerInventory invPrivate, InventorySlot cursor)
        {
            System.Diagnostics.Debug.Assert(cursor.Empty);

            using Buffer buffer = new();

            _id = 0;

            Update(invPrivate, cursor, 0);
        }

        public void Update(PlayerInventory invPrivate, InventorySlot cursor, int offset)
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

        internal WorldRenderer(
            ConcurrentQueue<ClientboundPlayingPacket> outPackets)
            : base(outPackets) { }

        // title, worldboarder, chattings
    }

    internal abstract class ObjectRenderer : Renderer
    {

        private bool _disconnected = false;
        public bool Disconnected => _disconnected;


        private ChunkLocation _loc;
        private int _d = -1;


        internal ObjectRenderer(
            ConcurrentQueue<ClientboundPlayingPacket> outPackets,
            ChunkLocation loc, int d)
            : base(outPackets)
        {
            System.Diagnostics.Debug.Assert(d > 0);

            _loc = loc;

            _d = d;
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

            _d = d;
        }

        public bool CanRender(Vector p)
        {
            if (_disconnected)
            {
                return false;
            }

            System.Diagnostics.Debug.Assert(_d > 0);

            ChunkGrid grid = ChunkGrid.Generate(_loc, _d);
            ChunkLocation loc = ChunkLocation.Generate(p);
            return grid.Contains(loc);
        }

        // particles
    }

    internal sealed class EntityRenderer : ObjectRenderer
    {

        internal EntityRenderer(
            ConcurrentQueue<ClientboundPlayingPacket> outPackets,
            ChunkLocation loc, int d) 
            : base(outPackets, loc, d) 
        {
            
        }

        internal void RelMoveAndRotate(
            int id, 
            Vector p, Vector pPrev, Angles look)
        {
            System.Diagnostics.Debug.Assert(!Disconnected);

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
                false));
            Render(new EntityHeadLookPacket(id, x));
        
        }

        internal void RelMove(int id, Vector p, Vector pPrev)
        {
            System.Diagnostics.Debug.Assert(!Disconnected);

            double dx = (p.X - pPrev.X) * (32 * 128), 
                dy = (p.Y - pPrev.Y) * (32 * 128), 
                dz = (p.Z - pPrev.Z) * (32 * 128);
            System.Diagnostics.Debug.Assert((dx >= short.MinValue) && (dx <= short.MaxValue));
            System.Diagnostics.Debug.Assert((dy >= short.MinValue) && (dy <= short.MaxValue));
            System.Diagnostics.Debug.Assert((dz >= short.MinValue) && (dz <= short.MaxValue));

            Render(new EntityRelMovePacket(
                id,
                (short)dx, (short)dy, (short)dz,
                false));

        }

        internal void Rotate(int id, Angles look)
        {
            System.Diagnostics.Debug.Assert(!Disconnected);

            (byte x, byte y) = look.ConvertToProtocolFormat();
            Render(new EntityLookPacket(id, x, y, false));
            Render(new EntityHeadLookPacket(id, x));
        }

        internal void Stand(int id)
        {
            System.Diagnostics.Debug.Assert(!Disconnected);

            Render(new EntityPacket(id));
        }

        internal void Teleport(int id, Vector p, Angles look, bool onGround)
        {
            System.Diagnostics.Debug.Assert(!Disconnected);

            (byte x, byte y) = look.ConvertToProtocolFormat();
            Render(new EntityTeleportPacket(
                id,
                p.X, p.Y, p.Z,
                x, y,
                onGround));
        }

        internal void SetBlockAppearance(Block block, BlockLocation loc)
        {
            System.Diagnostics.Debug.Assert(Disconnected == false);

            int blockId = block.GetId();
            //int blockId = Block.Podzol.GetId();
            Render(new BlockChangePacket(loc.X, loc.Y, loc.Z, blockId));
        }

        internal void ChangeForms(int id, bool sneaking, bool sprinting)
        {
            System.Diagnostics.Debug.Assert(!Disconnected);

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

        internal void SetEquipmentsData(
            int id, 
            (byte[] mainHand, byte[] offHand) equipmentsData)
        {
            System.Diagnostics.Debug.Assert(equipmentsData.mainHand != null);
            System.Diagnostics.Debug.Assert(equipmentsData.offHand != null);

            System.Diagnostics.Debug.Assert(!Disconnected);

            Render(new EntityEquipmentPacket(id, 0, equipmentsData.mainHand));
            Render(new EntityEquipmentPacket(id, 1, equipmentsData.offHand));
        }
        
        internal void SetEntityStatus(int id, byte v)
        {
            System.Diagnostics.Debug.Assert(!Disconnected);

            Render(new EntityStatusPacket(id, v));
        }

        internal void SpawnPlayer(
            int id, System.Guid uniqueId, 
            Vector p, Angles look, 
            bool sneaking, bool sprinting,
            (byte[], byte[]) equipmentsData)
        {
            System.Diagnostics.Debug.Assert(uniqueId != System.Guid.Empty);

            System.Diagnostics.Debug.Assert(!Disconnected);

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

            SetEquipmentsData(id, equipmentsData);
        }

        internal void SpawnItemEntity(
            int id, System.Guid uniqueId,
            Vector p, Angles look,
            ItemStack stack)
        {
            System.Diagnostics.Debug.Assert(uniqueId != System.Guid.Empty);

            System.Diagnostics.Debug.Assert(!Disconnected);

            (byte x, byte y) = look.ConvertToProtocolFormat();
            Render(new SpawnEntityPacket(
                id, uniqueId, 2,
                p.X, p.Y, p.Z,
                x, y,
                1, 0, 0, 0));

            using EntityMetadata metadata = new();
            metadata.AddBool(5, true);
            metadata.AddItemStack(6, stack);
            Render(new EntityMetadataPacket(
                id, metadata.WriteData()));
        }

        internal void DestroyEntity(int id)
        {
            System.Diagnostics.Debug.Assert(!Disconnected);

            Render(new DestroyEntitiesPacket(id));
        }

    }

    internal sealed class ParticleObjectRenderer : ObjectRenderer
    {
        public ParticleObjectRenderer(
            ConcurrentQueue<ClientboundPlayingPacket> outPackets,
            ChunkLocation loc, int d)
            : base(outPackets, loc, d)
        {
        }

        internal void Move(Vector[] points, float r, float g, float b)
        {
            System.Diagnostics.Debug.Assert(r > 0.0D);
            System.Diagnostics.Debug.Assert(r <= 1.0D);
            System.Diagnostics.Debug.Assert(g > 0.0D);
            System.Diagnostics.Debug.Assert(g <= 1.0D);
            System.Diagnostics.Debug.Assert(b > 0.0D);
            System.Diagnostics.Debug.Assert(b <= 1.0D);

            foreach (Vector p in points)
            {
                System.Diagnostics.Debug.Assert(p.X >= float.MinValue);
                System.Diagnostics.Debug.Assert(p.X <= float.MaxValue);
                System.Diagnostics.Debug.Assert(p.Y >= float.MinValue);
                System.Diagnostics.Debug.Assert(p.Y <= float.MaxValue);
                System.Diagnostics.Debug.Assert(p.Z >= float.MinValue);
                System.Diagnostics.Debug.Assert(p.Z <= float.MaxValue);
                Render(new ParticlesPacket(
                    30, true,
                    (float)p.X, (float)p.Y, (float)p.Z,
                    r, g, b,
                    1.0F, 0));
            }
        }

    }

}

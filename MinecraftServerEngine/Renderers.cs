using Common;
using Containers;

using MinecraftPrimitives;

namespace MinecraftServerEngine
{
    using PhysicsEngine;
    using System.Security.Principal;

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

        internal void AddPlayerWithLaytency(
            UserId userId, string username,
            UserProperty[] properties,
            long ticks)
        {
            System.Diagnostics.Debug.Assert(userId != UserId.Null);
            System.Diagnostics.Debug.Assert(username != "");
            System.Diagnostics.Debug.Assert(ticks <= int.MaxValue);
            System.Diagnostics.Debug.Assert(ticks >= int.MinValue);

            if (properties == null)
            {
                properties = [];
            }

            (string Name, string Value, string Signature)[] _properties =
                new (string Name, string Value, string Signature)[properties.Length];

            for (int i = 0; i < properties.Length; ++i)
            {
                UserProperty property = properties[i];
                _properties[i] = (Name: property.Name, Value: property.Value, Signature: property.Signature);
            }

            long ms = ticks * 50;
            System.Diagnostics.Debug.Assert(ms >= int.MinValue);
            System.Diagnostics.Debug.Assert(ms <= int.MaxValue);
            Render(new PlayerListItemAddPacket(userId.Value, username, _properties, (int)ms));
        }

        internal void RemovePlayer(UserId id)
        {
            System.Diagnostics.Debug.Assert(id != UserId.Null);

            Render(new PlayerListItemRemovePacket(id.Value));
        }

        internal void UpdatePlayerLatency(UserId id, long ticks)
        {
            System.Diagnostics.Debug.Assert(id != UserId.Null);
            System.Diagnostics.Debug.Assert(ticks <= int.MaxValue);
            System.Diagnostics.Debug.Assert(ticks >= int.MinValue);

            long ms = ticks * 50;
            System.Diagnostics.Debug.Assert(ms >= int.MinValue);
            System.Diagnostics.Debug.Assert(ms <= int.MaxValue);
            Render(new PlayerListItemUpdateLatencyPacket(id.Value, (int)ms));
        }
    }

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

    internal sealed class WindowRenderer : Renderer
    {

        // TODO: Remove offset parameter.
        // offset value is only for to render private inventory when _id == -1;
        internal void Update(
            SharedInventory sharedInventory,
            PlayerInventory playerInventory, InventorySlot cursor)
        {
            System.Diagnostics.Debug.Assert(playerInventory != null);
            System.Diagnostics.Debug.Assert(cursor != null);

            using MinecraftProtocolDataStream buffer = new();

            if (sharedInventory == null)
            {
                int windowId = 0;

                foreach (InventorySlot slot in playerInventory.Slots)
                {
                    System.Diagnostics.Debug.Assert(slot != null);
                    slot.WriteData(buffer);
                }

                int totalSlots = playerInventory.GetTotalSlotCount();
                System.Diagnostics.Debug.Assert(totalSlots > 0);

                System.Diagnostics.Debug.Assert(windowId >= byte.MinValue);
                System.Diagnostics.Debug.Assert(windowId <= byte.MaxValue);
                System.Diagnostics.Debug.Assert(totalSlots > 0);
                Render(new WindowItemsPacket(
                    (byte)windowId, totalSlots, buffer.ReadData()));
            }
            else
            {
                int windowId = 1;

                System.Diagnostics.Debug.Assert(sharedInventory.Slots != null);
                foreach (InventorySlot slot in sharedInventory.Slots)
                {
                    System.Diagnostics.Debug.Assert(slot != null);
                    slot.WriteData(buffer);
                }

                foreach (InventorySlot slot in playerInventory.GetPrimarySlots())
                {
                    System.Diagnostics.Debug.Assert(slot != null);
                    slot.WriteData(buffer);
                }

                int totalSlots = sharedInventory.GetTotalSlotCount() + PlayerInventory.PrimarySlotCount;
                byte[] data = buffer.ReadData();

                System.Diagnostics.Debug.Assert(windowId >= byte.MinValue);
                System.Diagnostics.Debug.Assert(windowId <= byte.MaxValue);
                System.Diagnostics.Debug.Assert(totalSlots > 0);
                Render(new WindowItemsPacket(
                    (byte)windowId, totalSlots, data));

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

            Update(null, invPrivate, cursor);
        }

        internal void Open(
            SharedInventory sharedInventory,
            PlayerInventory playerInventory, InventorySlot cursor)
        {
            System.Diagnostics.Debug.Assert(sharedInventory != null);
            System.Diagnostics.Debug.Assert(playerInventory != null);
            System.Diagnostics.Debug.Assert(cursor != null);

            int windowId = 1;
            string title = sharedInventory.Title;
            int totalSharedSlots = sharedInventory.GetTotalSlotCount();

            System.Diagnostics.Debug.Assert(totalSharedSlots > 0);

            System.Diagnostics.Debug.Assert(windowId >= byte.MinValue);
            System.Diagnostics.Debug.Assert(windowId <= byte.MaxValue);
            Render(new OpenWindowPacket((byte)windowId, "minecraft:chest", title, (byte)totalSharedSlots));

            Update(sharedInventory, playerInventory, cursor);
        }

        internal void Reset(PlayerInventory playerInventory, InventorySlot cursor)
        {
            System.Diagnostics.Debug.Assert(cursor.Empty);

            Update(null, playerInventory, cursor);
        }

        internal bool HandleMainHandSlot(PlayerInventory playerInventory)
        {
            System.Diagnostics.Debug.Assert(playerInventory != null);

            using MinecraftProtocolDataStream buffer = new();

            InventorySlot mainSlot = playerInventory.HandleMainHandSlot2();

            mainSlot.WriteData(buffer);

            int slot = PlayerInventory.HotbarSlotsOffset + playerInventory.IndexMainHandSlot;

            int windowId = 0;
            System.Diagnostics.Debug.Assert(windowId >= sbyte.MinValue);
            System.Diagnostics.Debug.Assert(windowId <= sbyte.MaxValue);
            System.Diagnostics.Debug.Assert(slot >= short.MinValue);
            System.Diagnostics.Debug.Assert(slot <= short.MaxValue);
            Render(new SetSlotPacket((sbyte)windowId, (short)slot, buffer.ReadData()));

            return mainSlot.Empty;
        }

    }

    /*internal sealed class ChunkRenderer : Renderer
    {

    }*/

    internal sealed class WorldRenderer : Renderer
    {

        internal WorldRenderer(
            ConcurrentQueue<ClientboundPlayingPacket> outPackets)
            : base(outPackets) { }

        // title, worldboarder, chattings, sound, particles

        internal void PlaySound(string name, int category, Vector p, double volume, double pitch)
        {
            System.Diagnostics.Debug.Assert(name != null);
            System.Diagnostics.Debug.Assert(string.IsNullOrEmpty(name) == false);
            System.Diagnostics.Debug.Assert(volume >= 0.0F);
            System.Diagnostics.Debug.Assert(volume <= 1.0F);
            System.Diagnostics.Debug.Assert(pitch >= 0.5F);
            System.Diagnostics.Debug.Assert(pitch <= 2.0F);

            Render(new NamedSoundEffectPacket(
                //"entity.player.attack.strong", 7,
                name, category,
                (int)(p.X * 8), (int)(p.Y * 8), (int)(p.Z * 8),
                (float)volume, (float)pitch));
        }

        internal void DisplayTitle(int fadeIn, int stay, int fadeOut, string data)
        {
            System.Diagnostics.Debug.Assert(fadeIn >= 0);
            System.Diagnostics.Debug.Assert(stay >= 0);
            System.Diagnostics.Debug.Assert(fadeOut >= 0);

            System.Diagnostics.Debug.Assert(data != null);
            System.Diagnostics.Debug.Assert(string.IsNullOrEmpty(data) == false);

            Render(new SetTimesAndDisplayTitlePacket(fadeIn, stay, fadeOut));
            Render(new SetTitlePacket(data));
        }

        internal void OpenBossBar(System.Guid id, string title, double health,
            BossBarColor color, BossBarDivision division)
        {
            System.Diagnostics.Debug.Assert(id != System.Guid.Empty);
            System.Diagnostics.Debug.Assert(title != null);
            System.Diagnostics.Debug.Assert(health >= 0.0);
            System.Diagnostics.Debug.Assert(health <= 1.0);

            Render(new OpenBossBarPacket(id, title, (float)health, (int)color, (int)division, 0x01));
        }

        internal void UpdateBossBarHealth(System.Guid id, double health)
        {
            System.Diagnostics.Debug.Assert(id != System.Guid.Empty);
            System.Diagnostics.Debug.Assert(health >= 0.0);
            System.Diagnostics.Debug.Assert(health <= 1.0);

            Render(new UpdateBossBarHealthPacket(id, (float)health));
        }

        internal void CloseBossBar(System.Guid id)
        {
            System.Diagnostics.Debug.Assert(id != System.Guid.Empty);

            Render(new CloseBossBarPacket(id));
        }

        internal void WriteChatInChatBox(string data)
        {
            System.Diagnostics.Debug.Assert(data != null);
            System.Diagnostics.Debug.Assert(string.IsNullOrEmpty(data) == false);

            Render(new ClientboundChatmessagePacket(data, 0x00));
        }
    }

    internal abstract class ObjectRenderer : Renderer
    {
        private bool _blindness;

        // This is different from the actual disconnection with the Connection.
        // It means the actual disconnection of the Renderer.
        private bool _disconnected = false;
        public bool Disconnected => _disconnected;


        private ChunkLocation _loc;
        private int _d = -1;


        internal ObjectRenderer(
            ConcurrentQueue<ClientboundPlayingPacket> outPackets,
            ChunkLocation loc, int d, bool blindness)
            : base(outPackets)
        {
            System.Diagnostics.Debug.Assert(d > 0);

            _loc = loc;

            _d = d;

            _blindness = blindness;
        }


        internal void ApplyBlindness(bool f)
        {
            System.Diagnostics.Debug.Assert(_disconnected == false);

            _blindness = f;
        }

        public void Disconnect()
        {
            System.Diagnostics.Debug.Assert(_disconnected == false);

            _disconnected = true;
        }

        public void Update(ChunkLocation loc)
        {
            System.Diagnostics.Debug.Assert(_disconnected == false);

            _loc = loc;
        }

        public void Update(int d)
        {
            System.Diagnostics.Debug.Assert(_disconnected == false);
            System.Diagnostics.Debug.Assert(d > 0);

            _d = d;
        }

        public bool CanRender(Vector p)
        {
            if (_disconnected == true)
            {
                return false;
            }

            if (_blindness == true)
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
            ChunkLocation loc, int d, bool blindness)
            : base(outPackets, loc, d, blindness)
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

        internal void ShowParticles(
            Particle particle,
            Vector v,
            double speed, int count,
            double r, double g, double b)
        {
            System.Diagnostics.Debug.Assert(r >= 0.0D);
            System.Diagnostics.Debug.Assert(r <= 1.0D);
            System.Diagnostics.Debug.Assert(g >= 0.0D);
            System.Diagnostics.Debug.Assert(g <= 1.0D);
            System.Diagnostics.Debug.Assert(b >= 0.0D);
            System.Diagnostics.Debug.Assert(b <= 1.0D);
            System.Diagnostics.Debug.Assert(speed >= 0.0F);
            System.Diagnostics.Debug.Assert(speed <= 1.0F);
            System.Diagnostics.Debug.Assert(count >= 0);

            if (count == 0)
            {
                return;
            }

            if (particle != Particle.Reddust)
            {
                r = 0.0F;
                g = 0.0F;
                b = 0.0F;
            }

            System.Diagnostics.Debug.Assert(v.X >= double.MinValue);
            System.Diagnostics.Debug.Assert(v.X <= double.MaxValue);
            System.Diagnostics.Debug.Assert(v.Y >= double.MinValue);
            System.Diagnostics.Debug.Assert(v.Y <= double.MaxValue);
            System.Diagnostics.Debug.Assert(v.Z >= double.MinValue);
            System.Diagnostics.Debug.Assert(v.Z <= double.MaxValue);
            Render(new ParticlesPacket(
                (int)particle, true,
                (float)v.X, (float)v.Y, (float)v.Z,
                (float)r, (float)g, (float)b,
                (float)speed, count));
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

            metadata.AddByte(17, 0b01111111);

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

        internal void AddEffect(
            int entityId, byte effectId,
            byte amplifier, int duration, byte flags)
        {
            System.Diagnostics.Debug.Assert(Disconnected == false);

            Render(new EntityEffectPacket(entityId, effectId, amplifier, duration, flags));
        }

        internal void Animate(int id, EntityAnimation animation)
        {
            System.Diagnostics.Debug.Assert(Disconnected == false);

            Render(new ClientboundAnimationPacket(id, (byte)animation));
        }

        internal void DestroyEntity(int id)
        {
            System.Diagnostics.Debug.Assert(Disconnected == false);

            Render(new DestroyEntitiesPacket(id));
        }

    }

    internal sealed class ParticleObjectRenderer : ObjectRenderer
    {
        public ParticleObjectRenderer(
            ConcurrentQueue<ClientboundPlayingPacket> outPackets,
            ChunkLocation loc, int d)
            : base(outPackets, loc, d, false)
        {
        }

        internal void Move(
            Particle particle,
            Vector[] points,
            double speed, int count,
            double r, double g, double b)
        {
            System.Diagnostics.Debug.Assert(r >= 0.0D);
            System.Diagnostics.Debug.Assert(r <= 1.0D);
            System.Diagnostics.Debug.Assert(g >= 0.0D);
            System.Diagnostics.Debug.Assert(g <= 1.0D);
            System.Diagnostics.Debug.Assert(b >= 0.0D);
            System.Diagnostics.Debug.Assert(b <= 1.0D);
            System.Diagnostics.Debug.Assert(speed >= 0.0F);
            System.Diagnostics.Debug.Assert(speed <= 1.0F);
            System.Diagnostics.Debug.Assert(count >= 0);

            if (count == 0)
            {
                return;
            }

            if (particle != Particle.Reddust)
            {
                r = 0.0F;
                g = 0.0F;
                b = 0.0F;
            }

            foreach (Vector p in points)
            {
                System.Diagnostics.Debug.Assert(p.X >= double.MinValue);
                System.Diagnostics.Debug.Assert(p.X <= double.MaxValue);
                System.Diagnostics.Debug.Assert(p.Y >= double.MinValue);
                System.Diagnostics.Debug.Assert(p.Y <= double.MaxValue);
                System.Diagnostics.Debug.Assert(p.Z >= double.MinValue);
                System.Diagnostics.Debug.Assert(p.Z <= double.MaxValue);
                Render(new ParticlesPacket(
                    (int)particle, true,
                    (float)p.X, (float)p.Y, (float)p.Z,
                    (float)r, (float)g, (float)b,
                   (float)speed, count));
            }
        }

    }

}

﻿using Common;
using Containers;

using MinecraftServerEngine.Entities;
using MinecraftServerEngine.Protocols;
using MinecraftServerEngine.Blocks;
using MinecraftServerEngine.Items;
using MinecraftServerEngine.Physics;
using MinecraftServerEngine.Particles;

namespace MinecraftServerEngine.Renderers
{
    internal sealed class EntityRenderer : PhysicsObjectRenderer
    {


        internal EntityRenderer(
            ConcurrentQueue<ClientboundPlayingPacket> outPackets,
            ChunkLocation loc, int d, bool blindness)
            : base(outPackets, loc, d, blindness)
        {

        }

        internal void Teleport(int id, Vector p, EntityAngles look, bool onGround)
        {
            System.Diagnostics.Debug.Assert(IsDisconnected == false);

            (byte x, byte y) = look.ConvertToProtocolFormat();
            Render(new EntityTeleportPacket(
                id,
                p.X, p.Y, p.Z,
                x, y,
                onGround));
        }

        internal void RelativeMoveAndRotate(
            int id,
            Vector prevPos,  Vector pos, EntityAngles look)
        {
            System.Diagnostics.Debug.Assert(IsDisconnected == false);

            double dx = (pos.X - prevPos.X) * (32 * 128),
                dy = (pos.Y - prevPos.Y) * (32 * 128),
                dz = (pos.Z - prevPos.Z) * (32 * 128);
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

        internal void RelativeMove(int id, Vector prevPos, Vector pos)
        {
            System.Diagnostics.Debug.Assert(IsDisconnected == false);

            double dx = (pos.X - prevPos.X) * (32 * 128);
            double dy = (pos.Y - prevPos.Y) * (32 * 128);
            double dz = (pos.Z - prevPos.Z) * (32 * 128);
            
            System.Diagnostics.Debug.Assert(dx >= short.MinValue && dx <= short.MaxValue);
            System.Diagnostics.Debug.Assert(dy >= short.MinValue && dy <= short.MaxValue);
            System.Diagnostics.Debug.Assert(dz >= short.MinValue && dz <= short.MaxValue);

            Render(new EntityRelMovePacket(
                id,
                (short)dx, (short)dy, (short)dz,
                false));

        }

        internal void RelativeMoveAndRotate(
            int id,
            double dx, double dy, double dz, EntityAngles look)
        {
            System.Diagnostics.Debug.Assert(dx >= short.MinValue && dx <= short.MaxValue);
            System.Diagnostics.Debug.Assert(dy >= short.MinValue && dy <= short.MaxValue);
            System.Diagnostics.Debug.Assert(dz >= short.MinValue && dz <= short.MaxValue);
            
            System.Diagnostics.Debug.Assert(IsDisconnected == false);

            (byte x, byte y) = look.ConvertToProtocolFormat();
            Render(new EntityRelMoveLookPacket(
                id,
                (short)dx, (short)dy, (short)dz,
                x, y,
                false));
            Render(new EntityHeadLookPacket(id, x));

        }

        internal void RelativeMove(int id, double dx, double dy, double dz)
        {
            System.Diagnostics.Debug.Assert(dx >= short.MinValue && dx <= short.MaxValue);
            System.Diagnostics.Debug.Assert(dy >= short.MinValue && dy <= short.MaxValue);
            System.Diagnostics.Debug.Assert(dz >= short.MinValue && dz <= short.MaxValue);

            System.Diagnostics.Debug.Assert(IsDisconnected == false);

            Render(new EntityRelMovePacket(
                id,
                (short)dx, (short)dy, (short)dz,
                false));

        }

        internal void Rotate(int id, EntityAngles look)
        {
            System.Diagnostics.Debug.Assert(IsDisconnected == false);

            (byte x, byte y) = look.ConvertToProtocolFormat();
            Render(new EntityLookPacket(id, x, y, false));
            Render(new EntityHeadLookPacket(id, x));
        }

        internal void Stand(int id)
        {
            System.Diagnostics.Debug.Assert(IsDisconnected == false);

            Render(new EntityPacket(id));
        }

      
        internal void SetBlockAppearance(Block block, BlockLocation loc)
        {
            System.Diagnostics.Debug.Assert(IsDisconnected == false);

            int blockId = block.GetId();
            //int blockId = Block.Podzol.GetId();
            Render(new BlockChangePacket(loc.X, loc.Y, loc.Z, blockId));
        }

        internal void ChangeForms(int id, bool sneaking, bool sprinting)
        {
            System.Diagnostics.Debug.Assert(IsDisconnected == false);

            byte flags = 0x00;

            if (sneaking)
            {
                flags |= 0x02;
            }
            if (sprinting)
            {
                flags |= 0x08;
            }

            using MinecraftProtocolDataStream stream = new();

            {
                using EntityMetadata metadata = new();

                metadata.AddByte(0, flags);
                metadata.WriteData(stream);
            }

            byte[] data = stream.ReadData();

            Render(new EntityMetadataPacket(id, data));
        }

        internal void ShowParticles(
            Particle particle,
            Vector v,
            double extra, int count,
            double offsetX, double offsetY, double offsetZ)
        {
            System.Diagnostics.Debug.Assert(IsDisconnected == false);

            System.Diagnostics.Debug.Assert(offsetX >= 0.0D);
            System.Diagnostics.Debug.Assert(offsetX <= 1.0D);
            System.Diagnostics.Debug.Assert(offsetY >= 0.0D);
            System.Diagnostics.Debug.Assert(offsetY <= 1.0D);
            System.Diagnostics.Debug.Assert(offsetZ >= 0.0D);
            System.Diagnostics.Debug.Assert(offsetZ <= 1.0D);
            System.Diagnostics.Debug.Assert(extra >= 0.0F);
            System.Diagnostics.Debug.Assert(extra <= 1.0F);
            System.Diagnostics.Debug.Assert(count >= 0);

            //if (count == 0)
            //{
            //    return;
            //}

            //if (particle != Particle.Reddust)
            //{
            //    r = 0.0F;
            //    g = 0.0F;
            //    b = 0.0F;
            //}

            System.Diagnostics.Debug.Assert(v.X >= double.MinValue);
            System.Diagnostics.Debug.Assert(v.X <= double.MaxValue);
            System.Diagnostics.Debug.Assert(v.Y >= double.MinValue);
            System.Diagnostics.Debug.Assert(v.Y <= double.MaxValue);
            System.Diagnostics.Debug.Assert(v.Z >= double.MinValue);
            System.Diagnostics.Debug.Assert(v.Z <= double.MaxValue);
            Render(new ParticlesPacket(
                (int)particle, true,
                (float)v.X, (float)v.Y, (float)v.Z,
                (float)offsetX, (float)offsetY, (float)offsetZ,
                (float)extra, count));
        }

        internal void SetEquipmentsData(
            int id,
            byte[] mainHand, byte[] offHand,
            byte[] helmet
            )
        {
            System.Diagnostics.Debug.Assert(helmet != null);
            System.Diagnostics.Debug.Assert(mainHand != null);
            System.Diagnostics.Debug.Assert(offHand != null);

            System.Diagnostics.Debug.Assert(IsDisconnected == false);

            Render(new EntityEquipmentPacket(id, 0, mainHand));
            Render(new EntityEquipmentPacket(id, 1, offHand));
            Render(new EntityEquipmentPacket(id, 5, helmet));
        }

        internal void CollectItemEntity(
            int itemEntityId,
            int collectorEntityId,
            int pickupCount)
        {
            System.Diagnostics.Debug.Assert(pickupCount >= Item.MinCount);

            System.Diagnostics.Debug.Assert(IsDisconnected == false);

            Render(new ItemCollectingPacket(itemEntityId, collectorEntityId, pickupCount));
        }

        internal void SetEntityStatus(int id, byte v)
        {
            System.Diagnostics.Debug.Assert(IsDisconnected == false);

            Render(new EntityStatusPacket(id, v));
        }

        internal void SpawnPlayer(
            int id, System.Guid uniqueId,
            Vector p, EntityAngles look,
            bool sneaking, bool sprinting
            )
        {
            System.Diagnostics.Debug.Assert(uniqueId != System.Guid.Empty);

            System.Diagnostics.Debug.Assert(IsDisconnected == false);

            byte flags = 0x00;

            if (sneaking)
            {
                flags |= 0x02;
            }
            if (sprinting)
            {
                flags |= 0x08;
            }

            using MinecraftProtocolDataStream stream = new();

            {
                using EntityMetadata metadata = new();

                metadata.AddByte(0, flags);
                metadata.AddByte(13, 0xFF);  // The Displayed Skin Parts bit mask

                metadata.WriteData(stream);
            }

            byte[] data = stream.ReadData();

            (byte x, byte y) = look.ConvertToProtocolFormat();
            Render(new SpawnNamedEntityPacket(
                id, uniqueId,
                p.X, p.Y, p.Z,
                x, y,
                data));
        }

        internal void SpawnItemEntity(
            int id, System.Guid uniqueId,
            Vector p, EntityAngles look,
            ItemStack stack)
        {
            System.Diagnostics.Debug.Assert(uniqueId != System.Guid.Empty);

            System.Diagnostics.Debug.Assert(IsDisconnected == false);

            using MinecraftProtocolDataStream stream = new();

            (byte x, byte y) = look.ConvertToProtocolFormat();
            Render(new SpawnEntityPacket(
                id, uniqueId, 2,
                p.X, p.Y, p.Z,
                x, y,
                1, 0, 0, 0));

            {
                using EntityMetadata metadata = new();
                metadata.AddBool(5, true);
                metadata.AddItemStack(6, stack);
                metadata.WriteData(stream);
            }

            byte[] data = stream.ReadData();

            Render(new EntityMetadataPacket(id, data));
        }

        internal void AddEffect(
            int entityId, byte effectId,
            byte amplifier, int duration, byte flags)
        {
            System.Diagnostics.Debug.Assert(IsDisconnected == false);

            Render(new EntityEffectPacket(entityId, effectId, amplifier, duration, flags));
        }

        internal void Animate(int id, EntityAnimation animation)
        {
            System.Diagnostics.Debug.Assert(IsDisconnected == false);

            Render(new ClientboundAnimationPacket(id, (byte)animation));
        }

        internal void DestroyEntity(int id)
        {
            System.Diagnostics.Debug.Assert(IsDisconnected == false);

            Render(new DestroyEntitiesPacket(id));
        }

    }

}

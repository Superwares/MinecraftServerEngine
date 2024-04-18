using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Protocol.Entity;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Protocol
{
    public abstract class Report
    {
        internal abstract void Write(Buffer buffer);

    }

    internal class LoadChunkReport : Report
    {
        public readonly Chunk.Vector p;
        private readonly int _Mask;
        private readonly byte[] _Data;

        public LoadChunkReport(Chunk c)
        {
            (int mask, byte[] data) = Chunk.Write(c);

            p = c.p;
            _Mask = mask;
            _Data = data;
        }

        internal override void Write(Buffer buffer)
        {
            LoadChunkPacket packet = new(p.x, p.z, true, _Mask, _Data);
            packet.Write(buffer);
        }

    }

    internal class LoadEmptyChunkReport : Report
    {
        public readonly Chunk.Vector p;

        public LoadEmptyChunkReport(Chunk.Vector p)
        {
            this.p = p;
        }

        internal override void Write(Buffer buffer)
        {
            (int mask, byte[] data) = Chunk.Write();
            LoadChunkPacket packet = new(p.x, p.z, true, mask, data);
            packet.Write(buffer);
        }

    }

    internal class UnloadChunkReport : Report
    {
        public readonly Chunk.Vector p;

        public UnloadChunkReport(Chunk.Vector p)
        {
            this.p = p;
        }

        internal override void Write(Buffer buffer)
        {
            UnloadChunkPacket packet = new(p.x, p.z);
            packet.Write(buffer);
        }
    }

    internal class UnloadChunksReport : Report
    {
        public readonly Chunk.Vector[] P;

        public UnloadChunksReport(Chunk.Vector[] P)
        {
            this.P = P;
        }

        internal override void Write(Buffer buffer)
        {
            foreach (Chunk.Vector p in P)
            {
                UnloadChunkPacket packet = new(p.x, p.z);
                packet.Write(buffer);
            }
        }
    }

    internal class KeepaliveReport : Report
    {
        public readonly long Payload;

        public KeepaliveReport()
        {
            Payload = new Random().NextInt64();
        }

        internal override void Write(Buffer buffer)
        {
            KeepaliveRequestPacket packet = new(Payload);
            packet.Write(buffer);
        }

    }

    public class PlayerAbilitiesReport : Report
    {
        public readonly bool Invulnerable, Flying, AllowFlying, CreativeMode;
        public readonly float FlyingSpeed, FovModifier;

        public PlayerAbilitiesReport(
            bool invulnerable, bool flying, bool allowFlying, bool creativeMode,
            float flyingSpeed, float fovModifier)
        {
            Invulnerable = invulnerable;
            Flying = flying;
            AllowFlying = allowFlying;
            CreativeMode = creativeMode;
            FlyingSpeed = flyingSpeed; FovModifier = fovModifier;
        }

        internal override void Write(Buffer buffer)
        {
            SetPlayerAbilitiesPacket packet = new(
                Invulnerable, Flying, AllowFlying, CreativeMode,
                FlyingSpeed, FovModifier);
            packet.Write(buffer);
        }

    }

    public abstract class TeleportReport : Report
    {
        public readonly Entity.Vector Pos;
        public readonly Entity.Look Look;
        public readonly int Payload;

        public TeleportReport(
            Entity.Vector pos, Entity.Look look)
        {
            Debug.Assert(
                look.yaw >= Entity.Look.MinYaw &&
                look.yaw <= Entity.Look.MaxYaw &&
                look.pitch >= Entity.Look.MinPitch &&
                look.pitch <= Entity.Look.MaxPitch);

            Pos = pos; Look = look;
            Payload = new Random().Next();  // TODO: Make own random generator in common library.
        }


    }

    public class AbsoluteTeleportReport : TeleportReport
    {
        public AbsoluteTeleportReport(Entity.Vector pos, Entity.Look look) 
            : base(pos, look) { }

        internal override void Write(Buffer buffer)
        {
            TeleportPacket packet = new(
                Pos.x, Pos.y, Pos.z,
                Look.yaw, Look.pitch,
                false, false, false, false, false, 
                Payload);
            packet.Write(buffer);
        }

    }

    public class RelativeTeleportReport : TeleportReport
    {
        public RelativeTeleportReport(Entity.Vector pos, Entity.Look look) 
            : base(pos, look) { }

        internal override void Write(Buffer buffer)
        {
            TeleportPacket packet = new(
                Pos.x, Pos.y, Pos.z,
                Look.yaw, Look.pitch,
                true, true, true, true, true, 
                Payload);
            packet.Write(buffer);
        }

    }

}

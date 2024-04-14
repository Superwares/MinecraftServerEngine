using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Protocol
{
    public abstract class Report
    {
        internal abstract void Write(Buffer buffer);

    }

    public class LoadChunkReport : Report
    {
        public readonly Chunk.Position p;
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
            LoadChunkPacket packet = new(p.X, p.Z, true, _Mask, _Data);
            packet.Write(buffer);
        }

    }

    public class LoadEmptyChunkReport : Report
    {
        public readonly Chunk.Position p;

        public LoadEmptyChunkReport(Chunk.Position p)
        {
            this.p = p;
        }

        internal override void Write(Buffer buffer)
        {
            (int mask, byte[] data) = Chunk.Write();
            LoadChunkPacket packet = new(p.X, p.Z, true, mask, data);
            packet.Write(buffer);
        }

    }

    public class UnloadChunkReport : Report
    {
        public readonly Chunk.Position p;

        public UnloadChunkReport(Chunk.Position p)
        {
            this.p = p;
        }

        internal override void Write(Buffer buffer)
        {
            UnloadChunkPacket packet = new(p.X, p.Z);
            packet.Write(buffer);
        }
    }

    public class UnloadChunksReport : Report
    {
        public readonly Chunk.Position[] P;

        public UnloadChunksReport(Chunk.Position[] P)
        {
            this.P = P;
        }

        internal override void Write(Buffer buffer)
        {
            foreach (Chunk.Position p in P)
            {
                UnloadChunkPacket packet = new(p.X, p.Z);
                packet.Write(buffer);
            }
        }
    }

    public class SetPlayerAbilitiesReport : Report
    {
        public readonly bool Invulnerable, Flying, AllowFlying, CreativeMode;
        public readonly float FlyingSpeed, FovModifier;

        public SetPlayerAbilitiesReport(
            bool invulnerable, bool flying, bool allowFlying, bool creativeMode,
            float flyingSpeed, float fovModifier)
        {
            Invulnerable = invulnerable; Flying = flying; AllowFlying = allowFlying; CreativeMode = creativeMode;
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

    public class AbsoluteTeleportReport : Report
    {
        public readonly double X, Y, Z;
        public readonly float Yaw, Pitch;
        public readonly int Payload;

        public AbsoluteTeleportReport(
            double x, double y, double z, 
            float yaw, float pitch,
            int payload)
        {
            X = x; Y = y; Z = z;
            Yaw = yaw; Pitch = pitch;
            Payload = payload;
        }

        internal override void Write(Buffer buffer)
        {
            TeleportPacket packet = new(
                X, Y, Z, 
                Yaw, Pitch, 
                false, false, false, false, false, 
                Payload);
            packet.Write(buffer);
        }

    }

    public class RelativeTeleportReport : Report
    {
        public readonly double X, Y, Z;
        public readonly float Yaw, Pitch;
        public readonly int Payload;

        public RelativeTeleportReport(
            double x, double y, double z,
            float yaw, float pitch,
            int payload)
        {
            X = x; Y = y; Z = z;
            Yaw = yaw; Pitch = pitch;
            Payload = payload;
        }

        internal override void Write(Buffer buffer)
        {
            TeleportPacket packet = new(
                X, Y, Z, 
                Yaw, Pitch, 
                true, true, true, true, true, 
                Payload);
            packet.Write(buffer);
        }

    }

}

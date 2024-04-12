using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Protocol
{
    public abstract class Packet
    {
        protected static readonly string _MinecraftVersion = "1.12.2";
        protected static readonly int _ProtocolVersion = 340;

        public enum States
        {
            Handshaking,
            Status,
            Login,
            Playing,
        }

        public enum WhereBound
        {
            Serverbound,
            Clientbound,
        }

        public abstract States State { get; }
        public abstract WhereBound BoundTo { get; }

        public readonly int Id;

        public Packet(int id) { Id = id; }

        internal abstract void WriteData(Buffer buffer);

        internal void Write(Buffer buffer)
        {
            buffer.WriteInt(Id, true);
            WriteData(buffer);
        }

    }

    internal abstract class HandshakingPacket(int id) : Packet(id)
    {
        public override States State => States.Handshaking;

    }

    internal abstract class StatusPacket(int id) : Packet(id)
    {
        public override States State => States.Status;

    }

    internal abstract class LoginPacket(int id) : Packet(id)
    {
        public override States State => States.Login;

    }

    public abstract class PlayingPacket(int id) : Packet(id)
    {
        public override States State => States.Playing;
    }

    internal abstract class ServerboundHandshakingPacket(int id) : HandshakingPacket(id)
    {
        public static readonly int SetProtocolPacketId = 0x00;

        public override WhereBound BoundTo { get { return WhereBound.Serverbound; } }

    }

    internal abstract class ClientboundStatusPacket(int id) : StatusPacket(id)
    {
        public static readonly int ResponsePacketId = 0x00;
        public static readonly int PongPacketId = 0x01;

        public override WhereBound BoundTo { get { return WhereBound.Clientbound; } }

    }

    internal abstract class ServerboundStatusPacket(int id) : StatusPacket(id)
    {
        public static readonly int RequestPacketId = 0x00;
        public static readonly int PingPacketId = 0x01;

        public override WhereBound BoundTo { get { return WhereBound.Serverbound; } }

    }

    internal abstract class ClientboundLoginPacket(int id) : LoginPacket(id)
    {
        public static readonly int DisconnectPacketId = 0x00;
        public static readonly int EncryptionRequestPacketId = 0x01;
        public static readonly int LoginSuccessPacketId = 0x02;
        public static readonly int SetCompressionPacketId = 0x03;

        public override WhereBound BoundTo => WhereBound.Clientbound;

    }

    internal abstract class ServerboundLoginPacket(int id) : LoginPacket(id)
    {
        public static readonly int StartLoginPacketId = 0x00;
        public static readonly int EncryptionResponsePacketId = 0x01;

        public override WhereBound BoundTo => WhereBound.Clientbound;

    }

    public abstract class ClientboundPlayingPacket(int id) : PlayingPacket(id)
    {
        public static readonly int LoadChunkPacketId = 0x20;
        public static readonly int UnloadChunkPacketId = 0x1D;
        public static readonly int JoinGamePacketId = 0x23;
        public static readonly int SetPlayerAbilitiesId = 0x2C;

        public override WhereBound BoundTo => WhereBound.Clientbound;

    }

    public abstract class ServerboundPlayingPacket(int id) : PlayingPacket(id)
    {
        public static readonly int ClientSettingsPacketId = 0x04;

        public override WhereBound BoundTo => WhereBound.Serverbound;

    }

    internal class SetProtocolPacket : ServerboundHandshakingPacket
    {
        public readonly int Version;
        public readonly string Hostname;
        public readonly ushort Port;
        public readonly States NextState;

        private static States ReadNextState(Buffer buffer)
        {
            int a = buffer.ReadInt(true);
            States nextState = (a == 1 ? States.Status : States.Login);
            if (!(a == 1 || a == 2))
                throw new InvalidEncodingException();

            return nextState;
        }

        internal static SetProtocolPacket Read(Buffer buffer)
        {
            return new SetProtocolPacket(
                buffer.ReadInt(true),
                buffer.ReadString(), buffer.ReadUshort(),
                ReadNextState(buffer));
        }

        private SetProtocolPacket(
            int version,
            string hostname, ushort port,
            States nextState) : base(SetProtocolPacketId)
        {
            Debug.Assert(version == _ProtocolVersion);
            Debug.Assert(port > 0);
            Debug.Assert(
                nextState == States.Status ||
                nextState == States.Login);

            Version = version;
            Hostname = hostname;
            Port = port;
            NextState = nextState;
        }

        public SetProtocolPacket(string hostname, ushort port, States nextState)
            : this(_ProtocolVersion, hostname, port, nextState) { }

        internal override void WriteData(Buffer buffer)
        {
            Debug.Assert(Id == SetProtocolPacketId);
            Debug.Assert(
                NextState == States.Status ||
                NextState == States.Login);

            int a = NextState == States.Status ? 1 : 2;
            buffer.WriteInt(Version, true);
            buffer.WriteString(Hostname);
            buffer.WriteUshort(Port);
            buffer.WriteInt(a, true);
        }

    }

    internal class ResponsePacket : ClientboundStatusPacket
    {

        public readonly int MaxPlayers;
        public readonly int OnlinePlayers;
        public readonly string Description;

        internal static ResponsePacket Read(Buffer buffer)
        {
            /*string jsonString = buffer.ReadString();*/
            // TODO
            throw new NotImplementedException();
        }

        public ResponsePacket(int maxPlayers, int onlinePlayers, string description)
            : base(ResponsePacketId)
        {
            Debug.Assert(maxPlayers >= onlinePlayers);

            MaxPlayers = maxPlayers;
            OnlinePlayers = onlinePlayers;
            Description = description;
        }

        internal override void WriteData(Buffer buffer)
        {
            // TODO
            string jsonString = "{\"version\":{\"name\":\"1.12.2\",\"protocol\":340},\"players\":{\"max\":100,\"online\":0,\"sample\":[]},\"description\":{\"text\":\"Hello, World!\"},\"favicon\":\"data:image/png;base64,<data>\",\"enforcesSecureChat\":true,\"previewsChat\":true}";

            buffer.WriteString(jsonString);
        }

    }

    internal class PongPacket : ClientboundStatusPacket
    {
        public readonly long Payload;

        internal static PongPacket Read(Buffer buffer)
        {
            return new(buffer.ReadLong());
        }

        public PongPacket(long payload) : base(PongPacketId)
        {
            Payload = payload;
        }

        internal override void WriteData(Buffer buffer)
        {
            buffer.WriteLong(Payload);
        }

    }

    internal class RequestPacket : ServerboundStatusPacket
    {
        internal static RequestPacket Read(Buffer buffer)
        {
            return new();
        }

        public RequestPacket() : base(RequestPacketId) { }

        internal override void WriteData(Buffer buffer) { }

    }

    internal class PingPacket : ServerboundStatusPacket
    {
        public readonly long Payload;

        internal static PingPacket Read(Buffer buffer)
        {
            return new(buffer.ReadLong());
        }

        private PingPacket(long payload) : base(PingPacketId)
        {
            Payload = payload;
        }

        public PingPacket() : this(DateTime.Now.Ticks) { }

        internal override void WriteData(Buffer buffer)
        {
            buffer.WriteLong(Payload);
        }

    }

    internal class DisconnectPacket : ClientboundLoginPacket
    {
        public readonly string Reason;

        internal static DisconnectPacket Read(Buffer buffer)
        {
            return new(buffer.ReadString());
        }

        public DisconnectPacket(string reason) : base(DisconnectPacketId)
        {
            Reason = reason;
        }

        internal override void WriteData(Buffer buffer)
        {
            buffer.WriteString(Reason);
        }

    }

    internal class EncryptionRequestPacket : ClientboundLoginPacket
    {
        internal static EncryptionRequestPacket Read(Buffer buffer)
        {
            throw new NotImplementedException();
        }

        private EncryptionRequestPacket() : base(EncryptionRequestPacketId)
        {
            throw new NotImplementedException();
        }

        internal override void WriteData(Buffer buffer)
        {
            throw new NotImplementedException();
        }

    }

    internal class LoginSuccessPacket : ClientboundLoginPacket
    {
        public readonly Guid UserId;
        public readonly string Username;

        internal static LoginSuccessPacket Read(Buffer buffer)
        {
            return new(
                Guid.Parse(buffer.ReadString()),
                buffer.ReadString());
        }

        public LoginSuccessPacket(Guid userId, string username)
            : base(LoginSuccessPacketId)
        {
            UserId = userId;
            Username = username;
        }

        internal override void WriteData(Buffer buffer)
        {
            buffer.WriteString(UserId.ToString());
            buffer.WriteString(Username);
        }

    }

    internal class SetCompressionPacket : ClientboundLoginPacket
    {
        public readonly int Threshold;

        internal static SetCompressionPacket Read(Buffer buffer)
        {
            return new(buffer.ReadInt(true));
        }

        public SetCompressionPacket(int threshold)
            : base(SetCompressionPacketId)
        {
            Threshold = threshold;
        }

        internal override void WriteData(Buffer buffer)
        {
            buffer.WriteInt(Threshold, true);
        }

    }

    internal class StartLoginPacket : ServerboundLoginPacket
    {
        public readonly string Username;

        internal static StartLoginPacket Read(Buffer buffer)
        {
            return new(buffer.ReadString());
        }

        public StartLoginPacket(string username) : base(StartLoginPacketId)
        {
            Username = username;
        }

        internal override void WriteData(Buffer buffer)
        {
            buffer.WriteString(Username);
        }
    }

    internal class EncryptionResponsePacket : ServerboundLoginPacket
    {
        internal static EncryptionResponsePacket Read(Buffer buffer)
        {
            throw new NotImplementedException();
        }

        public EncryptionResponsePacket() : base(EncryptionResponsePacketId)
        {
            throw new NotImplementedException();
        }

        internal override void WriteData(Buffer buffer)
        {
            throw new NotImplementedException();
        }
    }

    public class LoadChunk : ClientboundPlayingPacket
    {
        public readonly int XChunk, ZChunk;
        public readonly bool Continuous;
        public readonly int Mask;
        public readonly byte[] Data;

        internal static LoadChunk Read(Buffer buffer)
        {
            throw new NotImplementedException();
        }

        public LoadChunk(
            int xChunk, int zChunk,
            bool continuous, int mask, byte[] data)
            : base(LoadChunkPacketId)
        {
            XChunk = xChunk;
            ZChunk = zChunk;
            Continuous = continuous;
            Mask = mask;
            Data = data;  // TODO: move semantics
        }

        internal override void WriteData(Buffer buffer)
        {
            buffer.WriteInt(XChunk);
            buffer.WriteInt(ZChunk);
            buffer.WriteBool(Continuous);
            buffer.WriteInt(Mask, true);
            buffer.WriteInt(Data.Length, true);
            buffer.WriteData(Data);
            buffer.WriteInt(0, true);  // TODO: Block entities
        }

    }

    public class UnloadChunk : ClientboundPlayingPacket
    {
        public readonly int XChunk, ZChunk;

        internal static UnloadChunk Read(Buffer buffer)
        {
            throw new NotImplementedException();
        }

        public UnloadChunk(int xChunk, int zChunk) : base(UnloadChunkPacketId)
        {
            XChunk = xChunk;
            ZChunk = zChunk;
        }

        internal override void WriteData(Buffer buffer)
        {
            buffer.WriteInt(XChunk);
            buffer.WriteInt(ZChunk);
        }

    }

    public class JoinGamePacket : ClientboundPlayingPacket
    {
        private readonly int _entityId;
        private readonly byte _gamemode;
        private readonly int _dimension;
        private readonly byte _difficulty;
        private readonly string _levelType;
        private readonly bool _reducedDebugInfo;

        internal static JoinGamePacket Read(Buffer buffer)
        {
            throw new NotImplementedException();
        }

        public JoinGamePacket(
            int entityId,
            byte gamemode,
            int dimension,
            byte difficulty,
            string levelType,
            bool reducedDebugInfo) : base(JoinGamePacketId)
        {
            _entityId = entityId;
            _gamemode = gamemode;
            _dimension = dimension;
            _difficulty = difficulty;
            _levelType = levelType;
            _reducedDebugInfo = reducedDebugInfo;

        }

        internal override void WriteData(Buffer buffer)
        {
            buffer.WriteInt(_entityId);
            buffer.WriteByte(_gamemode);
            buffer.WriteInt(_dimension);
            buffer.WriteByte(_difficulty);
            buffer.WriteByte(0);
            buffer.WriteString(_levelType);
            buffer.WriteBool(_reducedDebugInfo);
        }
    }

    public class SetAbilitiesPacket : ClientboundPlayingPacket
    {
        private readonly byte _flags;
        private readonly float _flyingSpeed;
        private readonly float _fovModifier;  // fov * _fovModifier;

        internal static SetAbilitiesPacket Read(Buffer buffer)
        {
            throw new NotImplementedException();
        }

        public SetAbilitiesPacket(
            bool invulnerable, bool flying, bool allowFlying, bool creativeMode,
            float flyingSpeed, float fovModifier) : base(SetPlayerAbilitiesId)
        {
            byte flags = 0;
            if (invulnerable)
                flags |= 0x01;
            if (flying)
                flags |= 0x02;
            if (allowFlying)
                flags |= 0x04;
            if (creativeMode)
                flags |= 0x08;

            _flags = flags;
            _flyingSpeed = flyingSpeed;
            _fovModifier = fovModifier;
        }

        internal override void WriteData(Buffer buffer)
        {
            buffer.WriteByte(_flags);
            buffer.WriteFloat(_flyingSpeed);
            buffer.WriteFloat(_fovModifier);
        }

    }

    public class ClientSettingsPacket : ServerboundPlayingPacket
    {
        public readonly byte RenderDistance;

        internal static ClientSettingsPacket Read(Buffer buffer)
        {
            buffer.ReadString();
            byte renderDistance = buffer.ReadByte();
            buffer.ReadInt(true);
            buffer.ReadBool();
            buffer.ReadSbyte();
            buffer.ReadInt(true);

            return new(renderDistance);
        }

        public ClientSettingsPacket(byte renderDistance)
            : base(ClientSettingsPacketId)
        {
            RenderDistance = renderDistance;
        }

        internal override void WriteData(Buffer buffer)
        {
            throw new NotImplementedException();
        }
    }
}

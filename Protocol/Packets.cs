using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Protocol
{
    internal abstract class Packet
    {
        protected const string _MinecraftVersion = "1.12.2";
        protected const int _ProtocolVersion = 340;

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

        protected abstract void WriteData(Buffer buffer);

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

    internal abstract class PlayingPacket(int id) : Packet(id)
    {
        public override States State => States.Playing;
    }

    internal abstract class ServerboundHandshakingPacket(int id) : HandshakingPacket(id)
    {
        public const int SetProtocolPacketId = 0x00;

        public override WhereBound BoundTo { get { return WhereBound.Serverbound; } }

    }

    internal abstract class ClientboundStatusPacket(int id) : StatusPacket(id)
    {
        public const int ResponsePacketId = 0x00;
        public const int PongPacketId = 0x01;

        public override WhereBound BoundTo { get { return WhereBound.Clientbound; } }

    }

    internal abstract class ServerboundStatusPacket(int id) : StatusPacket(id)
    {
        public const int RequestPacketId = 0x00;
        public const int PingPacketId = 0x01;

        public override WhereBound BoundTo { get { return WhereBound.Serverbound; } }

    }

    internal abstract class ClientboundLoginPacket(int id) : LoginPacket(id)
    {
        public const int DisconnectPacketId = 0x00;
        public const int EncryptionRequestPacketId = 0x01;
        public const int LoginSuccessPacketId = 0x02;
        public const int SetCompressionPacketId = 0x03;

        public override WhereBound BoundTo => WhereBound.Clientbound;

    }

    internal abstract class ServerboundLoginPacket(int id) : LoginPacket(id)
    {
        public const int StartLoginPacketId = 0x00;
        public const int EncryptionResponsePacketId = 0x01;

        public override WhereBound BoundTo => WhereBound.Clientbound;

    }

    internal abstract class ClientboundPlayingPacket(int id) : PlayingPacket(id)
    {
        public const int LoadChunkPacketId = 0x20;
        public const int UnloadChunkPacketId = 0x1D;
        public const int KeepaliveRequestPacketId = 0x1F;
        public const int JoinGamePacketId = 0x23;
        public const int SetPlayerAbilitiesId = 0x2C;
        public const int TeleportPacketId = 0x2F;

        public override WhereBound BoundTo => WhereBound.Clientbound;

    }

    internal abstract class ServerboundPlayingPacket(int id) : PlayingPacket(id)
    {
        public const int ConfirmTeleportPacketId = 0x00;
        public const int ClientSettingsPacketId = 0x04;
        public const int KeepaliveResponsePacketId = 0x0B;
        public const int PlayerPacketId = 0x0C;
        public const int PlayerPositionPacketId = 0x0D;
        public const int PlayerPosAndLookPacketId = 0x0E;
        public const int PlayerLookPacketId = 0x0F;

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

        /// <summary>
        /// TODO: Add description.
        /// </summary>
        /// <exception cref="UnexpectedDataException">TODO: Why it's thrown.</exception>
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

        protected override void WriteData(Buffer buffer)
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

        /// <summary>
        /// TODO: Add description.
        /// </summary>
        /// <exception cref="UnexpectedDataException">TODO: Why it's thrown.</exception>
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

        protected override void WriteData(Buffer buffer)
        {
            // TODO
            string jsonString = "{\"version\":{\"name\":\"1.12.2\",\"protocol\":340},\"players\":{\"max\":100,\"online\":0,\"sample\":[]},\"description\":{\"text\":\"Hello, World!\"},\"favicon\":\"data:image/png;base64,<data>\",\"enforcesSecureChat\":true,\"previewsChat\":true}";

            buffer.WriteString(jsonString);
        }

    }

    internal class PongPacket : ClientboundStatusPacket
    {
        public readonly long Payload;

        /// <summary>
        /// TODO: Add description.
        /// </summary>
        /// <exception cref="UnexpectedDataException">TODO: Why it's thrown.</exception>
        internal static PongPacket Read(Buffer buffer)
        {
            return new(buffer.ReadLong());
        }

        public PongPacket(long payload) : base(PongPacketId)
        {
            Payload = payload;
        }

        protected override void WriteData(Buffer buffer)
        {
            buffer.WriteLong(Payload);
        }

    }

    internal class RequestPacket : ServerboundStatusPacket
    {
        /// <summary>
        /// TODO: Add description.
        /// </summary>
        /// <exception cref="UnexpectedDataException">TODO: Why it's thrown.</exception>
        internal static RequestPacket Read(Buffer buffer)
        {
            return new();
        }

        public RequestPacket() : base(RequestPacketId) { }

        protected override void WriteData(Buffer buffer) { }

    }

    internal class PingPacket : ServerboundStatusPacket
    {
        public readonly long Payload;

        /// <summary>
        /// TODO: Add description.
        /// </summary>
        /// <exception cref="UnexpectedDataException">TODO: Why it's thrown.</exception>
        internal static PingPacket Read(Buffer buffer)
        {
            return new(buffer.ReadLong());
        }

        private PingPacket(long payload) : base(PingPacketId)
        {
            Payload = payload;
        }

        public PingPacket() : this(DateTime.Now.Ticks) { }

        protected override void WriteData(Buffer buffer)
        {
            buffer.WriteLong(Payload);
        }

    }

    internal class DisconnectPacket : ClientboundLoginPacket
    {
        public readonly string Reason;

        /// <summary>
        /// TODO: Add description.
        /// </summary>
        /// <exception cref="UnexpectedDataException">TODO: Why it's thrown.</exception>
        internal static DisconnectPacket Read(Buffer buffer)
        {
            return new(buffer.ReadString());
        }

        public DisconnectPacket(string reason) : base(DisconnectPacketId)
        {
            Reason = reason;
        }

        protected override void WriteData(Buffer buffer)
        {
            buffer.WriteString(Reason);
        }

    }

    internal class EncryptionRequestPacket : ClientboundLoginPacket
    {
        /// <summary>
        /// TODO: Add description.
        /// </summary>
        /// <exception cref="UnexpectedDataException">TODO: Why it's thrown.</exception>
        internal static EncryptionRequestPacket Read(Buffer buffer)
        {
            throw new NotImplementedException();
        }

        private EncryptionRequestPacket() : base(EncryptionRequestPacketId)
        {
            throw new NotImplementedException();
        }

        protected override void WriteData(Buffer buffer)
        {
            throw new NotImplementedException();
        }

    }

    internal class LoginSuccessPacket : ClientboundLoginPacket
    {
        public readonly Guid UserId;
        public readonly string Username;

        /// <summary>
        /// TODO: Add description.
        /// </summary>
        /// <exception cref="UnexpectedDataException">TODO: Why it's thrown.</exception>
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

        protected override void WriteData(Buffer buffer)
        {
            buffer.WriteString(UserId.ToString());
            buffer.WriteString(Username);
        }

    }

    internal class SetCompressionPacket : ClientboundLoginPacket
    {
        public readonly int Threshold;

        /// <summary>
        /// TODO: Add description.
        /// </summary>
        /// <exception cref="UnexpectedDataException">TODO: Why it's thrown.</exception>
        internal static SetCompressionPacket Read(Buffer buffer)
        {
            return new(buffer.ReadInt(true));
        }

        public SetCompressionPacket(int threshold)
            : base(SetCompressionPacketId)
        {
            Threshold = threshold;
        }

        protected override void WriteData(Buffer buffer)
        {
            buffer.WriteInt(Threshold, true);
        }

    }

    internal class StartLoginPacket : ServerboundLoginPacket
    {
        public readonly string Username;

        /// <summary>
        /// TODO: Add description.
        /// </summary>
        /// <exception cref="UnexpectedDataException">TODO: Why it's thrown.</exception>
        internal static StartLoginPacket Read(Buffer buffer)
        {
            // TODO: Check the conditions of variables. If not correct, throw exception.
            return new(buffer.ReadString());
        }

        public StartLoginPacket(string username) : base(StartLoginPacketId)
        {
            // TODO: Assert variables.
            Username = username;
        }

        protected override void WriteData(Buffer buffer)
        {
            buffer.WriteString(Username);
        }
    }

    internal class EncryptionResponsePacket : ServerboundLoginPacket
    {
        /// <summary>
        /// TODO: Add description.
        /// </summary>
        /// <exception cref="UnexpectedDataException">TODO: Why it's thrown.</exception>
        internal static EncryptionResponsePacket Read(Buffer buffer)
        {
            // TODO: Check the conditions of variables. If not correct, throw exception.
            throw new NotImplementedException();
        }

        public EncryptionResponsePacket() : base(EncryptionResponsePacketId)
        {
            // TODO: Assert variables.
            throw new NotImplementedException();
        }

        protected override void WriteData(Buffer buffer)
        {
            throw new NotImplementedException();
        }
    }

    internal class LoadChunkPacket : ClientboundPlayingPacket
    {
        public readonly int XChunk, ZChunk;
        public readonly bool Continuous;
        public readonly int Mask;
        public readonly byte[] Data;

        /// <summary>
        /// TODO: Add description.
        /// </summary>
        /// <exception cref="UnexpectedDataException">TODO: Why it's thrown.</exception>
        internal static LoadChunkPacket Read(Buffer buffer)
        {
            // TODO: Check the conditions of variables. If not correct, throw exception.
            throw new NotImplementedException();
        }

        public LoadChunkPacket(
            int xChunk, int zChunk,
            bool continuous, int mask, byte[] data)
            : base(LoadChunkPacketId)
        {
            // TODO: Assert variables.

            XChunk = xChunk;
            ZChunk = zChunk;
            Continuous = continuous;
            Mask = mask;
            Data = data;  // TODO: move semantics
        }

        protected override void WriteData(Buffer buffer)
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

    internal class UnloadChunkPacket : ClientboundPlayingPacket
    {
        public readonly int XChunk, ZChunk;

        /// <summary>
        /// TODO: Add description.
        /// </summary>
        /// <exception cref="UnexpectedDataException">TODO: Why it's thrown.</exception>
        internal static UnloadChunkPacket Read(Buffer buffer)
        {
            throw new NotImplementedException();
        }

        public UnloadChunkPacket(int xChunk, int zChunk) : base(UnloadChunkPacketId)
        {
            XChunk = xChunk;
            ZChunk = zChunk;
        }

        protected override void WriteData(Buffer buffer)
        {
            buffer.WriteInt(XChunk);
            buffer.WriteInt(ZChunk);
        }

    }

    internal class KeepaliveRequestPacket : ClientboundPlayingPacket
    {
        public readonly long Payload;

        internal static KeepaliveRequestPacket Read(Buffer buffer)
        {
            throw new NotImplementedException();
        }

        public KeepaliveRequestPacket(long payload) : base(KeepaliveRequestPacketId)
        {
            Payload = payload;
        }

        protected override void WriteData(Buffer buffer)
        {
            buffer.WriteLong(Payload);
        }
    }

    internal class JoinGamePacket : ClientboundPlayingPacket
    {
        private readonly int _entityId;
        private readonly byte _gamemode;
        private readonly int _dimension;
        private readonly byte _difficulty;
        private readonly string _levelType;
        private readonly bool _reducedDebugInfo;

        /// <summary>
        /// TODO: Add description.
        /// </summary>
        /// <exception cref="UnexpectedDataException">TODO: Why it's thrown.</exception>
        internal static JoinGamePacket Read(Buffer buffer)
        {
            // TODO: Check the conditions of variables. If not correct, throw exception.
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
            // TODO: Assert variables.

            _entityId = entityId;
            _gamemode = gamemode;
            _dimension = dimension;
            _difficulty = difficulty;
            _levelType = levelType;
            _reducedDebugInfo = reducedDebugInfo;

        }

        protected override void WriteData(Buffer buffer)
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

    internal class SetPlayerAbilitiesPacket : ClientboundPlayingPacket
    {
        private readonly byte _flags;
        private readonly float _flyingSpeed;
        private readonly float _fovModifier;  // fov * _fovModifier;

        /// <summary>
        /// TODO: Add description.
        /// </summary>
        /// <exception cref="UnexpectedDataException">TODO: Why it's thrown.</exception>
        internal static SetPlayerAbilitiesPacket Read(Buffer buffer)
        {
            // TODO: Check the conditions of variables. If not correct, throw exception.
            throw new NotImplementedException();
        }

        public SetPlayerAbilitiesPacket(
            bool invulnerable, bool flying, bool allowFlying, bool creativeMode,
            float flyingSpeed, float fovModifier) : base(SetPlayerAbilitiesId)
        {
            // TODO: Assert variables.

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

        protected override void WriteData(Buffer buffer)
        {
            buffer.WriteByte(_flags);
            buffer.WriteFloat(_flyingSpeed);
            buffer.WriteFloat(_fovModifier);
        }

    }
    
    internal class TeleportPacket : ClientboundPlayingPacket
    {
        public readonly double X, Y, Z;
        public readonly float Yaw, Pitch;
        private readonly byte _flags;
        public readonly int Payload;

        /// <summary>
        /// TODO: Add description.
        /// </summary>
        /// <exception cref="UnexpectedDataException">TODO: Why it's thrown.</exception>
        internal static TeleportPacket Read(Buffer buffer)
        {
            // TODO: Check the conditions of variables. If not correct, throw exception.
            throw new NotImplementedException();
        }

        public TeleportPacket(
            double x, double y, double z, 
            float yaw, float pitch, 
            bool relativeX, bool relativeY, bool relativeZ,
            bool relativeYaw, bool relativePitch,
            int payload)
            : base(TeleportPacketId)
        {
            

            X = x; Y = y; Z = z;
            Yaw = yaw; Pitch = pitch;

            byte flags = 0;
            if (relativeX)
                flags |= (1 << 0);
            if (relativeY)
                flags |= (1 << 1);
            if (relativeZ)
                flags |= (1 << 2);
            if (relativeYaw)  // TODO: It is correct?
                flags |= (1 << 3);
            if (relativePitch)  // TODO: It is correct?
                flags |= (1 << 4);
            _flags = flags;

            Payload = payload;
        }

        protected override void WriteData(Buffer buffer)
        {
            buffer.WriteDouble(X);
            buffer.WriteDouble(Y);
            buffer.WriteDouble(Z);
            buffer.WriteFloat(Yaw);
            buffer.WriteFloat(Pitch);
            buffer.WriteByte(_flags);
            buffer.WriteInt(Payload, true);

        }

    }

    internal class ConfirmTeleportPacket : ServerboundPlayingPacket
    {
        public readonly int Payload;

        /// <summary>
        /// TODO: Add description.
        /// </summary>
        /// <exception cref="UnexpectedDataException">TODO: Why it's thrown.</exception>
        internal static ConfirmTeleportPacket Read(Buffer buffer)
        {
            return new(buffer.ReadInt(true));
        }

        public ConfirmTeleportPacket(int payload) : base(ConfirmTeleportPacketId)
        {
            Payload = payload;
        }

        protected override void WriteData(Buffer buffer)
        {
            throw new NotImplementedException();
        }

    }

    internal class ClientSettingsPacket : ServerboundPlayingPacket
    {
        public readonly byte RenderDistance;

        /// <summary>
        /// TODO: Add description.
        /// </summary>
        /// <exception cref="UnexpectedDataException">TODO: Why it's thrown.</exception>
        internal static ClientSettingsPacket Read(Buffer buffer)
        {
            buffer.ReadString();  // TODO
            byte renderDistance = buffer.ReadByte(); 
            buffer.ReadInt(true);  // TODO
            buffer.ReadBool();  // TODO
            buffer.ReadSbyte();  // TODO
            buffer.ReadInt(true);  // TODO

            return new(renderDistance);
        }

        private ClientSettingsPacket(byte renderDistance)
            : base(ClientSettingsPacketId)
        {
            RenderDistance = renderDistance;
        }

        protected override void WriteData(Buffer buffer)
        {
            throw new NotImplementedException();
        }
    }

    internal class KeepaliveResponsePacket : ServerboundPlayingPacket
    {
        public readonly long Payload;

        /// <summary>
        /// TODO: Add description.
        /// </summary>
        /// <exception cref="UnexpectedDataException">TODO: Why it's thrown.</exception>
        internal static KeepaliveResponsePacket Read(Buffer buffer)
        {
            return new(buffer.ReadLong());
        }

        private KeepaliveResponsePacket(long payload) : base(KeepaliveResponsePacketId)
        {
            Payload = payload;
        }

        protected override void WriteData(Buffer buffer)
        {
            throw new NotImplementedException();
        }

    }

    internal class PlayerPacket : ServerboundPlayingPacket
    {
        public readonly bool OnGround;

        /// <summary>
        /// TODO: Add description.
        /// </summary>
        /// <exception cref="UnexpectedDataException">TODO: Why it's thrown.</exception>
        internal static PlayerPacket Read(Buffer buffer)
        {
            return new(buffer.ReadBool());
        }

        public PlayerPacket(bool onGround) : base(PlayerPacketId)
        {
            OnGround = onGround;
        }

        protected override void WriteData(Buffer buffer)
        {
            throw new NotImplementedException();
        }

    }

    internal class PlayerPositionPacket : ServerboundPlayingPacket
    {
        public readonly double X, Y, Z;
        public readonly bool OnGround;

        /// <summary>
        /// TODO: Add description.
        /// </summary>
        /// <exception cref="UnexpectedDataException">TODO: Why it's thrown.</exception>
        internal static PlayerPositionPacket Read(Buffer buffer)
        {
            return new(
                buffer.ReadDouble(), buffer.ReadDouble(), buffer.ReadDouble(), 
                buffer.ReadBool());
        }

        public PlayerPositionPacket(
            double x, double y, double z,
            bool onGround)
            : base(PlayerPositionPacketId)
        {
            X = x; Y = y; Z = z;
            OnGround = onGround;
        }

        protected override void WriteData(Buffer buffer)
        {
            throw new NotImplementedException();
        }

    }

    internal class PlayerPosAndLookPacket : ServerboundPlayingPacket
    {
        public readonly double X, Y, Z;
        public readonly float Yaw, Pitch;
        public readonly bool OnGround;

        /// <summary>
        /// TODO: Add description.
        /// </summary>
        /// <exception cref="UnexpectedDataException">TODO: Why it's thrown.</exception>
        internal static PlayerPosAndLookPacket Read(Buffer buffer)
        {
            return new(
                buffer.ReadDouble(), buffer.ReadDouble(), buffer.ReadDouble(),
                buffer.ReadFloat(), buffer.ReadFloat(), 
                buffer.ReadBool());
        }

        private PlayerPosAndLookPacket(
            double x, double y, double z, 
            float yaw, float pitch,
            bool onGround)
            : base(PlayerPosAndLookPacketId)
        {
            X = x; Y = y; Z = z;
            Yaw = yaw; Pitch = pitch;
            OnGround = onGround;
        }

        protected override void WriteData(Buffer buffer)
        {
            throw new NotImplementedException();
        }

    }

    internal class PlayerLookPacket : ServerboundPlayingPacket
    {
        public readonly float Yaw, Pitch;
        public readonly bool OnGround;

        /// <summary>
        /// TODO: Add description.
        /// </summary>
        /// <exception cref="UnexpectedDataException">TODO: Why it's thrown.</exception>
        internal static PlayerLookPacket Read(Buffer buffer)
        {
            return new(
                buffer.ReadFloat(), buffer.ReadFloat(),
                buffer.ReadBool());
        }

        private PlayerLookPacket(
            float yaw, float pitch,
            bool onGround) 
            : base(PlayerLookPacketId)
        {
            Yaw = yaw; Pitch = pitch;
            OnGround = onGround;
        }

        protected override void WriteData(Buffer buffer)
        {
            throw new NotImplementedException();
        }
    }

}

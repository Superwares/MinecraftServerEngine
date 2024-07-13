using Containers;

namespace MinecraftServerEngine
{

    /*internal sealed class SlotData
    {
        public readonly short Id;
        public readonly byte Count;
        
        public static SlotData Read(byte[] data)
        {
            using Buffer buffer = new();
            buffer.WriteData(data);

            short id = buffer.ReadShort();
            if (id == -1)
                return new();

            byte count = buffer.ReadByte();
            short damage = buffer.ReadShort();
            Debug.Assert(damage == 0);
            byte nbt = buffer.ReadByte();
            Debug.Assert(nbt == 0x00);
            return new(id, count);
        }

        public SlotData(short id, byte count)
        {
            Id = id;
            Count = count;
        }

        public SlotData()
        {
            Id = -1;
        }

        public byte[] WriteData()
        {
            using Buffer buffer = new();

            if (Id == -1)
            {
                buffer.WriteShort(-1);
                return buffer.ReadData();
            }

            buffer.WriteShort(Id);
            buffer.WriteByte(Count);
            buffer.WriteShort(0);
            buffer.WriteByte(0x00);  // no NBT

            return buffer.ReadData();
        }

    }*/

    

    internal abstract class Packet
    {
        protected const string MinecraftVersion = "1.12.2";
        protected const int ProtocolVersion = 340;

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

        internal Packet(int id) 
        {
            System.Diagnostics.Debug.Assert(id >= 0);

            Id = id; 
        }

        protected abstract void WriteData(Buffer buffer);

        internal void Write(Buffer buffer)
        {
            System.Diagnostics.Debug.Assert(buffer != null);

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
        public const int SpawnEntityPacketId = 0x00;
        public const int SpawnNamedEntityPacketId = 0x05;
        public const int ClientboundConfirmTransactionPacketId = 0x11;
        public const int ClientboundCloseWindowPacketId = 0x12;
        public const int OpenWindowPacketId = 0x13;
        public const int WindowItemsPacketId = 0x14;
        public const int SetSlotPacketId = 0x16;
        public const int UnloadChunkPacketId = 0x1D;
        public const int ClientboundKeepAlivePacketId = 0x1F;
        public const int LoadChunkPacketId = 0x20;
        public const int ParticlesPacketId = 0x22;
        public const int JoinGamePacketId = 0x23;
        public const int EntityPacketId = 0x25;
        public const int EntityRelMovePacketId = 0x26;
        public const int EntityRelMoveLookPacketId = 0x27;
        public const int EntityLookPacketId = 0x28;
        public const int AbilitiesId = 0x2C;
        public const int PlayerListItemAddPacketId = 0x2E;
        public const int PlayerListItemUpdateLatencyPacketId = 0x2E;
        public const int PlayerListItemRemovePacketId = 0x2E;
        public const int TeleportPacketId = 0x2F;
        public const int DestroyEntitiesPacketId = 0x32;
        public const int EntityHeadLookPacketId = 0x36;
        public const int EntityMetadataPacketId = 0x3C;
        public const int EntityVelocityPacketId = 0x3E;
        public const int EntityEquipmentPacketId = 0x3F;
        public const int EntityTeleportPacketId = 0x4C;

        public override WhereBound BoundTo => WhereBound.Clientbound;

    }

    internal abstract class ServerboundPlayingPacket(int id) : PlayingPacket(id)
    {
        public const int TeleportAcceptPacketd = 0x00;
        public const int SettingsPacketId = 0x04;
        public const int ServerboundConfirmTransactionPacketId = 0x05;
        public const int ClickWindowPacketId = 0x07;
        public const int ServerboundCloseWindowPacketId = 0x08;
        public const int ServerboundKeepAlivePacketId = 0x0B;
        public const int PlayerPacketId = 0x0C;
        public const int PlayerPositionPacketId = 0x0D;
        public const int PlayerPosAndLookPacketId = 0x0E;
        public const int PlayerLookPacketId = 0x0F;
        public const int PlayerDiggingPacketId = 0x14;
        public const int EntityActionPacketId = 0x15;
        public const int ServerboundHeldItemSlotPacketId = 0x1A;
        public const int UseItemPacketId = 0x20;

        public override WhereBound BoundTo => WhereBound.Serverbound;

    }

    internal sealed class SetProtocolPacket : ServerboundHandshakingPacket
    {
        public readonly int Version;
        public readonly string Hostname;
        public readonly ushort Port;
        public readonly States NextState;

        private static States ReadNextState(Buffer buffer)
        {
            System.Diagnostics.Debug.Assert(buffer != null);

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
            System.Diagnostics.Debug.Assert(buffer != null);

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
            System.Diagnostics.Debug.Assert(version == ProtocolVersion);
            System.Diagnostics.Debug.Assert(port > 0);
            System.Diagnostics.Debug.Assert(
                nextState == States.Status ||
                nextState == States.Login);

            Version = version;
            Hostname = hostname;
            Port = port;
            NextState = nextState;
        }

        public SetProtocolPacket(string hostname, ushort port, States nextState)
            : this(ProtocolVersion, hostname, port, nextState) 
        { }

        protected override void WriteData(Buffer buffer)
        {
            System.Diagnostics.Debug.Assert(buffer != null);

            System.Diagnostics.Debug.Assert(Id == SetProtocolPacketId);
            System.Diagnostics.Debug.Assert(
                NextState == States.Status ||
                NextState == States.Login);

            int a = NextState == States.Status ? 1 : 2;
            buffer.WriteInt(Version, true);
            buffer.WriteString(Hostname);
            buffer.WriteUshort(Port);
            buffer.WriteInt(a, true);
        }

    }

    internal sealed class ResponsePacket : ClientboundStatusPacket
    {

        public readonly int MaxPlayers;
        public readonly int OnlinePlayers;
        public readonly string Description;

        /// <exception cref="UnexpectedDataException">TODO: Why it's thrown.</exception>
        internal static ResponsePacket Read(Buffer buffer)
        {
            System.Diagnostics.Debug.Assert(buffer != null);

            /*string jsonString = buffer.ReadString();*/
            // TODO
            throw new System.NotImplementedException();
        }

        public ResponsePacket(int maxPlayers, int onlinePlayers, string description)
            : base(ResponsePacketId)
        {
            System.Diagnostics.Debug.Assert(maxPlayers >= onlinePlayers);

            MaxPlayers = maxPlayers;
            OnlinePlayers = onlinePlayers;
            Description = description;
        }

        protected override void WriteData(Buffer buffer)
        {
            System.Diagnostics.Debug.Assert(buffer != null);

            // TODO: Using json serialization.
            string jsonString = "{\"version\":{\"name\":\"1.12.2\",\"protocol\":340},\"players\":{\"max\":100,\"online\":0,\"sample\":[]},\"description\":{\"text\":\"Hello, World!\"},\"favicon\":\"data:image/png;base64,<data>\",\"enforcesSecureChat\":true,\"previewsChat\":true}";

            buffer.WriteString(jsonString);
        }

    }

    internal sealed class PongPacket : ClientboundStatusPacket
    {
        public readonly long Payload;

        /// <summary>
        /// TODO: Add description.
        /// </summary>
        /// <exception cref="UnexpectedDataException">TODO: Why it's thrown.</exception>
        internal static PongPacket Read(Buffer buffer)
        {
            System.Diagnostics.Debug.Assert(buffer != null);

            return new(buffer.ReadLong());
        }

        public PongPacket(long payload) : base(PongPacketId)
        {
            Payload = payload;
        }

        protected override void WriteData(Buffer buffer)
        {
            System.Diagnostics.Debug.Assert(buffer != null);

            buffer.WriteLong(Payload);
        }

    }

    internal sealed class RequestPacket : ServerboundStatusPacket
    {
        /// <summary>
        /// TODO: Add description.
        /// </summary>
        /// <exception cref="UnexpectedDataException">TODO: Why it's thrown.</exception>
        internal static RequestPacket Read(Buffer buffer)
        {
            System.Diagnostics.Debug.Assert(buffer != null);

            return new();
        }

        public RequestPacket() : base(RequestPacketId) { }

        protected override void WriteData(Buffer buffer) 
        {
            System.Diagnostics.Debug.Assert(buffer != null);
        }

    }

    internal sealed class PingPacket : ServerboundStatusPacket
    {
        public readonly long Payload;

        /// <summary>
        /// TODO: Add description.
        /// </summary>
        /// <exception cref="UnexpectedDataException">TODO: Why it's thrown.</exception>
        internal static PingPacket Read(Buffer buffer)
        {
            System.Diagnostics.Debug.Assert(buffer != null);

            return new(buffer.ReadLong());
        }

        private PingPacket(long payload) : base(PingPacketId)
        {
            Payload = payload;
        }

        public PingPacket() : this(System.DateTime.Now.Ticks) { }

        protected override void WriteData(Buffer buffer)
        {
            System.Diagnostics.Debug.Assert(buffer != null);

            buffer.WriteLong(Payload);
        }

    }

    internal sealed class DisconnectPacket : ClientboundLoginPacket
    {
        public readonly string Reason;

        /// <summary>
        /// TODO: Add description.
        /// </summary>
        /// <exception cref="UnexpectedDataException">TODO: Why it's thrown.</exception>
        internal static DisconnectPacket Read(Buffer buffer)
        {
            System.Diagnostics.Debug.Assert(buffer != null);

            return new(buffer.ReadString());
        }

        public DisconnectPacket(string reason) : base(DisconnectPacketId)
        {
            Reason = reason;
        }

        protected override void WriteData(Buffer buffer)
        {
            System.Diagnostics.Debug.Assert(buffer != null);

            buffer.WriteString(Reason);
        }

    }

    internal sealed class EncryptionRequestPacket : ClientboundLoginPacket
    {
        /// <summary>
        /// TODO: Add description.
        /// </summary>
        /// <exception cref="UnexpectedDataException">TODO: Why it's thrown.</exception>
        internal static EncryptionRequestPacket Read(Buffer buffer)
        {
            System.Diagnostics.Debug.Assert(buffer != null);

            throw new System.NotImplementedException();
        }

        private EncryptionRequestPacket() : base(EncryptionRequestPacketId)
        {
            throw new System.NotImplementedException();
        }

        protected override void WriteData(Buffer buffer)
        {
            System.Diagnostics.Debug.Assert(buffer != null);

            throw new System.NotImplementedException();
        }

    }

    internal sealed class LoginSuccessPacket : ClientboundLoginPacket
    {
        public readonly System.Guid UserId;
        public readonly string Username;

        /// <summary>
        /// TODO: Add description.
        /// </summary>
        /// <exception cref="UnexpectedDataException">TODO: Why it's thrown.</exception>
        internal static LoginSuccessPacket Read(Buffer buffer)
        {
            System.Diagnostics.Debug.Assert(buffer != null);

            return new(
                System.Guid.Parse(buffer.ReadString()),
                buffer.ReadString());
        }

        public LoginSuccessPacket(System.Guid userId, string username)
            : base(LoginSuccessPacketId)
        {
            UserId = userId;
            Username = username;
        }

        protected override void WriteData(Buffer buffer)
        {
            System.Diagnostics.Debug.Assert(buffer != null);

            buffer.WriteString(UserId.ToString());
            buffer.WriteString(Username);
        }

    }

    internal sealed class SetCompressionPacket : ClientboundLoginPacket
    {
        public readonly int Threshold;

        /// <summary>
        /// TODO: Add description.
        /// </summary>
        /// <exception cref="UnexpectedDataException">TODO: Why it's thrown.</exception>
        internal static SetCompressionPacket Read(Buffer buffer)
        {
            System.Diagnostics.Debug.Assert(buffer != null);

            return new(buffer.ReadInt(true));
        }

        public SetCompressionPacket(int threshold)
            : base(SetCompressionPacketId)
        {
            Threshold = threshold;
        }

        protected override void WriteData(Buffer buffer)
        {
            System.Diagnostics.Debug.Assert(buffer != null);

            buffer.WriteInt(Threshold, true);
        }

    }

    internal sealed class StartLoginPacket : ServerboundLoginPacket
    {
        public readonly string Username;

        /// <summary>
        /// TODO: Add description.
        /// </summary>
        /// <exception cref="UnexpectedDataException">TODO: Why it's thrown.</exception>
        internal static StartLoginPacket Read(Buffer buffer)
        {
            System.Diagnostics.Debug.Assert(buffer != null);

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
            System.Diagnostics.Debug.Assert(buffer != null);

            buffer.WriteString(Username);
        }
    }

    internal sealed class EncryptionResponsePacket : ServerboundLoginPacket
    {
        /// <summary>
        /// TODO: Add description.
        /// </summary>
        /// <exception cref="UnexpectedDataException">TODO: Why it's thrown.</exception>
        internal static EncryptionResponsePacket Read(Buffer buffer)
        {
            System.Diagnostics.Debug.Assert(buffer != null);

            // TODO: Check the conditions of variables. If not correct, throw exception.
            throw new System.NotImplementedException();
        }

        public EncryptionResponsePacket() : base(EncryptionResponsePacketId)
        {
            // TODO: Assert variables.
            throw new System.NotImplementedException();
        }

        protected override void WriteData(Buffer buffer)
        {
            System.Diagnostics.Debug.Assert(buffer != null);

            throw new System.NotImplementedException();
        }
    }

    internal sealed class SpawnEntityPacket : ClientboundPlayingPacket
    {
        public readonly int EntityId;
        public readonly System.Guid UniqueId;
        public readonly sbyte Type;
        public readonly double X, Y, Z;
        public readonly byte Yaw, Pitch;
        public readonly int Data;
        public readonly short VelocityX, VelocityY, VelocityZ;

        public static SpawnEntityPacket Read(Buffer buffer)
        {
            System.Diagnostics.Debug.Assert(buffer != null);

            throw new System.NotImplementedException();
        }
        
        public SpawnEntityPacket(
            int entityId, System.Guid uniqueId,
            sbyte type, double x, double y, double z,
            byte yaw, byte pitch,
            int data,
            short vx, short vy, short vz) : base(SpawnEntityPacketId)
        {
            EntityId = entityId;
            UniqueId = uniqueId;
            Type = type;
            X = x; Y = y; Z = z;
            Yaw = yaw; Pitch = pitch;
            Data = data;
            VelocityX = vx; VelocityY = vy; VelocityZ = vz;
        }

        protected override void WriteData(Buffer buffer)
        {
            System.Diagnostics.Debug.Assert(buffer != null);

            buffer.WriteInt(EntityId, true);
            buffer.WriteGuid(UniqueId);
            buffer.WriteSbyte(Type);
            buffer.WriteDouble(X); buffer.WriteDouble(Y); buffer.WriteDouble(Z);
            buffer.WriteByte(Yaw); buffer.WriteByte(Pitch);
            buffer.WriteInt(Data);
            if (Data > 0)
            {
                buffer.WriteShort(VelocityX);
                buffer.WriteShort(VelocityY);
                buffer.WriteShort(VelocityZ);
            }
        }

    }

    internal sealed class SpawnNamedEntityPacket : ClientboundPlayingPacket
    {
        public readonly int EntityId;
        public readonly System.Guid UniqueId;
        public readonly double X, Y, Z;
        public readonly byte Yaw, Pitch;
        public readonly byte[] Data;

        public static SpawnNamedEntityPacket Read(Buffer buffer)
        {
            System.Diagnostics.Debug.Assert(buffer != null);

            throw new System.NotImplementedException();
        }

        public SpawnNamedEntityPacket(
            int entityId, 
            System.Guid uniqueId, 
            double x, double y, double z, 
            byte yaw, byte pitch,
            byte[] data) : base(SpawnNamedEntityPacketId)
        {
            EntityId = entityId; 
            UniqueId = uniqueId;
            X = x; Y = y; Z = z;
            Yaw = yaw; Pitch = pitch;
            Data = data;
        }

        protected override void WriteData(Buffer buffer)
        {
            System.Diagnostics.Debug.Assert(buffer != null);

            buffer.WriteInt(EntityId, true);
            buffer.WriteGuid(UniqueId);
            buffer.WriteDouble(X); buffer.WriteDouble(Y); buffer.WriteDouble(Z);
            buffer.WriteByte(Yaw); buffer.WriteByte(Pitch);
            buffer.WriteData(Data);
        }

    }

    internal sealed class ClientboundConfirmTransactionPacket : ClientboundPlayingPacket
    {
        public readonly sbyte WindowId;
        public readonly short ActionNumber;
        public readonly bool Accepted;

        public static ClientboundConfirmTransactionPacket Read(Buffer buffer)
        {
            System.Diagnostics.Debug.Assert(buffer != null);

            throw new System.NotImplementedException();
        }

        public ClientboundConfirmTransactionPacket(
            sbyte windowId, short actionNumber, bool accepted)
            : base(ClientboundConfirmTransactionPacketId)
        {
            WindowId = windowId;
            ActionNumber = actionNumber;
            Accepted = accepted;
        }

        protected override void WriteData(Buffer buffer)
        {
            System.Diagnostics.Debug.Assert(buffer != null);

            buffer.WriteSbyte(WindowId);
            buffer.WriteShort(ActionNumber);
            buffer.WriteBool(Accepted);
        }

    }

    internal sealed class ClientboundCloseWindowPacket : ClientboundPlayingPacket
    {
        public readonly byte WindowId;

        public static ClientboundCloseWindowPacket Read(Buffer buffer)
        {
            System.Diagnostics.Debug.Assert(buffer != null);

            throw new System.NotImplementedException();
        }

        public ClientboundCloseWindowPacket(byte windowId) 
            : base(ClientboundCloseWindowPacketId)
        {
            WindowId = windowId;
        }

        protected override void WriteData(Buffer buffer)
        {
            System.Diagnostics.Debug.Assert(buffer != null);

            buffer.WriteByte(WindowId);
        }

    }

    internal sealed class OpenWindowPacket : ClientboundPlayingPacket
    {
        public readonly byte WindowId;
        public readonly string WindowType;
        public readonly string WindowTitle;
        public readonly byte SlotCount;
        
        public static OpenWindowPacket Read(Buffer buffer)
        {
            System.Diagnostics.Debug.Assert(buffer != null);

            throw new System.NotImplementedException();
        }

        public OpenWindowPacket(
            byte windowId, string windowType, string windowTitle, byte slotCount) 
            : base(OpenWindowPacketId)
        {
            
            WindowId = windowId;
            WindowType = windowType;
            WindowTitle = windowTitle;
            SlotCount = slotCount;
        }

        protected override void WriteData(Buffer buffer)
        {
            System.Diagnostics.Debug.Assert(buffer != null);

            System.Diagnostics.Debug.Assert(WindowId > 0);
            buffer.WriteByte(WindowId);
            buffer.WriteString(WindowType);
            buffer.WriteString("{\"text\":\"foo\"}");
            buffer.WriteByte(SlotCount);
        }

    }

    internal sealed class WindowItemsPacket : ClientboundPlayingPacket
    {
        public readonly byte WindowId;
        public readonly int Count;
        public readonly byte[] Data;

        public static WindowItemsPacket Read(Buffer buffer)
        {
            System.Diagnostics.Debug.Assert(buffer != null);

            throw new System.NotImplementedException();
        }

        public WindowItemsPacket(byte windowId, int count, byte[] data)
            : base(WindowItemsPacketId)
        {
            System.Diagnostics.Debug.Assert(count > 0);
            System.Diagnostics.Debug.Assert(data != null);

            WindowId = windowId;

            Count = count;
            Data = data;
        }

        protected override void WriteData(Buffer buffer)
        {
            System.Diagnostics.Debug.Assert(buffer != null);

            System.Diagnostics.Debug.Assert(buffer != null);

            buffer.WriteByte(WindowId);

            System.Diagnostics.Debug.Assert(Count >= short.MinValue);
            System.Diagnostics.Debug.Assert(Count <= short.MaxValue);
            buffer.WriteShort((short)Count);

            System.Diagnostics.Debug.Assert(Data != null);
            buffer.WriteData(Data);
        }

    }

    internal sealed class SetSlotPacket : ClientboundPlayingPacket
    {
        public readonly sbyte WindowId;
        public readonly short SlotNumber;
        public readonly byte[] Data;

        public static SetSlotPacket Read(Buffer buffer)
        {
            System.Diagnostics.Debug.Assert(buffer != null);

            throw new System.NotImplementedException();
        }

        public SetSlotPacket(sbyte windowId, short slotNumber, byte[] data) 
            : base(SetSlotPacketId)
        {
            System.Diagnostics.Debug.Assert(slotNumber >= 0);
            System.Diagnostics.Debug.Assert(data != null);

            WindowId = windowId;
            SlotNumber = slotNumber;
            Data = data;
        }

        protected override void WriteData(Buffer buffer)
        {
            System.Diagnostics.Debug.Assert(buffer != null);

            buffer.WriteSbyte(WindowId);
            buffer.WriteShort(SlotNumber);
            buffer.WriteData(Data);
        }

    }

    internal sealed class UnloadChunkPacket : ClientboundPlayingPacket
    {
        public readonly int XChunk, ZChunk;

        /// <summary>
        /// TODO: Add description.
        /// </summary>
        /// <exception cref="UnexpectedDataException">TODO: Why it's thrown.</exception>
        internal static UnloadChunkPacket Read(Buffer buffer)
        {
            System.Diagnostics.Debug.Assert(buffer != null);

            throw new System.NotImplementedException();
        }

        public UnloadChunkPacket(int xChunk, int zChunk) : base(UnloadChunkPacketId)
        {
            XChunk = xChunk;
            ZChunk = zChunk;
        }

        protected override void WriteData(Buffer buffer)
        {
            System.Diagnostics.Debug.Assert(buffer != null);

            buffer.WriteInt(XChunk);
            buffer.WriteInt(ZChunk);
        }

    }

    internal sealed class ClientboundKeepAlivePacket : ClientboundPlayingPacket
    {
        public readonly long Payload;

        internal static ClientboundKeepAlivePacket Read(Buffer buffer)
        {
            System.Diagnostics.Debug.Assert(buffer != null);

            throw new System.NotImplementedException();
        }

        public ClientboundKeepAlivePacket(long payload) : base(ClientboundKeepAlivePacketId)
        {
            Payload = payload;
        }

        protected override void WriteData(Buffer buffer)
        {
            System.Diagnostics.Debug.Assert(buffer != null);

            buffer.WriteLong(Payload);
        }
    }

    internal sealed class LoadChunkPacket : ClientboundPlayingPacket
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
            System.Diagnostics.Debug.Assert(buffer != null);

            // TODO: Check the conditions of variables. If not correct, throw exception.
            throw new System.NotImplementedException();
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
            System.Diagnostics.Debug.Assert(buffer != null);

            buffer.WriteInt(XChunk);
            buffer.WriteInt(ZChunk);
            buffer.WriteBool(Continuous);
            buffer.WriteInt(Mask, true);
            buffer.WriteInt(Data.Length, true);
            buffer.WriteData(Data);
            buffer.WriteInt(0, true);  // TODO: Block entities
        }

    }

    internal sealed class ParticlesPacket : ClientboundPlayingPacket
    {
        internal readonly int ParticleId;
        internal readonly bool Flag;  // ExtendedRange?
        internal readonly float X, Y, Z;
        internal readonly float OffsetX, OffsetY, OffsetZ;
        internal readonly float Extra;
        internal readonly int Count;

        internal static ParticlesPacket Read(Buffer buffer)
        {
            System.Diagnostics.Debug.Assert(buffer != null);

            throw new System.NotImplementedException();
        }

        public ParticlesPacket(
            int particleId, bool extendedRange, 
            float x, float y, float z, 
            float offsetX, float offsetY, float offsetZ, 
            float speed, int extra) : base(ParticlesPacketId)
        {
            ParticleId = particleId;
            Flag = extendedRange;
            X = x; Y = y; Z = z;
            OffsetX = offsetX; OffsetY = offsetY; OffsetZ = offsetZ;
            Extra = speed;
            Count = extra;
        }

        protected override void WriteData(Buffer buffer)
        {
            System.Diagnostics.Debug.Assert(buffer != null);

            buffer.WriteInt(ParticleId);
            buffer.WriteBool(Flag);
            buffer.WriteFloat(X); buffer.WriteFloat(Y); buffer.WriteFloat(Z);
            buffer.WriteFloat(OffsetX); buffer.WriteFloat(OffsetY); buffer.WriteFloat(OffsetZ);
            buffer.WriteFloat(Extra);
            buffer.WriteInt(Count);
        }

    }

    internal sealed class JoinGamePacket : ClientboundPlayingPacket
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
            System.Diagnostics.Debug.Assert(buffer != null);

            // TODO: Check the conditions of variables. If not correct, throw exception.
            throw new System.NotImplementedException();
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
            System.Diagnostics.Debug.Assert(buffer != null);

            buffer.WriteInt(_entityId);
            buffer.WriteByte(_gamemode);
            buffer.WriteInt(_dimension);
            buffer.WriteByte(_difficulty);
            buffer.WriteByte(0);
            buffer.WriteString(_levelType);
            buffer.WriteBool(_reducedDebugInfo);
        }
    }

    internal sealed class EntityPacket : ClientboundPlayingPacket
    {
        public readonly int EntityId;

        internal static EntityPacket Read(Buffer buffer)
        {
            System.Diagnostics.Debug.Assert(buffer != null);

            throw new System.NotImplementedException();
        }

        public EntityPacket(int entityId) : base(EntityPacketId)
        {
            EntityId = entityId;
        }

        protected override void WriteData(Buffer buffer)
        {
            System.Diagnostics.Debug.Assert(buffer != null);

            buffer.WriteInt(EntityId, true);
        }

    }

    internal sealed class EntityRelMovePacket : ClientboundPlayingPacket
    {
        public readonly int EntityId;
        public readonly short DeltaX, DeltaY, DeltaZ;
        public readonly bool OnGround;

        internal static EntityRelMovePacket Read(Buffer buffer)
        {
            System.Diagnostics.Debug.Assert(buffer != null);

            throw new System.NotImplementedException();
        }

        public EntityRelMovePacket(
            int entityId,
            short dx, short dy, short dz,
            bool onGround) : base(EntityRelMovePacketId)
        {
            EntityId = entityId;
            DeltaX = dx; DeltaY = dy; DeltaZ = dz;
            OnGround = onGround;
        }

        protected override void WriteData(Buffer buffer)
        {
            System.Diagnostics.Debug.Assert(buffer != null);

            buffer.WriteInt(EntityId, true);
            buffer.WriteShort(DeltaX); buffer.WriteShort(DeltaY); buffer.WriteShort(DeltaZ);
            buffer.WriteBool(OnGround);
        }

    }

    internal sealed class EntityRelMoveLookPacket : ClientboundPlayingPacket
    {
        public readonly int EntityId;
        public readonly short DeltaX, DeltaY, DeltaZ;
        public readonly byte Yaw, Pitch;
        public readonly bool OnGround;

        internal static EntityRelMoveLookPacket Read(Buffer buffer)
        {
            System.Diagnostics.Debug.Assert(buffer != null);

            throw new System.NotImplementedException();
        }

        public EntityRelMoveLookPacket(
            int entityId,
            short dx, short dy, short dz,
            byte yaw, byte pitch,
            bool onGround) : base(EntityRelMoveLookPacketId)
        {
            EntityId = entityId;
            DeltaX = dx; DeltaY = dy; DeltaZ = dz;
            Yaw = yaw; Pitch = pitch;
            OnGround = onGround;
        }

        protected override void WriteData(Buffer buffer)
        {
            System.Diagnostics.Debug.Assert(buffer != null);

            buffer.WriteInt(EntityId, true);
            buffer.WriteShort(DeltaX); buffer.WriteShort(DeltaY); buffer.WriteShort(DeltaZ);
            buffer.WriteByte(Yaw); buffer.WriteByte(Pitch);
            buffer.WriteBool(OnGround);
        }

    }

    internal sealed class EntityLookPacket : ClientboundPlayingPacket
    {
        public readonly int EntityId;
        public readonly byte Yaw, Pitch;
        public readonly bool OnGround;

        internal static EntityLookPacket Read(Buffer buffer)
        {
            System.Diagnostics.Debug.Assert(buffer != null);

            throw new System.NotImplementedException();
        }

        public EntityLookPacket(
            int entityId,
            byte yaw, byte pitch,
            bool onGround) : base(EntityLookPacketId)
        {
            EntityId = entityId;
            Yaw = yaw; Pitch = pitch;
            OnGround = onGround;
        }

        protected override void WriteData(Buffer buffer)
        {
            System.Diagnostics.Debug.Assert(buffer != null);

            
            buffer.WriteInt(EntityId, true);
            buffer.WriteByte(Yaw); buffer.WriteByte(Pitch);
            buffer.WriteBool(OnGround);
        }

    }

    internal sealed class AbilitiesPacket : ClientboundPlayingPacket
    {
        private readonly byte _flags;
        private readonly float _flyingSpeed;
        private readonly float _fovModifier;  // fov * _fovModifier;

        /// <summary>
        /// TODO: Add description.
        /// </summary>
        /// <exception cref="UnexpectedDataException">TODO: Why it's thrown.</exception>
        internal static AbilitiesPacket Read(Buffer buffer)
        {
            System.Diagnostics.Debug.Assert(buffer != null);

            // TODO: Check the conditions of variables. If not correct, throw exception.
            throw new System.NotImplementedException();
        }

        public AbilitiesPacket(
            bool invulnerable, bool flying, bool allowFlying, bool creativeMode,
            float flyingSpeed, float fovModifier) : base(AbilitiesId)
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
            System.Diagnostics.Debug.Assert(buffer != null);

            buffer.WriteByte(_flags);
            buffer.WriteFloat(_flyingSpeed);
            buffer.WriteFloat(_fovModifier);
        }

    }
    
    internal sealed class PlayerListItemAddPacket : ClientboundPlayingPacket
    {
        public readonly System.Guid UniqueId;
        public readonly string Username;
        public readonly int Ping;

        internal static PlayerListItemAddPacket Read(Buffer buffer)
        {
            System.Diagnostics.Debug.Assert(buffer != null);

            throw new System.NotImplementedException();
        }

        public PlayerListItemAddPacket(
            System.Guid uniqueId, string username, int ms)
            : base(PlayerListItemAddPacketId)
        {
            UniqueId = uniqueId;
            Username = username;
            Ping = ms;
        }

        protected override void WriteData(Buffer buffer)
        {
            System.Diagnostics.Debug.Assert(buffer != null);

            buffer.WriteInt(0, true);
            buffer.WriteInt(1, true);
            buffer.WriteGuid(UniqueId);
            buffer.WriteString(Username);
            buffer.WriteInt(0, true);  
            buffer.WriteInt(0, true);  // gamemode
            buffer.WriteInt(Ping, true);  // latency
            buffer.WriteBool(false);
        }

    }

    internal sealed class PlayerListItemUpdateLatencyPacket : ClientboundPlayingPacket
    {
        public readonly System.Guid UniqueId;
        public readonly int Laytency;

        internal static PlayerListItemUpdateLatencyPacket Read(Buffer buffer)
        {
            System.Diagnostics.Debug.Assert(buffer != null);

            throw new System.NotImplementedException();
        }

        public PlayerListItemUpdateLatencyPacket(System.Guid uniqueId, int laytency) 
            : base(PlayerListItemUpdateLatencyPacketId)
        {
            UniqueId = uniqueId;
            Laytency = laytency;
        }

        protected override void WriteData(Buffer buffer)
        {
            System.Diagnostics.Debug.Assert(buffer != null);

            buffer.WriteInt(2, true);
            buffer.WriteInt(1, true);
            buffer.WriteGuid(UniqueId);
            buffer.WriteInt(Laytency, true);
        }

    }

    internal sealed class PlayerListItemRemovePacket : ClientboundPlayingPacket
    {
        public readonly System.Guid UniqueId;

        internal static PlayerListItemRemovePacket Read(Buffer buffer)
        {
            System.Diagnostics.Debug.Assert(buffer != null);

            throw new System.NotImplementedException();
        }

        public PlayerListItemRemovePacket(System.Guid uniqueId)
            : base(PlayerListItemRemovePacketId)
        {
            UniqueId = uniqueId;
        }

        protected override void WriteData(Buffer buffer)
        {
            System.Diagnostics.Debug.Assert(buffer != null);

            buffer.WriteInt(4, true);
            buffer.WriteInt(1, true);
            buffer.WriteGuid(UniqueId);
        }
    }

    internal sealed class TeleportPacket : ClientboundPlayingPacket
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
            System.Diagnostics.Debug.Assert(buffer != null);

            // TODO: Check the conditions of variables. If not correct, throw exception.
            throw new System.NotImplementedException();
        }

        public TeleportPacket(
            double x, double y, double z, 
            float yaw, float pitch, 
            bool relativeX, bool relativeY, bool relativeZ,
            bool relativeYaw, bool relativePitch,
            int payload) : base(TeleportPacketId)
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
            System.Diagnostics.Debug.Assert(buffer != null);

            buffer.WriteDouble(X);
            buffer.WriteDouble(Y);
            buffer.WriteDouble(Z);
            buffer.WriteFloat(Yaw);
            buffer.WriteFloat(Pitch);
            buffer.WriteByte(_flags);
            buffer.WriteInt(Payload, true);

        }

    }

    internal sealed class DestroyEntitiesPacket : ClientboundPlayingPacket
    {
        public readonly int[] EntityIds;

        internal static DestroyEntitiesPacket Read(Buffer buffer)
        {
            System.Diagnostics.Debug.Assert(buffer != null);

            throw new System.NotImplementedException();
        }

        public DestroyEntitiesPacket(params int[] entityIds) : base(DestroyEntitiesPacketId)
        {
            EntityIds = entityIds;  // TODO: Copy
        }

        protected override void WriteData(Buffer buffer)
        {
            System.Diagnostics.Debug.Assert(buffer != null);

            buffer.WriteInt(EntityIds.Length, true);
            foreach (int id in EntityIds)
                buffer.WriteInt(id, true);
        }

    }

    internal sealed class EntityHeadLookPacket : ClientboundPlayingPacket
    {
        public readonly int EntityId;
        public readonly byte Yaw;

        internal static EntityHeadLookPacket Read(Buffer buffer)
        {
            System.Diagnostics.Debug.Assert(buffer != null);

            throw new System.NotImplementedException();
        }

        public EntityHeadLookPacket(
            int entityId, byte yaw) : base(EntityHeadLookPacketId)
        {
            EntityId = entityId;
            Yaw = yaw;
        }

        protected override void WriteData(Buffer buffer)
        {
            System.Diagnostics.Debug.Assert(buffer != null);

            buffer.WriteInt(EntityId, true);
            buffer.WriteByte(Yaw);
        }

    }

    internal sealed class EntityMetadataPacket : ClientboundPlayingPacket
    {
        public readonly int EntityId;
        public readonly byte[] Data;

        internal static EntityMetadataPacket Read(Buffer buffer)
        {
            System.Diagnostics.Debug.Assert(buffer != null);

            throw new System.NotImplementedException();
        }

        public EntityMetadataPacket(
            int entityId, byte[] data) : base(EntityMetadataPacketId)
        {
            EntityId = entityId;
            Data = data;
        }

        protected override void WriteData(Buffer buffer)
        {
            System.Diagnostics.Debug.Assert(buffer != null);

            buffer.WriteInt(EntityId, true);
            buffer.WriteData(Data);
        }

    }

    internal sealed class EntityVelocityPacket : ClientboundPlayingPacket
    {
        public readonly int EntityId;
        public readonly short X, Y, Z;

        internal static EntityVelocityPacket Read(Buffer buffer)
        {
            System.Diagnostics.Debug.Assert(buffer != null);

            throw new System.NotImplementedException();
        }

        public EntityVelocityPacket(int entityId, short x, short y, short z)
            : base(EntityVelocityPacketId)
        {
            EntityId = entityId;
            X = x; Y = y; Z = z;
        }

        protected override void WriteData(Buffer buffer)
        {
            System.Diagnostics.Debug.Assert(buffer != null);

            buffer.WriteInt(EntityId, true);
            buffer.WriteShort(X); buffer.WriteShort(Y); buffer.WriteShort(Z);
        }

    }

    internal sealed class EntityEquipmentPacket : ClientboundPlayingPacket
    {
        public readonly int EntityId;
        public readonly int Slot;
        public readonly byte[] Data;

        internal static EntityEquipmentPacket Read(Buffer buffer)
        {
            System.Diagnostics.Debug.Assert(buffer != null);

            throw new System.NotImplementedException();
        }

        public EntityEquipmentPacket(int idEntity, int slot, byte[] data)
            : base(EntityEquipmentPacketId)
        {
            System.Diagnostics.Debug.Assert(data != null);

            EntityId = idEntity;
            Slot = slot;
            Data = data;
        }

        protected override void WriteData(Buffer buffer)
        {
            System.Diagnostics.Debug.Assert(buffer != null);

            buffer.WriteInt(EntityId, true);
            buffer.WriteInt(Slot, true);
            buffer.WriteData(Data);
        }
    }

    internal sealed class EntityTeleportPacket : ClientboundPlayingPacket
    {
        public readonly int EntityId;
        public readonly double X, Y, Z;
        public readonly float Yaw, Pitch;
        public readonly bool OnGround;

        internal static EntityTeleportPacket Read(Buffer buffer)
        {
            System.Diagnostics.Debug.Assert(buffer != null);

            throw new System.NotImplementedException();
        }

        public EntityTeleportPacket(
            int entityId, 
            double x, double y, double z, 
            float yaw, float pitch,
            bool onGround) : base(EntityTeleportPacketId)
        {
            EntityId = entityId;
            X = x; Y = y; Z = z;
            Yaw = yaw; Pitch = pitch;
            OnGround = onGround;
        }

        protected override void WriteData(Buffer buffer)
        {
            System.Diagnostics.Debug.Assert(buffer != null);

            buffer.WriteInt(EntityId, true);
            buffer.WriteDouble(X); buffer.WriteDouble(Y); buffer.WriteDouble(Z);
            buffer.WriteFloat(Yaw); buffer.WriteFloat(Pitch);
            buffer.WriteBool(OnGround);
        }

    }

    internal sealed class TeleportAcceptPacket : ServerboundPlayingPacket
    {
        public readonly int Payload;

        /// <summary>
        /// TODO: Add description.
        /// </summary>
        /// <exception cref="UnexpectedDataException">TODO: Why it's thrown.</exception>
        internal static TeleportAcceptPacket Read(Buffer buffer)
        {
            System.Diagnostics.Debug.Assert(buffer != null);

            return new(buffer.ReadInt(true));
        }

        public TeleportAcceptPacket(int payload) : base(TeleportAcceptPacketd)
        {
            Payload = payload;
        }

        protected override void WriteData(Buffer buffer)
        {
            System.Diagnostics.Debug.Assert(buffer != null);

            throw new System.NotImplementedException();
        }

    }

    internal sealed class SettingsPacket : ServerboundPlayingPacket
    {
        public readonly byte RenderDistance;

        /// <summary>
        /// TODO: Add description.
        /// </summary>
        /// <exception cref="UnexpectedDataException">TODO: Why it's thrown.</exception>
        internal static SettingsPacket Read(Buffer buffer)
        {
            System.Diagnostics.Debug.Assert(buffer != null);

            buffer.ReadString();  // TODO
            byte renderDistance = buffer.ReadByte(); 
            buffer.ReadInt(true);  // TODO
            buffer.ReadBool();  // TODO
            buffer.ReadSbyte();  // TODO
            buffer.ReadInt(true);  // TODO

            return new(renderDistance);
        }

        private SettingsPacket(byte renderDistance)
            : base(SettingsPacketId)
        {
            RenderDistance = renderDistance;
        }

        protected override void WriteData(Buffer buffer)
        {
            System.Diagnostics.Debug.Assert(buffer != null);

            throw new System.NotImplementedException();
        }
    }

    internal sealed class ServerboundConfirmTransactionPacket : ServerboundPlayingPacket
    {
        public readonly sbyte WindowId;
        public readonly short ActionNumber;
        public readonly bool Accepted;

        internal static ServerboundConfirmTransactionPacket Read(Buffer buffer)
        {
            System.Diagnostics.Debug.Assert(buffer != null);

            return new(buffer.ReadSbyte(), buffer.ReadShort(), buffer.ReadBool());
        }

        private ServerboundConfirmTransactionPacket(
            sbyte windowId, short actionNumber, bool accepted)
            : base(ServerboundConfirmTransactionPacketId)
        {
            WindowId = windowId;
            ActionNumber = actionNumber;
            Accepted = accepted;
        }

        protected override void WriteData(Buffer buffer)
        {
            System.Diagnostics.Debug.Assert(buffer != null);

            throw new System.NotImplementedException();
        }

    }

    internal sealed class ClickWindowPacket : ServerboundPlayingPacket
    {
        public readonly byte WindowId;
        public readonly short Slot;
        public readonly sbyte Button;
        public readonly short Action;
        public readonly int Mode;
        public readonly byte[] Data;

        internal static ClickWindowPacket Read(Buffer buffer)
        {
            System.Diagnostics.Debug.Assert(buffer != null);

            return new(
                buffer.ReadByte(),
                buffer.ReadShort(), buffer.ReadSbyte(), buffer.ReadShort(), buffer.ReadInt(true),
                buffer.ReadData());
        }

        private ClickWindowPacket(
            byte idWindow,
            short slot, sbyte button, short action, int mode,
            byte[] data) : base(ClickWindowPacketId)
        {
            WindowId = idWindow;
            Slot = slot;
            Button = button;
            Action = action;
            Mode = mode;
            Data = data;
        }

        protected override void WriteData(Buffer buffer)
        {
            System.Diagnostics.Debug.Assert(buffer != null);

            throw new System.NotImplementedException();
        }

    }

    internal sealed class ServerboundCloseWindowPacket : ServerboundPlayingPacket
    {
        public readonly byte WindowId;

        internal static ServerboundCloseWindowPacket Read(Buffer buffer)
        {
            System.Diagnostics.Debug.Assert(buffer != null);

            return new(buffer.ReadByte());
        }

        private ServerboundCloseWindowPacket(byte windowId) 
            : base(ServerboundCloseWindowPacketId)
        {
            WindowId = windowId;
        }

        protected override void WriteData(Buffer buffer)
        {
            System.Diagnostics.Debug.Assert(buffer != null);

            throw new System.NotImplementedException();
        }

    }

    internal sealed class ServerboundKeepAlivePacket : ServerboundPlayingPacket
    {
        public readonly long Payload;

        /// <summary>
        /// TODO: Add description.
        /// </summary>
        /// <exception cref="UnexpectedDataException">TODO: Why it's thrown.</exception>
        internal static ServerboundKeepAlivePacket Read(Buffer buffer)
        {
            System.Diagnostics.Debug.Assert(buffer != null);

            return new(buffer.ReadLong());
        }

        private ServerboundKeepAlivePacket(long payload) : base(ServerboundKeepAlivePacketId)
        {
            Payload = payload;
        }

        protected override void WriteData(Buffer buffer)
        {
            System.Diagnostics.Debug.Assert(buffer != null);

            throw new System.NotImplementedException();
        }

    }

    internal sealed class PlayerPacket : ServerboundPlayingPacket
    {
        public readonly bool OnGround;

        /// <summary>
        /// TODO: Add description.
        /// </summary>
        /// <exception cref="UnexpectedDataException">TODO: Why it's thrown.</exception>
        internal static PlayerPacket Read(Buffer buffer)
        {
            System.Diagnostics.Debug.Assert(buffer != null);

            return new(buffer.ReadBool());
        }

        public PlayerPacket(bool onGround) : base(PlayerPacketId)
        {
            OnGround = onGround;
        }

        protected override void WriteData(Buffer buffer)
        {
            System.Diagnostics.Debug.Assert(buffer != null);

            throw new System.NotImplementedException();
        }

    }

    internal sealed class PlayerPositionPacket : ServerboundPlayingPacket
    {
        public readonly double X, Y, Z;
        public readonly bool OnGround;

        /// <summary>
        /// TODO: Add description.
        /// </summary>
        /// <exception cref="UnexpectedDataException">TODO: Why it's thrown.</exception>
        internal static PlayerPositionPacket Read(Buffer buffer)
        {
            System.Diagnostics.Debug.Assert(buffer != null);

            return new(
                buffer.ReadDouble(), buffer.ReadDouble(), buffer.ReadDouble(), 
                buffer.ReadBool());
        }

        public PlayerPositionPacket(double x, double y, double z, bool onGround)
            : base(PlayerPositionPacketId)
        {
            X = x; Y = y; Z = z;
            OnGround = onGround;
        }

        protected override void WriteData(Buffer buffer)
        {
            System.Diagnostics.Debug.Assert(buffer != null);

            throw new System.NotImplementedException();
        }

    }

    internal sealed class PlayerPosAndLookPacket : ServerboundPlayingPacket
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
            System.Diagnostics.Debug.Assert(buffer != null);

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
            System.Diagnostics.Debug.Assert(buffer != null);

            throw new System.NotImplementedException();
        }

    }

    internal sealed class PlayerLookPacket : ServerboundPlayingPacket
    {
        public readonly float Yaw, Pitch;
        public readonly bool OnGround;

        /// <summary>
        /// TODO: Add description.
        /// </summary>
        /// <exception cref="UnexpectedDataException">TODO: Why it's thrown.</exception>
        internal static PlayerLookPacket Read(Buffer buffer)
        {
            System.Diagnostics.Debug.Assert(buffer != null);

            return new(
                buffer.ReadFloat(), buffer.ReadFloat(),
                buffer.ReadBool());
        }

        public PlayerLookPacket(
            float yaw, float pitch,
            bool onGround) 
            : base(PlayerLookPacketId)
        {
            Yaw = yaw; Pitch = pitch;
            OnGround = onGround;
        }

        protected override void WriteData(Buffer buffer)
        {
            System.Diagnostics.Debug.Assert(buffer != null);

            throw new System.NotImplementedException();
        }
    }

    internal sealed class PlayerDiggingPacket : ServerboundPlayingPacket
    {
        public readonly int Status;
        // TODO: Make x as a 26-bit integer,
        // followed by z as a 26-bit integer,
        // followed by y as a 12-bit integer
        // (all signed, two's complement).
        public readonly byte Position; 
        public readonly byte Face;

        internal static PlayerDiggingPacket Read(Buffer buffer)
        {
            System.Diagnostics.Debug.Assert(buffer != null);

            return new(buffer.ReadInt(true), buffer.ReadByte(), buffer.ReadByte());
        }

        private PlayerDiggingPacket(int status, byte p, byte face) 
            : base(PlayerDiggingPacketId)
        {
            Status = status;
            Position = p;
            Face = face;
        }

        protected override void WriteData(Buffer buffer)
        {
            System.Diagnostics.Debug.Assert(buffer != null);

            throw new System.NotImplementedException();
        }
    }

    internal sealed class EntityActionPacket : ServerboundPlayingPacket
    {
        public readonly int EntityId;
        public readonly int ActionId;
        public readonly int JumpBoost;

        internal static EntityActionPacket Read(Buffer buffer)
        {
            System.Diagnostics.Debug.Assert(buffer != null);

            return new(buffer.ReadInt(true), buffer.ReadInt(true), buffer.ReadInt(true));
        }

        private EntityActionPacket(int entityId, int actionId, int jumpBoost) 
            : base(EntityActionPacketId)
        {
            EntityId = entityId;
            ActionId = actionId;
            JumpBoost = jumpBoost;
        }

        protected override void WriteData(Buffer buffer)
        {
            System.Diagnostics.Debug.Assert(buffer != null);

            throw new System.NotImplementedException();
        }

    }

    internal sealed class ServerboundHeldItemSlotPacket : ServerboundPlayingPacket
    {
        public readonly int Slot;

        internal static ServerboundHeldItemSlotPacket Read(Buffer buffer)
        {
            System.Diagnostics.Debug.Assert(buffer != null);

            return new((int)buffer.ReadShort());
        }

        private ServerboundHeldItemSlotPacket(int slot) : base(ServerboundHeldItemSlotPacketId)
        {
            Slot = slot;
        }

        protected override void WriteData(Buffer buffer)
        {
            System.Diagnostics.Debug.Assert(buffer != null);

            throw new System.NotImplementedException();
        }
    }

    internal sealed class UseItemPacket : ServerboundPlayingPacket
    {
        public readonly int Hand;

        internal static UseItemPacket Read(Buffer buffer)
        {
            System.Diagnostics.Debug.Assert(buffer != null);

            return new(buffer.ReadInt(true));
        }

        private UseItemPacket(int hand) : base(UseItemPacketId)
        {
            Hand = hand;
        }

        protected override void WriteData(Buffer buffer)
        {
            System.Diagnostics.Debug.Assert(buffer != null);

            throw new System.NotImplementedException();
        }
    }

}

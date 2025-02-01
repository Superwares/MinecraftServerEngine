
namespace MinecraftPrimitives
{

    /*public sealed class SlotData
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



    public abstract class Packet
    {
        public const string MinecraftVersion = "1.12.2";
        public const int ProtocolVersion = 340;

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

        protected abstract void WriteData(MinecraftProtocolDataStream buffer);

        public void Write(MinecraftProtocolDataStream buffer)
        {
            if (buffer == null)
            {
                throw new System.ArgumentNullException(nameof(buffer));
            }

            buffer.WriteInt(Id, true);
            WriteData(buffer);
        }

    }

    public abstract class HandshakingPacket : Packet
    {
        public override States State => States.Handshaking;

        internal HandshakingPacket(int id) : base(id)
        {

        }

    }

    public abstract class StatusPacket : Packet
    {
        public override States State => States.Status;

        internal StatusPacket(int id) : base(id)
        {

        }

    }

    public abstract class LoginPacket : Packet
    {
        public override States State => States.Login;

        internal LoginPacket(int id) : base(id)
        {

        }

    }

    public abstract class PlayingPacket : Packet
    {
        public override States State => States.Playing;

        internal PlayingPacket(int id) : base(id)
        {

        }
    }

    public abstract class ServerboundHandshakingPacket : HandshakingPacket
    {
        public const int SetProtocolPacketId = 0x00;

        internal ServerboundHandshakingPacket(int id) : base(id)
        {

        }

        public override WhereBound BoundTo { get { return WhereBound.Serverbound; } }

    }

    public abstract class ClientboundStatusPacket : StatusPacket
    {
        public const int ResponsePacketId = 0x00;
        public const int PongPacketId = 0x01;

        public override WhereBound BoundTo { get { return WhereBound.Clientbound; } }

        internal ClientboundStatusPacket(int id) : base(id)
        {

        }

    }

    public abstract class ServerboundStatusPacket : StatusPacket
    {
        public const int RequestPacketId = 0x00;
        public const int PingPacketId = 0x01;

        public override WhereBound BoundTo { get { return WhereBound.Serverbound; } }

        internal ServerboundStatusPacket(int id) : base(id)
        {

        }


    }

    public abstract class ClientboundLoginPacket : LoginPacket
    {
        public const int DisconnectPacketId = 0x00;
        public const int EncryptionRequestPacketId = 0x01;
        public const int LoginSuccessPacketId = 0x02;
        public const int SetCompressionPacketId = 0x03;

        public override WhereBound BoundTo => WhereBound.Clientbound;

        internal ClientboundLoginPacket(int id) : base(id)
        {

        }


    }

    public abstract class ServerboundLoginPacket : LoginPacket
    {
        public const int StartLoginPacketId = 0x00;
        public const int EncryptionResponsePacketId = 0x01;

        public override WhereBound BoundTo => WhereBound.Clientbound;

        internal ServerboundLoginPacket(int id) : base(id)
        {

        }


    }

    public abstract class ClientboundPlayingPacket : PlayingPacket
    {
        public const int SpawnEntityPacketId = 0x00;
        public const int SpawnNamedEntityPacketId = 0x05;
        public const int ClientboundAnimationPacketId = 0x06;
        public const int BlockChangePacketId = 0x0B;
        public const int BossBarPacketId = 0x0C;
        public const int ClientboundChatmessagePacketId = 0x0F;
        public const int ClientboundConfirmTransactionPacketId = 0x11;
        public const int ClientboundCloseWindowPacketId = 0x12;
        public const int OpenWindowPacketId = 0x13;
        public const int WindowItemsPacketId = 0x14;
        public const int SetSlotPacketId = 0x16;
        public const int SetCooldownPacketId = 0x17;
        public const int NamedSoundEffectPacketId = 0x19;
        public const int EntityStatusPacketId = 0x1B;
        public const int UnloadChunkPacketId = 0x1D;
        public const int GameStatePacketId = 0x1E;
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
        public const int RemoveEntityEffectPacketId = 0x33;
        public const int RespawnPacketId = 0x35;
        public const int EntityHeadLookPacketId = 0x36;
        public const int WorldBorderPacketId = 0x38;
        public const int EntityMetadataPacketId = 0x3C;
        public const int EntityVelocityPacketId = 0x3E;
        public const int EntityEquipmentPacketId = 0x3F;
        public const int SetExperiencePacketId = 0x40;
        public const int UpdateHealthPacketId = 0x41;
        public const int TimeUpdatePacketId = 0x47;
        public const int TitlePacketId = 0x48;
        public const int EntityTeleportPacketId = 0x4C;
        public const int EntityPropertiesPacketId = 0x4E;
        public const int EntityEffectPacketId = 0x4F;



        public override WhereBound BoundTo => WhereBound.Clientbound;

        internal ClientboundPlayingPacket(int id) : base(id)
        {
        }

    }

    public abstract class ServerboundPlayingPacket : PlayingPacket
    {
        public const int TeleportAcceptPacketId = 0x00;
        public const int ServerboundChatMessagePacketId = 0x02;
        public const int SettingsPacketId = 0x04;
        public const int ServerboundConfirmTransactionPacketId = 0x05;
        public const int ClickWindowPacketId = 0x07;
        public const int ServerboundCloseWindowPacketId = 0x08;
        public const int ServerboundCustomPayloadPacketId = 0x09;
        public const int UseEntityPacketId = 0x0A;
        public const int ServerboundKeepAlivePacketId = 0x0B;
        public const int PlayerPacketId = 0x0C;
        public const int PlayerPositionPacketId = 0x0D;
        public const int PlayerPosAndLookPacketId = 0x0E;
        public const int PlayerLookPacketId = 0x0F;
        public const int PlayerDigPacketId = 0x14;
        public const int EntityActionPacketId = 0x15;
        public const int ServerboundHeldItemSlotPacketId = 0x1A;
        public const int ServerboundAnimationPacketId = 0x1D;
        public const int BlockPlacementPacketId = 0x1F;
        public const int UseItemPacketId = 0x20;

        public override WhereBound BoundTo => WhereBound.Serverbound;

        internal ServerboundPlayingPacket(int id) : base(id)
        {
        }

    }

    public sealed class SetProtocolPacket : ServerboundHandshakingPacket
    {
        public readonly int Version;
        public readonly string Hostname;
        public readonly ushort Port;
        public readonly States NextState;

        private static States ReadNextState(MinecraftProtocolDataStream buffer)
        {
            if (buffer == null)
            {
                throw new System.ArgumentNullException(nameof(buffer));
            }

            int a = buffer.ReadInt(true);
            States nextState = (a == 1 ? States.Status : States.Login);
            if (!(a == 1 || a == 2))
            {
                throw new InvalidEncodingException();
            }

            return nextState;
        }

        /// <summary>
        /// TODO: Add description.
        /// </summary>
        /// <exception cref="UnexpectedDataException">TODO: Why it's thrown.</exception>
        public static SetProtocolPacket Read(MinecraftProtocolDataStream buffer)
        {
            if (buffer == null)
            {
                throw new System.ArgumentNullException(nameof(buffer));
            }

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

        protected override void WriteData(MinecraftProtocolDataStream buffer)
        {
            if (buffer == null)
            {
                throw new System.ArgumentNullException(nameof(buffer));
            }

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

    public sealed class ResponsePacket : ClientboundStatusPacket
    {

        public readonly int MaxPlayers;
        public readonly int OnlinePlayers;
        public readonly string Description;

        /// <exception cref="UnexpectedDataException">TODO: Why it's thrown.</exception>
        public static ResponsePacket Read(MinecraftProtocolDataStream buffer)
        {
            if (buffer == null)
            {
                throw new System.ArgumentNullException(nameof(buffer));
            }

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

        protected override void WriteData(MinecraftProtocolDataStream buffer)
        {
            if (buffer == null)
            {
                throw new System.ArgumentNullException(nameof(buffer));
            }

            // TODO: Using json serialization.
            string jsonString = "{\"version\":{\"name\":\"1.12.2\",\"protocol\":340},\"players\":{\"max\":100,\"online\":0,\"sample\":[]},\"description\":{\"text\":\"Hello, World!\"},\"favicon\":\"data:image/png;base64,<data>\",\"enforcesSecureChat\":true,\"previewsChat\":true}";

            buffer.WriteString(jsonString);
        }

    }

    public sealed class PongPacket : ClientboundStatusPacket
    {
        public readonly long Payload;

        /// <summary>
        /// TODO: Add description.
        /// </summary>
        /// <exception cref="UnexpectedDataException">TODO: Why it's thrown.</exception>
        public static PongPacket Read(MinecraftProtocolDataStream buffer)
        {
            if (buffer == null)
            {
                throw new System.ArgumentNullException(nameof(buffer));
            }

            return new(buffer.ReadLong());
        }

        public PongPacket(long payload) : base(PongPacketId)
        {
            Payload = payload;
        }

        protected override void WriteData(MinecraftProtocolDataStream buffer)
        {
            if (buffer == null)
            {
                throw new System.ArgumentNullException(nameof(buffer));
            }

            buffer.WriteLong(Payload);
        }

    }

    public sealed class RequestPacket : ServerboundStatusPacket
    {
        /// <summary>
        /// TODO: Add description.
        /// </summary>
        /// <exception cref="UnexpectedDataException">TODO: Why it's thrown.</exception>
        public static RequestPacket Read(MinecraftProtocolDataStream buffer)
        {
            if (buffer == null)
            {
                throw new System.ArgumentNullException(nameof(buffer));
            }

            return new();
        }

        public RequestPacket() : base(RequestPacketId) { }

        protected override void WriteData(MinecraftProtocolDataStream buffer)
        {
            if (buffer == null)
            {
                throw new System.ArgumentNullException(nameof(buffer));
            }
        }

    }

    public sealed class PingPacket : ServerboundStatusPacket
    {
        public readonly long Payload;

        /// <summary>
        /// TODO: Add description.
        /// </summary>
        /// <exception cref="UnexpectedDataException">TODO: Why it's thrown.</exception>
        public static PingPacket Read(MinecraftProtocolDataStream buffer)
        {
            if (buffer == null)
            {
                throw new System.ArgumentNullException(nameof(buffer));
            }

            return new(buffer.ReadLong());
        }

        private PingPacket(long payload) : base(PingPacketId)
        {
            Payload = payload;
        }

        public PingPacket() : this(System.DateTime.Now.Ticks) { }

        protected override void WriteData(MinecraftProtocolDataStream buffer)
        {
            if (buffer == null)
            {
                throw new System.ArgumentNullException(nameof(buffer));
            }

            buffer.WriteLong(Payload);
        }

    }

    public sealed class DisconnectPacket : ClientboundLoginPacket
    {
        public readonly string Reason;

        /// <summary>
        /// TODO: Add description.
        /// </summary>
        /// <exception cref="UnexpectedDataException">TODO: Why it's thrown.</exception>
        public static DisconnectPacket Read(MinecraftProtocolDataStream buffer)
        {
            if (buffer == null)
            {
                throw new System.ArgumentNullException(nameof(buffer));
            }

            return new(buffer.ReadString());
        }

        public DisconnectPacket(string reason) : base(DisconnectPacketId)
        {
            Reason = reason;
        }

        protected override void WriteData(MinecraftProtocolDataStream buffer)
        {
            if (buffer == null)
            {
                throw new System.ArgumentNullException(nameof(buffer));
            }

            buffer.WriteString(Reason);
        }

    }

    public sealed class EncryptionRequestPacket : ClientboundLoginPacket
    {
        /// <summary>
        /// TODO: Add description.
        /// </summary>
        /// <exception cref="UnexpectedDataException">TODO: Why it's thrown.</exception>
        public static EncryptionRequestPacket Read(MinecraftProtocolDataStream buffer)
        {
            if (buffer == null)
            {
                throw new System.ArgumentNullException(nameof(buffer));
            }

            throw new System.NotImplementedException();
        }

        private EncryptionRequestPacket() : base(EncryptionRequestPacketId)
        {
            throw new System.NotImplementedException();
        }

        protected override void WriteData(MinecraftProtocolDataStream buffer)
        {
            if (buffer == null)
            {
                throw new System.ArgumentNullException(nameof(buffer));
            }

            throw new System.NotImplementedException();
        }

    }

    public sealed class LoginSuccessPacket : ClientboundLoginPacket
    {
        public readonly System.Guid UserId;
        public readonly string Username;

        /// <summary>
        /// TODO: Add description.
        /// </summary>
        /// <exception cref="UnexpectedDataException">TODO: Why it's thrown.</exception>
        public static LoginSuccessPacket Read(MinecraftProtocolDataStream buffer)
        {
            if (buffer == null)
            {
                throw new System.ArgumentNullException(nameof(buffer));
            }

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

        protected override void WriteData(MinecraftProtocolDataStream buffer)
        {
            if (buffer == null)
            {
                throw new System.ArgumentNullException(nameof(buffer));
            }

            buffer.WriteString(UserId.ToString());
            buffer.WriteString(Username);
        }

    }

    public sealed class SetCompressionPacket : ClientboundLoginPacket
    {
        public readonly int Threshold;

        /// <summary>
        /// TODO: Add description.
        /// </summary>
        /// <exception cref="UnexpectedDataException">TODO: Why it's thrown.</exception>
        public static SetCompressionPacket Read(MinecraftProtocolDataStream buffer)
        {
            if (buffer == null)
            {
                throw new System.ArgumentNullException(nameof(buffer));
            }

            return new(buffer.ReadInt(true));
        }

        public SetCompressionPacket(int threshold)
            : base(SetCompressionPacketId)
        {
            Threshold = threshold;
        }

        protected override void WriteData(MinecraftProtocolDataStream buffer)
        {
            if (buffer == null)
            {
                throw new System.ArgumentNullException(nameof(buffer));
            }

            buffer.WriteInt(Threshold, true);
        }

    }

    public sealed class StartLoginPacket : ServerboundLoginPacket
    {
        public readonly string Username;

        /// <summary>
        /// TODO: Add description.
        /// </summary>
        /// <exception cref="UnexpectedDataException">TODO: Why it's thrown.</exception>
        public static StartLoginPacket Read(MinecraftProtocolDataStream buffer)
        {
            if (buffer == null)
            {
                throw new System.ArgumentNullException(nameof(buffer));
            }

            // TODO: Check the conditions of variables. If not correct, throw exception.
            return new(buffer.ReadString());
        }

        public StartLoginPacket(string username) : base(StartLoginPacketId)
        {
            // TODO: Assert variables.
            Username = username;
        }

        protected override void WriteData(MinecraftProtocolDataStream buffer)
        {
            if (buffer == null)
            {
                throw new System.ArgumentNullException(nameof(buffer));
            }

            buffer.WriteString(Username);
        }
    }

    public sealed class EncryptionResponsePacket : ServerboundLoginPacket
    {
        /// <summary>
        /// TODO: Add description.
        /// </summary>
        /// <exception cref="UnexpectedDataException">TODO: Why it's thrown.</exception>
        public static EncryptionResponsePacket Read(MinecraftProtocolDataStream buffer)
        {
            if (buffer == null)
            {
                throw new System.ArgumentNullException(nameof(buffer));
            }

            // TODO: Check the conditions of variables. If not correct, throw exception.
            throw new System.NotImplementedException();
        }

        public EncryptionResponsePacket() : base(EncryptionResponsePacketId)
        {
            // TODO: Assert variables.
            throw new System.NotImplementedException();
        }

        protected override void WriteData(MinecraftProtocolDataStream buffer)
        {
            if (buffer == null)
            {
                throw new System.ArgumentNullException(nameof(buffer));
            }

            throw new System.NotImplementedException();
        }
    }

    public sealed class SpawnEntityPacket : ClientboundPlayingPacket
    {
        public readonly int EntityId;
        public readonly System.Guid UniqueId;
        public readonly sbyte Type;
        public readonly double X, Y, Z;
        public readonly byte Yaw, Pitch;
        public readonly int Data;
        public readonly short VelocityX, VelocityY, VelocityZ;

        public static SpawnEntityPacket Read(MinecraftProtocolDataStream buffer)
        {
            if (buffer == null)
            {
                throw new System.ArgumentNullException(nameof(buffer));
            }

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

        protected override void WriteData(MinecraftProtocolDataStream buffer)
        {
            if (buffer == null)
            {
                throw new System.ArgumentNullException(nameof(buffer));
            }

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

    public sealed class SpawnNamedEntityPacket : ClientboundPlayingPacket
    {
        public readonly int EntityId;
        public readonly System.Guid UniqueId;
        public readonly double X, Y, Z;
        public readonly byte Yaw, Pitch;
        public readonly byte[] Data;

        public static SpawnNamedEntityPacket Read(MinecraftProtocolDataStream buffer)
        {
            if (buffer == null)
            {
                throw new System.ArgumentNullException(nameof(buffer));
            }

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

        protected override void WriteData(MinecraftProtocolDataStream buffer)
        {
            if (buffer == null)
            {
                throw new System.ArgumentNullException(nameof(buffer));
            }

            buffer.WriteInt(EntityId, true);
            buffer.WriteGuid(UniqueId);
            buffer.WriteDouble(X); buffer.WriteDouble(Y); buffer.WriteDouble(Z);
            buffer.WriteByte(Yaw); buffer.WriteByte(Pitch);
            buffer.WriteData(Data);
        }

    }

    public sealed class ClientboundAnimationPacket : ClientboundPlayingPacket
    {
        public readonly int EntityId;

        /**
         * 0: Swing main arm
         * 1: Take damage
         * 2: Leave bed
         * 3: Swing offhand
         * 4: Critical effect
         * 5: Magic critical effect
         */
        public readonly byte Data;

        public static ClientboundAnimationPacket Read(MinecraftProtocolDataStream buffer)
        {
            if (buffer == null)
            {
                throw new System.ArgumentNullException(nameof(buffer));
            }

            throw new System.NotImplementedException();
        }

        public ClientboundAnimationPacket(
            int entityId,
            byte data) : base(ClientboundAnimationPacketId)
        {
            EntityId = entityId;
            Data = data;
        }

        protected override void WriteData(MinecraftProtocolDataStream buffer)
        {
            if (buffer == null)
            {
                throw new System.ArgumentNullException(nameof(buffer));
            }

            buffer.WriteInt(EntityId, true);
            buffer.WriteByte(Data);
        }

    }

    public sealed class BlockChangePacket : ClientboundPlayingPacket
    {
        public readonly int X, Y, Z;
        public readonly int BlockId;

        public static BlockChangePacket Read(MinecraftProtocolDataStream buffer)
        {
            if (buffer == null)
            {
                throw new System.ArgumentNullException(nameof(buffer));
            }

            throw new System.NotImplementedException();
        }

        public BlockChangePacket(
            int x, int y, int z,
            int blockId)
            : base(BlockChangePacketId)
        {
            X = x; Y = y; Z = z;
            BlockId = blockId;
        }

        protected override void WriteData(MinecraftProtocolDataStream buffer)
        {
            if (buffer == null)
            {
                throw new System.ArgumentNullException(nameof(buffer));
            }

            buffer.WritePosition(X, Y, Z);
            buffer.WriteInt(BlockId, true);
        }

    }

    public sealed class OpenBossBarPacket : ClientboundPlayingPacket
    {
        public readonly System.Guid UniqueId;
        public readonly string Title;
        public readonly float Health;
        public readonly int Color;
        public readonly int Division;
        public readonly byte Flags;

        public static OpenBossBarPacket Read(MinecraftProtocolDataStream buffer)
        {
            if (buffer == null)
            {
                throw new System.ArgumentNullException(nameof(buffer));
            }

            throw new System.NotImplementedException();
        }

        public OpenBossBarPacket(
            System.Guid id,
            string title,
            float health,
            int color, int division,
            byte flags)
            : base(BossBarPacketId)
        {
            UniqueId = id;
            Title = title;
            Health = health;
            Color = color;
            Division = division;
            Flags = flags;

        }

        protected override void WriteData(MinecraftProtocolDataStream buffer)
        {
            if (buffer == null)
            {
                throw new System.ArgumentNullException(nameof(buffer));
            }

            buffer.WriteGuid(UniqueId);
            buffer.WriteInt(0, true);
            buffer.WriteString(Title);
            buffer.WriteFloat(Health);
            buffer.WriteInt(Color, true);
            buffer.WriteInt(Division, true);
            buffer.WriteByte(Flags);
        }
    }

    public sealed class CloseBossBarPacket : ClientboundPlayingPacket
    {
        public readonly System.Guid UniqueId;

        public static CloseBossBarPacket Read(MinecraftProtocolDataStream buffer)
        {
            if (buffer == null)
            {
                throw new System.ArgumentNullException(nameof(buffer));
            }

            throw new System.NotImplementedException();
        }

        public CloseBossBarPacket(System.Guid id) : base(BossBarPacketId)
        {
            UniqueId = id;
        }

        protected override void WriteData(MinecraftProtocolDataStream buffer)
        {
            if (buffer == null)
            {
                throw new System.ArgumentNullException(nameof(buffer));
            }

            buffer.WriteGuid(UniqueId);
            buffer.WriteInt(1, true);
        }
    }

    public sealed class UpdateBossBarHealthPacket : ClientboundPlayingPacket
    {
        public readonly System.Guid UniqueId;
        public readonly float Health;

        public static UpdateBossBarHealthPacket Read(MinecraftProtocolDataStream buffer)
        {
            if (buffer == null)
            {
                throw new System.ArgumentNullException(nameof(buffer));
            }

            throw new System.NotImplementedException();
        }

        public UpdateBossBarHealthPacket(
            System.Guid id,
            float health)
            : base(BossBarPacketId)
        {
            UniqueId = id;
            Health = health;
        }

        protected override void WriteData(MinecraftProtocolDataStream buffer)
        {
            if (buffer == null)
            {
                throw new System.ArgumentNullException(nameof(buffer));
            }

            buffer.WriteGuid(UniqueId);
            buffer.WriteInt(2, true);
            buffer.WriteFloat(Health);
        }
    }

    public sealed class UpdateBossBarTitlePacket : ClientboundPlayingPacket
    {
        public readonly System.Guid UniqueId;
        public readonly string Title;

        public static UpdateBossBarTitlePacket Read(MinecraftProtocolDataStream buffer)
        {
            if (buffer == null)
            {
                throw new System.ArgumentNullException(nameof(buffer));
            }

            throw new System.NotImplementedException();
        }

        public UpdateBossBarTitlePacket(
            System.Guid id,
            string title)
            : base(BossBarPacketId)
        {
            UniqueId = id;
            Title = title;

        }

        protected override void WriteData(MinecraftProtocolDataStream buffer)
        {
            if (buffer == null)
            {
                throw new System.ArgumentNullException(nameof(buffer));
            }

            buffer.WriteGuid(UniqueId);
            buffer.WriteInt(3, true);
            buffer.WriteString(Title);
        }
    }

    public sealed class UpdateBossBarStylePacket : ClientboundPlayingPacket
    {
        public readonly System.Guid UniqueId;
        public readonly int Color;
        public readonly int Division;

        public static UpdateBossBarStylePacket Read(MinecraftProtocolDataStream buffer)
        {
            if (buffer == null)
            {
                throw new System.ArgumentNullException(nameof(buffer));
            }

            throw new System.NotImplementedException();
        }

        public UpdateBossBarStylePacket(
            System.Guid id,
            int color, int division)
            : base(BossBarPacketId)
        {
            UniqueId = id;
            Color = color;
            Division = division;

        }

        protected override void WriteData(MinecraftProtocolDataStream buffer)
        {
            if (buffer == null)
            {
                throw new System.ArgumentNullException(nameof(buffer));
            }

            buffer.WriteGuid(UniqueId);
            buffer.WriteInt(4, true);
            buffer.WriteInt(Color, true);
            buffer.WriteInt(Division, true);
        }
    }

    public sealed class UpdateBossBarFlagsPacket : ClientboundPlayingPacket
    {
        public readonly System.Guid UniqueId;
        public readonly byte Flags;

        public static UpdateBossBarFlagsPacket Read(MinecraftProtocolDataStream buffer)
        {
            if (buffer == null)
            {
                throw new System.ArgumentNullException(nameof(buffer));
            }

            throw new System.NotImplementedException();
        }

        public UpdateBossBarFlagsPacket(
            System.Guid id,
            byte flags)
            : base(BossBarPacketId)
        {
            UniqueId = id;
            Flags = flags;

        }

        protected override void WriteData(MinecraftProtocolDataStream buffer)
        {
            if (buffer == null)
            {
                throw new System.ArgumentNullException(nameof(buffer));
            }

            buffer.WriteGuid(UniqueId);
            buffer.WriteInt(5, true);
            buffer.WriteByte(Flags);
        }
    }

    public sealed class ClientboundChatmessagePacket : ClientboundPlayingPacket
    {
        public readonly string Data;
        public readonly byte Position;

        public static ClientboundChatmessagePacket Read(MinecraftProtocolDataStream buffer)
        {
            if (buffer == null)
            {
                throw new System.ArgumentNullException(nameof(buffer));
            }

            throw new System.NotImplementedException();
        }

        public ClientboundChatmessagePacket(
            string data,
            byte position)
            : base(ClientboundChatmessagePacketId)
        {
            Data = data;
            Position = position;
        }

        protected override void WriteData(MinecraftProtocolDataStream buffer)
        {
            if (buffer == null)
            {
                throw new System.ArgumentNullException(nameof(buffer));
            }

            buffer.WriteString(Data);
            buffer.WriteByte(Position);
        }

    }

    public sealed class ClientboundConfirmTransactionPacket : ClientboundPlayingPacket
    {
        public readonly sbyte WindowId;
        public readonly short ActionNumber;
        public readonly bool Accepted;

        public static ClientboundConfirmTransactionPacket Read(MinecraftProtocolDataStream buffer)
        {
            if (buffer == null)
            {
                throw new System.ArgumentNullException(nameof(buffer));
            }

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

        protected override void WriteData(MinecraftProtocolDataStream buffer)
        {
            if (buffer == null)
            {
                throw new System.ArgumentNullException(nameof(buffer));
            }

            buffer.WriteSbyte(WindowId);
            buffer.WriteShort(ActionNumber);
            buffer.WriteBool(Accepted);
        }

    }

    public sealed class ClientboundCloseWindowPacket : ClientboundPlayingPacket
    {
        public readonly byte WindowId;

        public static ClientboundCloseWindowPacket Read(MinecraftProtocolDataStream buffer)
        {
            if (buffer == null)
            {
                throw new System.ArgumentNullException(nameof(buffer));
            }

            throw new System.NotImplementedException();
        }

        public ClientboundCloseWindowPacket(byte windowId)
            : base(ClientboundCloseWindowPacketId)
        {
            WindowId = windowId;
        }

        protected override void WriteData(MinecraftProtocolDataStream buffer)
        {
            if (buffer == null)
            {
                throw new System.ArgumentNullException(nameof(buffer));
            }

            buffer.WriteByte(WindowId);
        }

    }

    public sealed class OpenWindowPacket : ClientboundPlayingPacket
    {
        public readonly byte WindowId;
        public readonly string WindowType;
        public readonly string WindowTitle;
        public readonly byte SlotCount;

        public static OpenWindowPacket Read(MinecraftProtocolDataStream buffer)
        {
            if (buffer == null)
            {
                throw new System.ArgumentNullException(nameof(buffer));
            }

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

        protected override void WriteData(MinecraftProtocolDataStream buffer)
        {
            if (buffer == null)
            {
                throw new System.ArgumentNullException(nameof(buffer));
            }

            System.Diagnostics.Debug.Assert(WindowId > 0);
            buffer.WriteByte(WindowId);
            buffer.WriteString(WindowType);
            buffer.WriteString("{\"text\":\"foo\"}");
            buffer.WriteByte(SlotCount);
        }

    }

    public sealed class WindowItemsPacket : ClientboundPlayingPacket
    {
        public readonly byte WindowId;
        public readonly int Count;
        public readonly byte[] Data;

        public static WindowItemsPacket Read(MinecraftProtocolDataStream buffer)
        {
            if (buffer == null)
            {
                throw new System.ArgumentNullException(nameof(buffer));
            }

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

        protected override void WriteData(MinecraftProtocolDataStream buffer)
        {
            if (buffer == null)
            {
                throw new System.ArgumentNullException(nameof(buffer));
            }

            if (buffer == null)
            {
                throw new System.ArgumentNullException(nameof(buffer));
            }

            buffer.WriteByte(WindowId);

            System.Diagnostics.Debug.Assert(Count >= short.MinValue);
            System.Diagnostics.Debug.Assert(Count <= short.MaxValue);
            buffer.WriteShort((short)Count);

            System.Diagnostics.Debug.Assert(Data != null);
            buffer.WriteData(Data);
        }

    }

    public sealed class SetSlotPacket : ClientboundPlayingPacket
    {
        public readonly sbyte WindowId;
        public readonly short SlotNumber;
        public readonly byte[] Data;

        public static SetSlotPacket Read(MinecraftProtocolDataStream buffer)
        {
            if (buffer == null)
            {
                throw new System.ArgumentNullException(nameof(buffer));
            }

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

        protected override void WriteData(MinecraftProtocolDataStream buffer)
        {
            if (buffer == null)
            {
                throw new System.ArgumentNullException(nameof(buffer));
            }

            buffer.WriteSbyte(WindowId);
            buffer.WriteShort(SlotNumber);
            buffer.WriteData(Data);
        }

    }

    public sealed class NamedSoundEffectPacket : ClientboundPlayingPacket
    {
        public readonly string SoundName;
        public readonly int SoundCategory;
        public readonly int EffectX, EffectY, EffectZ;
        public readonly float Volume, Pitch;

        public static NamedSoundEffectPacket Read(MinecraftProtocolDataStream buffer)
        {
            if (buffer == null)
            {
                throw new System.ArgumentNullException(nameof(buffer));
            }

            throw new System.NotImplementedException();
        }

        public NamedSoundEffectPacket(
            string soundName, int soundCategory,
            int effectX, int effectY, int effectZ,
            float volume, float pitch)
            : base(NamedSoundEffectPacketId)
        {
            SoundName = soundName;
            SoundCategory = soundCategory;
            EffectX = effectX; EffectY = effectY; EffectZ = effectZ;
            Volume = volume; Pitch = pitch;
        }

        protected override void WriteData(MinecraftProtocolDataStream buffer)
        {
            if (buffer == null)
            {
                throw new System.ArgumentNullException(nameof(buffer));
            }

            buffer.WriteString(SoundName);
            buffer.WriteInt(SoundCategory, true);
            buffer.WriteInt(EffectX);
            buffer.WriteInt(EffectY);
            buffer.WriteInt(EffectZ);
            buffer.WriteFloat(Volume);
            buffer.WriteFloat(Pitch);
        }

    }

    public sealed class EntityStatusPacket : ClientboundPlayingPacket
    {
        public readonly int EntityId;
        public readonly byte Status;

        public static EntityStatusPacket Read(MinecraftProtocolDataStream buffer)
        {
            if (buffer == null)
            {
                throw new System.ArgumentNullException(nameof(buffer));
            }

            throw new System.NotImplementedException();
        }

        public EntityStatusPacket(int idEntity, byte status) : base(EntityStatusPacketId)
        {
            EntityId = idEntity;
            Status = status;
        }

        protected override void WriteData(MinecraftProtocolDataStream buffer)
        {
            if (buffer == null)
            {
                throw new System.ArgumentNullException(nameof(buffer));
            }

            buffer.WriteInt(EntityId);
            buffer.WriteByte(Status);
        }
    }

    public sealed class UnloadChunkPacket : ClientboundPlayingPacket
    {
        public readonly int XChunk, ZChunk;

        /// <summary>
        /// TODO: Add description.
        /// </summary>
        /// <exception cref="UnexpectedDataException">TODO: Why it's thrown.</exception>
        public static UnloadChunkPacket Read(MinecraftProtocolDataStream buffer)
        {
            if (buffer == null)
            {
                throw new System.ArgumentNullException(nameof(buffer));
            }

            throw new System.NotImplementedException();
        }

        public UnloadChunkPacket(int xChunk, int zChunk) : base(UnloadChunkPacketId)
        {
            XChunk = xChunk;
            ZChunk = zChunk;
        }

        protected override void WriteData(MinecraftProtocolDataStream buffer)
        {
            if (buffer == null)
            {
                throw new System.ArgumentNullException(nameof(buffer));
            }

            buffer.WriteInt(XChunk);
            buffer.WriteInt(ZChunk);
        }

    }

    public sealed class GameStatePacket : ClientboundPlayingPacket
    {
        public readonly byte Reason;
        public readonly float Value;

        public static GameStatePacket Read(MinecraftProtocolDataStream buffer)
        {
            if (buffer == null)
            {
                throw new System.ArgumentNullException(nameof(buffer));
            }

            throw new System.NotImplementedException();
        }

        internal GameStatePacket(byte reason, float value) : base(GameStatePacketId)
        {
            Reason = reason;
            Value = value;
        }

        protected override void WriteData(MinecraftProtocolDataStream buffer)
        {
            if (buffer == null)
            {
                throw new System.ArgumentNullException(nameof(buffer));
            }

            buffer.WriteByte(Reason);
            buffer.WriteFloat(Value);
        }

    }

    public sealed class ClientboundKeepAlivePacket : ClientboundPlayingPacket
    {
        public readonly long Payload;

        public static ClientboundKeepAlivePacket Read(MinecraftProtocolDataStream buffer)
        {
            if (buffer == null)
            {
                throw new System.ArgumentNullException(nameof(buffer));
            }

            throw new System.NotImplementedException();
        }

        public ClientboundKeepAlivePacket(long payload) : base(ClientboundKeepAlivePacketId)
        {
            Payload = payload;
        }

        protected override void WriteData(MinecraftProtocolDataStream buffer)
        {
            if (buffer == null)
            {
                throw new System.ArgumentNullException(nameof(buffer));
            }

            buffer.WriteLong(Payload);
        }
    }

    public sealed class LoadChunkPacket : ClientboundPlayingPacket
    {
        public readonly int XChunk, ZChunk;
        public readonly bool Continuous;
        public readonly int Mask;
        public readonly byte[] Data;

        /// <summary>
        /// TODO: Add description.
        /// </summary>
        /// <exception cref="UnexpectedDataException">TODO: Why it's thrown.</exception>
        public static LoadChunkPacket Read(MinecraftProtocolDataStream buffer)
        {
            if (buffer == null)
            {
                throw new System.ArgumentNullException(nameof(buffer));
            }

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

        protected override void WriteData(MinecraftProtocolDataStream buffer)
        {
            if (buffer == null)
            {
                throw new System.ArgumentNullException(nameof(buffer));
            }

            buffer.WriteInt(XChunk);
            buffer.WriteInt(ZChunk);
            buffer.WriteBool(Continuous);
            buffer.WriteInt(Mask, true);
            buffer.WriteInt(Data.Length, true);
            buffer.WriteData(Data);
            buffer.WriteInt(0, true);  // TODO: Block entities
        }

    }

    public sealed class ParticlesPacket : ClientboundPlayingPacket
    {
        public readonly int ParticleId;
        public readonly bool Flag;  // ExtendedRange?
        public readonly float X, Y, Z;
        public readonly float OffsetX, OffsetY, OffsetZ;
        public readonly float Extra;
        public readonly int Count;

        public static ParticlesPacket Read(MinecraftProtocolDataStream buffer)
        {
            if (buffer == null)
            {
                throw new System.ArgumentNullException(nameof(buffer));
            }

            throw new System.NotImplementedException();
        }

        public ParticlesPacket(
            int particleId, bool extendedRange,
            float x, float y, float z,
            float offsetX, float offsetY, float offsetZ,
            float speed, int count) : base(ParticlesPacketId)
        {
            ParticleId = particleId;
            Flag = extendedRange;
            X = x; Y = y; Z = z;
            OffsetX = offsetX; OffsetY = offsetY; OffsetZ = offsetZ;
            Extra = speed;
            Count = count;
        }

        protected override void WriteData(MinecraftProtocolDataStream buffer)
        {
            if (buffer == null)
            {
                throw new System.ArgumentNullException(nameof(buffer));
            }

            buffer.WriteInt(ParticleId);
            buffer.WriteBool(Flag);
            buffer.WriteFloat(X); buffer.WriteFloat(Y); buffer.WriteFloat(Z);
            buffer.WriteFloat(OffsetX); buffer.WriteFloat(OffsetY); buffer.WriteFloat(OffsetZ);
            buffer.WriteFloat(Extra);
            buffer.WriteInt(Count);
        }

    }

    public sealed class JoinGamePacket : ClientboundPlayingPacket
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
        public static JoinGamePacket Read(MinecraftProtocolDataStream buffer)
        {
            if (buffer == null)
            {
                throw new System.ArgumentNullException(nameof(buffer));
            }

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
        protected override void WriteData(MinecraftProtocolDataStream buffer)
        {
            if (buffer == null)
            {
                throw new System.ArgumentNullException(nameof(buffer));
            }

            buffer.WriteInt(_entityId);
            buffer.WriteByte(_gamemode);
            buffer.WriteInt(_dimension);
            buffer.WriteByte(_difficulty);
            buffer.WriteByte(100);
            buffer.WriteString(_levelType);
            buffer.WriteBool(_reducedDebugInfo);
        }
    }

    public sealed class EntityPacket : ClientboundPlayingPacket
    {
        public readonly int EntityId;

        public static EntityPacket Read(MinecraftProtocolDataStream buffer)
        {
            if (buffer == null)
            {
                throw new System.ArgumentNullException(nameof(buffer));
            }

            throw new System.NotImplementedException();
        }

        public EntityPacket(int entityId) : base(EntityPacketId)
        {
            EntityId = entityId;
        }

        protected override void WriteData(MinecraftProtocolDataStream buffer)
        {
            if (buffer == null)
            {
                throw new System.ArgumentNullException(nameof(buffer));
            }

            buffer.WriteInt(EntityId, true);
        }

    }

    public sealed class EntityRelMovePacket : ClientboundPlayingPacket
    {
        public readonly int EntityId;
        public readonly short DeltaX, DeltaY, DeltaZ;
        public readonly bool OnGround;

        public static EntityRelMovePacket Read(MinecraftProtocolDataStream buffer)
        {
            if (buffer == null)
            {
                throw new System.ArgumentNullException(nameof(buffer));
            }

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

        protected override void WriteData(MinecraftProtocolDataStream buffer)
        {
            if (buffer == null)
            {
                throw new System.ArgumentNullException(nameof(buffer));
            }

            buffer.WriteInt(EntityId, true);
            buffer.WriteShort(DeltaX); buffer.WriteShort(DeltaY); buffer.WriteShort(DeltaZ);
            buffer.WriteBool(OnGround);
        }

    }

    public sealed class EntityRelMoveLookPacket : ClientboundPlayingPacket
    {
        public readonly int EntityId;
        public readonly short DeltaX, DeltaY, DeltaZ;
        public readonly byte Yaw, Pitch;
        public readonly bool OnGround;

        public static EntityRelMoveLookPacket Read(MinecraftProtocolDataStream buffer)
        {
            if (buffer == null)
            {
                throw new System.ArgumentNullException(nameof(buffer));
            }

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

        protected override void WriteData(MinecraftProtocolDataStream buffer)
        {
            if (buffer == null)
            {
                throw new System.ArgumentNullException(nameof(buffer));
            }

            buffer.WriteInt(EntityId, true);
            buffer.WriteShort(DeltaX); buffer.WriteShort(DeltaY); buffer.WriteShort(DeltaZ);
            buffer.WriteByte(Yaw); buffer.WriteByte(Pitch);
            buffer.WriteBool(OnGround);
        }

    }

    public sealed class EntityLookPacket : ClientboundPlayingPacket
    {
        public readonly int EntityId;
        public readonly byte Yaw, Pitch;
        public readonly bool OnGround;

        public static EntityLookPacket Read(MinecraftProtocolDataStream buffer)
        {
            if (buffer == null)
            {
                throw new System.ArgumentNullException(nameof(buffer));
            }

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

        protected override void WriteData(MinecraftProtocolDataStream buffer)
        {
            if (buffer == null)
            {
                throw new System.ArgumentNullException(nameof(buffer));
            }


            buffer.WriteInt(EntityId, true);
            buffer.WriteByte(Yaw); buffer.WriteByte(Pitch);
            buffer.WriteBool(OnGround);
        }

    }

    public sealed class AbilitiesPacket : ClientboundPlayingPacket
    {
        private readonly byte _flags;
        private readonly float _flyingSpeed;
        private readonly float _fovModifier;  // fov * _fovModifier;

        /// <summary>
        /// TODO: Add description.
        /// </summary>
        /// <exception cref="UnexpectedDataException">TODO: Why it's thrown.</exception>
        public static AbilitiesPacket Read(MinecraftProtocolDataStream buffer)
        {
            if (buffer == null)
            {
                throw new System.ArgumentNullException(nameof(buffer));
            }

            // TODO: Check the conditions of variables. If not correct, throw exception.
            throw new System.NotImplementedException();
        }

        public AbilitiesPacket(
            bool invulnerable, bool flying, bool canFly, bool canInstantlyBuild,
            float flyingSpeed, float fovModifier) : base(AbilitiesId)
        {
            // TODO: Assert variables.

            byte flags = 0;
            if (invulnerable)
            {
                flags |= 0x01;
            }
            if (flying)
            {
                flags |= 0x02;
            }
            if (canFly)
            {
                flags |= 0x04;
            }
            if (canInstantlyBuild)
            {
                flags |= 0x08;
            }

            _flags = flags;
            _flyingSpeed = flyingSpeed;
            _fovModifier = fovModifier;
        }

        protected override void WriteData(MinecraftProtocolDataStream buffer)
        {
            if (buffer == null)
            {
                throw new System.ArgumentNullException(nameof(buffer));
            }

            buffer.WriteByte(_flags);
            buffer.WriteFloat(_flyingSpeed);
            buffer.WriteFloat(_fovModifier);
        }

    }

    public sealed class PlayerListItemAddPacket : ClientboundPlayingPacket
    {
        public readonly System.Guid UniqueId;
        public readonly string Username;
        public readonly (string Name, string Value, string Signature)[] Properties;
        public readonly int Ping;

        public static PlayerListItemAddPacket Read(MinecraftProtocolDataStream buffer)
        {
            if (buffer == null)
            {
                throw new System.ArgumentNullException(nameof(buffer));
            }

            throw new System.NotImplementedException();
        }

        public PlayerListItemAddPacket(
            System.Guid uniqueId, string username,
            (string Name, string Value, string Signature)[] properties,
            int ms)
            : base(PlayerListItemAddPacketId)
        {
            UniqueId = uniqueId;
            Username = username;
            Properties = properties != null ? properties : [];
            Ping = ms;
        }

        protected override void WriteData(MinecraftProtocolDataStream buffer)
        {
            if (buffer == null)
            {
                throw new System.ArgumentNullException(nameof(buffer));
            }

            buffer.WriteInt(0, true);
            buffer.WriteInt(1, true);
            buffer.WriteGuid(UniqueId);
            buffer.WriteString(Username);
            buffer.WriteInt(Properties.Length, true);
            foreach ((string Name, string Value, string Signature) in Properties)
            {
                buffer.WriteString(Name);
                buffer.WriteString(Value);
                buffer.WriteBool(Signature != null);
                if (Signature != null)
                {
                    buffer.WriteString(Signature);
                }
            }
            buffer.WriteInt(2, true);  // gamemode
            buffer.WriteInt(Ping, true);  // latency
            buffer.WriteBool(false);
        }

    }

    public sealed class PlayerListItemUpdateLatencyPacket : ClientboundPlayingPacket
    {
        public readonly System.Guid UniqueId;
        public readonly int Laytency;

        public static PlayerListItemUpdateLatencyPacket Read(MinecraftProtocolDataStream buffer)
        {
            if (buffer == null)
            {
                throw new System.ArgumentNullException(nameof(buffer));
            }

            throw new System.NotImplementedException();
        }

        public PlayerListItemUpdateLatencyPacket(System.Guid uniqueId, int laytency)
            : base(PlayerListItemUpdateLatencyPacketId)
        {
            UniqueId = uniqueId;
            Laytency = laytency;
        }

        protected override void WriteData(MinecraftProtocolDataStream buffer)
        {
            if (buffer == null)
            {
                throw new System.ArgumentNullException(nameof(buffer));
            }

            buffer.WriteInt(2, true);
            buffer.WriteInt(1, true);
            buffer.WriteGuid(UniqueId);
            buffer.WriteInt(Laytency, true);
        }

    }

    public sealed class PlayerListItemRemovePacket : ClientboundPlayingPacket
    {
        public readonly System.Guid UniqueId;

        public static PlayerListItemRemovePacket Read(MinecraftProtocolDataStream buffer)
        {
            if (buffer == null)
            {
                throw new System.ArgumentNullException(nameof(buffer));
            }

            throw new System.NotImplementedException();
        }

        public PlayerListItemRemovePacket(System.Guid uniqueId)
            : base(PlayerListItemRemovePacketId)
        {
            UniqueId = uniqueId;
        }

        protected override void WriteData(MinecraftProtocolDataStream buffer)
        {
            if (buffer == null)
            {
                throw new System.ArgumentNullException(nameof(buffer));
            }

            buffer.WriteInt(4, true);
            buffer.WriteInt(1, true);
            buffer.WriteGuid(UniqueId);
        }
    }

    public sealed class TeleportPacket : ClientboundPlayingPacket
    {
        public readonly double X, Y, Z;
        public readonly float Yaw, Pitch;
        private readonly byte _flags;
        public readonly int Payload;

        /// <summary>
        /// TODO: Add description.
        /// </summary>
        /// <exception cref="UnexpectedDataException">TODO: Why it's thrown.</exception>
        public static TeleportPacket Read(MinecraftProtocolDataStream buffer)
        {
            if (buffer == null)
            {
                throw new System.ArgumentNullException(nameof(buffer));
            }

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

        protected override void WriteData(MinecraftProtocolDataStream buffer)
        {
            if (buffer == null)
            {
                throw new System.ArgumentNullException(nameof(buffer));
            }

            buffer.WriteDouble(X);
            buffer.WriteDouble(Y);
            buffer.WriteDouble(Z);
            buffer.WriteFloat(Yaw);
            buffer.WriteFloat(Pitch);
            buffer.WriteByte(_flags);
            buffer.WriteInt(Payload, true);

        }

    }

    public sealed class DestroyEntitiesPacket : ClientboundPlayingPacket
    {
        public readonly int[] EntityIds;

        public static DestroyEntitiesPacket Read(MinecraftProtocolDataStream buffer)
        {
            if (buffer == null)
            {
                throw new System.ArgumentNullException(nameof(buffer));
            }

            throw new System.NotImplementedException();
        }

        public DestroyEntitiesPacket(params int[] entityIds) : base(DestroyEntitiesPacketId)
        {
            EntityIds = entityIds;  // TODO: Copy
        }

        protected override void WriteData(MinecraftProtocolDataStream buffer)
        {
            if (buffer == null)
            {
                throw new System.ArgumentNullException(nameof(buffer));
            }

            buffer.WriteInt(EntityIds.Length, true);
            foreach (int id in EntityIds)
                buffer.WriteInt(id, true);
        }

    }

    public sealed class RemoveEntityEffectPacket : ClientboundPlayingPacket
    {
        public readonly int EntityId;
        public readonly byte EffectId;

        public static RemoveEntityEffectPacket Read(MinecraftProtocolDataStream buffer)
        {
            if (buffer == null)
            {
                throw new System.ArgumentNullException(nameof(buffer));
            }

            throw new System.NotImplementedException();
        }

        public RemoveEntityEffectPacket(
            int entityId,
            byte effectId) : base(RemoveEntityEffectPacketId)
        {
            EntityId = entityId;
            EffectId = effectId;
        }

        protected override void WriteData(MinecraftProtocolDataStream buffer)
        {
            if (buffer == null)
            {
                throw new System.ArgumentNullException(nameof(buffer));
            }

            buffer.WriteInt(EntityId, true);
            buffer.WriteByte(EffectId);
        }

    }

    public sealed class RespawnPacket : ClientboundPlayingPacket
    {
        public readonly int Dimension;
        public readonly byte Difficulty;
        public readonly byte Gamemode;
        public readonly string LevelType;

        public static RespawnPacket Read(MinecraftProtocolDataStream buffer)
        {
            if (buffer == null)
            {
                throw new System.ArgumentNullException(nameof(buffer));
            }

            throw new System.NotImplementedException();
        }

        internal RespawnPacket(
            int dimension, byte difficulty, byte gamemode, string levelType)
            : base(RespawnPacketId)
        {
            Dimension = dimension;
            Difficulty = difficulty;
            Gamemode = gamemode;
            LevelType = levelType;
        }

        protected override void WriteData(MinecraftProtocolDataStream buffer)
        {
            if (buffer == null)
            {
                throw new System.ArgumentNullException(nameof(buffer));
            }

            buffer.WriteInt(Dimension);
            buffer.WriteByte(Difficulty);
            buffer.WriteByte(Gamemode);
            buffer.WriteString(LevelType);
        }

    }

    public sealed class EntityHeadLookPacket : ClientboundPlayingPacket
    {
        public readonly int EntityId;
        public readonly byte Yaw;

        public static EntityHeadLookPacket Read(MinecraftProtocolDataStream buffer)
        {
            if (buffer == null)
            {
                throw new System.ArgumentNullException(nameof(buffer));
            }

            throw new System.NotImplementedException();
        }

        public EntityHeadLookPacket(
            int entityId, byte yaw) : base(EntityHeadLookPacketId)
        {
            EntityId = entityId;
            Yaw = yaw;
        }

        protected override void WriteData(MinecraftProtocolDataStream buffer)
        {
            if (buffer == null)
            {
                throw new System.ArgumentNullException(nameof(buffer));
            }

            buffer.WriteInt(EntityId, true);
            buffer.WriteByte(Yaw);
        }

    }

    public sealed class WorldBorderSizePacket : ClientboundPlayingPacket
    {
        public readonly double Diameter;

        public static WorldBorderSizePacket Read(MinecraftProtocolDataStream buffer)
        {
            if (buffer == null)
            {
                throw new System.ArgumentNullException(nameof(buffer));
            }

            throw new System.NotImplementedException();
        }

        public WorldBorderSizePacket(double diameter) : base(WorldBorderPacketId)
        {
            Diameter = diameter;
        }

        protected override void WriteData(MinecraftProtocolDataStream buffer)
        {
            if (buffer == null)
            {
                throw new System.ArgumentNullException(nameof(buffer));
            }

            buffer.WriteInt(0, true);
            buffer.WriteDouble(Diameter);
        }

    }

    public sealed class WorldBorderLerpSizePacket : ClientboundPlayingPacket
    {
        public readonly double OldDiameter, NewDiameter;
        public readonly long Speed;

        public static WorldBorderLerpSizePacket Read(MinecraftProtocolDataStream buffer)
        {
            if (buffer == null)
            {
                throw new System.ArgumentNullException(nameof(buffer));
            }

            throw new System.NotImplementedException();
        }

        public WorldBorderLerpSizePacket(
            double oldDiameter, double newDiameter,
            long speed) : base(WorldBorderPacketId)
        {
            OldDiameter = oldDiameter; NewDiameter = newDiameter;
            Speed = speed;
        }

        protected override void WriteData(MinecraftProtocolDataStream buffer)
        {
            if (buffer == null)
            {
                throw new System.ArgumentNullException(nameof(buffer));
            }

            buffer.WriteInt(1, true);
            buffer.WriteDouble(OldDiameter); buffer.WriteDouble(NewDiameter);
            buffer.WriteLong(Speed, true);
        }

    }

    public sealed class WorldBorderCenterPacket : ClientboundPlayingPacket
    {
        public readonly double X, Z;

        public static WorldBorderCenterPacket Read(MinecraftProtocolDataStream buffer)
        {
            if (buffer == null)
            {
                throw new System.ArgumentNullException(nameof(buffer));
            }

            throw new System.NotImplementedException();
        }

        public WorldBorderCenterPacket(double x, double z) : base(WorldBorderPacketId)
        {
            X = x; Z = z;
        }

        protected override void WriteData(MinecraftProtocolDataStream buffer)
        {
            if (buffer == null)
            {
                throw new System.ArgumentNullException(nameof(buffer));
            }

            buffer.WriteInt(2, true);
            buffer.WriteDouble(X); buffer.WriteDouble(Z);
        }

    }

    public sealed class WorldBorderInitPacket : ClientboundPlayingPacket
    {
        public readonly double X, Z;
        public readonly double OldDiameter, NewDiameter;
        public readonly long Speed;
        public readonly int PortalTeleportBoundary = 29999984;
        public readonly int WarningTime = 0;
        public readonly int WarningBlocks = 0;

        public static WorldBorderInitPacket Read(MinecraftProtocolDataStream buffer)
        {
            if (buffer == null)
            {
                throw new System.ArgumentNullException(nameof(buffer));
            }

            throw new System.NotImplementedException();
        }

        public WorldBorderInitPacket(
            double x, double z,
            double oldDiameter, double newDiameter,
            long speed) : base(WorldBorderPacketId)
        {
            X = x; Z = z;
            OldDiameter = oldDiameter; NewDiameter = newDiameter;
            Speed = speed;
        }

        protected override void WriteData(MinecraftProtocolDataStream buffer)
        {
            if (buffer == null)
            {
                throw new System.ArgumentNullException(nameof(buffer));
            }

            buffer.WriteInt(3, true);
            buffer.WriteDouble(X); buffer.WriteDouble(Z);
            buffer.WriteDouble(OldDiameter); buffer.WriteDouble(NewDiameter);
            buffer.WriteLong(Speed, true);
            buffer.WriteInt(PortalTeleportBoundary, true);
            buffer.WriteInt(WarningTime, true);
            buffer.WriteInt(WarningBlocks, true);
        }

    }

    public sealed class EntityMetadataPacket : ClientboundPlayingPacket
    {
        public readonly int EntityId;
        public readonly byte[] Data;

        public static EntityMetadataPacket Read(MinecraftProtocolDataStream buffer)
        {
            if (buffer == null)
            {
                throw new System.ArgumentNullException(nameof(buffer));
            }

            throw new System.NotImplementedException();
        }

        public EntityMetadataPacket(
            int entityId, byte[] data) : base(EntityMetadataPacketId)
        {
            EntityId = entityId;
            Data = data;
        }

        protected override void WriteData(MinecraftProtocolDataStream buffer)
        {
            if (buffer == null)
            {
                throw new System.ArgumentNullException(nameof(buffer));
            }

            buffer.WriteInt(EntityId, true);
            buffer.WriteData(Data);
        }

    }

    public sealed class EntityVelocityPacket : ClientboundPlayingPacket
    {
        public readonly int EntityId;
        public readonly short X, Y, Z;

        public static EntityVelocityPacket Read(MinecraftProtocolDataStream buffer)
        {
            if (buffer == null)
            {
                throw new System.ArgumentNullException(nameof(buffer));
            }

            throw new System.NotImplementedException();
        }

        public EntityVelocityPacket(int entityId, short x, short y, short z) : base(EntityVelocityPacketId)
        {
            EntityId = entityId;
            X = x; Y = y; Z = z;
        }

        protected override void WriteData(MinecraftProtocolDataStream buffer)
        {
            if (buffer == null)
            {
                throw new System.ArgumentNullException(nameof(buffer));
            }

            buffer.WriteInt(EntityId, true);
            buffer.WriteShort(X); buffer.WriteShort(Y); buffer.WriteShort(Z);
        }

    }

    public sealed class EntityEquipmentPacket : ClientboundPlayingPacket
    {
        public readonly int EntityId;
        public readonly int Slot;
        public readonly byte[] Data;

        public static EntityEquipmentPacket Read(MinecraftProtocolDataStream buffer)
        {
            if (buffer == null)
            {
                throw new System.ArgumentNullException(nameof(buffer));
            }

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

        protected override void WriteData(MinecraftProtocolDataStream buffer)
        {
            if (buffer == null)
            {
                throw new System.ArgumentNullException(nameof(buffer));
            }

            buffer.WriteInt(EntityId, true);
            buffer.WriteInt(Slot, true);
            buffer.WriteData(Data);
        }
    }

    public sealed class SetExperiencePacket : ClientboundPlayingPacket
    {
        public readonly float Ratio;
        public readonly int Level;
        public readonly int TotalExperience;

        public static SetExperiencePacket Read(MinecraftProtocolDataStream buffer)
        {
            if (buffer == null)
            {
                throw new System.ArgumentNullException(nameof(buffer));
            }

            throw new System.NotImplementedException();
        }

        public SetExperiencePacket(
            float ratio, int level, int totalExperience) : base(SetExperiencePacketId)
        {
            if (ratio < 0 || ratio > 1)
            {
                throw new System.ArgumentOutOfRangeException(nameof(ratio));
            }

            if (level < 0)
            {
                throw new System.ArgumentOutOfRangeException(nameof(level));
            }

            if (totalExperience < 0)
            {
                throw new System.ArgumentOutOfRangeException(nameof(level));
            }

            Ratio = ratio;
            Level = level;
            TotalExperience = totalExperience;
        }

        protected override void WriteData(MinecraftProtocolDataStream buffer)
        {
            if (buffer == null)
            {
                throw new System.ArgumentNullException(nameof(buffer));
            }

            buffer.WriteFloat(Ratio);
            buffer.WriteInt(Level, true);
            buffer.WriteInt(TotalExperience, true);
        }

    }

    public sealed class UpdateHealthPacket : ClientboundPlayingPacket
    {
        public readonly float Health;
        public readonly int Food;
        public readonly float FoodSaturation;

        public static UpdateHealthPacket Read(MinecraftProtocolDataStream buffer)
        {
            if (buffer == null)
            {
                throw new System.ArgumentNullException(nameof(buffer));
            }

            throw new System.NotImplementedException();
        }

        public UpdateHealthPacket(
            float health, int food, float foodSaturation) : base(UpdateHealthPacketId)
        {
            Health = health;
            Food = food;
            FoodSaturation = foodSaturation;
        }

        protected override void WriteData(MinecraftProtocolDataStream buffer)
        {
            if (buffer == null)
            {
                throw new System.ArgumentNullException(nameof(buffer));
            }

            buffer.WriteFloat(Health);
            buffer.WriteInt(Food, true);
            buffer.WriteFloat(FoodSaturation);
        }
    }

    public sealed class TimeUpdatePacket : ClientboundPlayingPacket
    {
        public readonly long WorldAge;

        // The world (or region) time, in ticks. If negative the sun will stop moving at the Math.abs of the time
        public readonly long TimeOfDay;

        public static TimeUpdatePacket Read(MinecraftProtocolDataStream buffer)
        {
            if (buffer == null)
            {
                throw new System.ArgumentNullException(nameof(buffer));
            }

            throw new System.NotImplementedException();
        }

        public TimeUpdatePacket(long worldAge, long timeOfDay) : base(TimeUpdatePacketId)
        {
            WorldAge = worldAge;
            TimeOfDay = timeOfDay;
        }

        protected override void WriteData(MinecraftProtocolDataStream buffer)
        {
            if (buffer == null)
            {
                throw new System.ArgumentNullException(nameof(buffer));
            }

            buffer.WriteLong(WorldAge);
            buffer.WriteLong(TimeOfDay);
        }
    }

    public sealed class SetTitlePacket : ClientboundPlayingPacket
    {
        public readonly string Data;

        public static SetTitlePacket Read(MinecraftProtocolDataStream buffer)
        {
            if (buffer == null)
            {
                throw new System.ArgumentNullException(nameof(buffer));
            }

            throw new System.NotImplementedException();
        }

        public SetTitlePacket(string data) : base(TitlePacketId)
        {
            Data = data;
        }

        protected override void WriteData(MinecraftProtocolDataStream buffer)
        {
            if (buffer == null)
            {
                throw new System.ArgumentNullException(nameof(buffer));
            }

            buffer.WriteInt(0, true);
            buffer.WriteString(Data);
        }
    }

    public sealed class SetSubtitlePacket : ClientboundPlayingPacket
    {
        public readonly string Data;

        public static SetSubtitlePacket Read(MinecraftProtocolDataStream buffer)
        {
            if (buffer == null)
            {
                throw new System.ArgumentNullException(nameof(buffer));
            }

            throw new System.NotImplementedException();
        }

        public SetSubtitlePacket(string data) : base(TitlePacketId)
        {
            Data = data;
        }

        protected override void WriteData(MinecraftProtocolDataStream buffer)
        {
            if (buffer == null)
            {
                throw new System.ArgumentNullException(nameof(buffer));
            }

            buffer.WriteInt(1, true);
            buffer.WriteString(Data);
        }
    }

    public sealed class SetActionBarPacket : ClientboundPlayingPacket
    {
        public readonly string Data;

        public static SetActionBarPacket Read(MinecraftProtocolDataStream buffer)
        {
            if (buffer == null)
            {
                throw new System.ArgumentNullException(nameof(buffer));
            }

            throw new System.NotImplementedException();
        }

        public SetActionBarPacket(string data) : base(TitlePacketId)
        {
            Data = data;
        }

        protected override void WriteData(MinecraftProtocolDataStream buffer)
        {
            if (buffer == null)
            {
                throw new System.ArgumentNullException(nameof(buffer));
            }

            buffer.WriteInt(2, true);
            buffer.WriteString(Data);
        }
    }

    public sealed class SetTimesAndDisplayTitlePacket : ClientboundPlayingPacket
    {
        public readonly int FadeIn;
        public readonly int Stay;
        public readonly int FadeOut;

        public static SetTimesAndDisplayTitlePacket Read(MinecraftProtocolDataStream buffer)
        {
            if (buffer == null)
            {
                throw new System.ArgumentNullException(nameof(buffer));
            }

            throw new System.NotImplementedException();
        }

        public SetTimesAndDisplayTitlePacket(int fadeIn, int stay, int fadeOut) : base(TitlePacketId)
        {
            FadeIn = fadeIn;
            Stay = stay;
            FadeOut = fadeOut;
        }

        protected override void WriteData(MinecraftProtocolDataStream buffer)
        {
            if (buffer == null)
            {
                throw new System.ArgumentNullException(nameof(buffer));
            }

            buffer.WriteInt(3, true);
            buffer.WriteInt(FadeIn);
            buffer.WriteInt(Stay);
            buffer.WriteInt(FadeOut);
        }
    }

    public sealed class HideTitlePacket : ClientboundPlayingPacket
    {

        public static HideTitlePacket Read(MinecraftProtocolDataStream buffer)
        {
            if (buffer == null)
            {
                throw new System.ArgumentNullException(nameof(buffer));
            }

            throw new System.NotImplementedException();
        }

        public HideTitlePacket() : base(TitlePacketId)
        {
        }

        protected override void WriteData(MinecraftProtocolDataStream buffer)
        {
            if (buffer == null)
            {
                throw new System.ArgumentNullException(nameof(buffer));
            }

            buffer.WriteInt(4, true);
        }
    }

    public sealed class ResetTitlePacket : ClientboundPlayingPacket
    {

        public static ResetTitlePacket Read(MinecraftProtocolDataStream buffer)
        {
            if (buffer == null)
            {
                throw new System.ArgumentNullException(nameof(buffer));
            }

            throw new System.NotImplementedException();
        }

        public ResetTitlePacket() : base(TitlePacketId)
        {
        }

        protected override void WriteData(MinecraftProtocolDataStream buffer)
        {
            if (buffer == null)
            {
                throw new System.ArgumentNullException(nameof(buffer));
            }

            buffer.WriteInt(5, true);
        }
    }

    public sealed class EntityTeleportPacket : ClientboundPlayingPacket
    {
        public readonly int EntityId;
        public readonly double X, Y, Z;
        public readonly byte Yaw, Pitch;
        public readonly bool OnGround;

        public static EntityTeleportPacket Read(MinecraftProtocolDataStream buffer)
        {
            if (buffer == null)
            {
                throw new System.ArgumentNullException(nameof(buffer));
            }

            throw new System.NotImplementedException();
        }

        public EntityTeleportPacket(
            int entityId,
            double x, double y, double z,
            byte yaw, byte pitch,
            bool onGround) : base(EntityTeleportPacketId)
        {
            EntityId = entityId;
            X = x; Y = y; Z = z;
            Yaw = yaw; Pitch = pitch;
            OnGround = onGround;
        }

        protected override void WriteData(MinecraftProtocolDataStream buffer)
        {
            if (buffer == null)
            {
                throw new System.ArgumentNullException(nameof(buffer));
            }

            buffer.WriteInt(EntityId, true);
            buffer.WriteDouble(X); buffer.WriteDouble(Y); buffer.WriteDouble(Z);
            buffer.WriteByte(Yaw); buffer.WriteByte(Pitch);
            buffer.WriteBool(OnGround);
        }

    }

    public sealed class EntityPropertiesPacket : ClientboundPlayingPacket
    {
        public readonly int EntityId;
        public readonly (string, double)[] Properties;

        public static EntityPropertiesPacket Read(MinecraftProtocolDataStream buffer)
        {
            if (buffer == null)
            {
                throw new System.ArgumentNullException(nameof(buffer));
            }

            throw new System.NotImplementedException();
        }

        public EntityPropertiesPacket(
            int entityId,
            (string, double)[] properties) : base(EntityPropertiesPacketId)
        {
            EntityId = entityId;
            Properties = properties;
        }

        protected override void WriteData(MinecraftProtocolDataStream buffer)
        {
            if (buffer == null)
            {
                throw new System.ArgumentNullException(nameof(buffer));
            }

            buffer.WriteInt(EntityId, true);
            buffer.WriteInt(Properties.Length);
            foreach ((string key, double value) in Properties)
            {
                buffer.WriteString(key);
                buffer.WriteDouble(value);
                buffer.WriteInt(0, true);
            }

        }

    }

    public sealed class EntityEffectPacket : ClientboundPlayingPacket
    {
        public readonly int EntityId;
        public readonly byte EffectId;
        public readonly byte Amplifier;
        public readonly int Duration;
        public readonly byte Flags;

        public static EntityEffectPacket Read(MinecraftProtocolDataStream buffer)
        {
            if (buffer == null)
            {
                throw new System.ArgumentNullException(nameof(buffer));
            }

            throw new System.NotImplementedException();
        }

        public EntityEffectPacket(
            int entityId,
            byte effectId,
            byte amplifier,
            int duration,
            byte flags) : base(EntityEffectPacketId)
        {
            EntityId = entityId;
            EffectId = effectId;
            Amplifier = amplifier;
            Duration = duration;
            Flags = flags;
        }

        protected override void WriteData(MinecraftProtocolDataStream buffer)
        {
            if (buffer == null)
            {
                throw new System.ArgumentNullException(nameof(buffer));
            }

            buffer.WriteInt(EntityId, true);
            buffer.WriteByte(EffectId);
            buffer.WriteByte(Amplifier);
            buffer.WriteInt(Duration, true);
            buffer.WriteByte(Flags);
        }

    }

    public sealed class TeleportAcceptPacket : ServerboundPlayingPacket
    {
        public readonly int Payload;

        /// <summary>
        /// TODO: Add description.
        /// </summary>
        /// <exception cref="UnexpectedDataException">TODO: Why it's thrown.</exception>
        public static TeleportAcceptPacket Read(MinecraftProtocolDataStream buffer)
        {
            if (buffer == null)
            {
                throw new System.ArgumentNullException(nameof(buffer));
            }

            return new(buffer.ReadInt(true));
        }

        public TeleportAcceptPacket(int payload) : base(TeleportAcceptPacketId)
        {
            Payload = payload;
        }

        protected override void WriteData(MinecraftProtocolDataStream buffer)
        {
            if (buffer == null)
            {
                throw new System.ArgumentNullException(nameof(buffer));
            }

            throw new System.NotImplementedException();
        }

    }

    public sealed class ServerboundChatMessagePacket : ServerboundPlayingPacket
    {
        public readonly string Text;

        /// <summary>
        /// TODO: Add description.
        /// </summary>
        /// <exception cref="UnexpectedDataException">TODO: Why it's thrown.</exception>
        public static ServerboundChatMessagePacket Read(MinecraftProtocolDataStream buffer)
        {
            if (buffer == null)
            {
                throw new System.ArgumentNullException(nameof(buffer));
            }

            return new(buffer.ReadString());
        }

        public ServerboundChatMessagePacket(string text) : base(ServerboundChatMessagePacketId)
        {
            Text = text;
        }

        protected override void WriteData(MinecraftProtocolDataStream buffer)
        {
            if (buffer == null)
            {
                throw new System.ArgumentNullException(nameof(buffer));
            }

            throw new System.NotImplementedException();
        }

    }

    public sealed class SettingsPacket : ServerboundPlayingPacket
    {
        public readonly byte RenderDistance;

        /// <summary>
        /// TODO: Add description.
        /// </summary>
        /// <exception cref="UnexpectedDataException">TODO: Why it's thrown.</exception>
        public static SettingsPacket Read(MinecraftProtocolDataStream buffer)
        {
            if (buffer == null)
            {
                throw new System.ArgumentNullException(nameof(buffer));
            }

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

        protected override void WriteData(MinecraftProtocolDataStream buffer)
        {
            if (buffer == null)
            {
                throw new System.ArgumentNullException(nameof(buffer));
            }

            throw new System.NotImplementedException();
        }
    }

    public sealed class ServerboundConfirmTransactionPacket : ServerboundPlayingPacket
    {
        public readonly sbyte WindowId;
        public readonly short ActionNumber;
        public readonly bool Accepted;

        public static ServerboundConfirmTransactionPacket Read(MinecraftProtocolDataStream buffer)
        {
            if (buffer == null)
            {
                throw new System.ArgumentNullException(nameof(buffer));
            }

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

        protected override void WriteData(MinecraftProtocolDataStream buffer)
        {
            if (buffer == null)
            {
                throw new System.ArgumentNullException(nameof(buffer));
            }

            throw new System.NotImplementedException();
        }

    }

    public sealed class ClickWindowPacket : ServerboundPlayingPacket
    {
        public readonly byte WindowId;
        public readonly short Slot;
        public readonly sbyte Button;
        public readonly short Action;
        public readonly int Mode;
        public readonly byte[] Data;

        public static ClickWindowPacket Read(MinecraftProtocolDataStream buffer)
        {
            if (buffer == null)
            {
                throw new System.ArgumentNullException(nameof(buffer));
            }

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

        protected override void WriteData(MinecraftProtocolDataStream buffer)
        {
            if (buffer == null)
            {
                throw new System.ArgumentNullException(nameof(buffer));
            }

            throw new System.NotImplementedException();
        }

    }

    public sealed class ServerboundCloseWindowPacket : ServerboundPlayingPacket
    {
        public readonly byte WindowId;

        public static ServerboundCloseWindowPacket Read(MinecraftProtocolDataStream buffer)
        {
            if (buffer == null)
            {
                throw new System.ArgumentNullException(nameof(buffer));
            }

            return new(buffer.ReadByte());
        }

        private ServerboundCloseWindowPacket(byte windowId)
            : base(ServerboundCloseWindowPacketId)
        {
            WindowId = windowId;
        }

        protected override void WriteData(MinecraftProtocolDataStream buffer)
        {
            if (buffer == null)
            {
                throw new System.ArgumentNullException(nameof(buffer));
            }

            throw new System.NotImplementedException();
        }

    }

    public sealed class UseEntityPacket : ServerboundPlayingPacket
    {
        public readonly int EntityId;
        public readonly int Type;
        public readonly int Hand;

        public static UseEntityPacket Read(MinecraftProtocolDataStream buffer)
        {
            if (buffer == null)
            {
                throw new System.ArgumentNullException(nameof(buffer));
            }

            int entityid = buffer.ReadInt(true);
            int type = buffer.ReadInt(true);
            if (type == 2)
            {
                buffer.ReadFloat();
                buffer.ReadFloat();
                buffer.ReadFloat();
            }

            int hand;
            if (type == 0 || type == 2)
            {
                hand = buffer.ReadInt(true);
            }
            else
            {
                hand = -1;
            }

            return new(entityid, type, hand);
        }

        internal UseEntityPacket(int target, int type, int hand) : base(UseEntityPacketId)
        {
            EntityId = target;
            Type = type;
            Hand = hand;
        }

        protected override void WriteData(MinecraftProtocolDataStream buffer)
        {
            if (buffer == null)
            {
                throw new System.ArgumentNullException(nameof(buffer));
            }

            throw new System.NotImplementedException();
        }
    }

    public sealed class ServerboundKeepAlivePacket : ServerboundPlayingPacket
    {
        public readonly long Payload;

        /// <summary>
        /// TODO: Add description.
        /// </summary>
        /// <exception cref="UnexpectedDataException">TODO: Why it's thrown.</exception>
        public static ServerboundKeepAlivePacket Read(MinecraftProtocolDataStream buffer)
        {
            if (buffer == null)
            {
                throw new System.ArgumentNullException(nameof(buffer));
            }

            return new(buffer.ReadLong());
        }

        private ServerboundKeepAlivePacket(long payload) : base(ServerboundKeepAlivePacketId)
        {
            Payload = payload;
        }

        protected override void WriteData(MinecraftProtocolDataStream buffer)
        {
            if (buffer == null)
            {
                throw new System.ArgumentNullException(nameof(buffer));
            }

            throw new System.NotImplementedException();
        }

    }

    public sealed class PlayerPacket : ServerboundPlayingPacket
    {
        public readonly bool OnGround;

        /// <summary>
        /// TODO: Add description.
        /// </summary>
        /// <exception cref="UnexpectedDataException">TODO: Why it's thrown.</exception>
        public static PlayerPacket Read(MinecraftProtocolDataStream buffer)
        {
            if (buffer == null)
            {
                throw new System.ArgumentNullException(nameof(buffer));
            }

            return new(buffer.ReadBool());
        }

        public PlayerPacket(bool onGround) : base(PlayerPacketId)
        {
            OnGround = onGround;
        }

        protected override void WriteData(MinecraftProtocolDataStream buffer)
        {
            if (buffer == null)
            {
                throw new System.ArgumentNullException(nameof(buffer));
            }

            throw new System.NotImplementedException();
        }

    }

    public sealed class PlayerPositionPacket : ServerboundPlayingPacket
    {
        public readonly double X, Y, Z;
        public readonly bool OnGround;

        /// <summary>
        /// TODO: Add description.
        /// </summary>
        /// <exception cref="UnexpectedDataException">TODO: Why it's thrown.</exception>
        public static PlayerPositionPacket Read(MinecraftProtocolDataStream buffer)
        {
            if (buffer == null)
            {
                throw new System.ArgumentNullException(nameof(buffer));
            }

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

        protected override void WriteData(MinecraftProtocolDataStream buffer)
        {
            if (buffer == null)
            {
                throw new System.ArgumentNullException(nameof(buffer));
            }

            throw new System.NotImplementedException();
        }

    }

    public sealed class PlayerPosAndLookPacket : ServerboundPlayingPacket
    {
        public readonly double X, Y, Z;
        public readonly float Yaw, Pitch;
        public readonly bool OnGround;

        /// <summary>
        /// TODO: Add description.
        /// </summary>
        /// <exception cref="UnexpectedDataException">TODO: Why it's thrown.</exception>
        public static PlayerPosAndLookPacket Read(MinecraftProtocolDataStream buffer)
        {
            if (buffer == null)
            {
                throw new System.ArgumentNullException(nameof(buffer));
            }

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

        protected override void WriteData(MinecraftProtocolDataStream buffer)
        {
            if (buffer == null)
            {
                throw new System.ArgumentNullException(nameof(buffer));
            }

            throw new System.NotImplementedException();
        }

    }

    public sealed class PlayerLookPacket : ServerboundPlayingPacket
    {
        public readonly float Yaw, Pitch;
        public readonly bool OnGround;

        /// <summary>
        /// TODO: Add description.
        /// </summary>
        /// <exception cref="UnexpectedDataException">TODO: Why it's thrown.</exception>
        public static PlayerLookPacket Read(MinecraftProtocolDataStream buffer)
        {
            if (buffer == null)
            {
                throw new System.ArgumentNullException(nameof(buffer));
            }

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

        protected override void WriteData(MinecraftProtocolDataStream buffer)
        {
            if (buffer == null)
            {
                throw new System.ArgumentNullException(nameof(buffer));
            }

            throw new System.NotImplementedException();
        }
    }

    public sealed class PlayerDigPacket : ServerboundPlayingPacket
    {
        public readonly int Status;
        // TODO: Make x as a 26-bit integer,
        // followed by z as a 26-bit integer,
        // followed by y as a 12-bit integer
        // (all signed, two's complement).
        public readonly byte[] Position;
        public readonly byte Face;

        public static PlayerDigPacket Read(MinecraftProtocolDataStream buffer)
        {
            if (buffer == null)
            {
                throw new System.ArgumentNullException(nameof(buffer));
            }

            return new(buffer.ReadInt(true), buffer.ReadData(8), buffer.ReadByte());
        }

        internal PlayerDigPacket(int status, byte[] p, byte face)
            : base(PlayerDigPacketId)
        {
            Status = status;
            Position = p;
            Face = face;
        }

        protected override void WriteData(MinecraftProtocolDataStream buffer)
        {
            if (buffer == null)
            {
                throw new System.ArgumentNullException(nameof(buffer));
            }

            throw new System.NotImplementedException();
        }
    }

    public sealed class EntityActionPacket : ServerboundPlayingPacket
    {
        public readonly int EntityId;
        public readonly int ActionId;
        public readonly int JumpBoost;

        public static EntityActionPacket Read(MinecraftProtocolDataStream buffer)
        {
            if (buffer == null)
            {
                throw new System.ArgumentNullException(nameof(buffer));
            }

            return new(buffer.ReadInt(true), buffer.ReadInt(true), buffer.ReadInt(true));
        }

        private EntityActionPacket(int entityId, int actionId, int jumpBoost)
            : base(EntityActionPacketId)
        {
            EntityId = entityId;
            ActionId = actionId;
            JumpBoost = jumpBoost;
        }

        protected override void WriteData(MinecraftProtocolDataStream buffer)
        {
            if (buffer == null)
            {
                throw new System.ArgumentNullException(nameof(buffer));
            }

            throw new System.NotImplementedException();
        }

    }

    public sealed class ServerboundHeldItemSlotPacket : ServerboundPlayingPacket
    {
        public readonly int Slot;

        public static ServerboundHeldItemSlotPacket Read(MinecraftProtocolDataStream buffer)
        {
            if (buffer == null)
            {
                throw new System.ArgumentNullException(nameof(buffer));
            }

            return new((int)buffer.ReadShort());
        }

        private ServerboundHeldItemSlotPacket(int slot) : base(ServerboundHeldItemSlotPacketId)
        {
            Slot = slot;
        }

        protected override void WriteData(MinecraftProtocolDataStream buffer)
        {
            if (buffer == null)
            {
                throw new System.ArgumentNullException(nameof(buffer));
            }

            throw new System.NotImplementedException();
        }
    }

    public sealed class ServerboundAnimationPacket : ServerboundPlayingPacket
    {
        public readonly int Hand;

        public static ServerboundAnimationPacket Read(MinecraftProtocolDataStream buffer)
        {
            if (buffer == null)
            {
                throw new System.ArgumentNullException(nameof(buffer));
            }

            return new(buffer.ReadInt(true));
        }

        internal ServerboundAnimationPacket(int hand) : base(ServerboundAnimationPacketId)
        {
            Hand = hand;
        }

        protected override void WriteData(MinecraftProtocolDataStream buffer)
        {
            if (buffer == null)
            {
                throw new System.ArgumentNullException(nameof(buffer));
            }

            throw new System.NotImplementedException();
        }
    }

    public sealed class UseItemPacket : ServerboundPlayingPacket
    {
        public readonly int Hand;

        public static UseItemPacket Read(MinecraftProtocolDataStream buffer)
        {
            if (buffer == null)
            {
                throw new System.ArgumentNullException(nameof(buffer));
            }

            return new(buffer.ReadInt(true));
        }

        private UseItemPacket(int hand) : base(UseItemPacketId)
        {
            Hand = hand;
        }

        protected override void WriteData(MinecraftProtocolDataStream buffer)
        {
            if (buffer == null)
            {
                throw new System.ArgumentNullException(nameof(buffer));
            }

            throw new System.NotImplementedException();
        }
    }

}

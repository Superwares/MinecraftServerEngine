
using System;
using System.Diagnostics;

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

    public Packet() { }

    public abstract void Write(Buffer buffer);

}

public abstract class HandshakingPacket : Packet
{
    public override States State { get { return States.Handshaking; } }

}

public abstract class StatusPacket : Packet
{
    public override States State { get { return States.Status; } }

}

public abstract class LoginPacket : Packet
{
    public override States State { get { return States.Login; } }

}

public abstract class ServerboundHandshakingPacket : HandshakingPacket
{
    public enum Ids
    {
        HandshakePacketId = 0x00,
    }

    protected readonly Ids _Id;
    public Ids Id { get { return _Id; } }
    public override WhereBound BoundTo { get { return WhereBound.Serverbound; } }

    protected ServerboundHandshakingPacket(Ids id)
    {
        _Id = id;
    }

}
public abstract class ClientboundStatusPacket : StatusPacket
{
    public enum Ids
    {
        ResponsePacketId = 0x00,
        PongPacketId = 0x01,
    }

    protected readonly Ids _Id;
    public Ids Id { get { return _Id; } }
    public override WhereBound BoundTo { get { return WhereBound.Clientbound; } }

    protected ClientboundStatusPacket(Ids id)
    {
        _Id = id;
    }

}

public abstract class ServerboundStatusPacket : StatusPacket
{
    public enum Ids
    {
        RequestPacketId = 0x00,
        PingPacketId = 0x01,
    }

    protected readonly Ids _Id;
    public Ids Id { get { return _Id; } }
    public override WhereBound BoundTo { get { return WhereBound.Serverbound; } }

    protected ServerboundStatusPacket(Ids id)
    {
        _Id = id;
    }

}

public abstract class ClientboundLoginPacket : LoginPacket
{
    public enum Ids
    {
        DisconnectPacketId = 0x00,
        EncryptionRequestPacketId = 0x01,
        LoginSuccessPacketId = 0x02,
        SetCompressionPacketId = 0x03,
    }

    protected readonly Ids _Id;
    public Ids Id => _Id;
    public override WhereBound BoundTo => WhereBound.Clientbound;

    protected ClientboundLoginPacket(Ids id)
    {
        _Id = id;
    }

}

public abstract class ServerboundLoginPacket : LoginPacket
{
    public enum Ids
    {
        StartLoginPacketId = 0x00,
        EncryptionResponsePacketId = 0x01,
    }

    protected readonly Ids _Id;
    public Ids Id => _Id;
    public override WhereBound BoundTo => WhereBound.Clientbound;

    protected ServerboundLoginPacket(Ids id)
    {
        _Id = id;
    }

}

public class SetProtocolPacket : ServerboundHandshakingPacket
{
    public readonly int Version;
    public readonly string Hostname;
    public readonly ushort Port;
    public readonly States NextState;

    public static SetProtocolPacket Read(Buffer buffer)
    {
        return new SetProtocolPacket(
            buffer.ReadInt(true),
            buffer.ReadString(), buffer.ReadUshort(),
            buffer.ReadInt(true) == 1 ? States.Status : States.Login);
    }

    private SetProtocolPacket(
        int version,
        string hostname, ushort port,
        States nextState)
        : base(Ids.HandshakePacketId)
    {
        Debug.Assert(version == _ProtocolVersion);
        Debug.Assert(port > 0);
        Debug.Assert(
            NextState == States.Status ||
            NextState == States.Login);

        Version = version;
        Hostname = hostname;
        Port = port;
        NextState = nextState;
    }

    public SetProtocolPacket(string hostname, ushort port, States nextState)
        : this(_ProtocolVersion, hostname, port, nextState)
    { }

    public override void Write(Buffer buffer)
    {
        Debug.Assert(_Id == Ids.HandshakePacketId);
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

public class ResponsePacket : ClientboundStatusPacket
{

    public readonly int MaxPlayers;
    public readonly int OnlinePlayers;
    public readonly string Description;

    public static ResponsePacket Read(Buffer buffer)
    {
        string jsonString = buffer.ReadString();
        // TODO
        throw new NotImplementedException();
    }

    public ResponsePacket(
        int maxPlayers, int onlinePlayers, string description)
        : base(Ids.ResponsePacketId)
    {
        Debug.Assert(maxPlayers >= onlinePlayers);

        MaxPlayers = maxPlayers;
        OnlinePlayers = onlinePlayers;
        Description = description;
    }

    public override void Write(Buffer buffer)
    {
        string jsonString =
            String.Format("{" +
                "\"version\":" +
                    "{" +
                        "\"name\":\"{0}\"," +
                        "\"protocol\":{1}" +
                    "}," +
                "\"players\":" +
                    "{" +
                        "\"max\":{2}," +
                        "\"online\":{3}," +
                        "\"sample\":[]" +
                    "}," +
                "\"description\":{\"text\":\"{4}\"}," +
                "\"favicon\":\"data:image/png;base64,<data>\"," +
                "\"enforcesSecureChat\":true," +
                "\"previewsChat\":true" +
            "}", _MinecraftVersion, _ProtocolVersion, MaxPlayers, OnlinePlayers, Description);
        
        buffer.WriteString(jsonString);
    }

}

public class PongPacket : ClientboundStatusPacket
{
    public readonly long Payload;

    public static PongPacket Read(Buffer buffer)
    {
        return new(buffer.ReadLong());
    }

    public PongPacket(long payload) : base(Ids.PongPacketId)
    {
        Payload = payload;
    }

    public override void Write(Buffer buffer)
    {
        buffer.WriteLong(Payload);
    }

}

public class RequestPacket : ServerboundStatusPacket
{
    public static RequestPacket Read(Buffer buffer)
    {
        return new();
    }

    public RequestPacket() : base(Ids.RequestPacketId) { }

    public override void Write(Buffer buffer) { }

}

public class PingPacket : ServerboundStatusPacket
{
    public readonly long Payload;

    public static PingPacket Read(Buffer buffer)
    {
        return new(buffer.ReadLong());
    }

    private PingPacket(long payload) : base(Ids.PingPacketId)
    {
        Payload = payload;
    }

    public PingPacket() : this(DateTime.Now.Ticks) { }

    public override void Write(Buffer buffer)
    {
        buffer.WriteLong(Payload);
    }

}

public class DisconnectPacket : ClientboundLoginPacket
{
    public readonly string Reason;

    public static DisconnectPacket Read(Buffer buffer)
    {
        return new(buffer.ReadString());
    }

    public DisconnectPacket(string reason) : base(Ids.DisconnectPacketId)
    {
        Reason = reason;
    }
    
    public override void Write(Buffer buffer)
    {
        buffer.WriteString(Reason);
    }

}

public class EncryptionRequestPacket : ClientboundLoginPacket
{
    public static EncryptionRequestPacket Read(Buffer buffer)
    {
        throw new NotImplementedException();
    }

    private EncryptionRequestPacket() : base(Ids.EncryptionRequestPacketId)
    {
        throw new NotImplementedException();
    }
    
    public override void Write(Buffer buffer)
    { 
        throw new NotImplementedException(); 
    }

}

public class LoginSuccessPacket : ClientboundLoginPacket
{
    public readonly Guid UserId;
    public readonly string Username;

    public static LoginSuccessPacket Read(Buffer buffer)
    {
        return new(
            Guid.Parse(buffer.ReadString()), 
            buffer.ReadString());
    }

    public LoginSuccessPacket(Guid userId, string username) 
        : base(Ids.LoginSuccessPacketId)
    {
        UserId = userId;
        Username = username;
    }
    
    public override void Write(Buffer buffer)
    {
        buffer.WriteString(UserId.ToString());
        buffer.WriteString(Username);
    }

}

public class SetCompressionPacket : ClientboundLoginPacket
{
    public readonly int Threshold;

    public static SetCompressionPacket Read(Buffer buffer)
    {
        return new(buffer.ReadInt(true));
    }

    public SetCompressionPacket(int threshold) 
        : base(Ids.SetCompressionPacketId)
    {
        Threshold = threshold;
    }
    
    public override void Write(Buffer buffer)
    {
        buffer.WriteInt(Threshold, true);
    }

}
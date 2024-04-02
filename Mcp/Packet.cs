
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
    private readonly int _Version;
    private readonly string _Hostname;
    private readonly ushort _Port;
    private readonly States _NextState;

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
            _NextState == States.Status ||
            _NextState == States.Login);

        _Version = version;
        _Hostname = hostname;
        _Port = port;
        _NextState = nextState;
    }

    public SetProtocolPacket(string hostname, ushort port, States nextState)
        : this(_ProtocolVersion, hostname, port, nextState)
    { }

    public override void Write(Buffer buffer)
    {
        Debug.Assert(_Id == Ids.HandshakePacketId);
        Debug.Assert(
            _NextState == States.Status ||
            _NextState == States.Login);
        int a = _NextState == States.Status ? 1 : 2;
        buffer.WriteInt(_Version, true);
        buffer.WriteString(_Hostname);
        buffer.WriteUshort(_Port);
        buffer.WriteInt(a, true);
    }
}

public class ResponsePacket : ClientboundStatusPacket
{

    private readonly int _maxPlayers;
    private readonly int _onlinePlayers;
    private readonly string _description;

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

        _maxPlayers = maxPlayers;
        _onlinePlayers = onlinePlayers;
        _description = description;
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
            "}", _MinecraftVersion, _ProtocolVersion, _maxPlayers, _onlinePlayers, _description);
        
        buffer.WriteString(jsonString);
    }

}

public class PongPacket : ClientboundStatusPacket
{
    private readonly long _Payload;

    public static PongPacket Read(Buffer buffer)
    {
        return new(buffer.ReadLong());
    }

    public PongPacket(long payload) : base(Ids.PongPacketId)
    {
        _Payload = payload;
    }

    public override void Write(Buffer buffer)
    {
        buffer.WriteLong(_Payload);
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
    private readonly long _Payload;

    public static PingPacket Read(Buffer buffer)
    {
        return new(buffer.ReadLong());
    }

    private PingPacket(long payload) : base(Ids.PingPacketId)
    {
        _Payload = payload;
    }

    public PingPacket() : this(DateTime.Now.Ticks) { }

    public override void Write(Buffer buffer)
    {
        buffer.WriteLong(_Payload);
    }
}


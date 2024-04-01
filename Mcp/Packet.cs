
using System.Diagnostics;

public abstract class Packet
{
    public enum States
    {
        Handshaking,
        Status,
        Login,
        Playing,
    }

    public Packet() { }

    public abstract void Write(Buffer buffer);
}

public abstract class HandshakingPacket : Packet
{
    public enum PacketIds
    {
        HandshakePacket = 0x00,
    }

    private readonly PacketIds _Id;

    public PacketIds Id { get { return _Id; } }
    public States State { get { return States.Handshaking; } }

    public HandshakingPacket(PacketIds id) 
    {
        _Id = id;
    }

}

public class HandshakePacket : HandshakingPacket
{
    private readonly int _Version;
    private readonly string _Hostname;
    private readonly ushort _Port;
    private readonly States _NextState;

    public static HandshakePacket 
        ReadPacketHandshakingHandshake(Buffer buffer)
    {
        return new HandshakePacket(
            buffer.ReadInt(true), 
            buffer.ReadString(), buffer.ReadUshort(), 
            buffer.ReadInt(true) == 1 ? States.Status : States.Login);
    }

    private HandshakePacket(
        int version, 
        string hostname, ushort port,
        States nextState) 
        : base(PacketIds.HandshakePacket)
    {
        _Version = version;
        _Hostname = hostname;
        _Port = port;
        _NextState = nextState;
    }

    public override void Write(Buffer buffer)
    {
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


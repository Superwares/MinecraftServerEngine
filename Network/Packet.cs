
namespace Mcp
{
    public abstract class Packet
    {
        public enum States
        {
            Handshaking = 0,
            Status = 1,
            Login = 2,
            Playing = 3,
        }

        protected readonly int _id;
        
        public Packet(int id)
        {
            _id = id;
        }
    }

    public abstract class PacketHandshaking : Packet
    {

        public PacketHandshaking(int id) : base(id) { }
    }

    public class PacketHandshakingHandshake : PacketHandshaking
    {
        private readonly int _protocolVersion;
        private readonly string _hostname;
        private readonly int _port;
        private readonly Packet.States _nextState;

        public static PacketHandshakingHandshake ReadPacketHandshakingHandshake(Buffer buffer)
        {

        }

        private PacketHandshakingHandshake(
            int protocolVecsion, string hostname, int port, States nextState) 
            : base(0x00)
        {
            _protocolVersion = protocolVecsion;
            _hostname = hostname;
            _port = port;
            _nextState = nextState;
        }
    }
}

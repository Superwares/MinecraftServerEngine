using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text;

namespace Application
{

    // TODO
    public class E1 : Exception
    {
        public E1() : base("No data to read.") { }
    }

    // TODO
    public class E2 : Exception
    {
        public E2() : base("Unexpected Data.") { }
    }

    // TODO: Check system is little- or big-endian.
    public class Buffer
    {
        private static readonly int _EXPANSION_FACTOR = 2;
        private static readonly float _LOAD_FACTOR = 0.7F;

        private static readonly byte _SEGMENT_BITS = 0x7F;
        private static readonly byte _CONTINUE_BIT = 0x80;

        private static readonly int _BOOL_DATATYPE_SIZE = 1;
        private static readonly int _SBYTE_DATATYPE_SIZE = 1;
        private static readonly int _BYTE_DATATYPE_SIZE = 1;
        private static readonly int _SHORT_DATATYPE_SIZE = 2;
        private static readonly int _USHORT_DATATYPE_SIZE = 2;
        private static readonly int _INT_DATATYPE_SIZE = 4;
        private static readonly int _LONG_DATATYPE_SIZE = 8;
        private static readonly int _FLOAT_DATATYPE_SIZE = 4;
        private static readonly int _DOUBLE_DATATYPE_SIZE = 8;
        private static readonly int _GUID_DATATYPE_SIZE = 16;

        private const int _INITIAL_DATA_SIZE = 16;

        private int _size;
        private byte[] _data;

        private int _first = 0, _last = 0;
        public int Size
        {
            get
            {
                Debug.Assert(_first >= 0);
                Debug.Assert(_last >= _first);
                return _last - _first;
            }
        }

        public Buffer() 
        {
            _size = _INITIAL_DATA_SIZE;
            _data = new byte[_INITIAL_DATA_SIZE];
        }

        internal Buffer(byte[] data)
        {
            _size = data.Length;
            _data = new byte[_size];
            Array.Copy(data, _data, _size);
        }

        public bool IsEmpty()
        {
            Debug.Assert(Size >= 0);
            return (Size == 0);
        }

        private byte ExtractByte()
        {
            Debug.Assert(_last >= _first);
            if (_first == _last)
                throw new E1();

            return _data[_first++];
        }

        private byte[] ExtractBytes(int size)
        {
            Debug.Assert(size >= 0);
            if (size == 0)
            {
                return [];
            }
            else if (size == 1)
            {
                return [ExtractByte()];
            }

            Debug.Assert(_last >= _first);

            if (_first + size > _last)
                throw new E1();

            byte[] data = new byte[size];
            Array.Copy(_data, _first, data, 0, size);
            _first += size;

            return data;
        }

        private void ExpandData(int addedSize)
        {
            Debug.Assert(addedSize >= 0);
            if (addedSize == 0)
                return;

            int prevSize = _size,
                newSize = prevSize,
                requiredSize = _last + addedSize;

            if (addedSize > 1)
                while (((float)requiredSize / (float)newSize) >= _LOAD_FACTOR)
                    newSize *= _EXPANSION_FACTOR;
            else
                if (((float)requiredSize / (float)newSize) >= _LOAD_FACTOR)
                newSize *= _EXPANSION_FACTOR;

            Debug.Assert(prevSize <= newSize);

            _size = newSize;

            var newData = new byte[newSize];
            if (Size > 0)
                Array.Copy(_data, _first, newData, _first, Size);
            _data = newData;
        }

        private void InsertByte(byte data)
        {
            Debug.Assert(_last >= _first);

            ExpandData(1);
            _data[_last++] = data;
        }

        private void InsertBytes(byte[] data)
        {
            Debug.Assert(_last >= _first);

            int size = data.Length;
            ExpandData(size);
            Array.Copy(data, 0, _data, _last, size);
            _last += size;
        }

        public bool ReadBool()
        {
            byte data = ExtractByte();
            Debug.Assert(data != 0x01 | data != 0x00);
            return (data > 0x00);
        }

        public sbyte ReadSbyte()
        {
            return (sbyte)ExtractByte();
        }

        public byte ReadByte()
        {
            return (byte)ExtractByte();
        }

        public short ReadShort()
        {
            byte[] data = ExtractBytes(_SHORT_DATATYPE_SIZE);
            Debug.Assert(data.Length == _SHORT_DATATYPE_SIZE);

            return (short)(
                ((short)data[0] << 8) |
                ((short)data[1] << 0));
        }

        public ushort ReadUshort()
        {
            byte[] data = ExtractBytes(_USHORT_DATATYPE_SIZE);
            Debug.Assert(data.Length == _USHORT_DATATYPE_SIZE);

            return (ushort)(
                ((ushort)data[0] << 8) |
                ((ushort)data[1] << 0));
        }

        public int ReadInt()
        {
            byte[] data = ExtractBytes(_INT_DATATYPE_SIZE);
            Debug.Assert(data.Length == _INT_DATATYPE_SIZE);

            return (int)(
                ((int)data[0] << 24) |
                ((int)data[1] << 16) |
                ((int)data[2] << 8) |
                ((int)data[3] << 0));
        }

        public long ReadLong()
        {
            byte[] data = ExtractBytes(_LONG_DATATYPE_SIZE);
            Debug.Assert(data.Length == _LONG_DATATYPE_SIZE);

            return (long)(
                ((long)data[0] << 56) |
                ((long)data[1] << 48) |
                ((long)data[2] << 40) |
                ((long)data[3] << 32) |
                ((long)data[4] << 24) |
                ((long)data[5] << 16) |
                ((long)data[6] << 8) |
                ((long)data[7] << 0));
        }

        public float ReadFLoat()
        {
            byte[] data = ExtractBytes(_FLOAT_DATATYPE_SIZE);
            Debug.Assert(data.Length == _FLOAT_DATATYPE_SIZE);
            Array.Reverse(data);
            return BitConverter.ToSingle(data);
        }

        public double ReadDouble()
        {
            byte[] data = ExtractBytes(_DOUBLE_DATATYPE_SIZE);
            Debug.Assert(data.Length == _DOUBLE_DATATYPE_SIZE);
            Array.Reverse(data);
            return BitConverter.ToDouble(data);
        }

        public int ReadInt(bool decode)
        {
            if (decode == false)
                return ReadInt();

            uint uvalue = 0;
            int position = 0;

            while (true)
            {
                byte data = ExtractByte();

                uvalue |= (uint)(data & _SEGMENT_BITS) << position;
                if ((data & _CONTINUE_BIT) == 0)
                    break;

                position += 7;

                if (position >= 32)
                    throw new E2();

                Debug.Assert(position > 0);
            }

            return (int)uvalue;
        }

        public long ReadLong(bool decode)
        {
            if (decode == false)
                return ReadLong();

            ulong uvalue = 0;
            int position = 0;

            while (true)
            {
                byte data = ExtractByte();

                uvalue |= (ulong)(data & _SEGMENT_BITS) << position;
                if ((data & _CONTINUE_BIT) == 0)
                    break;

                position += 7;

                if (position >= 64)
                    throw new E2();

                Debug.Assert(position > 0);
            }

            return (long)uvalue;
        }

        public string ReadString()
        {
            int size = ReadInt(true);
            Debug.Assert(size >= 0);

            byte[] data = ExtractBytes(size);
            return BitConverter.ToString(data);
        }

        public Guid ReadGuid()
        {
            byte[] data = ExtractBytes(_GUID_DATATYPE_SIZE);
            Debug.Assert(data.Length == _GUID_DATATYPE_SIZE);

            return new Guid(data);
        }

        public void WriteBool(bool value)
        {
            if (value == true)
                InsertByte(1);
            else
                InsertByte(0);
        }

        public void WriteSbyte(sbyte value)
        {
            InsertByte((byte)value);
        }

        public void WriteByte(byte value)
        {
            InsertByte((byte)value);
        }

        public void WriteShort(short value)
        {
            byte[] data = BitConverter.GetBytes(value);
            Debug.Assert(data.Length == _SHORT_DATATYPE_SIZE);
            Array.Reverse(data);
            InsertBytes(data);
        }

        public void WriteUshort(ushort value)
        {
            byte[] data = BitConverter.GetBytes(value);
            Debug.Assert(data.Length == _USHORT_DATATYPE_SIZE);
            Array.Reverse(data);
            InsertBytes(data);
        }

        public void WriteInt(int value)
        {
            byte[] data = BitConverter.GetBytes(value);
            Debug.Assert(data.Length == _INT_DATATYPE_SIZE);
            Array.Reverse(data);
            InsertBytes(data);
        }

        public void WriteLong(long value)
        {
            byte[] data = BitConverter.GetBytes(value);
            Debug.Assert(data.Length == _LONG_DATATYPE_SIZE);
            Array.Reverse(data);
            InsertBytes(data);
        }

        public void WriteFloat(float value)
        {
            byte[] data = BitConverter.GetBytes(value);
            Debug.Assert(data.Length == _FLOAT_DATATYPE_SIZE);
            Array.Reverse(data);
            InsertBytes(data);
        }

        public void WriteDouble(double value)
        {
            byte[] data = BitConverter.GetBytes(value);
            Debug.Assert(data.Length == _DOUBLE_DATATYPE_SIZE);
            Array.Reverse(data);
            InsertBytes(data);
        }

        public void WriteInt(int value, bool encode)
        {
            if (encode == false)
            {
                WriteInt(value);
                return;
            }

            uint uvalue = (uint)value;
            while (true)
            {
                if ((uvalue & ~_SEGMENT_BITS) == 0)
                {
                    InsertByte((byte)uvalue);
                    break;
                }

                InsertByte((byte)((uvalue & _SEGMENT_BITS) | _CONTINUE_BIT));
                uvalue >>= 7;
            }
        }

        public void WriteLong(long value, bool encode)
        {
            if (encode == false)
            {
                WriteLong(value);
                return;
            }

            uint uvalue = (uint)value;
            while (true)
            {
                if ((uvalue & ~_SEGMENT_BITS) == 0)
                {
                    InsertByte((byte)uvalue);
                    break;
                }

                InsertByte((byte)((uvalue & _SEGMENT_BITS) | _CONTINUE_BIT));
                uvalue >>= 7;
            }
        }

        public void WriteString(string value)
        {
            int size = value.Length;
            WriteInt(size, true);

            byte[] data = Encoding.UTF8.GetBytes(value);
            InsertBytes(data);
        }

        public void WriteGuid(Guid value)
        {
            byte[] data = value.ToByteArray();
            Debug.Assert(data.Length == _GUID_DATATYPE_SIZE);
            InsertBytes(data);
        }

    }

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

    public abstract class PlayingPacket : Packet
    {
        public override States State => States.Playing;
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

    public abstract class ClientboundPlayingPacket : PlayingPacket
    {
        public enum Ids
        {

        }

        protected readonly Ids _Id;
        public Ids Id => _Id;
        public override WhereBound BoundTo => WhereBound.Clientbound;

        protected ClientboundPlayingPacket(Ids id)
        {
            _Id = id;
        }
    }

    public abstract class ServerboundPlayingPacket : PlayingPacket
    {
        public enum Ids
        {

        }

        protected readonly Ids _Id;
        public Ids Id => _Id;
        public override WhereBound BoundTo => WhereBound.Serverbound;

        protected ServerboundPlayingPacket(Ids id)
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

    public class StartLoginPacket : ServerboundLoginPacket
    {
        public readonly string Username;

        public static StartLoginPacket Read(Buffer buffer)
        {
            return new(buffer.ReadString());
        }

        public StartLoginPacket(string username) : base(Ids.StartLoginPacketId)
        {
            Username = username;
        }

        public override void Write(Buffer buffer)
        {
            buffer.WriteString(Username);
        }
    }

    public class EncryptionResponsePacket : ServerboundLoginPacket
    {
        public static EncryptionResponsePacket Read(Buffer buffer)
        {
            throw new NotImplementedException();
        }

        public EncryptionResponsePacket() : base(Ids.EncryptionResponsePacketId)
        {
            throw new NotImplementedException();
        }

        public override void Write(Buffer buffer)
        {
            throw new NotImplementedException();
        }
    }

    internal class Client
    {
        private static readonly byte _SEGMENT_BITS = 0x7F;
        private static readonly byte _CONTINUE_BIT = 0x80;

        private int _x = 0, _y = 0;
        private byte[]? _data = null;

        private Socket _socket;

        internal static Client Accept(Socket socket)
        {
            Debug.Assert(socket.Blocking == false);
            // TODO: the socket is Binding and listening correctly.
            Debug.Assert(socket.IsBound == true);
            
            Socket newSocket = socket.Accept();
            newSocket.Blocking = false;

            return new(newSocket);
        }

        private Client(Socket socket)
        {
            Debug.Assert(socket.Blocking == false);

            _socket = socket;
        }

        public Client(string ipAddress, ushort port)
        {
            _socket = new(SocketType.Stream, ProtocolType.Tcp);
            _socket.Blocking = false;

            IPEndPoint localEndPoint = new(IPAddress.Parse(ipAddress), port);
            _socket.Connect(localEndPoint);
        }

        private byte RecvByte()
        {
            byte[] buffer = new byte[1];
            int n = _socket.Receive(buffer);
            Debug.Assert(n == 1);

            return buffer[0];
        }

        private int RecvInt()
        {
            uint uvalue = (uint)_x;
            int position = _y;

            try
            {
                while (true)
                {
                    byte b = RecvByte();

                    uvalue |= (uint)(b & _SEGMENT_BITS) << position;
                    if ((b & _CONTINUE_BIT) == 0)
                        break;

                    position += 7;

                    if (position >= 32)
                        throw new E2();

                    Debug.Assert(position > 0);
                }
            }
            finally
            {
                _x = (int)uvalue;
                _y = position;
                Debug.Assert(_data == null);
            }

            return (int)uvalue;
        }

        public Buffer Recv()
        {
            if (_data == null)
            {
                int size = RecvInt();
                _x = size;
                _y = 0;

                if (size == 0)
                    return new();  // TODO: make EmptyBuffer and return it.

                Debug.Assert(_data == null);
                _data = new byte[size];
                Debug.Assert(size > 0);
            }

            int availSize = _x;
            int totalSize = _y;

            do
            {
                try
                {
                    byte[] buffer = new byte[availSize];
                    int n = _socket.Receive(buffer);

                    availSize -= n;
                    totalSize += n;
                }
                finally
                {
                    _x = availSize;
                    _y = totalSize;
                }
            } while (availSize > 0);

            Buffer retval = new(_data);

            _x = 0;
            _y = 0;
            _data = null;

            return retval;
        }

        public void Send()
        {
            throw new NotImplementedException();
        }

    }

    public class PlayerConnection
    {
        private Client _client;

        internal PlayerConnection(Client client)
        {
            _client = client;
        }

        public PlayerConnection(string ipAddress, ushort port)
        {
            _client = new(ipAddress, port);

            throw new NotImplementedException();
        }

        public PlayingPacket Recv()
        {
            throw new NotImplementedException();
        }

        public void Send(PlayingPacket packet)
        {
            throw new NotImplementedException();
        }
    }

    public class Listener
    {
        private Socket _socket;

        private Queue<Client> _clientQueue = new();
        private Queue<Packet.States> _stateQueue = new();

        public Listener()
        {
            _socket = new Socket(SocketType.Stream, ProtocolType.Tcp);

        }

        private void HandleNonplayerConnections(
            ConcurrentQueue<PlayerConnection> playerConnQueue)
        {
            int clientQueueCount = _clientQueue.Count;
            Debug.Assert(clientQueueCount == _stateQueue.Count);
            if (clientQueueCount == 0)
                return;
            for (int i = 0; i < clientQueueCount; ++i)
            {
                Client client = _clientQueue.Dequeue();
                Packet.States state = _stateQueue.Dequeue();

                try
                {
                    if (state == Packet.States.Playing)
                        throw new NotImplementedException();
                    
                    if (state == Packet.States.Handshaking)
                    {
                        Buffer buffer = client.Recv();
                        SetProtocolPacket packet = SetProtocolPacket.Read(buffer);
                        Console.WriteLine(
                            $"Version: {packet.Version}, " +
                            $"Hostname: {packet.Hostname}, " +
                            $"Port: {packet.Port}, " +
                            $"NextState: {packet.NextState}");
                        state = packet.NextState;
                        Debug.Assert(
                            state == Packet.States.Status ||
                            state == Packet.States.Login);
                    }

                    if (state == Packet.States.Status)
                    {
                        continue;
                    } else if (state == Packet.States.Login)
                    {
                        continue;
                    }

                    throw new NotImplementedException();

                }
                catch (SocketException e)
                {
                    if (e.SocketErrorCode != SocketError.WouldBlock)
                        throw new NotImplementedException();


                }
                catch (E2)
                {
                    throw new NotImplementedException();
                }
                catch (Exception)
                {
                    throw new NotImplementedException();
                }
                finally
                {
                    _clientQueue.Enqueue(client);
                    _stateQueue.Enqueue(state);
                }
            }

        }

        public void Accept(ushort port, ConcurrentQueue<PlayerConnection> playerConnQueue)
        {
            try
            {
                IPEndPoint localEndPoint = new(IPAddress.Any, port);

                _socket.Blocking = false;
                _socket.Bind(localEndPoint);
                _socket.Listen();
            }
            catch (Exception)
            {
                throw new NotImplementedException();
            }

            while (true)
            {
                try
                {
                    HandleNonplayerConnections(playerConnQueue);

                    Client client = Client.Accept(_socket);
                    _clientQueue.Enqueue(client);

                    /*return new(client);*/

                }
                catch (SocketException e)
                {
                    if (e.SocketErrorCode != SocketError.WouldBlock)
                        throw new NotImplementedException();
                }
                catch (Exception)
                {
                    throw new NotImplementedException();
                }
            }
        }
    }

    public class Application
    {
        public static void Main()
        {
            Console.WriteLine("Hello, World!");


        }
    }
}
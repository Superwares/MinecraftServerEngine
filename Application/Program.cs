using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text;
using Containers;

namespace Application
{


    public abstract class McpException : Exception
    {
        public McpException(string message) : base(message) { }
    }

    // TODO
    public class EndofFileException : McpException
    {
        public EndofFileException() : base("EOF reached unexpectedly.") { }
    }

    // TODO
    public class UnexpectedDataException : McpException
    {
        public UnexpectedDataException() : base("Received unexpected data from the network.") { }
    }

    internal static class SocketMethods
    {
        public static byte RecvByte(Socket socket)
        {
            byte[] buffer = new byte[1];

            int n = socket.Receive(buffer);
            if (n == 0)
                throw new EndofFileException();

            Debug.Assert(n == 1);

            return buffer[0];
        }

        public static int RecvBytes(
            Socket socket, byte[] buffer, int offset, int size)
        {
            int n = socket.Receive(buffer, offset, size, SocketFlags.None);
            if (n == 0)
                throw new EndofFileException();

            Debug.Assert(n <= size);

            return n;
        }

        public static void SendByte(Socket socket, byte v)
        {
            int n = socket.Send([v]);
            Debug.Assert(n == 1);
        }

        public static void SendBytes(Socket socket, byte[] data)
        {
            Debug.Assert(data != null);
            int n = socket.Send(data);
            Debug.Assert(n == data.Length);
        }

        public static void Close(Socket socket)
        {
            socket.Close();
        }
    }

    // TODO: Check system is little- or big-endian.
    public class Buffer
    {
        private static readonly int _EXPANSION_FACTOR = 2;
        private static readonly float _LOAD_FACTOR = 0.7F;

        private static readonly byte _SEGMENT_BITS = 0x7F;
        private static readonly byte _CONTINUE_BIT = 0x80;

        /*private static readonly int _BOOL_DATATYPE_SIZE = 1;
        private static readonly int _SBYTE_DATATYPE_SIZE = 1;
        private static readonly int _BYTE_DATATYPE_SIZE = 1;*/
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

        public bool IsEmpty()
        {
            Debug.Assert(Size >= 0);
            return (Size == 0);
        }

        private byte ExtractByte()
        {
            Debug.Assert(_last >= _first);
            if (_first == _last)
                throw new EndofFileException();

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
                throw new EndofFileException();

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
                    throw new UnexpectedDataException();

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
                    throw new UnexpectedDataException();

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

        internal byte[] ReadData()
        {
            return ExtractBytes(Size);
        }

        internal void WriteData(byte[] data)
        {
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
                nextState == States.Status ||
                nextState == States.Login);

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
            string jsonString = "{\"version\":{\"name\":\"1.12.2\",\"protocol\":340},\"players\":{\"max\":100,\"online\":0,\"sample\":[]},\"description\":{\"text\":\"Hello, World!\"},\"favicon\":\"data:image/png;base64,<data>\",\"enforcesSecureChat\":true,\"previewsChat\":true}";

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

    public class Connection
    {
        private static readonly byte _SEGMENT_BITS = 0x7F;
        private static readonly byte _CONTINUE_BIT = 0x80;

        public enum Steps
        {
            Handshake,
            Request,
            Ping,
            StartLogin,
            JoinGame,
            Playing,
        }

        public Steps step = Steps.Handshake;

        private int _x = 0, _y = 0;
        private byte[]? _data = null;

        private Socket _socket;

        internal static Connection Accept(Socket socket)
        {
            // TODO: the socket is Binding and listening correctly.
            Debug.Assert(socket.IsBound == true);

            Socket newSocket = socket.Accept();
            newSocket.Blocking = false;

            return new(newSocket);
        }

        private Connection(Socket socket)
        {
            _socket = socket;
        }

        private int RecvSize()
        {
            uint uvalue = (uint)_x;
            int position = _y;

            try
            {
                while (true)
                {
                    byte b = SocketMethods.RecvByte(_socket);

                    uvalue |= (uint)(b & _SEGMENT_BITS) << position;
                    if ((b & _CONTINUE_BIT) == 0)
                        break;

                    position += 7;

                    if (position >= 32)
                        throw new UnexpectedDataException();

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

        private void SendSize(int size)
        {
            uint uvalue = (uint)size;

            while (true)
            {
                if ((uvalue & ~_SEGMENT_BITS) == 0)
                {
                    SocketMethods.SendByte(_socket, (byte)uvalue);
                    break;
                }

                SocketMethods.SendByte(
                    _socket, (byte)((uvalue & _SEGMENT_BITS) | _CONTINUE_BIT));
                uvalue >>= 7;
            }

        }

        public Buffer RecvBuffer()
        {
            if (_data == null)
            {
                int size = RecvSize();
                _x = size;
                Debug.Assert(_y == 0);

                if (size == 0)
                    return new();  // TODO: make EmptyBuffer and return it.

                Debug.Assert(_data == null);
                _data = new byte[size];
                Debug.Assert(size > 0);
            }

            int availSize = _x, offset = _y;

            do
            {
                try
                {
                    int n = SocketMethods.RecvBytes(_socket, _data, offset, availSize);
                    Debug.Assert(n <= availSize);

                    availSize -= n;
                    offset += n;
                }
                finally
                {
                    _x = availSize;
                    _y = offset;
                }
            } while (availSize > 0);

            Buffer result = new();
            result.WriteData(_data);

            _x = 0;
            _y = 0;
            _data = null;


            return result;
        }

        public void SendBuffer(Buffer buffer)
        {
            byte[] data = buffer.ReadData();
            SendSize(data.Length);
            SocketMethods.SendBytes(_socket, data);
        }

        public PlayingPacket RecvPacket()
        {
            throw new NotImplementedException();
        }

        public void SendPacket(PlayingPacket packet)
        {
            throw new NotImplementedException();
        }

        public bool Handle()
        {
            if (step == Steps.Handshake)
            {
                Buffer buffer = RecvBuffer();

                int packetId = buffer.ReadInt(true);
                if (ServerboundHandshakingPacket.Ids.HandshakePacketId !=
                    (ServerboundHandshakingPacket.Ids)packetId)
                    throw new UnexpectedDataException();

                SetProtocolPacket packet = SetProtocolPacket.Read(buffer);
                Packet.States state = packet.NextState;

                if (state == Packet.States.Status)
                    step = Steps.Request;
                else if (state == Packet.States.Login)
                    step = Steps.StartLogin;
                else
                    throw new NotImplementedException();
            }

            if (step == Steps.Request)
            {
                Buffer buffer = RecvBuffer();

                int packetId = buffer.ReadInt(true);
                if (ServerboundStatusPacket.Ids.RequestPacketId !=
                    (ServerboundStatusPacket.Ids)packetId)
                    throw new UnexpectedDataException();

                RequestPacket requestPacket = RequestPacket.Read(buffer);

                // TODO
                ResponsePacket responsePacket = new(100, 10, "Hello, World!");
                buffer.WriteInt((int)responsePacket.Id, true);
                responsePacket.Write(buffer);
                SendBuffer(buffer);

                step = Steps.Ping;
            }

            if (step == Steps.Ping)
            {
                Buffer buffer = RecvBuffer();

                int packetId = buffer.ReadInt(true);
                if (ServerboundStatusPacket.Ids.PingPacketId !=
                    (ServerboundStatusPacket.Ids)packetId)
                    throw new UnexpectedDataException();

                PingPacket inPacket = PingPacket.Read(buffer);

                PongPacket pongPacket = new(inPacket.Payload);
                buffer.WriteInt((int)pongPacket.Id, true);
                pongPacket.Write(buffer);
                SendBuffer(buffer);
            }

            else if (step == Steps.StartLogin)
            {
                throw new NotImplementedException();
            }

            return true;
        }

        public void Close()
        {
            SocketMethods.Close(_socket);
        }
    }

    public class Listener
    {
        private Socket _socket;

        private Channel<Connection> _guestConns = new();

        public Listener()
        {
            _socket = new Socket(SocketType.Stream, ProtocolType.Tcp);

        }

        private void HandleNonplayerConnections(object? obj)
        {
            Debug.Assert(obj != null);
            SyncQueue<Connection> playerConns = (SyncQueue<Connection>)obj;

            if (_guestConns.Empty == true)
                return;

            bool skip = false;

            while (true)
            {
                Connection connection = _guestConns.Dequeue();
                skip = false;

                Connection guestConn = _guestConns.Dequeue();

                try
                {
                    guestConn.Handle();
                }
                catch (SocketException e)
                {
                    if (e.SocketErrorCode != SocketError.WouldBlock)
                        throw new NotImplementedException();

                    Debug.Assert(e.SocketErrorCode == SocketError.WouldBlock);
                }
                catch (McpException)
                {
                    skip = true;
                }
                catch (Exception e)
                {
                    throw new NotImplementedException();
                }

                if (skip == false)
                    _guestConns.Enqueue(guestConn);
                
            }

        }

        public void Accept(ushort port, SyncQueue<Connection> playerConns)
        {
            Thread handler = new(HandleNonplayerConnections);
            handler.Start(playerConns);

            try
            {
                IPEndPoint localEndPoint = new(IPAddress.Any, port);

                _socket.Blocking = false;
                _socket.Bind(localEndPoint);
                _socket.Listen();

                while (true)
                {
                    Connection connection = Connection.Accept(_socket);
                    _guestConns.Enqueue(connection);

                }
                
            }
            catch (Exception)
            {
                throw new NotImplementedException();
            }
            finally
            {
                _guestConns.Exit();
                handler.Join();
            }

        }

    }

    public class Application
    {
        public static void Main()
        {
            Console.WriteLine("Hello, World!");

            ushort port = 25565;

            ConcurrentQueue<PlayerConnection> playerConnQueue = new();

            Listener listener = new();
            listener.Accept(port, playerConnQueue);
        }
    }
}
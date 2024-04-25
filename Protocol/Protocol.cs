using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;  // TODO: Use custom socket object in common library.
using Applications;
using Containers;

namespace Protocol
{
    internal static class SocketMethods
    {
        public static Socket Establish(ushort port)
        {
            Socket socket = new(SocketType.Stream, ProtocolType.Tcp);

            IPEndPoint localEndPoint = new(IPAddress.Any, port);
            socket.Bind(localEndPoint);
            socket.Listen();

            return socket;
        }

        /// <summary>
        /// TODO: Add description.
        /// </summary>
        /// <param name="socket">TODO: Add description.</param>
        /// <returns>TODO: Add description.</returns>
        /// <exception cref="TryAgainException">TODO: Why it's thrown.</exception>
        public static Socket Accept(Socket socket)
        {
            try
            {
                Socket newSocket = socket.Accept();

                return newSocket;
            }
            catch (SocketException e)
            {
                if (e.SocketErrorCode == SocketError.WouldBlock)
                    throw new TryAgainException();

                throw;
            }

            throw new NotImplementedException();
        }

        /// <summary>
        /// TODO: Add description.
        /// </summary>
        /// <param name="socket">TODO: Add description.</param>
        /// <param name="span">TODO: Add description.</param>
        public static bool Poll(Socket socket, TimeSpan span)  // TODO: Use own TimeSpan in common library.
        {
            // TODO: check the socket is binding and listening.

            if (IsBlocking(socket) &&
                socket.Poll(span, SelectMode.SelectRead) == false)
            {
                return false;
            }

            return true;
        }

        public static void SetBlocking(Socket socket, bool f)
        {
            socket.Blocking = f;
        }

        public static bool IsBlocking(Socket socket)
        {
            return socket.Blocking;
        }

        /// <summary>
        /// TODO: Add description.
        /// </summary>
        /// <param name="socket">TODO: Add description.</param>
        /// <param name="buffer">TODO: Add description.</param>
        /// <param name="offset">TODO: Add description.</param>
        /// <param name="size">TODO: Add description.</param>
        /// <returns></returns>
        /// <exception cref="DisconnectedClientException">TODO: Why it's thrown.</exception>
        /// <exception cref="TryAgainException">TODO: Why it's thrown.</exception>
        public static int RecvBytes(
            Socket socket, byte[] buffer, int offset, int size)
        {
            try
            {
                int n = socket.Receive(buffer, offset, size, SocketFlags.None);
                if (n == 0)
                    throw new DisconnectedClientException();

                Debug.Assert(n <= size);

                return n;
            }
            catch (SocketException e)
            {
                if (e.SocketErrorCode == SocketError.WouldBlock)
                    throw new TryAgainException();

                throw;
            }

        }

        /// <summary>
        /// TODO: Add description..
        /// </summary>
        /// <param name="socket">TODO: Add description..</param>
        /// <returns>TODO: Add description..</returns>
        /// <exception cref="DisconnectedClientException">TODO: Why it's thrown.</exception>
        /// <exception cref="TryAgainException">TODO: Why it's thrown.</exception>
        public static byte RecvByte(Socket socket)
        {
            byte[] buffer = new byte[1];

            int n = RecvBytes(socket, buffer, 0, 1);
            Debug.Assert(n == 1);

            return buffer[0];
        }

        /// <summary>
        /// TODO: Add description..
        /// </summary>
        /// <param name="socket">TODO: Add description..</param>
        /// <param name="data">TODO: Add description..</param>
        /// <exception cref="DisconnectedClientException">TODO: Why it's thrown.</exception>
        public static void SendBytes(Socket socket, byte[] data)
        {
            try
            {
                Debug.Assert(data != null);
                int n = socket.Send(data);
                Debug.Assert(n == data.Length);
            }
            catch (SocketException e)
            {
                if (e.SocketErrorCode == SocketError.ConnectionAborted)
                    throw new DisconnectedClientException();

                throw;
            }

        }

        /// <summary>
        /// TODO: Add description.
        /// </summary>
        /// <param name="socket">TODO: Add description.</param>
        /// <param name="v">TODO: Add description.</param>
        /// <exception cref="DisconnectedClientException">TODO: Why it's thrown.</exception>
        public static void SendByte(Socket socket, byte v)
        {
            SendBytes(socket, [v]);
        }

    }

    internal sealed class Client : IDisposable
    {
        private bool _isDisposed = false;

        private const int _TimeoutLimit = 100;
        private int _tryAgainCount = 0;

        private const byte _SegmentBits = 0x7F;
        private const byte _ContinueBit = 0x80;

        private int _x = 0, _y = 0;
        private byte[]? _data = null;

        private Socket _socket;

        /// <summary>
        /// TODO: Add description.
        /// </summary>
        /// <param name="socket">TODO: Add description.</param>
        /// <returns>TODO: Add description.</returns>
        /// <exception cref="TryAgainException">TODO: Why it's thrown.</exception>
        internal static Client Accept(Socket socket)
        {
            //TODO: Check the socket is Binding and listening correctly.

            Socket newSocket = SocketMethods.Accept(socket);
            SocketMethods.SetBlocking(newSocket, false);

            /*Console.WriteLine($"socket: {socket.LocalEndPoint}");*/


            return new(newSocket);
        }

        private Client(Socket socket)
        {
            Debug.Assert(SocketMethods.IsBlocking(socket) == false);
            _socket = socket;
        }

        ~Client()
        {
            Dispose(false);
        }

        /// <summary>
        /// TODO: Add description.
        /// </summary>
        /// <returns>TODO: Add description.</returns>
        /// <exception cref="UnexpectedClientBehaviorExecption">TODO: Why it's thrown.</exception>
        /// <exception cref="DisconnectedClientException">TODO: Why it's thrown.</exception>
        /// <exception cref="TryAgainException">TODO: Why it's thrown.</exception>
        private int RecvSize()
        {
            Debug.Assert(!_isDisposed);

            Debug.Assert(SocketMethods.IsBlocking(_socket) == false);

            uint uvalue = (uint)_x;
            int position = _y;

            try
            {
                while (true)
                {
                    byte v = SocketMethods.RecvByte(_socket);

                    uvalue |= (uint)(v & _SegmentBits) << position;
                    if ((v & _ContinueBit) == 0)
                        break;

                    position += 7;

                    if (position >= 32)
                        throw new InvalidEncodingException();

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

        /// <summary>
        /// TODO: Add description.
        /// </summary>
        /// <param name="size">TODO: Add description.</param>
        /// <exception cref="DisconnectedClientException">TODO: Why it's thrown.</exception>
        private void SendSize(int size)
        {
            Debug.Assert(!_isDisposed);

            uint uvalue = (uint)size;

            while (true)
            {
                if ((uvalue & ~_SegmentBits) == 0)
                {
                    SocketMethods.SendByte(_socket, (byte)uvalue);
                    break;
                }

                SocketMethods.SendByte(
                    _socket, (byte)((uvalue & _SegmentBits) | _ContinueBit));
                uvalue >>= 7;
            }

        }

        /*public void A()
        {
            SocketMethods.SetBlocking(_socket, true);
        }*/

        /// <summary>
        /// TODO: Add description.
        /// </summary>
        /// <param name="buffer">TODO: Add description.</param>
        /// <exception cref="UnexpectedClientBehaviorExecption">TODO: Why it's thrown.</exception>
        /// <exception cref="DisconnectedClientException">TODO: Why it's thrown.</exception>
        /// <exception cref="TryAgainException">TODO: Why it's thrown.</exception>
        public void Recv(Buffer buffer)
        {
            Debug.Assert(!_isDisposed);

            Debug.Assert(SocketMethods.IsBlocking(_socket) == false);

            try
            {
                if (_data == null)
                {
                    int size = RecvSize();
                    _x = size;
                    _y = 0;

                    if (size == 0) return;

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

                buffer.WriteData(_data);

                _x = 0;
                _y = 0;
                _data = null;

                _tryAgainCount = 0;
            }
            catch (TryAgainException)
            {
                /*Console.WriteLine($"count: {_count}");*/
                if (_TimeoutLimit < _tryAgainCount++)
                {
                    throw new DataRecvTimeoutException();
                }

                throw;
            }

        }

        /// <summary>
        /// TODO: Add description.
        /// </summary>
        /// <param name="buffer">TODO: Add description.</param>
        /// <exception cref="DisconnectedClientException">TODO: Why it's thrown.</exception>
        public void Send(Buffer buffer)
        {
            Debug.Assert(!_isDisposed);

            byte[] data = buffer.ReadData();
            SendSize(data.Length);
            SocketMethods.SendBytes(_socket, data);
        }

        private void Dispose(bool disposing)
        {
            if (_isDisposed) return;

            if (disposing == true)
            {
                // Release managed resources.
                _socket.Dispose();
                _data = null;
            }

            // Release unmanaged resources.

            _isDisposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void Close()
        {
            Dispose();
        }

    }

    public sealed class BoundingBox
    {
        private readonly float _width, _height;
        private float Width => _width;
        private float Height => _height;

        public BoundingBox(float width, float height)
        {
            _width = width; _height = height;
        }

    }

    public abstract class Block
    {
        private readonly int _id;
        public int Id => _id;

        private readonly int _metadate;
        public int Metadata => _metadate;

        public Block(int id, int metadata)
        {
            _id = id;
            _metadate = metadata;
        }

        public ulong GetGlobalPaletteID()
        {
            byte metadata = (byte)_metadate;
            Debug.Assert((metadata & 0b_11110000) == 0);  // metadata is 4 bits

            ushort id = (ushort)_id;
            Debug.Assert((id & 0b_11111110_00000000) == 0);  // id is 9 bits
            return (ulong)(id << 4 | metadata);  // 13 bits
        }

    }

    public class Air : Block
    {
        public Air() : base(0, 0) 
        {
        }

    }

    public class Stone : Block
    {
        public Stone() : base(1, 0) 
        {
        }

    }

    public class Granite : Block
    {
        public Granite() : base(1, 1) 
        {
        }

    }

    public class PolishedGranite : Block
    {
        public PolishedGranite() : base(1, 2) { }
    }

    public class Chunk
    {
        public const int Width = 16;
        public const int Height = 16 * 16;

        public struct Vector : IEquatable<Vector>
        {
            public int x, z;

            public Vector(int x, int z)
            {
                this.x = x; this.z = z;
            }

            public static Vector Convert(Player.Vector pos)
            {
                return new(
                    (pos.X >= 0) ? ((int)pos.X / Width) : (((int)pos.X / (Width + 1)) - 1),
                    (pos.Z >= 0) ? ((int)pos.Z / Width) : (((int)pos.Z / (Width + 1)) - 1));
            }

            public bool Equals(Vector other)
            {
                return (x == other.x) && (z == other.z);
            }

        }

        public class Grid : IEquatable<Grid>
        {
            public static Grid GenerateAround(Vector center, int d)
            {
                Debug.Assert(d >= 0);
                if (d == 0)
                    return new(center, center);

                int xMax = center.x + d, zMax = center.z + d,
                    xMin = center.x - d, zMin = center.z - d;

                /*int a = (2 * d) + 1;
                int length = a * a;
                Position[] positions = new Position[length];

                int i = 0;
                for (int z = zMin; z <= zMax; ++z)
                {
                    for (int x = xMin; x <= xMax; ++x)
                    {
                        positions[i++] = new(x, z);
                    }
                }
                Debug.Assert(i == length);*/

                Debug.Assert(xMax > xMin);
                Debug.Assert(zMax > zMin);
                return new(new(xMax, zMax), new(xMin, zMin));
            }

            public static Grid GenerateBetween(Grid grid1, Grid grid2)
            {
                Vector max3 = new(Math.Min(grid1._max.x, grid2._max.x), Math.Min(grid1._max.z, grid2._max.z)),
                    min3 = new(Math.Max(grid1._min.x, grid2._min.x), Math.Max(grid1._min.z, grid2._min.z));
                if (max3.x < min3.x)
                    (max3.x, min3.x) = (min3.x, max3.x);
                if (max3.z < min3.z)
                    (max3.z, min3.z) = (min3.z, max3.z);

                return new(max3, min3);
            }

            private readonly Vector _max, _min;

            Grid(Vector max, Vector min)
            {
                _max = max; _min = min;
            }

            public bool Contains(Vector p)
            {
                return (p.x <= _max.x && p.x >= _min.x && p.z <= _max.z && p.z >= _min.z);
            }

            public System.Collections.Generic.IEnumerable<Vector> GetVectors()
            {
                for (int z = _min.z; z <= _max.z; ++z)
                {
                    for (int x = _min.x; x <= _max.x; ++x)
                    {
                        yield return new(x, z);
                    }
                }

            }

            public bool Equals(Grid other)
            {
                Debug.Assert(other != null);
                return (other._max.Equals(_max) && other._min.Equals(_min));
            }

        }

        private class Section
        {
            public static readonly int Width = Chunk.Width;
            public static readonly int Height = Chunk.Height / Width;

            public static readonly int BlockTotalCount = Width * Width * Height;
            // (0, 0, 0) to (16, 16, 16)
            private Block?[] _blocks = new Block?[BlockTotalCount];

            internal static void Write(Buffer buffer, Section section)
            {
                int blockBitCount = 13;
                buffer.WriteByte((byte)blockBitCount);
                buffer.WriteInt(0, true);  // Write pallete as globally

                int blockBitTotalCount = (BlockTotalCount) * blockBitCount,
                    ulongBitCount = (sizeof(ulong) * 8);  // TODO: Make as constants
                int dataLength = blockBitTotalCount / ulongBitCount;
                Debug.Assert(blockBitTotalCount % ulongBitCount == 0);
                ulong[] data = new ulong[dataLength];

                for (int y = 0; y < Height; ++y)
                {
                    for (int z = 0; z < Width; ++z)
                    {
                        for (int x = 0; x < Width; ++x)
                        {
                            int i = (((y * Height) + z) * Width) + x;

                            int start = (i * blockBitCount) / ulongBitCount,
                                offset = (i * blockBitCount) % ulongBitCount,
                                end = (((i + 1) * blockBitCount) - 1) / ulongBitCount;

                            Block? block = section._blocks[i];
                            if (block == null)
                                block = new Air();

                            if (y == 10)
                                block = new Stone();

                            ulong id = block.GetGlobalPaletteID();
                            Debug.Assert((id >> blockBitCount) == 0);

                            data[start] |= (id << offset);

                            if (start != end)
                            {
                                data[end] = (id >> (ulongBitCount - offset));
                            }

                        }
                    }
                }

                Debug.Assert(unchecked((long)ulong.MaxValue) == -1);
                buffer.WriteInt(dataLength, true);
                for (int i = 0; i < dataLength; ++i)
                {
                    buffer.WriteLong((long)data[i]);  // TODO
                }

                // TODO
                for (int y = 0; y < Height; ++y)
                {
                    for (int z = 0; z < Width; ++z)
                    {
                        for (int x = 0; x < Width; x += 2)
                        {
                            buffer.WriteByte(byte.MaxValue / 2);

                        }
                    }
                }

                // TODO
                for (int y = 0; y < Height; ++y)
                {
                    for (int z = 0; z < Width; ++z)
                    {
                        for (int x = 0; x < Width; x += 2)
                        {
                            buffer.WriteByte(byte.MaxValue / 2);

                        }
                    }
                }


            }

        }

        public static readonly int SectionTotalCount = Height / Section.Height;
        // bottom to top
        private Section?[] _sections = new Section?[SectionTotalCount];

        internal static (int, byte[]) Write(Chunk chunk)
        {
            Buffer buffer = new();

            int mask = 0;
            Debug.Assert(SectionTotalCount == 16);
            for (int i = 0; i < SectionTotalCount; ++i)
            {
                Section? section = chunk._sections[i];
                if (section == null) continue;

                mask |= (1 << i);  // TODO;
                Section.Write(buffer, section);
            }

            // TODO
            for (int z = 0; z < Width; ++z)
            {
                for (int x = 0; x < Width; ++x)
                {
                    buffer.WriteByte(127);  // Void Biome
                }
            }

            return (mask, buffer.ReadData());
        }

        internal static (int, byte[]) Write()
        {
            Buffer buffer = new();

            int mask = 0;
            Debug.Assert(SectionTotalCount == 16);

            // TODO: biomes
            for (int z = 0; z < Width; ++z)
            {
                for (int x = 0; x < Width; ++x)
                {
                    buffer.WriteByte(127);  // Void Biome
                }
            }

            return (mask, buffer.ReadData());
        }

        internal static (int, byte[]) Write2()
        {
            Buffer buffer = new();

            int mask = 0;
            Debug.Assert(SectionTotalCount == 16);

            Section section = new();

            mask |= (1 << 3);  // TODO;
            Section.Write(buffer, section);

            // TODO: biomes
            for (int z = 0; z < Width; ++z)
            {
                for (int x = 0; x < Width; ++x)
                {
                    buffer.WriteByte(127);  // Void Biome
                }
            }

            return (mask, buffer.ReadData());
        }

        public readonly Vector p;

        public Chunk(Vector p)
        {
            this.p = p;
        }



    }

    /*public class ChunkTable
    {
        

        private readonly Table<Chunk.Vector, Chunk> _chunks = new();
        private readonly Table<int, Chunk.Vector[]> _entityToChunks = new();

        public void Init(IReadOnlyEntity entity)
        {
            Debug.Assert(!_isDisposed);

            Chunk.Vector pChunk = Chunk.Vector.Convert(player.pos);

            if (!_chunkToPlayers.Contains(pChunk))
                _chunkToPlayers.Insert(pChunk, new());

            _chunkToPlayers.Lookup(pChunk).Insert(player.Id, player);
            Debug.Assert(!_cache.Contains(player.Id));
            _cache.Insert(player.Id, pChunk);
        }

        public void Close(int entityId)
        {
            Debug.Assert(!_isDisposed);

            Debug.Assert(_cache.Contains(playerId));
            Chunk.Vector pChunkPrev = _cache.Extract(playerId);

            Table<int, Player> players = _chunkToPlayers.Lookup(pChunkPrev);
            players.Extract(playerId);

            if (players.Empty)
                _chunkToPlayers.Extract(pChunkPrev);
        }

        public void Update(int entityId, Player.Vector pos)
        {
            Debug.Assert(!_isDisposed);

            Chunk.Vector pChunk = Chunk.Vector.Convert(pos);
            Debug.Assert(_cache.Contains(playerId));
            Chunk.Vector pChunkPrev = _cache.Extract(playerId);

            if (!pChunk.Equals(pChunkPrev))
            {
                Table<int, Player> players = _chunkToPlayers.Lookup(pChunkPrev);
                Player player = players.Extract(playerId);

                if (players.Empty)
                    _chunkToPlayers.Extract(pChunkPrev);

                if (!_chunkToPlayers.Contains(pChunk))
                    _chunkToPlayers.Insert(pChunk, new());

                _chunkToPlayers.Lookup(pChunk).Insert(playerId, player);
            }

            _cache.Insert(playerId, pChunk);
        }

        public bool Contains(Chunk.Vector p)
        {
            Debug.Assert(!_isDisposed);

            return _chunkToPlayers.Contains(p);
        }

        public IReadOnlyTable<int, Player> Search(Chunk.Vector pos)
        {
            Debug.Assert(!_isDisposed);

            return _chunkToPlayers.Lookup(pos);
        }

    }*/

    internal interface IUpdateOnlyEntityIdList
    {
        int Alloc();
        void Dealloc(int id);

    }

    public sealed class EntityIdList : IUpdateOnlyEntityIdList, IDisposable
    {
        private bool _disposed = false;

        private readonly NumList _numList = new();
        private readonly Queue<int> _deallocedIds = new();

        ~EntityIdList() => Dispose(false);

        internal int Alloc()
        {
            Debug.Assert(!_disposed);

            return _numList.Alloc();

        }
        int IUpdateOnlyEntityIdList.Alloc()
        {
            return Alloc();
        }

        internal void Dealloc(int id)
        {
            Debug.Assert(!_disposed);

            _deallocedIds.Enqueue(id);
        }

        void IUpdateOnlyEntityIdList.Dealloc(int id)
        {
            /*Console.Write("Dealloc!");*/
            Dealloc(id);
        }

        public void Reset()
        {
            Debug.Assert(!_disposed);

            /*Console.Write("Reset!");*/

            while (!_deallocedIds.Empty)
            {
                int id = _deallocedIds.Dequeue();
                _numList.Dealloc(id);
            }

            Debug.Assert(_deallocedIds.Empty);
        }

        private void Dispose(bool disposing)
        {
            if (_disposed) return;

            Debug.Assert(_numList.Empty);
            Debug.Assert(_deallocedIds.Empty);

            if (disposing == true)
            {
                // Release managed resources.
                _numList.Dispose();
                _deallocedIds.Dispose();
            }

            // Release unmanaged resources.

            _disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

    }

    internal interface IUpdateOnlyPlayerList
    {
        void Add(Guid uniqueId, string username);
        void Remove(Guid uniqueId);

    }

    public sealed class PlayerList : IUpdateOnlyPlayerList, IDisposable
    {
        private class Item
        {
            public readonly Guid UniqueId;
            public readonly string Username;
            /*public int latency;*/

            public Item(Guid uniqueId, string username)
            {
                UniqueId = uniqueId;
                Username = username;
            }

        }

        private bool _isDisposed = false;

        private readonly Table<Guid, Item> _addedItems = new();
        private readonly Queue<Item> _removedItems = new();
        private readonly Table<Guid, Item> _items = new();

        ~PlayerList() => Dispose();

        public void Reset()
        {
            foreach (Item item in _addedItems.GetValues())
            {
                _items.Insert(item.UniqueId, item);
            }

            _addedItems.Flush();
            _removedItems.Flush();  // TODO: Release explicitely resources for no garbage.

            Debug.Assert(_addedItems.Empty);
            Debug.Assert(_removedItems.Empty);
        }

        public bool Contains(Guid uniqueId)
        {
            if (_addedItems.Contains(uniqueId))
                return true;
            if (_items.Contains(uniqueId))
                return true;

            return false;
        }

        void IUpdateOnlyPlayerList.Add(Guid uniqueId, string username)
        {
            Debug.Assert(!Contains(uniqueId));

            Item item = new(uniqueId, username);
            _addedItems.Insert(uniqueId, item);
        }

        void IUpdateOnlyPlayerList.Remove(Guid uniqueId)
        {
            Debug.Assert(Contains(uniqueId));

            Item item = _items.Extract(uniqueId);
            _removedItems.Enqueue(item);
        }

        public System.Collections.Generic.IEnumerable<(Guid, string)> GetPlayers()
        {
            foreach (Item item in _items.GetValues())
            {
                yield return (item.UniqueId, item.Username);
            }
        }

        public System.Collections.Generic.IEnumerable<(Guid, string)> GetAddedPlayers()
        {
            foreach (Item item in _addedItems.GetValues())
            {
                yield return (item.UniqueId, item.Username);
            }
        }

        public System.Collections.Generic.IEnumerable<(Guid, string)> GetRemovedPlayers()
        {
            foreach (Item item in _removedItems.GetValues())
            {
                yield return (item.UniqueId, item.Username);
            }
        }

        private void Dispose(bool disposing)
        {
            if (_isDisposed) return;

            if (disposing == true)
            {
                // Release managed resources.
            }

            // Release unmanaged resources.

            _isDisposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

    }

    internal interface IUpdateOnlyEntityRenderingTable
    {
        void Init(IRenderOnlyEntity entity);
        void Close(int id);
        void Update(int id, Entity.Vector pos);

    }

    public sealed class EntityRenderingTable : IUpdateOnlyEntityRenderingTable, IDisposable
    {
        private bool _isDisposed = false;

        private readonly Table<Chunk.Vector, Table<int, IRenderOnlyEntity>> 
            _chunkToEntities = new();  // Disposable
        private readonly Table<int, Chunk.Vector> _entityToChunk = new();

        public EntityRenderingTable() { }

        ~EntityRenderingTable() => Dispose(false);

        void IUpdateOnlyEntityRenderingTable.Init(IRenderOnlyEntity entity)
        {
            Debug.Assert(!_isDisposed);

            Chunk.Vector pChunk = Chunk.Vector.Convert(entity.Position);

            if (!_chunkToEntities.Contains(pChunk))
                _chunkToEntities.Insert(pChunk, new());

            _chunkToEntities.Lookup(pChunk).Insert(entity.Id, entity);
            Debug.Assert(!_entityToChunk.Contains(entity.Id));
            _entityToChunk.Insert(entity.Id, pChunk);
        }

        void IUpdateOnlyEntityRenderingTable.Close(int id)
        {
            Debug.Assert(!_isDisposed);

            Debug.Assert(_entityToChunk.Contains(id));
            Chunk.Vector pChunkPrev = _entityToChunk.Extract(id);

            Table<int, IRenderOnlyEntity> entities = _chunkToEntities.Lookup(pChunkPrev);
            entities.Extract(id);

            if (entities.Empty)
                _chunkToEntities.Extract(pChunkPrev);
        }

        void IUpdateOnlyEntityRenderingTable.Update(int id, Entity.Vector p)
        {
            Debug.Assert(!_isDisposed);

            Chunk.Vector pChunk = Chunk.Vector.Convert(p);
            Debug.Assert(_entityToChunk.Contains(id));
            Chunk.Vector pChunkPrev = _entityToChunk.Extract(id);

            if (!pChunk.Equals(pChunkPrev))
            {
                Table<int, IRenderOnlyEntity> entities = _chunkToEntities.Lookup(pChunkPrev);
                IRenderOnlyEntity entity = entities.Extract(id);

                if (entities.Empty)
                    _chunkToEntities.Extract(pChunkPrev);

                if (!_chunkToEntities.Contains(pChunk))
                    _chunkToEntities.Insert(pChunk, new());

                _chunkToEntities.Lookup(pChunk).Insert(id, entity);
            }

            _entityToChunk.Insert(id, pChunk);
        }

        internal bool Contains(Chunk.Vector p)
        {
            Debug.Assert(!_isDisposed);

            return _chunkToEntities.Contains(p);
        }

        internal IReadOnlyTable<int, IRenderOnlyEntity> Search(Chunk.Vector pos)
        {
            Debug.Assert(!_isDisposed);

            return _chunkToEntities.Lookup(pos);
        }

        private void Dispose(bool disposing)
        {
            if (_isDisposed) return;

            if (disposing == true)
            {
                // Release managed resources.
                _chunkToEntities.Dispose();
            }

            // Release unmanaged resources.

            _isDisposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

    }

    internal sealed class WindowHelper : IDisposable
    {
        private bool _disposed = false;

        /**
         * -1: Window is closed.
         *  0: Window is opened with only self inventory.
         * >0: Window is opened with self and other inventory.
         */
        private int _windowId = -1;

        private PlayerInventory _inventorySelf;
        private Inventory? _inventoryOther = null;

        private Item? _itemCursor = null;

        public WindowHelper(
            int idConn, Queue<ClientboundPlayingPacket> outPackets, 
            PlayerInventory inventorySelf)
        {
            _windowId = 0;

            CompleteSelfPlayerInventoryRenderer renderer = new(_windowId, outPackets);

            int i = 0, n = renderer.SlotCount;
            var arr = new SlotData[n];
            lock (inventorySelf._SharedObject)
            {
                inventorySelf.AddRenderer(idConn, renderer);

                foreach (Item? item in inventorySelf.Items)
                {
                    if (item == null)
                    {
                        arr[i++] = new();
                        continue;
                    }

                    Debug.Assert(item.Id >= short.MinValue);
                    Debug.Assert(item.Id <= short.MaxValue);
                    Debug.Assert(item.Count >= byte.MinValue);
                    Debug.Assert(item.Count <= byte.MaxValue);
                    arr[i++] = new((short)item.Id, (byte)item.Count);
                }

                Debug.Assert(_windowId >= byte.MinValue);
                Debug.Assert(_windowId <= byte.MaxValue);
                outPackets.Enqueue(new SetWindowItemsPacket((byte)_windowId, arr));

                Debug.Assert(i == n);
            }

            _inventorySelf = inventorySelf;
        }

        ~WindowHelper() => Debug.Assert(false);

        public int GetWindowId()
        {
            Debug.Assert(!_disposed);

            Debug.Assert(_windowId >= 0);
            Debug.Assert(_windowId > 0 ?
                _inventoryOther != null : _inventoryOther == null);

            return _windowId;
        }

        public bool IsWindowOpened()
        {
            Debug.Assert(!_disposed);

            Debug.Assert(_windowId > 0 ?
                _inventoryOther != null : _inventoryOther == null);

            return _windowId >= 0;
        }

        public void ReopenWindowWithOtherInventory(
            int idConn, Queue<ClientboundPlayingPacket> outPackets, 
            Inventory inventoryOther)
        {
            Debug.Assert(!_disposed);

            Debug.Assert(_windowId == 0);
            Debug.Assert(_inventoryOther == null);

            int n;
            string windowType;

            _windowId = (new Random().Next() % 100) + 1;  // TODO

            InventoryRenderer rendererOther;

            switch (inventoryOther)
            {
                default:
                    throw new NotImplementedException();
                case PlayerInventory:
                    windowType = "minecraft:chest";
                    rendererOther = new OtherPlayerInventoryRenderer(_windowId, outPackets);
                    break;
                case ChestInventory:
                    windowType = "minecraft:chest";
                    rendererOther = new ChestInventoryRenderer(_windowId, outPackets);
                    break;
            }

            n = rendererOther.SlotCount;
            Debug.Assert(n % 9 == 0);
            SelfPlayerInventoryRenderer rendererSelf = new(_windowId, outPackets, n);

            lock (_inventorySelf._SharedObject)
            {
                Debug.Assert(_windowId >= byte.MinValue);
                Debug.Assert(_windowId <= byte.MaxValue);
                Debug.Assert(n >= byte.MinValue);
                Debug.Assert(n <= byte.MaxValue);
                outPackets.Enqueue(new OpenWindowPacket(
                    (byte)_windowId, windowType, "", (byte)n));

                _inventorySelf.RemoveRenderer(idConn);
                _inventorySelf.AddRenderer(idConn, rendererSelf);

            }
            
            lock (inventoryOther._SharedObject)
            {
                int i = 0;
                var arr = new SlotData[n];
                switch (inventoryOther)
                {
                    default:
                        throw new NotImplementedException();
                    case PlayerInventory playerInventory:
                        playerInventory.AddRenderer(idConn, (PlayerInventoryRenderer)rendererOther);
                        Debug.Assert(rendererOther is PlayerInventoryRenderer);

                        foreach (Item? item in playerInventory.PrimaryItems)
                        {
                            if (item == null)
                            {
                                arr[i++] = new();
                                continue;
                            }

                            Debug.Assert(item.Id >= short.MinValue);
                            Debug.Assert(item.Id <= short.MaxValue);
                            Debug.Assert(item.Count >= byte.MinValue);
                            Debug.Assert(item.Count <= byte.MaxValue);
                            arr[i++] = new((short)item.Id, (byte)item.Count);
                        }

                        break;
                    case ChestInventory chestInventory:
                        chestInventory.AddRenderer(idConn, (ChestInventoryRenderer)rendererOther);
                        Debug.Assert(rendererOther is ChestInventoryRenderer);

                        foreach(Item? item in chestInventory.Items)
                        {
                            if (item == null)
                            {
                                arr[i++] = new();
                                continue;
                            }

                            Debug.Assert(item.Id >= short.MinValue);
                            Debug.Assert(item.Id <= short.MaxValue);
                            Debug.Assert(item.Count >= byte.MinValue);
                            Debug.Assert(item.Count <= byte.MaxValue);
                            arr[i++] = new((short)item.Id, (byte)item.Count);
                        }

                        break;
                }

                Debug.Assert(_windowId >= byte.MinValue);
                Debug.Assert(_windowId <= byte.MaxValue);
                outPackets.Enqueue(new SetWindowItemsPacket((byte)_windowId, arr));

                Debug.Assert(i == n);

            }

            _inventoryOther = inventoryOther;
        }

        public void ResetWindow(
            int idConn, Queue<ClientboundPlayingPacket> outPackets)
        {
            Debug.Assert(!_disposed);

            Debug.Assert(_windowId > 0);
            Debug.Assert(_inventoryOther != null);

            _windowId = 0;

            lock (_inventoryOther._SharedObject)
            {
                _inventoryOther.RemoveRenderer(idConn);
            }

            CompleteSelfPlayerInventoryRenderer renderer = new(_windowId, outPackets);

            /*int i = 0, n = 9;
            var arr = new SlotData[n];*/
            lock (_inventorySelf._SharedObject)
            {
                _inventorySelf.RemoveRenderer(idConn);
                _inventorySelf.AddRenderer(idConn, renderer);

                /*foreach (Item? item in _inventorySelf.Items)
                {
                    if (item == null)
                    {
                        arr[i++] = new();
                        continue;
                    }

                    Debug.Assert(item.Id >= short.MinValue);
                    Debug.Assert(item.Id <= short.MaxValue);
                    Debug.Assert(item.Count >= byte.MinValue);
                    Debug.Assert(item.Count <= byte.MaxValue);
                    arr[i++] = new((short)item.Id, (byte)item.Count);

                    if (i == n)
                        break;
                }*/

                /*Debug.Assert(_windowId >= byte.MinValue);
                Debug.Assert(_windowId <= byte.MaxValue);
                outPackets.Enqueue(new SetWindowItemsPacket((byte)_windowId, arr));*/

                {
                    Item? item = _inventorySelf.OffhandItem;
                    SlotData slotData;
                    if (item == null)
                    {
                        slotData = new();
                    }
                    else
                    {
                        Debug.Assert(item.Id >= short.MinValue);
                        Debug.Assert(item.Id <= short.MaxValue);
                        Debug.Assert(item.Count >= byte.MinValue);
                        Debug.Assert(item.Count <= byte.MaxValue);
                        slotData = new((short)item.Id, (byte)item.Count);
                    }
                    outPackets.Enqueue(new SetSlotPacket((sbyte)_windowId, 45, slotData));
                }
            }

            /*if (_itemCursor != null)
            {
                // TODO: Drop item if _iremCursor is not null.
                _itemCursor = null;
            }*/

            Debug.Assert(_itemCursor == null);
        }

        /*public void ClickLeftMouseButton(
            int index,
            Item? itemSlot,
            Queue<ClientboundPlayingPacket> outPackets)
        {
            Debug.Assert(!_disposed);

            Debug.Assert(_windowId >= 0);

            if (_windowId == 0)
            {
                Debug.Assert(_inventoryOther == null);
                if (index == 0)
                {
                    bool contains = _inventorySelf.ContainsInCraftingOutput();
                    Item? itemTake = contains ? _inventorySelf.TakeFromCraftingOutput() : null;
                    bool different;
                    if (itemSlot != null && itemTake != null)
                    {
                        different = itemSlot.Equals(itemTake);
                    }
                    else if (itemSlot == null && itemTake == null)
                    {
                        different = true;
                    }
                    else
                    {
                        different = false;
                    }

                    if (different)
                        throw new UnexpectedValueException($"ClickWindowPacket.Data");

                    if (contains && _itemCursor == null)
                    {
                        _itemCursor = itemTake;

                        Debug.Assert(_itemCursor != null);
                    }

                }
                else if (index > 0 && index <= 4)
                {
                    throw new NotImplementedException();
                }
                else if (index > 4 && index <= 8)
                {
                    throw new NotImplementedException();
                }
                else if (index > 8 && index <= 44)
                {
                    int indexNormalized = index - 9;
                    bool contains = _inventorySelf.ContainsInMain(indexNormalized);
                    Item? itemTaked = contains ? _inventorySelf.TakeFromMain(indexNormalized) : null;
                    bool different;
                    if (itemSlot != null && itemTaked != null)
                        different = itemSlot.Equals(itemTaked);
                    else if (itemSlot == null && itemTaked == null)
                        different = true;
                    else
                        different = false;

                    if (different)
                        throw new UnexpectedValueException($"ClickWindowPacket.Data");

                    if (contains && _itemCursor != null)
                    {
                        Debug.Assert(itemTaked != null);

                        _inventorySelf.PutIntoMain(indexNormalized, _itemCursor);
                        _itemCursor = itemTaked;

                    }
                    else if (contains)
                    {
                        Debug.Assert(itemTaked != null);

                        _itemCursor = itemTaked;

                    }
                    else if (_itemCursor != null)
                    {
                        Debug.Assert(itemTaked == null);

                        _inventorySelf.PutIntoMain(indexNormalized, _itemCursor);
                        _itemCursor = null;

                    }
                    else
                    {
                        // Nothing
                    }

                }
                else if (index == 45)
                {
                    throw new NotImplementedException();
                }
                else
                    throw new UnexpectedValueException($"ClickWindowPacket.SlotNumber {index}");
            }
            else
            {
                Debug.Assert(_windowId > 0);

                throw new NotImplementedException();

            }

        }*/

        private void Dispose(bool disposing)
        {
            if (_disposed) return;

            // Assertion.
            /*Debug.Assert(_opened = false);
            Debug.Assert(_idWindow == -1);
            Debug.Assert(_otherInventory == null);*/

            if (disposing == true)
            {
                // Release managed resources.
                
            }

            // Release unmanaged resources.
            

            _disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void Close(int id)
        {
            /*if (_opened)
                CloseWindow(id, _idWindow);*/

            Dispose();
        }

    }

    public sealed class Connection : IDisposable
    {
        private sealed class TeleportationRecord
        {
            private const ulong TickLimit = 20;  // 1 seconds, 20 ticks

            public readonly int _payload;
            private ulong _ticks = 0;

            public TeleportationRecord(int payload)
            {
                _payload = payload;
            }

            public void Confirm(int payload)
            {
                if (payload != _payload)
                    throw new UnexpectedValueException("TeleportationPayload");
            }

            public void Update()
            {
                Debug.Assert(_ticks >= 0);

                if (_ticks++ > TickLimit)
                {
                    throw new TeleportationConfirmTimeoutException();
                }

            }

        }

        private sealed class KeepaliveObserver
        {
            private const ulong TickLimit = 10000 / 50;  // 10 seconds, 200 ticks

            private long _payload;
            private bool _isConfirmed = true;

            public KeepaliveObserver() { }

            public void Confirm(long payload)
            {
                if (_payload != payload)
                    throw new UnexpectedValueException("KeepalivePayload");

                _isConfirmed = true;
            }

            public void Update(
                ulong serverTicks, 
                Queue<ClientboundPlayingPacket> outPackets)
            {
                Debug.Assert(serverTicks % TickLimit >= 0);
                if (serverTicks % TickLimit > 0)
                    return;

                if (!_isConfirmed)
                    throw new KeepaliveTimeoutException();

                _isConfirmed = false;
                _payload = new Random().NextInt64();
                outPackets.Enqueue(new RequestKeepAlivePacket(_payload));
            }

        }

        public sealed class ClientsideSettings(byte renderDistance)
        {
            public const int MinRenderDistance = 2, MaxRenderDistance = 32;

            public int renderDistance = renderDistance;

        }

        private Client _client;  // dispoasble

        public readonly int Id;

        public readonly Guid UserId;
        public readonly string Username;

        public readonly ClientsideSettings Settings;

        private Chunk.Grid? _renderedChunkGrid = null;

        private Set<int> _renderedEntityIds = new();  // Disposable

        private readonly Queue<TeleportationRecord> _teleportationRecords = new();  // dispoasble

        private readonly KeepaliveObserver keepaliveChecker = new();

        private readonly Queue<LoadChunkPacket> _loadChunkPackets = new();  // dispoasble
        private readonly Queue<ClientboundPlayingPacket> _outPackets = new();  // dispoasble

        private readonly WindowHelper _windowHelper;  // disposable

        private bool _isDisposed = false;

        internal Connection(
            int id,
            Client client,
            Guid userId, string username,
            ClientsideSettings settings,
            PlayerList playerList,
            Player player)
        {
            Id = id;

            _client = client;

            UserId = userId;
            Username = username;

            Settings = settings;

            _outPackets.Enqueue(new SetPlayerAbilitiesPacket(
                true, false, true, true, 0.1f, 0));

            foreach ((Guid otherUniqueId, string otherUsername) in playerList.GetPlayers())
            {
                _outPackets.Enqueue(new AddPlayerListItemPacket(otherUniqueId, otherUsername));
            }

            player.Connect(_outPackets);

            _windowHelper = new(Id, _outPackets, new PlayerInventory());

            /*{
                SlotData slotData = new(280, 64);
                _outPackets.Enqueue(new SetSlotPacket(0, 27, slotData.WriteData()));
            }*/
        }

        ~Connection()
        {
            Dispose(false);
        }

        /// <summary>
        /// TODO: Add description.
        /// </summary>
        /// <returns>TODO: Add description.</returns>
        /// <exception cref="DisconnectedClientException">TODO: Why it's thrown.</exception>
        public void Control(ulong serverTicks, Player player)
        {
            Debug.Assert(!_isDisposed);

            /*if (serverTicks == 100)
            {
                SlotData slotData = new(426, 64);

                _outPackets.Enqueue(new OpenWindowPacket(1, "minecraft:chest", "WindowTitle!", 27));
                _outPackets.Enqueue(new SetSlotPacket(1, 30, slotData));
                
            }*/

            /*if (serverTicks == 100)
            {
                _windowHelper.ReopenWindowWithOtherInventory(Id, _outPackets, new PlayerInventory());
            }*/

            try
            {
                Buffer buffer = new();

                while (true)
                {
                    _client.Recv(buffer);

                    int packetId = buffer.ReadInt(true);
                    switch (packetId)
                    {
                        default:
                            Console.WriteLine($"packetId: 0x{packetId:X}");
                            /*throw new NotImplementedException();*/
                            buffer.Flush();
                            break;
                        case ServerboundPlayingPacket.ConfirmTeleportationPacketId:
                            {
                                ConfirmTeleportationPacket packet = ConfirmTeleportationPacket.Read(buffer);

                                if (_teleportationRecords.Empty)
                                    throw new UnexpectedPacketException();

                                TeleportationRecord record = _teleportationRecords.Dequeue();
                                record.Confirm(packet.Payload);
                            }
                            break;
                        case ServerboundPlayingPacket.SetClientSettingsPacketId:
                            {
                                SetClientSettingsPacket packet = SetClientSettingsPacket.Read(buffer);

                                throw new NotImplementedException();
                            }
                            break;
                        case ServerboundPlayingPacket.ServerboundConfirmTransactionPacketId:
                            {
                                ServerboundConfirmTransactionPacket packet = 
                                    ServerboundConfirmTransactionPacket.Read(buffer);

                                Console.WriteLine(
                                    $"WindowId: {packet.WindowId}, " +
                                    $"ActionNumber: {packet.ActionNumber}, " +
                                    $"Accepted: {packet.Accepted}, ");
                            }
                            break;
                        case ServerboundPlayingPacket.ClickWindowPacketId:
                            {
                                ClickWindowPacket packet = ClickWindowPacket.Read(buffer);

                                {
                                    SlotData slotData = SlotData.Read(packet.Data);

                                    Console.WriteLine();
                                    Console.WriteLine(
                                        $"WindowId: {packet.WindowId}, " +
                                        $"SlotNumber: {packet.SlotNumber}, " +
                                        $"ButtonNumber: {packet.ButtonNumber}, " +
                                        $"ActionNumber: {packet.ActionNumber}, " +
                                        $"ModeNumber: {packet.ModeNumber}, " +
                                        $"SlotData.Id: {slotData.Id}, " +
                                        $"SlotData.Count: {slotData.Count}, ");
                                }

                                if (packet.WindowId < 0)
                                    throw new UnexpectedValueException($"ClickWindowPacket.WindowId {packet.WindowId}");

                                Debug.Assert(packet.WindowId >= 0);

                                if (!_windowHelper.IsWindowOpened())
                                    throw new UnexpectedValueException($"ClickWindowPacket.WindowId {packet.WindowId}");



                                /*switch (packet.ModeNumber)
                                {
                                    default:
                                        throw new UnexpectedValueException($"ClickWindowPacket.ModeNumber {packet.ModeNumber}");
                                    case 0:
                                        _windowHelper.ClickLeftMouseButton(packet.SlotNumber);
                                        break;
                                }*/

                                _outPackets.Enqueue(new ClientboundConfirmTransactionPacket(
                                        (sbyte)packet.WindowId, packet.ActionNumber, true));

                                /*{
                                    SlotData slotData = new(280, 1, 0);
                                    _outPackets.Enqueue(new SetSlotPacket(0, 27, slotData.WriteData()));
                                }
                                {
                                    SlotData slotData = new();
                                    _outPackets.Enqueue(new SetSlotPacket(-1, 27, slotData.WriteData()));
                                }*/
                            }
                            break;
                        case ServerboundPlayingPacket.ServerboundCloseWindowPacketId:
                            {
                                ServerboundCloseWindowPacket packet =
                                    ServerboundCloseWindowPacket.Read(buffer);

                                if (packet.WindowId < 0)
                                    throw new UnexpectedValueException($"ClickWindowPacket.WindowId {packet.WindowId}");

                                if (_windowHelper.GetWindowId() != packet.WindowId)
                                    throw new UnexpectedValueException($"ServerboundCloseWIndowPacket.WindowId {packet.WindowId}");

                                

                                if (_windowHelper.GetWindowId() > 0)
                                {
                                    _windowHelper.ResetWindow(Id, _outPackets);
                                }
                                else
                                {
                                    // TODO: if window closed that has zero id and
                                    // a item in cursor slot is not empty, the item must be droped.
                                }

                            }
                            break;
                        case ServerboundPlayingPacket.ResponseKeepAlivePacketId:
                            {
                                ResponseKeepAlivePacket packet = ResponseKeepAlivePacket.Read(buffer);

                                keepaliveChecker.Confirm(packet.Payload);
                            }
                            break;
                        case ServerboundPlayingPacket.PlayerPacketId:
                            {
                                PlayerPacket packet = PlayerPacket.Read(buffer);

                                if (!_teleportationRecords.Empty)
                                {
                                    Console.Write("Ignore Any Controls");
                                    break;
                                }

                                player.Stand(packet.OnGround);
                            }
                            break;
                        case ServerboundPlayingPacket.PlayerPositionPacketId:
                            {
                                PlayerPositionPacket packet = PlayerPositionPacket.Read(buffer);

                                if (!_teleportationRecords.Empty)
                                {
                                    Console.Write("Ignore Any Controls");
                                    break;
                                }

                                player.Control(new(packet.X, packet.Y, packet.Z));
                                player.Stand(packet.OnGround);
                            }
                            break;
                        case ServerboundPlayingPacket.PlayerPosAndLookPacketId:
                            {
                                PlayerPosAndLookPacket packet = PlayerPosAndLookPacket.Read(buffer);

                                if (!_teleportationRecords.Empty)
                                {
                                    Console.Write("Ignore Any Controls");
                                    break;
                                }

                                player.Control(new(packet.X, packet.Y, packet.Z));
                                player.Rotate(new(packet.Yaw, packet.Pitch));
                                player.Stand(packet.OnGround);
                            }
                            break;
                        case ServerboundPlayingPacket.PlayerLookPacketId:
                            {
                                PlayerLookPacket packet = PlayerLookPacket.Read(buffer);

                                if (!_teleportationRecords.Empty)
                                {
                                    Console.Write("Ignore Any Controls");
                                    break;
                                }

                                player.Rotate(new(packet.Yaw, packet.Pitch));
                                player.Stand(packet.OnGround);
                            }
                            break;
                        case ServerboundPlayingPacket.EntityActionPacketId:
                            {
                                EntityActionPacket packet = EntityActionPacket.Read(buffer);

                                if (!_teleportationRecords.Empty)
                                {
                                    Console.Write("Ignore Any Controls");
                                    break;
                                }

                                switch (packet.ActionId)
                                {
                                    default:
                                        throw new UnexpectedValueException("EntityAction.ActoinId");
                                    case 0:
                                        if (player.IsSneaking)
                                            throw new UnexpectedValueException("Entity.Sneaking");
                                        /*Console.Write("Seanking!");*/
                                        player.Sneak();
                                        break;
                                    case 1:
                                        if (!player.IsSneaking)
                                            throw new UnexpectedValueException("Entity.Sneaking");
                                        /*Console.Write("Unseanking!");*/
                                        player.Unsneak();
                                        break;
                                    case 3:
                                        if (player.IsSprinting)
                                            throw new UnexpectedValueException("Entity.Sprinting");
                                        player.Sprint();
                                        break;
                                    case 4:
                                        if (!player.IsSprinting)
                                            throw new UnexpectedValueException("Entity.Sprinting");
                                        player.Unsprint();
                                        break;
                                }

                                if (packet.JumpBoost > 0)
                                    throw new UnexpectedValueException("EntityAction.JumpBoost");

                            }
                            break;
                    }

                    if (!buffer.Empty)
                        throw new BufferOverflowException();
                }
            }
            catch (UnexpectedClientBehaviorExecption e)
            {
                // TODO: send disconnected message to client.

                Console.WriteLine(e.Message);
                player.Disconnect();

                throw new DisconnectedClientException();
            }
            catch (DisconnectedClientException)
            {
                player.Disconnect();
                throw;
            }
            catch (TryAgainException)
            {

            }

            foreach (TeleportationRecord record in _teleportationRecords.GetValues())
                record.Update();

            keepaliveChecker.Update(serverTicks, _outPackets);
        }

        public void UpdatePlayerList(PlayerList playerList)
        {
            foreach ((Guid uniqueId, string username) in playerList.GetAddedPlayers())
            {
                _outPackets.Enqueue(new AddPlayerListItemPacket(uniqueId, username));
            }
            foreach ((Guid uniqueId, string username) in playerList.GetRemovedPlayers())
            {
                _outPackets.Enqueue(new RemovePlayerListItemPacket(uniqueId));
            }
        }

        // TODO: Make chunks to readonly using interface? in this function.
        public void RenderChunks(Table<Chunk.Vector, Chunk> chunks, Player player)
        {
            Debug.Assert(!_isDisposed);

            Chunk.Vector pChunkCenter = Chunk.Vector.Convert(player.Position);
            int d = Settings.renderDistance;
            Debug.Assert(d >= ClientsideSettings.MinRenderDistance);
            Debug.Assert(d <= ClientsideSettings.MaxRenderDistance);

            Chunk.Grid grid = Chunk.Grid.GenerateAround(pChunkCenter, d);

            if (_renderedChunkGrid == null)
            {
                int mask; byte[] data;

                foreach (Chunk.Vector pChunk in grid.GetVectors())
                {
                    /*if (chunks.Contains(pChunk))
                    {
                        Chunk chunk = chunks.Lookup(pChunk);
                        (mask, data) = Chunk.Write(chunk);
                    }
                    else
                    {
                        (mask, data) = Chunk.Write();
                    }*/
                    (mask, data) = Chunk.Write2();

                    _loadChunkPackets.Enqueue(new LoadChunkPacket(
                        pChunk.x, pChunk.z, true, mask, data));
                }

            }
            else
            {
                Chunk.Grid gridPrev = _renderedChunkGrid;

                if (gridPrev.Equals(grid))
                    return;

                Chunk.Grid gridBetween = Chunk.Grid.GenerateBetween(grid, gridPrev);

                foreach (Chunk.Vector pChunk in grid.GetVectors())
                {
                    if (gridBetween.Contains(pChunk))
                        continue;

                    int mask; byte[] data;

                    /*if (chunks.Contains(pChunk))
                    {
                        Chunk chunk = chunks.Lookup(pChunk);
                        (mask, data) = Chunk.Write(chunk);
                    }
                    else
                    {
                        (mask, data) = Chunk.Write();
                    }*/

                    (mask, data) = Chunk.Write2();

                    _loadChunkPackets.Enqueue(new LoadChunkPacket(
                        pChunk.x, pChunk.z, true, mask, data));
                }

                foreach (Chunk.Vector pChunk in gridPrev.GetVectors())
                {
                    if (gridBetween.Contains(pChunk))
                        continue;

                    _outPackets.Enqueue(new UnloadChunkPacket(pChunk.x, pChunk.z));
                }
            }

            _renderedChunkGrid = grid;

        }

        public void RenderEntities(Player ownPlayer, EntityRenderingTable entitySearchTable)
        {
            Debug.Assert(!_isDisposed);

            using Queue<IRenderOnlyEntity> newEntities = new();
            using Queue<IRenderOnlyEntity> entities = new();

            using Set<int> prevRenderedEntityIds = _renderedEntityIds;
            Set<int> renderedEntityIds = new();

            Debug.Assert(_renderedChunkGrid != null);
            foreach (Chunk.Vector pChunk in _renderedChunkGrid.GetVectors())
            {
                if (!entitySearchTable.Contains(pChunk))
                    continue;

                IReadOnlyTable<int, IRenderOnlyEntity> entitiesInChunk = 
                    entitySearchTable.Search(pChunk);
                foreach (IRenderOnlyEntity entity in entitiesInChunk.GetValues())
                {
                    if (entity.Id == ownPlayer.Id) continue;

                    if (prevRenderedEntityIds.Contains(entity.Id))
                    {
                        prevRenderedEntityIds.Extract(entity.Id);
                        entities.Enqueue(entity);
                    }
                    else
                        newEntities.Enqueue(entity);

                    renderedEntityIds.Insert(entity.Id);
                }
            }

            while (!newEntities.Empty)
            {
                IRenderOnlyEntity entity = newEntities.Dequeue();
                Debug.Assert(entity.Id != ownPlayer.Id);

                entity.Spawn(_outPackets);

                entity.AddRenderer(_outPackets);
                
            }

            while (!entities.Empty)
            {
                IRenderOnlyEntity entity = entities.Dequeue();
                Debug.Assert(entity.Id != ownPlayer.Id);

                entity.AddRenderer(_outPackets);
            }

            if (!prevRenderedEntityIds.Empty)
            {
                int[] despawnedPlayerIds = prevRenderedEntityIds.Flush();

                _outPackets.Enqueue(new DestroyEntitiesPacket(despawnedPlayerIds));
            }

            Debug.Assert(newEntities.Empty);
            Debug.Assert(entities.Empty);

            _renderedEntityIds = renderedEntityIds;
        }

        /// <summary>
        /// TODO: Add description.
        /// </summary>
        /// <exception cref="DisconnectedClientException">TODO: Why it's thrown.</exception>
        public void SendData(Player player)
        {
            Debug.Assert(!_isDisposed);

            if (_outPackets.Empty) return;

            using Buffer buffer = new();

            try
            {
                while (!_loadChunkPackets.Empty)
                {
                    LoadChunkPacket packet = _loadChunkPackets.Dequeue();

                    packet.Write(buffer);
                    _client.Send(buffer);

                    Debug.Assert(buffer.Empty);
                }

                while (!_outPackets.Empty)
                {
                    ClientboundPlayingPacket packet = _outPackets.Dequeue();

                    if (packet is TeleportPacket teleportPacket)
                    {
                        TeleportationRecord report = new(teleportPacket.Payload);
                        _teleportationRecords.Enqueue(report);
                    }
                    /*else if (packet is EntityLookPacket)
                    {
                        Console.Write("LookPacket!");
                    }*/

                    packet.Write(buffer);
                    _client.Send(buffer);

                    Debug.Assert(buffer.Empty);
                }
            }
            catch (DisconnectedClientException)
            {
                player.Disconnect();
                buffer.Flush();

                throw;
            }

            Debug.Assert(_outPackets.Empty);
        }

        private void Dispose(bool disposing)
        {
            if (_isDisposed) return;

            Debug.Assert(_renderedEntityIds.Empty);
            Debug.Assert(_teleportationRecords.Empty);
            Debug.Assert(_loadChunkPackets.Empty);
            Debug.Assert(_outPackets.Empty);

            if (disposing == true)
            {
                // managed objects
                _client.Dispose();

                _renderedEntityIds.Dispose();
                _teleportationRecords.Dispose();
                _loadChunkPackets.Dispose();
                _outPackets.Dispose();
            }

            // unmanaged objects

            _isDisposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void Close()
        {
            // TODO: Release resources corrently for no garbage.
            _renderedEntityIds.Flush();
            _teleportationRecords.Flush();
            _loadChunkPackets.Flush();
            _outPackets.Flush();

            Dispose();
        }

    }

    public class ConnectionListener
    {
        private enum SetupSteps
        {
            JoinGame = 0,
            ClientSettings,
            PluginMessage, 
            StartPlay,
        }

        private readonly ConcurrentQueue<
            (Client, Guid, string, int, Connection.ClientsideSettings?, SetupSteps)
            > _clients = new();

        internal void Add(Client client, Guid userId, string username)
        {
            _clients.Enqueue((client, userId, username, -1, null, SetupSteps.JoinGame));
        }

        public void Accept(
            EntityIdList entityIdList,
            Queue<(Connection, Player)> connections, Queue<Entity> entities,
            EntityRenderingTable entityRenderingTable,
            PlayerList playerList,
            Entity.Vector posInit, Entity.Angles lookInit)
        {
            if (_clients.Empty) return;

            bool start, close;

            int count = _clients.Count;
            for (int i = 0; i < count; ++i)
            {
                using Buffer buffer = new();

                (Client client, 
                    Guid uniqueId, 
                    string username, 
                    int entityId,
                    Connection.ClientsideSettings? settings,
                    SetupSteps step) = 
                    _clients.Dequeue();
                start = close = false;

                try
                {
                    if (step == SetupSteps.JoinGame)
                    {
                        /*Console.WriteLine("JoinGame!");*/

                        Debug.Assert(settings == null);
                        Debug.Assert(entityId == -1);

                        // TODO: If already player exists, use id of that player object, not new alloc id.
                        entityId = entityIdList.Alloc();

                        JoinGamePacket packet = new(entityId, 0, 0, 0, "default", false);  // TODO
                        packet.Write(buffer);
                        client.Send(buffer);

                        step = SetupSteps.ClientSettings;
                    }

                    if (step == SetupSteps.ClientSettings)
                    {
                        /*Console.WriteLine("ClientSettings!");*/

                        Debug.Assert(settings == null);
                        Debug.Assert(entityId >= 0);

                        client.Recv(buffer);

                        int packetId = buffer.ReadInt(true);
                        if (ServerboundPlayingPacket.SetClientSettingsPacketId != packetId)
                            throw new UnexpectedPacketException();

                        SetClientSettingsPacket packet = SetClientSettingsPacket.Read(buffer);

                        if (buffer.Size > 0)
                            throw new BufferOverflowException();

                        settings = new(packet.RenderDistance);
    
                        step = SetupSteps.PluginMessage;
                    }

                    if (step == SetupSteps.PluginMessage)
                    {
                        /*Console.WriteLine("PluginMessage!");*/

                        Debug.Assert(settings != null);
                        Debug.Assert(entityId >= 0);

                        client.Recv(buffer);

                        int packetId = buffer.ReadInt(true);
                        if (0x09 != packetId)
                            throw new UnexpectedPacketException();

                        buffer.Flush();

                        if (buffer.Size > 0)
                            throw new BufferOverflowException();

                        step = SetupSteps.StartPlay;
                    }

                    Debug.Assert(!start);
                    Debug.Assert(!close);

                    start = true;
                }
                catch (TryAgainException)
                {
                    Debug.Assert(!start);
                    Debug.Assert(!close);

                    /*Console.WriteLine("TryAgainException!");*/
                }
                catch (UnexpectedClientBehaviorExecption)
                {
                    Debug.Assert(!start);
                    Debug.Assert(!close);

                    buffer.Flush();

                    close = true;

                    // TODO: Send why disconnected...

                    /*Console.WriteLine("UnexpectedBehaviorExecption!");*/
                }
                catch (DisconnectedClientException)
                {
                    Debug.Assert(!start);
                    Debug.Assert(!close);

                    buffer.Flush();

                    close = true;

                    /*Console.WriteLine("DisconnectedException!");*/
                }

                if (!start)
                {
                    if (!close)
                    {
                        Debug.Assert(step >= SetupSteps.JoinGame ? entityId >= 0 : true);
                        Debug.Assert(step >= SetupSteps.PluginMessage ? settings != null: true);

                        _clients.Enqueue((
                            client, 
                            uniqueId, username, 
                            entityId, 
                            settings,
                            step));
                    }
                    else
                    {
                        if (step >= SetupSteps.JoinGame)
                            entityIdList.Dealloc(entityId);

                        client.Close();
                    }

                    continue;
                }

                Debug.Assert(step == SetupSteps.StartPlay);
                Debug.Assert(!close);
                /*Console.Write($"Start init connection!: entityId: {entityId} ");*/

                Player player = new(
                    entityIdList,
                    entityRenderingTable,
                    entityId, 
                    uniqueId, 
                    posInit, lookInit, 
                    playerList,
                    username);

                Debug.Assert(settings != null);
                Connection conn = new(
                    entityId,
                    client,
                    uniqueId, username,
                    settings,
                    playerList,
                    player);

                connections.Enqueue((conn, player));
                entities.Enqueue(player);

                /*Console.Write("Finish init connection!");*/

            }

        }

    }

    public class GlobalListener
    {
        private static readonly TimeSpan _PendingTimeout = TimeSpan.FromSeconds(1);

        private readonly ConnectionListener _connListener;

        public GlobalListener(ConnectionListener connListener)
        {
            _connListener = connListener;
        }

        private int HandleVisitors(
            Queue<Client> visitors, Queue<int> levelQueue)
        {
            /*Console.Write(".");*/

            int count = visitors.Count;
            Debug.Assert(count == levelQueue.Count);
            if (count == 0) return 0;

            bool close, success;

            for (; count > 0; --count)
            {
                close = success = false;

                Client client = visitors.Dequeue();
                int level = levelQueue.Dequeue();

                /*Console.WriteLine($"count: {count}, level: {level}");*/

                Debug.Assert(level >= 0);
                using Buffer buffer = new();

                try
                {
                    if (level == 0)
                    {
                        /*Console.WriteLine("Handshake!");*/
                        client.Recv(buffer);

                        int packetId = buffer.ReadInt(true);
                        if (ServerboundHandshakingPacket.SetProtocolPacketId != packetId)
                            throw new UnexpectedPacketException();

                        SetProtocolPacket packet = SetProtocolPacket.Read(buffer);

                        if (!buffer.Empty)
                            throw new BufferOverflowException();

                        switch (packet.NextState)
                        {
                            default:
                                throw new InvalidEncodingException();
                            case Packet.States.Status:
                                level = 1;
                                break;
                            case Packet.States.Login:
                                level = 3;
                                break;
                        }

                    }

                    if (level == 1)  // Request
                    {
                        /*Console.WriteLine("Request!");*/
                        client.Recv(buffer);

                        int packetId = buffer.ReadInt(true);
                        if (ServerboundStatusPacket.RequestPacketId != packetId)
                            throw new UnexpectedPacketException();

                        RequestPacket requestPacket = RequestPacket.Read(buffer);

                        if (!buffer.Empty)
                            throw new BufferOverflowException();

                        // TODO
                        ResponsePacket responsePacket = new(100, 10, "Hello, World!");
                        responsePacket.Write(buffer);
                        client.Send(buffer);

                        level = 2;
                    }

                    if (level == 2)  // Ping
                    {
                        /*Console.WriteLine("Ping!");*/
                        client.Recv(buffer);

                        int packetId = buffer.ReadInt(true);
                        if (ServerboundStatusPacket.PingPacketId != packetId)
                            throw new UnexpectedPacketException();

                        PingPacket inPacket = PingPacket.Read(buffer);

                        if (!buffer.Empty)
                            throw new BufferOverflowException();

                        PongPacket outPacket = new(inPacket.Payload);
                        outPacket.Write(buffer);
                        client.Send(buffer);
                    }

                    if (level == 3)  // Start Login
                    {
                        client.Recv(buffer);

                        int packetId = buffer.ReadInt(true);
                        if (ServerboundLoginPacket.StartLoginPacketId != packetId)
                            throw new UnexpectedPacketException();

                        StartLoginPacket inPacket = StartLoginPacket.Read(buffer);

                        if (!buffer.Empty)
                            throw new BufferOverflowException();

                        // TODO: Check username is empty or invalid.

                        /*Console.Write("Start http request!");

                        // TODO: Use own http client in common library.
                        using HttpClient httpClient = new();
                        string url = string.Format("https://api.mojang.com/users/profiles/minecraft/{0}", inPacket.Username);
                        *//*Console.WriteLine(inPacket.Username);
                        Console.WriteLine($"url: {url}");*//*
                        using HttpRequestMessage request = new(HttpMethod.Get, url);

                        // TODO: handle HttpRequestException
                        using HttpResponseMessage response = httpClient.Send(request);

                        using Stream stream = response.Content.ReadAsStream();
                        using StreamReader reader = new(stream);
                        string str = reader.ReadToEnd();
                        System.Collections.Generic.Dictionary<string, string>? dictionary =
                            JsonSerializer.Deserialize<System.Collections.Generic.Dictionary<string, string>>(str);
                        Debug.Assert(dictionary != null);

                        Guid userId = Guid.Parse(dictionary["id"]);
                        string username = dictionary["name"];  // TODO: check username is valid
                        *//*Console.WriteLine($"userId: {userId}");
                        Console.WriteLine($"username: {username}");*//*

                        // TODO: Handle to throw exception
                        Debug.Assert(inPacket.Username == username);

                        Console.Write("Finish http request!");*/

                        Guid userId = Guid.NewGuid();
                        string username = inPacket.Username;

                        LoginSuccessPacket outPacket1 = new(userId, username);
                        outPacket1.Write(buffer);
                        client.Send(buffer);

                        // TODO: Must dealloc id when connection is disposed.
                        _connListener.Add(client, userId, username);

                        success = true;
                    }


                    Debug.Assert(buffer.Size == 0);

                    if (!success) close = true;

                }
                catch (TryAgainException)
                {
                    Debug.Assert(success == false);
                    Debug.Assert(close == false);

                    /*Console.Write($"TryAgain!");*/
                }
                catch (UnexpectedClientBehaviorExecption)
                {
                    Debug.Assert(success == false);
                    Debug.Assert(close == false);

                    close = true;

                    buffer.Flush();

                    if (level >= 3)  // >= Start Login level
                    {
                        Debug.Assert(false);
                        // TODO: Handle send Disconnect packet with reason.
                    }

                }
                catch (DisconnectedClientException)
                {
                    Debug.Assert(success == false);
                    Debug.Assert(close == false);
                    close = true;

                    /*Console.Write("~");*/

                    buffer.Flush();

                    /*Console.Write($"EndofFileException");*/
                }

                Debug.Assert(buffer.Empty);

                if (!success)
                {
                    if (close == false)
                    {
                        visitors.Enqueue(client);
                        levelQueue.Enqueue(level);
                    }
                    else
                    {
                        client.Close();
                    }

                    continue;
                }
                    
                Debug.Assert(close == false);

            }

            return visitors.Count;
        }

        public void StartRoutine(ConsoleApplication app, ushort port)
        {
            using Socket socket = SocketMethods.Establish(port);

            using Queue<Client> visitors = new();
            /*
             * 0: Handshake
             * 1: Request
             * 2: Ping
             * 3: Start Login
             */
            using Queue<int> levelQueue = new();

            SocketMethods.SetBlocking(socket, true);

            while (app.Running)
            {
                Console.Write(">");

                try
                {
                    if (!SocketMethods.IsBlocking(socket) &&
                        HandleVisitors(visitors, levelQueue) == 0)
                    {
                        SocketMethods.SetBlocking(socket, true);
                    }

                    if (SocketMethods.Poll(socket, _PendingTimeout))
                    {
                        Client client = Client.Accept(socket);
                        visitors.Enqueue(client);
                        levelQueue.Enqueue(0);

                        SocketMethods.SetBlocking(socket, false);
                    }
                }
                catch (TryAgainException)
                {
                    /*Console.WriteLine("TryAgainException!");*/
                    continue;
                }

            }

            // TODO: Handle client, send Disconnet Packet if the client's step is after StartLogin;
        }

    }


}

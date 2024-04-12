using Applications;
using Containers;
using Protocol;
using System;
using System.Threading;

namespace Application
{


    // TODO: Change to correct name.
    /*public class PacketTable<T> : IDisposable where T : PlayingPacket
    {
        private bool _disposed = false;

        private readonly Table<int, Queue<T>> _data = new();

        public PacketTable() { }

        ~PacketTable()
        {
            Dispose(false);
        }

        public void Init(int key)
        {
            Debug.Assert(!_disposed);

            Debug.Assert(!_data.Contains(key));

            Queue<T> queue = new();
            _data.Insert(key, queue);
        }

        public void Close(int key)
        {
            Debug.Assert(!_disposed);

            Debug.Assert(_data.Contains(key));

            Queue<T> queue = _data.Extract(key);
            Debug.Assert(queue.Count == 0);
        }

        public void Enqueue(int key, T value)
        {
            Debug.Assert(!_disposed);

            Debug.Assert(_data.Contains(key));

            Queue<T> queue = _data.Lookup(key);
            Debug.Assert(queue != null);

            queue.Enqueue(value);
        }

        public T Dequeue(int key)
        {
            Debug.Assert(!_disposed);

            Debug.Assert(_data.Contains(key));

            Queue<T> queue = _data.Lookup(key);
            Debug.Assert(queue != null);

            return queue.Dequeue();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;

            if (disposing == true)
            {
                // managed objects
                _data.Dispose();
            }

            // unmanaged objects

            _disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

    }*/

    public sealed class Server : ConsoleApplication
    {
        private bool _disposed = false;

        private readonly NumList _idList = new();
        private readonly ConcurrentQueue<Connection> _newConnections = new();
         
        /*private readonly Table<(int, int), Chunk> _chunks = new();*/

        private Server() { }

        ~Server() => Dispose(false);


        /*       private void InitOutPackets(int id)
        {
            Debug.Assert(_outPackets.ContainsKey(id) == false);
            _outPackets.Add(id, new());
        }*/

        private void StartCoreRoutine()
        {
            while (Running)
            {
                // recv packets using connections

                // Barrier

                int count = _newJoinedConnections.Count;
                for (int i = 0; i < count; ++i)
                {
                    Connection conn = _newJoinedConnections.Dequeue();


                    int id = conn.Id;

                    // TODO
                    Player player = new(id, new(0, 60, 0));
                    _newJoinedPlayers.Enqueue(player);

                    _inPackets.Init(id);
                    _outPackets.Init(id);

                    _connections.Enqueue(conn);
                }

                // Barrier

                // handle players

                // Barrier

                while (!_newJoinedPlayers.Empty)
                {
                    Player player = _newJoinedPlayers.Dequeue();

                    // load chunks


                    _players.Enqueue(player);
                }

                // Barrier

                while (_connections.Count > 0)
                {
                    Connection conn = _connections.Dequeue();

                    // send packets
                    _connections.Enqueue(conn);
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing == true)
                {
                    // Release managed resources.
                    _idList.Dispose();
                    _newConnections.Dispose();
                }

                // Release unmanaged resources.

                _disposed = true;
            }

            base.Dispose(disposing);
        }

        public static void Main()
        {
            ushort port = 25565;

            using Server app = new();


            Console.WriteLine("Hello, World!");

            Listener listener = new();
            app.Run(() => 
                listener.StartRoutine(app, port, app._idList, app._newConnections));

            while (app.Running)
            {
                Thread.Sleep(1000);
            }


        }

    }

}


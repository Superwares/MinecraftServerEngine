using Containers;

namespace Protocol
{
    internal sealed class EntityRenderer : System.IDisposable
    {
        private bool _disposed = false;

        private readonly Table<int, Queue<ClientboundPlayingPacket>> _table = new();  // Disposable

        public EntityRenderer() { }

        ~EntityRenderer() => System.Diagnostics.Debug.Assert(false);

        public void Render(ClientboundPlayingPacket packet)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            foreach (var outPackets in _table.GetValues())
            {
                outPackets.Enqueue(packet);
            }
        }

        public void Add(int id, Queue<ClientboundPlayingPacket> outPackets)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            _table.Insert(id, outPackets);
        }

        public void Remove(int id)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            _table.Extract(id);
        }

        public void Flush()
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            _table.Flush();
        }

        private void Dispose(bool disposing)
        {
            if (_disposed) return;

            System.Diagnostics.Debug.Assert(_table.Empty);

            if (disposing == true)
            {
                // Release managed resources.
                _table.Dispose();
            }

            // Release unmanaged resources.

            _disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            System.GC.SuppressFinalize(this);
        }

        public void Close() => Dispose();

    }

}

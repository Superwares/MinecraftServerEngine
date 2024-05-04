
using Containers;

namespace Protocol
{
    internal sealed class WorldLoader : System.IDisposable
    {
        private bool _disposed = false;

        private const int MAX_LOAD_COUNT = 10;

        private Set<Chunk.Vector> _chunkPositions = new();
        private Table<int, EntityRendererManager> _entityRendererManagers = new();  // Disposable

        public void Load(
            World world, Player player, 
            int d, Queue<ClientboundPlayingPacket> outPackets)
        {
            System.Diagnostics.Debug.Assert(d > 0);

            Chunk.Vector p = Chunk.Vector.Convert(player.Position);
            Chunk.Grid grid = Chunk.Grid.Generate(p, d);

            using Table<int, EntityRendererManager> prevEntityRendererManagers = _entityRendererManagers;
            Table<int, EntityRendererManager> entityRendererManagers = new();

            foreach (Chunk.Vector p in grid.GetVectorsInSpiral())
            {

            }

            _entityRendererManagers = entityRendererManagers;
        }

        private void Dispose(bool disposing)
        {
            if (_disposed) return;

            // Assertion

            if (disposing == true)
            {
                // managed objects

            }

            // unmanaged objects

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

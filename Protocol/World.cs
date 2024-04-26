using Containers;

namespace Protocol
{
    public abstract class World : System.IDisposable
    {
        private bool _disposed = false;

        public readonly int Id;

        private readonly Table<Chunk.Vector, Table<int, Entity>>
            _chunkToEntityIds = new();  // Disposable
        private readonly Table<int, Chunk.Grid> _entityIdToChunkGrid = new();  // Disposable

        private readonly Table<System.Guid, Player> _disconnectedPlayers = new();

        public World(int id) 
        {
            Id = id;
        }

        ~World() => System.Diagnostics.Debug.Assert(false);

        public virtual bool DetermineDespawningEntity(
            Entity entity, Table<System.Guid, int> userIdToWorldId)
        {
            throw new System.NotImplementedException();
        }

        public void StartRoutine()
        {

        }

        public Player InitPlayer()
        {
            throw new System.NotImplementedException();
        }

        private void SpaenEntity()
        {
            throw new System.NotImplementedException();
        }

        internal bool Contains(Chunk.Vector pos)
        {
            throw new System.NotImplementedException();
        }

        internal IReadOnlyTable<int, Entity> Search(Chunk.Vector pos)
        {
            throw new System.NotImplementedException();
        }

        internal void Update(int entityId, Entity.Vector pos)
        {
            throw new System.NotImplementedException();
        }

        public void Despawn()
        {
            throw new System.NotImplementedException();
        }

        public Entity RaycastClosestEntity(Entity.Vector p, Entity.Vector u, int d)
        {
            throw new System.NotImplementedException();
        }

        public Entity[] RaycastEntities(Entity.Vector p, Entity.Vector u, int d)
        {
            throw new System.NotImplementedException();
        }

        public Entity[] GetCollidedEntities(bool resolve)
        {
            throw new System.NotImplementedException();
        }

        private void Dispose(bool disposing)
        {
            if (_disposed) return;

            // Assertion.

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
            System.GC.SuppressFinalize(this);
        }

    }

    public sealed class Lobby : World
    {

    }

    public sealed class GameWorld : World
    {

    }

}

using Containers;
using System.Diagnostics;
using System.Numerics;

namespace Protocol
{
    public abstract class World : System.IDisposable
    {
        private bool _disposed = false;

        public readonly int Id;

        private readonly PlayerList _PlayerList = new();  // Disposable

        private readonly NumList _EntityIdList = new();  // Disposable

        private readonly Entity.Vector _PosSpawning;
        private readonly Entity.Angles _LookSpawning;

        private readonly Table<Chunk.Vector, Chunk> _Chunks = new();  // Disposable

        private readonly 
            Table<Chunk.Vector, Table<int, Entity>> _ChunkToEntities = new();  // Disposable
        private readonly Table<int, Chunk.Grid> _EntityToChunkGrid = new();  // Disposable

        private readonly Queue<Entity> _EntityPool = new();  // Disposable
        private readonly Queue<Entity> _DespawnedEntities = new();  // Disposable

        public World(
            int id,
            Entity.Vector posSpawning, Entity.Angles lookSpawning)
        {
            Id = id;
            _PosSpawning = posSpawning; _LookSpawning = lookSpawning;

        }

        ~World() => System.Diagnostics.Debug.Assert(false);

        public virtual bool DetermineAndDespawnEntity(Entity entity)
        {
            CloseEntityRendering(entity);
            entity.Flush();
            entity.Close();

            return true;
        }

        protected virtual bool CanConnectPlayer(System.Guid userId)
        {
            return false;
        }

        internal virtual void InitPlayer(
            Connection connection, 
            string username, System.Guid userId)
        {
            Player player = new(
                _EntityIdList,
                userId,
                _PosSpawning, _LookSpawning,
                username,
                _PlayerList);
            player.Connect(connection);
            _EntityPool.Enqueue(player);

        }

        public Queue<Entity> SpawnEntities()
        {
            Queue<Entity> entities = new();

            while (!_EntityPool.Empty)
            {
                Entity entity = _EntityPool.Dequeue();

                if (entity is Player player)
                {
                    _PlayerList.InitPlayer(player.UniqueId, player.Username);
                }

                InitEntityRendering(entity);
                entities.Enqueue(entity);
            }

            return entities;
        }

        public virtual void StartRoutine(long serverTicks)
        {
            _PlayerList.StartRoutine(serverTicks);
        }

        private void InitEntityRendering(Entity entity)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            Chunk.Grid grid = Chunk.Grid.Generate(entity.Position, entity.GetBoundingBox());
            foreach (Chunk.Vector p in grid.GetVectors())
            {
                if (!_ChunkToEntities.Contains(p))
                    _ChunkToEntities.Insert(p, new());

                Table<int, Entity> entities = _ChunkToEntities.Lookup(p);
                entities.Insert(entity.Id, entity);
            }

            Debug.Assert(!_EntityToChunkGrid.Contains(entity.Id));
            _EntityToChunkGrid.Insert(entity.Id, grid);
        }

        private void CloseEntityRendering(Entity entity)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            Debug.Assert(_EntityToChunkGrid.Contains(entity.Id));
            Chunk.Grid grid = _EntityToChunkGrid.Extract(entity.Id);

            foreach (Chunk.Vector p in grid.GetVectors())
            {
                Table<int, Entity> entities = _ChunkToEntities.Lookup(p);

                Entity entityInChunk = entities.Extract(entity.Id);
                Debug.Assert(ReferenceEquals(entityInChunk, entity));

                if (entities.Empty)
                    _ChunkToEntities.Extract(p);
            }

        }

        internal bool ContainsChunk(Chunk.Vector p)
        {
            return _Chunks.Contains(p);
        }

        internal Chunk GetChunk(Chunk.Vector p)
        {
            return _Chunks.Lookup(p);
        }

        internal bool ContainsEntities(Chunk.Vector p)
        {
            return _ChunkToEntities.Contains(p);
        }

        internal System.Collections.Generic.IEnumerable<Entity> GetEntities(Chunk.Vector p)
        {
            Table<int, Entity> entities = _ChunkToEntities.Lookup(p);
            foreach (Entity entity in entities.GetValues())
            {
                yield return entity;
            }
        }

        internal void UpdateEntityRendering(Entity entity)
        {
            Debug.Assert(_EntityToChunkGrid.Contains(entity.Id));
            Chunk.Grid gridPrev = _EntityToChunkGrid.Extract(entity.Id);
            Chunk.Grid grid = Chunk.Grid.Generate(entity.Position, entity.GetBoundingBox());

            if (!gridPrev.Equals(grid))
            {
                Chunk.Grid gridBetween = Chunk.Grid.Generate(grid, gridPrev);

                foreach (Chunk.Vector pChunk in gridPrev.GetVectors())
                {
                    if (gridBetween.Contains(pChunk))
                        continue;

                    Table<int, Entity> entities = _ChunkToEntities.Lookup(pChunk);

                    Entity entityInChunk = entities.Extract(entity.Id);
                    Debug.Assert(ReferenceEquals(entityInChunk, entity));

                    if (entities.Empty)
                        _ChunkToEntities.Extract(pChunk);
                }

                foreach (Chunk.Vector pChunk in grid.GetVectors())
                {
                    if (gridBetween.Contains(pChunk))
                        continue;

                    if (!_ChunkToEntities.Contains(pChunk))
                        _ChunkToEntities.Insert(pChunk, new());

                    Table<int, Entity> entities = _ChunkToEntities.Lookup(pChunk);
                    entities.Insert(entity.Id, entity);
                }
            }

            _EntityToChunkGrid.Insert(entity.Id, grid);
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

        public virtual void Reset()
        {
            System.Diagnostics.Debug.Assert(!_disposed);
            
        }

        protected virtual void Dispose(bool disposing)
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

    public sealed class TemporaryWorld : World
    {
        private bool _disposed = false;

        public TemporaryWorld(
            int id,
            Entity.Vector posSpawning, Entity.Angles lookSpawning)
            : base(id, posSpawning, lookSpawning) { }

        ~TemporaryWorld() => System.Diagnostics.Debug.Assert(false);

        protected override void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                // Assertion.

                if (disposing == true)
                {
                    // Release managed resources.

                }

                // Release unmanaged resources.

                _disposed = true;
            }

            base.Dispose(disposing);
        }

    }


    public sealed class PermanentWorld : World
    {
        private bool _disposed = false;

        private readonly Table<System.Guid, Player> _disconnectedPlayers = new();

        public PermanentWorld(
            int id,
            Entity.Vector posSpawning, Entity.Angles lookSpawning)
            : base(id, posSpawning, lookSpawning) { }

        ~PermanentWorld() => System.Diagnostics.Debug.Assert(false);

        public override bool DetermineAndDespawnEntity(Entity entity)
        {
            if (entity is Player player)
            {
                if (!_disconnectedPlayers.Contains(player.UniqueId))
                    _disconnectedPlayers.Insert(player.UniqueId, player);

                return false;
            }

            return base.DetermineAndDespawnEntity(entity);
        }

        protected override bool CanConnectPlayer(System.Guid userId)
        {
            return _disconnectedPlayers.Contains(userId);
        }

        protected internal override Player AcquirePlayer(
            string username, System.Guid userId)
        {
            bool contains = _disconnectedPlayers.Contains(userId);
            if (contains)
            {
                Player player = _disconnectedPlayers.Extract(userId);
                return player;
            }

            return base.AcquirePlayer(username, userId);
        }

        protected override void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                // Assertion.

                if (disposing == true)
                {
                    // Release managed resources.

                }

                // Release unmanaged resources.

                _disposed = true;
            }

            base.Dispose(disposing);
        }

    }



}

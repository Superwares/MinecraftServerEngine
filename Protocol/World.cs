using Containers;
using System.Diagnostics;
using System.Numerics;

namespace Protocol
{
    public abstract class World : System.IDisposable
    {
        private bool _disposed = false;

        private readonly PlayerList _PlayerList = new();

        private readonly NumList _EntityIdList = new();  // Disposable

        private readonly Entity.Vector _PosSpawning;
        private readonly Entity.Angles _LookSpawning;

        private readonly Table<Chunk.Vector, Chunk> _Chunks = new();  // Disposable

        private readonly Queue<Entity> _EntitySpawningPool = new();  // Disposable
        private readonly Queue<Entity> _EntityDespawningPool = new();

        private readonly 
            Table<Chunk.Vector, Table<int, Entity>> _ChunkToEntities = new();  // Disposable
        private readonly Table<int, Chunk.Grid> _EntityToChunkGrid = new();  // Disposable

        private readonly bool _isPlayerDespawnedOnDisconnection = true;

        public World(
            int id,
            Entity.Vector posSpawning, Entity.Angles lookSpawning)
        {
            Id = id;
            _PosSpawning = posSpawning; _LookSpawning = lookSpawning;

        }

        ~World() => System.Diagnostics.Debug.Assert(false);

        internal virtual void SpawnOrConnectPlayer(
            Client client, int renderDistance,
            string username, System.Guid uniqueId)
        {
            int id = _EntityIdList.Alloc();
            Player player = new(
                id,
                uniqueId,
                _PosSpawning, _LookSpawning,
                username);
            _PlayerList.InitPlayer(player.UniqueId, player.Username);

            player.Connect();

            _EntitySpawningPool.Enqueue(player);

        }

        public void DespawnEntities()
        {
            while (!_EntityDespawningPool.Empty)
            {
                Entity entity = _EntityDespawningPool.Dequeue();

                _EntityIdList.Dealloc(entity.Id);

                CloseEntityRendering(entity);
                entity.Flush();
                entity.Close();
            }
        }

        public void MoveEntity(Entity entity)
        {
            entity.Move();

            UpdateEntityRendering(entity);
        }

        public void SpawnEntities(Queue<Entity> entities)
        {
            while (!_EntitySpawningPool.Empty)
            {
                Entity entity = _EntitySpawningPool.Dequeue();

                InitEntityRendering(entity);

                entities.Enqueue(entity);
            }
        }

        public bool StartEntitRoutine(long serverTicks, Entity entity)
        {
            entity.StartRoutine(serverTicks, this);

            if (entity is Player player)
            {
                if (!player.IsConnected)
                {
                    if (_isPlayerDespawnedOnDisconnection)
                    {
                        _EntityDespawningPool.Enqueue(entity);
                        return true;
                    }
                }

                StartPlayerRoutine(serverTicks, player);
            }
            else
            {
                if (entity.IsDead())
                {
                    _EntityDespawningPool.Enqueue(entity);
                    return true;
                }
            }


            return false;
        }

        protected virtual void StartPlayerRoutine(long serverTicks, Player player) 
        { }

        public virtual void StartRoutine(long serverTicks)
        {
            /*while (!_EntityPool.Empty)
            {
                Entity entity = _EntityPool.Dequeue();

                *//*_Entities.Insert(entity.Id, entity);*//*
                InitEntityRendering(entity);

                entities.Enqueue(entity);
            }

            _PlayerList.StartRoutine(serverTicks);*/
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

        /*public virtual void Reset()
        {
            System.Diagnostics.Debug.Assert(!_disposed);
            
        }*/

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

        public void Close() => Dispose();

    }


}

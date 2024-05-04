using Containers;
using System.Diagnostics;
using System.Numerics;

namespace Protocol
{
    public abstract class World : System.IDisposable
    {
        private bool _disposed = false;

        private readonly PlayerList _PLAYER_LIST = new();

        private readonly NumList _ENTITY_ID_LIST = new();  // Disposable

        private readonly Entity.Vector _POS_SPAWNING;
        private readonly Entity.Angles _LOOK_SPAWNING;

        private readonly Table<Chunk.Vector, Chunk> _CHUNKS = new();  // Disposable

        private readonly Queue<Entity> _ENTITY_SPAWNING_POOL = new();  // Disposable
        private readonly Queue<Entity> _ENTITY_DESPAWNING_POOL = new();  // Disposable

        private readonly 
            Table<Chunk.Vector, Table<int, Entity>> _CHUNK_TO_ENTITIES = new();  // Disposable
        private readonly Table<int, Chunk.Grid> _ENTITY_TO_CHUNK_GRID = new();  // Disposable

        private readonly Queue<Entity> _DESPAWNED_ENTITIES = new();  // Disposable

        private readonly bool _canDespawnPlayerWhenDisconnection = true;

        public World(Entity.Vector posSpawning, Entity.Angles lookSpawning)
        {
            _POS_SPAWNING = posSpawning; _LOOK_SPAWNING = lookSpawning;
        }

        ~World() => System.Diagnostics.Debug.Assert(false);

        internal PublicInventory _Inventory = new ChestInventory();

        internal void PlayerListConnect(
            System.Guid uniqueId, Queue<ClientboundPlayingPacket> renderer)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            _PLAYER_LIST.Connect(uniqueId, renderer);
        }

        internal void PlayerListDisconnect(System.Guid uniqueId)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            _PLAYER_LIST.Disconnect(uniqueId);
        }

        internal void PlayerListKeepAlive(long serverTicks, System.Guid uniqueId)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            _PLAYER_LIST.KeepAlive(serverTicks, uniqueId);
        }

        protected internal abstract bool CanJoinWorld();

        protected internal virtual bool CanSpawnOrConnectPlayer(System.Guid uniqueId)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            foreach (Entity entity in _ENTITY_DESPAWNING_POOL.GetValues())
            {
                if (entity.UniqueId == uniqueId)
                {
                    System.Diagnostics.Debug.Assert(entity is Player);
                    return false;
                }
            }

            return true;
        }

        internal virtual Player SpawnOrConnectPlayer(string username, System.Guid uniqueId)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            int id = _ENTITY_ID_LIST.Alloc();
            Player player = new(
                id,
                uniqueId,
                _POS_SPAWNING, _LOOK_SPAWNING,
                username);

            _PLAYER_LIST.InitPlayer(player.UniqueId, player.Username);

            _ENTITY_SPAWNING_POOL.Enqueue(player);

            return player;
        }

        public void DespawnEntities()
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            while (!_ENTITY_DESPAWNING_POOL.Empty)
            {
                Entity entity = _ENTITY_DESPAWNING_POOL.Dequeue();

                CloseEntityRendering(entity);

                _DESPAWNED_ENTITIES.Enqueue(entity);
            }

        }

        public void MoveEntity(Entity entity)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            entity.Move();

            UpdateEntityRendering(entity);
        }

        public void SpawnEntities(Queue<Entity> entities)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            while (!_ENTITY_SPAWNING_POOL.Empty)
            {
                Entity entity = _ENTITY_SPAWNING_POOL.Dequeue();

                InitEntityRendering(entity);

                entities.Enqueue(entity);
            }
        }

        public void Reset()
        {
            while (!_DESPAWNED_ENTITIES.Empty)
            {
                Entity entity = _DESPAWNED_ENTITIES.Dequeue();

                if (entity is Player player)
                {
                    _PLAYER_LIST.ClosePlayer(player.UniqueId);
                }

                _ENTITY_ID_LIST.Dealloc(entity.Id);
                entity.Close();
            }
        }

        public bool StartEntitRoutine(long serverTicks, Entity entity)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            entity.StartRoutine(serverTicks, this);

            if (entity is Player player)
            {
                if (!player.IsConnected)
                {
                    if (_canDespawnPlayerWhenDisconnection)
                    {
                        _ENTITY_DESPAWNING_POOL.Enqueue(entity);
                        return true;
                    }
                }

                StartPlayerRoutine(serverTicks, player);
            }
            else
            {
                if (entity.IsDead())
                {
                    _ENTITY_DESPAWNING_POOL.Enqueue(entity);
                    return true;
                }
            }


            return false;
        }

        protected virtual void StartPlayerRoutine(long serverTicks, Player player) 
        {
            System.Diagnostics.Debug.Assert(!_disposed);
        }

        protected virtual void StartSubRoutine(long serverTicks)
        { 
        System.Diagnostics.Debug.Assert(!_disposed);}

        public void StartRoutine(long serverTicks) 
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            _PLAYER_LIST.StartRoutine(serverTicks);

            StartSubRoutine(serverTicks);
        }

        private void InitEntityRendering(Entity entity)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            Chunk.Grid grid = Chunk.Grid.Generate(entity.Position, entity.GetBoundingBox());
            foreach (Chunk.Vector p in grid.GetVectors())
            {
                if (!_CHUNK_TO_ENTITIES.Contains(p))
                {
                    _CHUNK_TO_ENTITIES.Insert(p, new());
                }

                Table<int, Entity> entities = _CHUNK_TO_ENTITIES.Lookup(p);
                entities.Insert(entity.Id, entity);
            }

            Debug.Assert(!_ENTITY_TO_CHUNK_GRID.Contains(entity.Id));
            _ENTITY_TO_CHUNK_GRID.Insert(entity.Id, grid);
        }

        private void CloseEntityRendering(Entity entity)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            Debug.Assert(_ENTITY_TO_CHUNK_GRID.Contains(entity.Id));
            Chunk.Grid grid = _ENTITY_TO_CHUNK_GRID.Extract(entity.Id);

            foreach (Chunk.Vector p in grid.GetVectors())
            {
                Table<int, Entity> entities = _CHUNK_TO_ENTITIES.Lookup(p);

                Entity entityInChunk = entities.Extract(entity.Id);
                Debug.Assert(ReferenceEquals(entityInChunk, entity));

                if (entities.Empty)
                {
                    _CHUNK_TO_ENTITIES.Extract(p);
                    entities.Close();
                }
            }

        }

        internal bool ContainsChunk(Chunk.Vector p)
        {
            return _CHUNKS.Contains(p);
        }

        internal Chunk GetChunk(Chunk.Vector p)
        {
            return _CHUNKS.Lookup(p);
        }

        internal bool ContainsEntities(Chunk.Vector p)
        {
            return _CHUNK_TO_ENTITIES.Contains(p);
        }

        internal System.Collections.Generic.IEnumerable<Entity> GetEntities(Chunk.Vector p)
        {
            Table<int, Entity> entities = _CHUNK_TO_ENTITIES.Lookup(p);
            foreach (Entity entity in entities.GetValues())
            {
                yield return entity;
            }
        }

        internal void UpdateEntityRendering(Entity entity)
        {
            Debug.Assert(_ENTITY_TO_CHUNK_GRID.Contains(entity.Id));
            Chunk.Grid gridPrev = _ENTITY_TO_CHUNK_GRID.Extract(entity.Id);
            Chunk.Grid grid = Chunk.Grid.Generate(entity.Position, entity.GetBoundingBox());

            if (!gridPrev.Equals(grid))
            {
                Chunk.Grid gridBetween = Chunk.Grid.Generate(grid, gridPrev);

                foreach (Chunk.Vector pChunk in gridPrev.GetVectors())
                {
                    if (gridBetween.Contains(pChunk))
                    {
                        continue;
                    }

                    Table<int, Entity> entities = _CHUNK_TO_ENTITIES.Lookup(pChunk);

                    Entity entityInChunk = entities.Extract(entity.Id);
                    Debug.Assert(ReferenceEquals(entityInChunk, entity));

                    if (entities.Empty)
                    {
                        _CHUNK_TO_ENTITIES.Extract(pChunk);
                        entities.Close();
                    }
                }

                foreach (Chunk.Vector pChunk in grid.GetVectors())
                {
                    if (gridBetween.Contains(pChunk))
                    {
                        continue;
                    }

                    if (!_CHUNK_TO_ENTITIES.Contains(pChunk))
                    {
                        _CHUNK_TO_ENTITIES.Insert(pChunk, new());
                    }

                    Table<int, Entity> entities = _CHUNK_TO_ENTITIES.Lookup(pChunk);
                    entities.Insert(entity.Id, entity);
                }
            }

            _ENTITY_TO_CHUNK_GRID.Insert(entity.Id, grid);
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

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;

            // Assertion.

            System.Diagnostics.Debug.Assert(_ENTITY_ID_LIST.Empty);

            System.Diagnostics.Debug.Assert(_CHUNKS.Empty);

            System.Diagnostics.Debug.Assert(_ENTITY_SPAWNING_POOL.Empty);
            System.Diagnostics.Debug.Assert(_ENTITY_DESPAWNING_POOL.Empty);

            System.Diagnostics.Debug.Assert(_CHUNK_TO_ENTITIES.Empty);
            System.Diagnostics.Debug.Assert(_ENTITY_TO_CHUNK_GRID.Empty);

            System.Diagnostics.Debug.Assert(_DESPAWNED_ENTITIES.Empty);

            if (disposing == true)
            {
                // Release managed resources.
                _PLAYER_LIST.Dispose();

                _ENTITY_ID_LIST.Dispose();

                _CHUNKS.Dispose();

                _ENTITY_SPAWNING_POOL.Dispose();
                _ENTITY_DESPAWNING_POOL.Dispose();

                _CHUNK_TO_ENTITIES.Dispose();
                _ENTITY_TO_CHUNK_GRID.Dispose();

                _DESPAWNED_ENTITIES.Dispose();

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

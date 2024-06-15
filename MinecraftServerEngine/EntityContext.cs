
using Containers;

namespace MinecraftServerEngine
{
    /*internal sealed class EntityContext : System.IDisposable
    {
        private bool _disposed = false;

        private readonly Lock _MUTEX = new();  // Disposable
        private readonly Table<ChunkLocation, Table<int, Entity>> _CHUNK_TO_ENTITIES = new();  // Disposable
        private readonly ConcurrentTable<int, ChunkGrid> _ENTITY_TO_CHUNKS = new();  // Disposable

        public EntityContext() { }
        ~EntityContext() => System.Diagnostics.Debug.Assert(false);

        private void InsertEntityToChunk(ChunkLocation loc, Entity entity)
        {
            _MUTEX.Hold();

            Table<int, Entity> entities;
            if (!_CHUNK_TO_ENTITIES.Contains(loc))
            {
                entities = new();
                _CHUNK_TO_ENTITIES.Insert(loc, entities);
            }
            else
            {
                entities = _CHUNK_TO_ENTITIES.Lookup(loc);
            }

            entities.Insert(entity.Id, entity);

            _MUTEX.Release();
        }

        private void ExtractEntityToChunk(ChunkLocation loc, Entity entity)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            _MUTEX.Hold();

            System.Diagnostics.Debug.Assert(_CHUNK_TO_ENTITIES.Contains(loc));
            Table<int, Entity> entities = _CHUNK_TO_ENTITIES.Lookup(loc);

            Entity extracted = entities.Extract(entity.Id);
            System.Diagnostics.Debug.Assert(ReferenceEquals(extracted, entity));

            if (entities.Empty)
            {
                _CHUNK_TO_ENTITIES.Extract(loc);
                entities.Dispose();
            }

            _MUTEX.Release();
        }

        public void InitEntityChunkMapping(Entity entity)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            ChunkGrid grid = ChunkGrid.Generate(entity.BB);
            foreach (ChunkLocation loc in grid.GetLocations())
            {
                InsertEntityToChunk(loc, entity);
            }

            System.Diagnostics.Debug.Assert(!_ENTITY_TO_CHUNKS.Contains(entity.Id));
            _ENTITY_TO_CHUNKS.Insert(entity.Id, grid);
        }

        public void CloseEntityChunkMapping(Entity entity)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(_ENTITY_TO_CHUNKS.Contains(entity.Id));
            ChunkGrid grid = _ENTITY_TO_CHUNKS.Extract(entity.Id);

            foreach (ChunkLocation loc in grid.GetLocations())
            {
                ExtractEntityToChunk(loc, entity);
            }

        }

        public void UpdateEntityChunkMapping(Entity entity)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(_ENTITY_TO_CHUNKS.Contains(entity.Id));
            ChunkGrid gridPrev = _ENTITY_TO_CHUNKS.Extract(entity.Id);
            ChunkGrid grid = ChunkGrid.Generate(entity.BB);

            if (!gridPrev.Equals(grid))
            {
                ChunkGrid? gridChunkBetween = ChunkGrid.Generate(grid, gridPrev);

                foreach (ChunkLocation loc in gridPrev.GetLocations())
                {
                    if (gridChunkBetween != null && gridChunkBetween.Contains(loc))
                    {
                        continue;
                    }

                    ExtractEntityToChunk(loc, entity);

                }

                foreach (ChunkLocation loc in grid.GetLocations())
                {
                    if (gridChunkBetween != null && gridChunkBetween.Contains(loc))
                    {
                        continue;
                    }

                    InsertEntityToChunk(loc, entity);
                }

            }

            _ENTITY_TO_CHUNKS.Insert(entity.Id, grid);
        }

        internal System.Collections.Generic.IEnumerable<Entity> GetEntities(ChunkLocation loc)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            if (!_CHUNK_TO_ENTITIES.Contains(loc))
            {
                return [];
            }

            Table<int, Entity> entities = _CHUNK_TO_ENTITIES.Lookup(loc);
            return entities.GetValues();
        }

        *//*public Entity RaycastClosestEntity(Entity.Vector p, Entity.Vector u, int d)
        {
            System.Diagnostics.Debug.Assert(!_disposed);
            
            throw new System.NotImplementedException();
        }

        public Entity[] RaycastEntities(Entity.Vector p, Entity.Vector u, int d)
        {
            System.Diagnostics.Debug.Assert(!_disposed);
            
            throw new System.NotImplementedException();
        }

        public Entity[] GetCollidedEntities(bool resolve)
        {
            System.Diagnostics.Debug.Assert(!_disposed);
            
            throw new System.NotImplementedException();
        }*//*

        public void Dispose()
        {
            // Assertion.
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(_CHUNK_TO_ENTITIES.Empty);
            System.Diagnostics.Debug.Assert(_ENTITY_TO_CHUNKS.Empty);

            // Release resources.
            _MUTEX.Dispose();
            _CHUNK_TO_ENTITIES.Dispose();
            _ENTITY_TO_CHUNKS.Dispose();

            // Finish.
            System.GC.SuppressFinalize(this);
            _disposed = true;
        }


    }*/
}

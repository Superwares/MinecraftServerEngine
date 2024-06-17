﻿using Common;
using Containers;
using PhysicsEngine;

namespace MinecraftServerEngine
{
    public abstract class World : PhysicsWorld
    {
        private bool _disposed = false;

        internal readonly PlayerList _PLAYER_LIST = new();  // Disposable

        private readonly Queue<Entity> _ENTITY_SPAWNING_POOL = new();  // Disposable
        internal readonly SwapQueue<Entity> _ENTITIES = new();  // Disposable
        internal readonly SwapQueue<Player> _PLAYERS = new();

        private readonly Table<System.Guid, Player> _DISCONNECTED_PLAYERS = new(); // Disposable

        internal readonly BlockContext _BLOCK_CTX = new();  // Disposable

        /*internal PublicInventory _Inventory = new ChestInventory();*/

        public World()
        {
            // Dummy code.
            for (int z = -10; z <= 10; ++z)
            {
                for (int x = -10; x <= 10; ++x)
                {
                    BlockLocation loc = new(x, 100, z);

                    _BLOCK_CTX.SetBlock(loc, Blocks.Stone);
                }
            }

        }

        ~World() => System.Diagnostics.Debug.Assert(false);

        public override IBoundingVolume[] GetTerrainBoundingVolumes(IBoundingVolume volume)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            switch (volume)
            {
                default:
                    throw new System.NotImplementedException();
                case AxisAlignedBoundingBox aabb:
                    return _BLOCK_CTX.GetBlockBoundingVolumes(aabb.Max, aabb.Min);
            }
        }

        public abstract bool CanJoinWorld();

        public void SpawnEntity(Entity entity)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            _ENTITY_SPAWNING_POOL.Enqueue(entity);

            throw new System.NotImplementedException();
        }

        protected abstract bool DetermineToDespawnPlayerOnDisconnect();

        public void StartRoutine(long serverTicks)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            /*if (serverTicks == 20 * 5)
            {
                SpawnItemEntity();
            }*/
        }

        public void StartEntityRoutine(long serverTicks, Entity entity, bool serversidePhysics)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            // TODO: Resolve Collisions with other entities.
            // TODO: Add Global Forces with OnGround flag. (Gravity, Damping Force, ...)
            if (serversidePhysics) 
            {
                entity.ApplyForce(
                        -1.0D * new Vector(1.0D - 0.91D, 1.0D - 0.9800000190734863D, 1.0D - 0.91D) *
                        entity.VELOCITY);  // Damping Force
                entity.ApplyForce(entity._m * 0.08D * new Vector(0.0D, -1.0D, 0.0D));  // Gravity

                /*entity.ApplyForce(entity.GetMass() * 0.001D * new Entity.Vector(0, -1, 0));  // Gravity*/
            }

            entity.StartInternalRoutine(serverTicks, this);
            entity.StartRoutine(serverTicks, this);
        }

        public void StartEntityRoutines(long serverTicks)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            try
            {
                do
                {
                    Entity entity = _ENTITIES.Dequeue();

                    System.Diagnostics.Debug.Assert(entity is not Player);

                    StartEntityRoutine(serverTicks, entity, true);

                    _ENTITIES.Enqueue(entity);
                } while (true);
            }
            catch (EmptyContainerException)
            {

            }

            System.Diagnostics.Debug.Assert(_ENTITIES.Empty);
        }

        public void StartPlayerRoutines(long serverTicks)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            try
            {
                do
                {
                    Player player = _PLAYERS.Dequeue();

                    StartEntityRoutine(serverTicks, player, !player.Connected);

                    _PLAYERS.Enqueue(player);
                } while (true);

            }
            catch (EmptyContainerException)
            {

            }

            System.Diagnostics.Debug.Assert(_PLAYERS.Empty);
        }

        public void HandlePlayerConnections(long serverTicks)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            try
            {
                do
                {
                    Player player = _PLAYERS.Dequeue();

                    if (player.HandlePlayerConnection(this))
                    {
                        System.Guid userId = player.UNIQUE_ID;

                        System.Diagnostics.Debug.Assert(!_DISCONNECTED_PLAYERS.Contains(userId));
                        _DISCONNECTED_PLAYERS.Insert(userId, player);

                    }

                    _PLAYERS.Enqueue(player);
                } while (true);

            }
            catch (EmptyContainerException)
            {

            }

            System.Diagnostics.Debug.Assert(_PLAYERS.Empty);
        }

        private void DestroyEntity(Entity entity)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            if (entity is Player player)
            {
                _PLAYER_LIST.Remove(player.UNIQUE_ID);

                Player playerExtracted = _DISCONNECTED_PLAYERS.Extract(player.UNIQUE_ID);
                System.Diagnostics.Debug.Assert(ReferenceEquals(playerExtracted, player));
            }

            CloseMapping(entity);

            _ENTITY_ID_LIST.Dealloc(entity.ID);

            entity.Flush();
            entity.Dispose();
        }

        public void DestroyEntities()
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            try
            {
                do
                {
                    Entity entity = _ENTITIES.Dequeue();

                    System.Diagnostics.Debug.Assert(entity is not Player);

                    if (entity.IsDead())
                    {
                        DestroyEntity(entity);
                    }
                    else
                    {
                        _ENTITIES.Enqueue(entity);
                    }

                } while (true);
            }
            catch (EmptyContainerException)
            {

            }

            System.Diagnostics.Debug.Assert(_ENTITIES.Empty);
        }

        public void DestroyPlayers()
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            try
            {
                do
                {
                    Player player = _PLAYERS.Dequeue();

                    if (!player.Connected)
                    {
                        System.Diagnostics.Debug.Assert(
                            _DISCONNECTED_PLAYERS.Contains(player.UNIQUE_ID));

                        if (DetermineToDespawnPlayerOnDisconnect())
                        {
                            DestroyEntity(player);
                        }
                    }
                    else
                    {
                        _PLAYERS.Enqueue(player);
                    }
                } while (true);

            }
            catch (EmptyContainerException)
            {

            }

            System.Diagnostics.Debug.Assert(_PLAYERS.Empty);
        }

        public void MoveEntities()
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            try
            {
                do
                {
                    Entity entity = _ENTITIES.Dequeue();

                    Move(entity);

                    _ENTITIES.Enqueue(entity);
                } while (true);
            }
            catch (EmptyContainerException)
            {

            }

            System.Diagnostics.Debug.Assert(_ENTITIES.Empty);
        }

        public void MovePlayers()
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            try
            {
                do
                {
                    Player player = _PLAYERS.Dequeue();

                    if (!player.Connected)
                    {
                        Move(player);
                    }

                    _PLAYERS.Enqueue(player);
                } while (true);

            }
            catch (EmptyContainerException)
            {

            }

            System.Diagnostics.Debug.Assert(_PLAYERS.Empty);
        }

        public void CreateEntities()
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            try
            {
                do
                {
                    Entity entity = _ENTITY_SPAWNING_POOL.Dequeue();

                    System.Diagnostics.Debug.Assert(entity is not Player);

                    int id = _ENTITY_ID_LIST.Alloc();
                    System.Guid uniqueId = System.Guid.NewGuid();

                    entity.Create(id, uniqueId, _P_SPAWE, _LOOK_SPAWE);

                    InitMapping(entity);

                    _ENTITIES.Enqueue(entity);
                } while (true);
            }
            catch (EmptyContainerException)
            {

            }

            System.Diagnostics.Debug.Assert(_ENTITY_SPAWNING_POOL.Empty);
        }

        protected abstract Player CreatePlayer();

        internal void CreateOrConnectPlayer(Client client, string username, System.Guid userId)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            Player player;

            if (_DISCONNECTED_PLAYERS.Contains(userId))
            {
                player = _DISCONNECTED_PLAYERS.Extract(userId);
            }
            else
            {
                int id = _ENTITY_ID_LIST.Alloc();

                player = CreatePlayer();
                player.Create(
                    id, userId, 
                    _P_SPAWE, _LOOK_SPAWE);

                InitMapping(player);

                _PLAYER_LIST.Add(userId, username);
            }
            
            player.Connect(client, this, userId);

            _PLAYERS.Enqueue(player);
        }

        public void HandlePlayerRenders()
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            try
            {
                do
                {
                    Player player = _PLAYERS.Dequeue();

                    player.Render(this);

                    _PLAYERS.Enqueue(player);
                } while (true);

            }
            catch (EmptyContainerException)
            {

            }
        }

        public override void Dispose()
        {
            // Assertion.
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(_ENTITY_SPAWNING_POOL.Empty);
            System.Diagnostics.Debug.Assert(_ENTITIES.Empty);
            System.Diagnostics.Debug.Assert(_PLAYERS.Empty);

            System.Diagnostics.Debug.Assert(_DISCONNECTED_PLAYERS.Empty);

            // Release resources.
            _PLAYER_LIST.Dispose();

            _ENTITY_SPAWNING_POOL.Dispose();
            _ENTITIES.Dispose();
            _PLAYERS.Dispose();

            _DISCONNECTED_PLAYERS.Dispose();

            _BLOCK_CTX.Dispose();

            // Finish.
            base.Dispose();
            _disposed = true;
        }
       

    }


}

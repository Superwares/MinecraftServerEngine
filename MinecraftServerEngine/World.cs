using Common;
using Containers;
using MinecraftPhysicsEngine;
using Sync;

namespace MinecraftServerEngine
{
    public abstract class World : PhysicsWorld
    {
        private bool _disposed = false;

        internal readonly PlayerList PlayerList = new();  // Disposable

        private readonly Queue<Entity> EntitySpawningPool = new();  // Disposable
        internal readonly SwapQueue<Entity> Entities = new();  // Disposable
        internal readonly SwapQueue<Player> Players = new();

        private readonly Table<System.Guid, Player> DisconnectedPlayers = new(); // Disposable

        internal readonly BlockContext BlockContext = new();  // Disposable

        /*internal PublicInventory _Inventory = new ChestInventory();*/

        public World() { }

        ~World() => System.Diagnostics.Debug.Assert(false);

        public abstract bool CanJoinWorld();

        public void SpawnEntity(Entity entity)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            EntitySpawningPool.Enqueue(entity);

            throw new System.NotImplementedException();
        }

        protected abstract bool DetermineToDespawnPlayerOnDisconnect();

        public void StartRoutine(
            Locker locker, Cond cond, Barrier barrier, long serverTicks)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            locker.Hold();
            cond.Wait();
            locker.Release();

            /*if (serverTicks == 20 * 5)
            {
                SpawnItemEntity();
            }*/

            barrier.SignalAndWait();
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
                        entity.Velocity);  // Damping Force
                entity.ApplyForce(entity.GetMass() * 0.08D * new Vector(0.0D, -1.0D, 0.0D));  // Gravity

                /*entity.ApplyForce(entity.GetMass() * 0.001D * new Entity.Vector(0, -1, 0));  // Gravity*/
            }

            entity.StartInternalRoutine(serverTicks, this);
            entity.StartRoutine(serverTicks, this);
        }

        public void StartEntityRoutines(
            Locker locker, Cond cond, Barrier barrier, long serverTicks)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            locker.Hold();
            cond.Wait();
            locker.Release();

            try
            {
                do
                {
                    Entity entity = Entities.Dequeue();

                    System.Diagnostics.Debug.Assert(entity is not Player);

                    StartEntityRoutine(serverTicks, entity, true);

                    Entities.Enqueue(entity);
                } while (true);
            }
            catch (EmptyContainerException) { }

            System.Diagnostics.Debug.Assert(Entities.Empty);

            barrier.SignalAndWait();
        }

        public void StartPlayerRoutines(
            Locker locker, Cond cond, Barrier barrier, long serverTicks)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            locker.Hold();
            cond.Wait();
            locker.Release();

            try
            {
                do
                {
                    Player player = Players.Dequeue();

                    StartEntityRoutine(serverTicks, player, !player.Connected);

                    Players.Enqueue(player);
                } while (true);

            }
            catch (EmptyContainerException) { }

            System.Diagnostics.Debug.Assert(Players.Empty);

            barrier.SignalAndWait();
        }

        public void HandlePlayerConnections(
            Locker locker, Cond cond, Barrier barrier, long serverTicks)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            locker.Hold();
            cond.Wait();
            locker.Release();

            try
            {
                do
                {
                    Player player = Players.Dequeue();

                    if (player.HandlePlayerConnection(this))
                    {
                        System.Guid userId = player.UniqueId;

                        System.Diagnostics.Debug.Assert(!DisconnectedPlayers.Contains(userId));
                        DisconnectedPlayers.Insert(userId, player);

                    }

                    Players.Enqueue(player);
                } while (true);

            }
            catch (EmptyContainerException) { }

            System.Diagnostics.Debug.Assert(Players.Empty);

            barrier.SignalAndWait();
        }

        private void DestroyEntity(Entity entity)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            if (entity is Player player)
            {
                PlayerList.Remove(player.UniqueId);

                Player playerExtracted = DisconnectedPlayers.Extract(player.UniqueId);
                System.Diagnostics.Debug.Assert(ReferenceEquals(playerExtracted, player));
            }

            CloseObject(entity);

            entity.Flush();
            entity.Dispose();
        }

        public void DestroyEntities(
            Locker locker, Cond cond, Barrier barrier)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            locker.Hold();
            cond.Wait();
            locker.Release();

            try
            {
                do
                {
                    Entity entity = Entities.Dequeue();

                    System.Diagnostics.Debug.Assert(entity is not Player);

                    if (entity.IsDead())
                    {
                        DestroyEntity(entity);
                    }
                    else
                    {
                        Entities.Enqueue(entity);
                    }

                } while (true);
            }
            catch (EmptyContainerException) { }

            System.Diagnostics.Debug.Assert(Entities.Empty);

            barrier.SignalAndWait();
        }

        public void DestroyPlayers(
            Locker locker, Cond cond, Barrier barrier)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            locker.Hold();
            cond.Wait();
            locker.Release();

            try
            {
                do
                {
                    Player player = Players.Dequeue();

                    if (!player.Connected)
                    {
                        System.Diagnostics.Debug.Assert(
                            DisconnectedPlayers.Contains(player.UniqueId));

                        if (DetermineToDespawnPlayerOnDisconnect())
                        {
                            DestroyEntity(player);
                        }
                    }
                    else
                    {
                        Players.Enqueue(player);
                    }
                } while (true);

            }
            catch (EmptyContainerException) { }

            System.Diagnostics.Debug.Assert(Players.Empty);

            barrier.SignalAndWait();
        }

        public void MoveEntity(Entity entity)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            MoveObject(BlockContext, entity);
        }

        public void MoveEntities(
            Locker locker, Cond cond, Barrier barrier)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            locker.Hold();
            cond.Wait();
            locker.Release();

            try
            {
                do
                {
                    Entity entity = Entities.Dequeue();

                    MoveEntity(entity);

                    Entities.Enqueue(entity);
                } while (true);
            }
            catch (EmptyContainerException) { }

            System.Diagnostics.Debug.Assert(Entities.Empty);

            barrier.SignalAndWait();
        }

        public void MovePlayers(
            Locker locker, Cond cond, Barrier barrier)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            locker.Hold();
            cond.Wait();
            locker.Release();

            try
            {
                do
                {
                    Player player = Players.Dequeue();

                    MoveEntity(player);

                    Players.Enqueue(player);
                } while (true);

            }
            catch (EmptyContainerException) { }

            System.Diagnostics.Debug.Assert(Players.Empty);

            barrier.SignalAndWait();
        }

        public void CreateEntities(
            Locker locker, Cond cond, Barrier barrier)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            locker.Hold();
            cond.Wait();
            locker.Release();

            try
            {
                do
                {
                    Entity entity = EntitySpawningPool.Dequeue();

                    System.Diagnostics.Debug.Assert(entity is not Player);

                    InitObject(entity);

                    Entities.Enqueue(entity);
                } while (true);
            }
            catch (EmptyContainerException) { }

            System.Diagnostics.Debug.Assert(EntitySpawningPool.Empty);

            barrier.SignalAndWait();
        }

        protected abstract Player CreatePlayer(System.Guid userId);

        internal void CreateOrConnectPlayer(Client client, string username, System.Guid userId)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            Player player;

            if (DisconnectedPlayers.Contains(userId))
            {
                player = DisconnectedPlayers.Extract(userId);
            }
            else
            {
                player = CreatePlayer(userId);

                InitObject(player);

                PlayerList.Add(userId, username);
            }
            
            player.Connect(client, this, userId);

            Players.Enqueue(player);
        }

        public void HandlePlayerRenders(
            Locker locker, Cond cond, Barrier barrier)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            locker.Hold();
            cond.Wait();
            locker.Release();

            try
            {
                do
                {
                    Player player = Players.Dequeue();

                    player.Render(this);

                    Players.Enqueue(player);
                } while (true);

            }
            catch (EmptyContainerException) { }

            barrier.SignalAndWait();
        }

        public override void Dispose()
        {
            // Assertion.
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(EntitySpawningPool.Empty);
            System.Diagnostics.Debug.Assert(Entities.Empty);
            System.Diagnostics.Debug.Assert(Players.Empty);

            System.Diagnostics.Debug.Assert(DisconnectedPlayers.Empty);

            // Release resources.
            PlayerList.Dispose();

            EntitySpawningPool.Dispose();
            Entities.Dispose();
            Players.Dispose();

            DisconnectedPlayers.Dispose();

            BlockContext.Dispose();

            // Finish.
            base.Dispose();
            _disposed = true;
        }
       

    }


}

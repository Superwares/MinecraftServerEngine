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

        public void StartRoutine(Barrier barrier, long serverTicks)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

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
                        -1.0D * 
                        new Vector(1.0D - 0.91D, 1.0D - 0.9800000190734863D, 1.0D - 0.91D) *
                        entity.Velocity);  // Damping Force
                entity.ApplyForce(entity.GetMass() * 0.08D * new Vector(0.0D, -1.0D, 0.0D));  // Gravity

                /*entity.ApplyForce(entity.GetMass() * 0.001D * new Entity.Vector(0, -1, 0));  // Gravity*/
            }

            entity.StartInternalRoutine(serverTicks, this);
            entity.StartRoutine(serverTicks, this);
        }

        public void StartEntityRoutines(
            Barrier barrier, long serverTicks)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            Entities.Swap();

            barrier.SignalAndWait();

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

        public void StartPlayerRoutines(Barrier barrier, long serverTicks)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            Players.Swap();

            barrier.SignalAndWait();

            try
            {
                do
                {
                    Player player = Players.Dequeue();

                    StartEntityRoutine(serverTicks, player, player.Disconnected);

                    Players.Enqueue(player);
                } while (true);

            }
            catch (EmptyContainerException) { }

            System.Diagnostics.Debug.Assert(Players.Empty);

            barrier.SignalAndWait();
        }

        public void HandlePlayerConnections(Barrier barrier, long serverTicks)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            Players.Swap();

            barrier.SignalAndWait();

            try
            {
                do
                {
                    Player player = Players.Dequeue();
                    System.Diagnostics.Debug.Assert(player != null);

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

            CloseObject(entity);

            entity.Flush();
            entity.Dispose();
        }

        public void DestroyEntities(Barrier barrier)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            Entities.Swap();

            barrier.SignalAndWait();

            try
            {
                do
                {
                    Entity entity = Entities.Dequeue();

                    System.Diagnostics.Debug.Assert(entity is not Player);

                    if (entity.IsDead())
                    {
                        DestroyEntity(entity);

                        continue;
                    }

                    Entities.Enqueue(entity);

                } while (true);
            }
            catch (EmptyContainerException) { }

            System.Diagnostics.Debug.Assert(Entities.Empty);

            barrier.SignalAndWait();
        }

        public void DestroyPlayers(Barrier barrier)
        {
            System.Diagnostics.Debug.Assert(barrier != null);

            System.Diagnostics.Debug.Assert(!_disposed);

            Players.Swap();

            barrier.SignalAndWait();

            try
            {
                do
                {
                    Player player = Players.Dequeue();
                    System.Diagnostics.Debug.Assert(player != null);

                    if (!player.Connected)
                    {
                        System.Diagnostics.Debug.Assert(
                            DisconnectedPlayers.Contains(player.UniqueId));

                        if (DetermineToDespawnPlayerOnDisconnect())
                        {
                            PlayerList.Remove(player.UniqueId);

                            Player playerExtracted = DisconnectedPlayers.Extract(player.UniqueId);
                            System.Diagnostics.Debug.Assert(ReferenceEquals(playerExtracted, player));

                            DestroyEntity(player);

                            continue;
                        }
                    }

                    Players.Enqueue(player);
                } while (true);

            }
            catch (EmptyContainerException) { }

            System.Diagnostics.Debug.Assert(Players.Empty);

            barrier.SignalAndWait();
        }

        public void MoveEntities(Barrier barrier)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            Entities.Swap();

            barrier.SignalAndWait();

            try
            {
                do
                {
                    Entity entity = Entities.Dequeue();

                    MoveObject(BlockContext, entity);

                    Entities.Enqueue(entity);
                } while (true);
            }
            catch (EmptyContainerException) { }

            System.Diagnostics.Debug.Assert(Entities.Empty);

            barrier.SignalAndWait();
        }

        public void MovePlayers(Barrier barrier)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            Players.Swap();

            barrier.SignalAndWait();

            try
            {
                do
                {
                    Player player = Players.Dequeue();

                    MoveObject(BlockContext, player);

                    Players.Enqueue(player);
                } while (true);

            }
            catch (EmptyContainerException) { }

            System.Diagnostics.Debug.Assert(Players.Empty);

            barrier.SignalAndWait();
        }

        public void CreateEntities(Barrier barrier)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

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
            System.Diagnostics.Debug.Assert(client != null);
            System.Diagnostics.Debug.Assert(username != "");
            System.Diagnostics.Debug.Assert(userId != System.Guid.Empty);

            System.Diagnostics.Debug.Assert(!_disposed);

            Player player;

            if (DisconnectedPlayers.Contains(userId))
            {
                player = DisconnectedPlayers.Extract(userId);
                System.Diagnostics.Debug.Assert(player != null);
            }
            else
            {
                player = CreatePlayer(userId);
                System.Diagnostics.Debug.Assert(player != null);

                InitObject(player);

                PlayerList.Add(userId, username);

                Players.Enqueue(player);
            }
            
            player.Connect(client, this, userId);

        }

        public void HandlePlayerRenders(Barrier barrier)
        {
            System.Diagnostics.Debug.Assert(barrier != null);

            System.Diagnostics.Debug.Assert(!_disposed);

            Players.Swap();

            barrier.SignalAndWait();

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

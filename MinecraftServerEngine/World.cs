using Common;
using Containers;
using Sync;

namespace MinecraftServerEngine
{
    using PhysicsEngine;

    public abstract class World : PhysicsWorld
    {
        private sealed class ObjectQueue : System.IDisposable
        {
            private class SwapQueue<T> : System.IDisposable
            {
                private bool _disposed = false;

                private int dequeue = 1;
                private readonly ConcurrentQueue<T> Queue1 = new();  // Disposable
                private readonly ConcurrentQueue<T> Queue2 = new();  // Disposable

                private ConcurrentQueue<T> GetQueueForDequeue()
                {
                    System.Diagnostics.Debug.Assert(!_disposed);

                    if (dequeue == 1)
                    {
                        return Queue1;
                    }
                    else if (dequeue == 2)
                    {
                        return Queue2;
                    }
                    else
                    {
                        System.Diagnostics.Debug.Assert(false);
                    }

                    throw new System.NotImplementedException();
                }

                private ConcurrentQueue<T> GetQueueForEnqueue()
                {
                    System.Diagnostics.Debug.Assert(!_disposed);

                    if (dequeue == 1)
                    {
                        return Queue2;
                    }
                    else if (dequeue == 2)
                    {
                        return Queue1;
                    }
                    else
                    {
                        System.Diagnostics.Debug.Assert(false);
                    }

                    throw new System.NotImplementedException();
                }

                public int Count
                {
                    get
                    {
                        System.Diagnostics.Debug.Assert(!_disposed);

                        ConcurrentQueue<T> queue = GetQueueForDequeue();
                        return queue.Count;
                    }
                }
                public bool Empty => (Count == 0);

                public SwapQueue() { }

                ~SwapQueue() => System.Diagnostics.Debug.Assert(false);

                public void Swap()
                {
                    System.Diagnostics.Debug.Assert(!_disposed);

                    ConcurrentQueue<T> queue = GetQueueForDequeue();

                    if (queue.Empty)
                    {
                        if (dequeue == 1)
                        {
                            System.Diagnostics.Debug.Assert(Queue1.Empty);

                            dequeue = 2;
                        }
                        else if (dequeue == 2)
                        {
                            System.Diagnostics.Debug.Assert(Queue2.Empty);

                            dequeue = 1;
                        }
                        else
                        {
                            System.Diagnostics.Debug.Assert(false);
                        }
                    }
                }

                public bool Dequeue(out T value)
                {
                    System.Diagnostics.Debug.Assert(!_disposed);

                    ConcurrentQueue<T> queue = GetQueueForDequeue();

                    return queue.Dequeue(out value);
                }

                public void Enqueue(T value)
                {
                    System.Diagnostics.Debug.Assert(value != null);

                    System.Diagnostics.Debug.Assert(!_disposed);

                    ConcurrentQueue<T> queue = GetQueueForEnqueue();
                    queue.Enqueue(value);
                }

                public void Dispose()
                {
                    // Assertions.
                    System.Diagnostics.Debug.Assert(!_disposed);

                    // Release resources.
                    Queue1.Dispose();
                    Queue2.Dispose();

                    // Finish.
                    System.GC.SuppressFinalize(this);
                    _disposed = true;
                }
            }

            private bool _disposed = false;

            private readonly Locker Locker = new();  // Disposable

            private readonly SwapQueue<PhysicsObject> Objects = new();  // Disposable
            private readonly SwapQueue<AbstractPlayer> Players = new();

            ~ObjectQueue() => System.Diagnostics.Debug.Assert(false);
            
            public void Swap()
            {
                System.Diagnostics.Debug.Assert(!_disposed);

                Objects.Swap();
                Players.Swap();
            }

            /// <summary>
            /// 
            /// </summary>
            /// <returns></returns>
            /// <exception cref="EmptyContainerException" />
            public bool Dequeue(out PhysicsObject obj)
            {
                System.Diagnostics.Debug.Assert(!_disposed);

                Locker.Hold();

                bool f;

                if (!Objects.Empty)
                {
                    f = Objects.Dequeue(out obj);
                }
                else
                {
                    f = Players.Dequeue(out AbstractPlayer player);
                    obj = player;
                }

                Locker.Release();

                return f;
            }

            /// <summary>
            /// 
            /// </summary>
            /// <returns></returns>
            /// <exception cref="EmptyContainerException" />
            public bool DequeuePlayer(out AbstractPlayer player)
            {
                System.Diagnostics.Debug.Assert(!_disposed);

                return Players.Dequeue(out player);
            }

            /// <summary>
            /// 
            /// </summary>
            /// <returns></returns>
            /// <exception cref="EmptyContainerException" />
            public bool DequeueNonPlayer(out PhysicsObject obj)
            {
                System.Diagnostics.Debug.Assert(!_disposed);

                return Objects.Dequeue(out obj);
            }

            public void Enqueue(PhysicsObject obj)
            {
                System.Diagnostics.Debug.Assert(obj != null);

                System.Diagnostics.Debug.Assert(!_disposed);

                if (obj is AbstractPlayer player)
                {
                    Players.Enqueue(player);
                }
                else
                {
                    Objects.Enqueue(obj);
                }
            }

            public void Dispose()
            {
                // Assertions.
                System.Diagnostics.Debug.Assert(!_disposed);

                System.Diagnostics.Debug.Assert(Objects.Empty);
                System.Diagnostics.Debug.Assert(Players.Empty);

                // Release resources.
                Locker.Dispose();

                Objects.Dispose();
                Players.Dispose();

                // Finish.
                System.GC.SuppressFinalize(this);
                _disposed = true;
            }

        }

        private bool _disposed = false;

        internal readonly PlayerList PlayerList = new();  // Disposable

        private readonly ConcurrentQueue<PhysicsObject> ObjectSpawningPool = new();  // Disposable
        private readonly ConcurrentQueue<PhysicsObject> ObjectDespawningPool = new();  // Disposable

        private readonly ObjectQueue Objects = new();  // Disposable

        internal readonly ConcurrentTable<int, Entity> EntitiesById = new();  // Disposable

        private readonly ConcurrentTable<UserId, AbstractPlayer> DisconnectedPlayers = new(); // Disposable

        internal readonly BlockContext BlockContext = new();  // Disposable

        public World() { }

        ~World() => System.Diagnostics.Debug.Assert(false);

        public abstract bool CanJoinWorld();

        public void SpawnObject(PhysicsObject obj)
        {
            System.Diagnostics.Debug.Assert(obj is not AbstractPlayer);

            System.Diagnostics.Debug.Assert(!_disposed);

            ObjectSpawningPool.Enqueue(obj);
        }

        protected abstract bool DetermineToDespawnPlayerOnDisconnect();

        internal void SwapObjectQueue()
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(Objects != null);
            Objects.Swap();
        }

        internal void StartObjectRoutines(long serverTicks)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            while (Objects.Dequeue(out PhysicsObject obj))
            {
                System.Diagnostics.Debug.Assert(obj != null);

                obj.StartRoutine(serverTicks,this);

                Objects.Enqueue(obj);
            }
        }

        internal void ControlPlayers(long serverTicks)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            while (Objects.DequeuePlayer(out AbstractPlayer player))
            {
                System.Diagnostics.Debug.Assert(player != null);

                player.Control(serverTicks, this);

                Objects.Enqueue(player);
            }
        }

        internal void HandleDeathEvents()
        {
            while (Objects.Dequeue(out PhysicsObject obj))
            {
                System.Diagnostics.Debug.Assert(obj != null);

                if (obj.IsDead())
                {
                    obj.OnDeath(this);

                    if (obj is not AbstractPlayer)
                    {
                        ObjectDespawningPool.Enqueue(obj);

                        continue;
                    }
                }

                Objects.Enqueue(obj);
            }
        }

        internal void HandleDisconnections()
        {
            while (Objects.DequeuePlayer(out AbstractPlayer player))
            {
                System.Diagnostics.Debug.Assert(player != null);

                if (player.HandleDisconnection(out UserId id, this))
                {
                    System.Diagnostics.Debug.Assert(id != UserId.Null);
                    System.Diagnostics.Debug.Assert(!DisconnectedPlayers.Contains(id));
                    DisconnectedPlayers.Insert(id, player);

                    if (DetermineToDespawnPlayerOnDisconnect())
                    {
                        PlayerList.Remove(id);

                        AbstractPlayer playerExtracted = DisconnectedPlayers.Extract(id);
                        System.Diagnostics.Debug.Assert(ReferenceEquals(playerExtracted, player));

                        ObjectDespawningPool.Enqueue(player);

                        continue;
                    }
                }

                Objects.Enqueue(player);
            }
        }

        internal void DestroyObjects()
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            while (ObjectDespawningPool.Dequeue(out PhysicsObject obj))
            {
                System.Diagnostics.Debug.Assert(obj != null);

                CloseObjectMapping(obj);

                obj.Flush();
                obj.Dispose();

                if (obj is Entity entity)
                {
                    Entity entityExtracted = EntitiesById.Extract(entity.Id);
                    System.Diagnostics.Debug.Assert(ReferenceEquals(entity, entityExtracted));
                }
            }
        }

        internal void MoveObjects()
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            while (Objects.Dequeue(out PhysicsObject obj))
            {
                System.Diagnostics.Debug.Assert(obj != null);

                (BoundingVolume volume, Vector v) = IntegrateObject(BlockContext, obj);

                obj.Move(volume, v);

                UpdateObjectMapping(obj);

                Objects.Enqueue(obj);
            }

        }

        protected abstract AbstractPlayer CreatePlayer(UserId id);

        internal void CreateOrConnectPlayer(Client client, string username, UserId id)
        {
            System.Diagnostics.Debug.Assert(client != null);
            System.Diagnostics.Debug.Assert(username != "");
            System.Diagnostics.Debug.Assert(id != UserId.Null);

            System.Diagnostics.Debug.Assert(!_disposed);

            AbstractPlayer player;

            if (DisconnectedPlayers.Contains(id))
            {
                player = DisconnectedPlayers.Extract(id);
                System.Diagnostics.Debug.Assert(player != null);
            }
            else
            {
                player = CreatePlayer(id);
                System.Diagnostics.Debug.Assert(player != null);

                ObjectSpawningPool.Enqueue(player);

                PlayerList.Add(id, username);
            }

            player.Connect(client, this, id);

        }

        internal void CreateObjects()
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            while (ObjectSpawningPool.Dequeue(out PhysicsObject obj))
            {
                System.Diagnostics.Debug.Assert(obj != null);

                InitObjectMapping(obj);

                Objects.Enqueue(obj);

                if (obj is Entity entity)
                {
                    EntitiesById.Insert(entity.Id, entity);
                }

            }

            System.Diagnostics.Debug.Assert(ObjectSpawningPool.Empty);
        }

        internal void LoadAndSendData()
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            while (Objects.DequeuePlayer(out AbstractPlayer player))
            {
                System.Diagnostics.Debug.Assert(player != null);

                player.LoadAndSendData(this);

                Objects.Enqueue(player);
            }

        }

        public override void Dispose()
        {
            // Assertion.
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(ObjectSpawningPool.Empty);
            System.Diagnostics.Debug.Assert(ObjectDespawningPool.Empty);

            System.Diagnostics.Debug.Assert(DisconnectedPlayers.Empty);


            // Release resources.
            PlayerList.Dispose();

            ObjectSpawningPool.Dispose();
            ObjectDespawningPool.Dispose();

            Objects.Dispose();

            DisconnectedPlayers.Dispose();

            BlockContext.Dispose();

            // Finish.
            base.Dispose();
            _disposed = true;
        }
       
    }


}

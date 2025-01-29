using Common;
using Containers;
using Sync;

using MinecraftPrimitives;

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
        internal readonly ConcurrentTable<UserId, AbstractPlayer> PlayersByUserId = new();  // Disposable
        internal readonly ConcurrentTable<string, AbstractPlayer> PlayersByUsername = new();  // Disposable

        internal readonly ConcurrentMap<UserId, WorldRenderer> WorldRenderersByUserId = new();  // Disposable

        private readonly ConcurrentTable<UserId, AbstractPlayer> DisconnectedPlayers = new(); // Disposable

        public readonly BlockContext BlockContext;  // Disposable


        private readonly ReadLocker LockerProgressBars = new();  // Disposable
        private readonly Map<System.Guid, ProgressBar> ProgressBars = new();  // Disposable

        public World()
        {
            BlockContext = BlockContext.LoadWithRegionFiles(@"region");
        }

        ~World()
        {
            System.Diagnostics.Debug.Assert(false);

            Dispose(false);
        }

        internal void Connect(UserId id, ConcurrentQueue<ClientboundPlayingPacket> OutPackets)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            PlayerListRenderer plRenderer = new(OutPackets);
            PlayerList.Connect(id, plRenderer);

            WorldRenderer renderer = new(OutPackets);
            WorldRenderersByUserId.Insert(id, renderer);

            // TODO: world border


            // TODO: Boss Bar
            System.Diagnostics.Debug.Assert(LockerProgressBars != null);
            LockerProgressBars.Read();

            try
            {
                System.Diagnostics.Debug.Assert(ProgressBars != null);
                foreach (ProgressBar progressBar in ProgressBars.GetValues())
                {
                    renderer.OpenBossBar(
                        progressBar.Id, progressBar.TitleData, progressBar.Health, progressBar.Color, progressBar.Division);
                }


            }
            finally
            {
                System.Diagnostics.Debug.Assert(LockerProgressBars != null);
                LockerProgressBars.Release();
            }
        }

        public void PlaySound(string name, int category, Vector p, double volume, double pitch)
        {
            if (volume < 0.0 || volume > 1.0)
            {
                throw new System.ArgumentOutOfRangeException(nameof(volume));
            }
            if (pitch < 0.5 || pitch > 2.0)
            {
                throw new System.ArgumentOutOfRangeException(nameof(pitch));
            }

            if (_disposed == true)
            {
                throw new System.ObjectDisposedException(GetType().Name);
            }

            foreach (WorldRenderer renderer in WorldRenderersByUserId.GetValues())
            {
                renderer.PlaySound(name, category, p, volume, pitch);
            }
        }

        public void DisplayTitle(
            Time fadeIn, Time stay, Time fadeOut,
            params TextComponent[] components)
        {
            if (_disposed == true)
            {
                throw new System.ObjectDisposedException(GetType().Name);
            }

            if (components == null || components.Length == 0)
            {
                return;
            }

            var extra = new object[components.Length];

            for (int i = 0; i < components.Length; ++i)
            {
                TextComponent component = components[i];

                extra[i] = new
                {
                    text = component.Text,
                    color = component.Color.GetName(),
                };
            }

            var chat = new
            {
                text = "",
                extra = extra,
            };

            string data = System.Text.Json.JsonSerializer.Serialize(chat);

            // TODO: 1tick = 50ms, make this variable to single constant.
            const int microsecondsPerTick = 50 * 1000;

            foreach (WorldRenderer renderer in WorldRenderersByUserId.GetValues())
            {
                System.Diagnostics.Debug.Assert(renderer != null);
                renderer.DisplayTitle(
                    (int)(fadeIn.Amount / microsecondsPerTick),
                    (int)(stay.Amount / microsecondsPerTick),
                    (int)(fadeOut.Amount / microsecondsPerTick),
                    data);
            }

        }

        public System.Guid OpenProgressBar(
            TextComponent[] title, double health,
            BossBarColor color, BossBarDivision division)
        {
            if (health < 0.0 || health > 1.0)
            {
                throw new System.ArgumentOutOfRangeException(nameof(health));
            }

            if (_disposed == true)
            {
                throw new System.ObjectDisposedException(GetType().Name);
            }

            if (title == null)
            {
                title = [];
            }

            ProgressBar progressBar = new(title, health, color, division);

            System.Diagnostics.Debug.Assert(LockerProgressBars != null);
            LockerProgressBars.Hold();

            try
            {
                System.Diagnostics.Debug.Assert(WorldRenderersByUserId != null);
                foreach (WorldRenderer renderer in WorldRenderersByUserId.GetValues())
                {
                    System.Diagnostics.Debug.Assert(renderer != null);
                    renderer.OpenBossBar(
                        progressBar.Id, progressBar.TitleData, progressBar.Health,
                        progressBar.Color, progressBar.Division);
                }

                System.Diagnostics.Debug.Assert(ProgressBars != null);
                ProgressBars.Insert(progressBar.Id, progressBar);

                return progressBar.Id;
            }
            finally
            {
                System.Diagnostics.Debug.Assert(LockerProgressBars != null);
                LockerProgressBars.Release();
            }


        }

        public void UpdateProgressBarHealth(System.Guid id, double health)
        {
            if (id == System.Guid.Empty)
            {
                throw new System.ArgumentNullException(nameof(id));
            }
            if (health < 0.0 || health > 1.0)
            {
                throw new System.ArgumentOutOfRangeException(nameof(health));
            }

            if (_disposed == true)
            {
                throw new System.ObjectDisposedException(GetType().Name);
            }

            System.Diagnostics.Debug.Assert(LockerProgressBars != null);
            LockerProgressBars.Hold();

            try
            {
                System.Diagnostics.Debug.Assert(ProgressBars != null);
                ProgressBar progressBar = ProgressBars.Lookup(id);

                progressBar.UpdateHealth(health);

                System.Diagnostics.Debug.Assert(WorldRenderersByUserId != null);
                foreach (WorldRenderer renderer in WorldRenderersByUserId.GetValues())
                {
                    System.Diagnostics.Debug.Assert(renderer != null);
                    renderer.UpdateBossBarHealth(progressBar.Id, progressBar.Health);
                }

            }
            finally
            {
                System.Diagnostics.Debug.Assert(LockerProgressBars != null);
                LockerProgressBars.Release();
            }
        }

        public void CloseProgressBar(System.Guid id)
        {
            if (id == System.Guid.Empty)
            {
                throw new System.ArgumentNullException(nameof(id));
            }

            if (_disposed == true)
            {
                throw new System.ObjectDisposedException(GetType().Name);
            }

            System.Diagnostics.Debug.Assert(LockerProgressBars != null);
            LockerProgressBars.Hold();

            try
            {
                System.Diagnostics.Debug.Assert(ProgressBars != null);
                ProgressBar progressBar = ProgressBars.Extract(id);

                System.Diagnostics.Debug.Assert(WorldRenderersByUserId != null);
                foreach (WorldRenderer renderer in WorldRenderersByUserId.GetValues())
                {
                    System.Diagnostics.Debug.Assert(renderer != null);
                    renderer.CloseBossBar(progressBar.Id);
                }

            }
            finally
            {
                System.Diagnostics.Debug.Assert(LockerProgressBars != null);
                LockerProgressBars.Release();
            }
        }

        public void WriteMessageInChatBox(params TextComponent[] components)
        {
            if (_disposed == true)
            {
                throw new System.ObjectDisposedException(GetType().Name);
            }

            if (components == null)
            {
                components = [];
            }

            var extra = new object[components.Length];

            for (int i = 0; i < components.Length; ++i)
            {
                TextComponent component = components[i];

                extra[i] = new
                {
                    text = component.Text,
                    color = component.Color.GetName(),
                };
            }

            var chat = new
            {
                text = "",
                extra = extra,
            };

            string data = System.Text.Json.JsonSerializer.Serialize(chat);

            System.Diagnostics.Debug.Assert(WorldRenderersByUserId != null);
            foreach (WorldRenderer renderer in WorldRenderersByUserId.GetValues())
            {
                System.Diagnostics.Debug.Assert(renderer != null);
                renderer.WriteChatInChatBox(data);
            }

        }

        internal void Disconnect(UserId id)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            PlayerList.Disconnect(id);

            WorldRenderersByUserId.Extract(id);

            // TODO: world border
            // TODO: Boss Bar
        }

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

        internal void StartObjectRoutines()
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            while (Objects.Dequeue(out PhysicsObject obj))
            {
                System.Diagnostics.Debug.Assert(obj != null);

                obj.StartRoutine(this);

                Objects.Enqueue(obj);
            }
        }

        internal void ControlPlayers()
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            while (Objects.DequeuePlayer(out AbstractPlayer player))
            {
                System.Diagnostics.Debug.Assert(player != null);

                player.Control(this);

                Objects.Enqueue(player);
            }
        }

        internal void HandleDeathEvents()
        {
            while (Objects.Dequeue(out PhysicsObject obj))
            {
                System.Diagnostics.Debug.Assert(obj != null);

                if (obj.HandleDeath())
                {
                    //if (obj is AbstractPlayer player)
                    //{
                    //    player.Respawn();
                    //}

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
            bool despawned = DetermineToDespawnPlayerOnDisconnect();
            bool cleanup;

            while (Objects.DequeuePlayer(out AbstractPlayer player))
            {
                System.Diagnostics.Debug.Assert(player != null);

                if (despawned == false)
                {
                    if (player.HandleDisconnection(out UserId userId, this) == true)
                    {
                        System.Diagnostics.Debug.Assert(userId != UserId.Null);

                        DisconnectedPlayers.Insert(userId, player);
                    }
                }
                else
                {
                    cleanup = false;

                    if (DisconnectedPlayers.Contains(player.UserId) == true)
                    {
                        DisconnectedPlayers.Extract(player.UserId);

                        cleanup = true;
                    }
                    else if (player.HandleDisconnection(out UserId userId, this) == true)
                    {
                        cleanup = true;
                    }

                    if (cleanup == true)
                    {
                        PlayerList.Remove(player.UserId);

                        PlayersByUserId.Extract(player.UserId);
                        PlayersByUsername.Extract(player.Username);

                        ObjectDespawningPool.Enqueue(player);

                        player.OnDisconnected();

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

                obj.Move(this, volume, v);

                UpdateObjectMapping(obj);

                Objects.Enqueue(obj);
            }

        }

        protected abstract AbstractPlayer CreatePlayer(UserId userId, string username);

        internal void ConnectPlayer(MinecraftClient client, string username, UserId userId)
        {
            System.Diagnostics.Debug.Assert(client != null);
            System.Diagnostics.Debug.Assert(username != "");
            System.Diagnostics.Debug.Assert(userId != UserId.Null);

            System.Diagnostics.Debug.Assert(!_disposed);

            AbstractPlayer player;

            if (DisconnectedPlayers.Contains(userId))
            {
                player = DisconnectedPlayers.Extract(userId);
                System.Diagnostics.Debug.Assert(player != null);
            }
            else
            {
                player = CreatePlayer(userId, username);
                System.Diagnostics.Debug.Assert(player != null);

                ObjectSpawningPool.Enqueue(player);

                PlayerList.Add(userId, username);

                PlayersByUserId.Insert(userId, player);
                PlayersByUsername.Insert(username, player);
            }

            player.Connect(client, this, userId);

        }

        internal void CreateObjects()
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            while (ObjectSpawningPool.Dequeue(out PhysicsObject obj) == true)
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

        protected override void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if (_disposed == false)
            {
                System.Diagnostics.Debug.Assert(ObjectSpawningPool.Empty == true);
                System.Diagnostics.Debug.Assert(ObjectDespawningPool.Empty == true);

                System.Diagnostics.Debug.Assert(EntitiesById.Empty == true);

                System.Diagnostics.Debug.Assert(DisconnectedPlayers.Empty == true);

                // If disposing equals true, dispose all managed
                // and unmanaged resources.
                if (disposing == true)
                {
                    PlayerList.Dispose();

                    ObjectSpawningPool.Dispose();
                    ObjectDespawningPool.Dispose();

                    Objects.Dispose();

                    EntitiesById.Dispose();
                    PlayersByUserId.Dispose();
                    PlayersByUsername.Dispose();

                    DisconnectedPlayers.Dispose();

                    BlockContext.Dispose();

                    LockerProgressBars.Dispose();
                    ProgressBars.Dispose();
                }

                // Call the appropriate methods to clean up
                // unmanaged resources here.
                // If disposing is false,
                // only the following code is executed.
                //CloseHandle(handle);
                //handle = IntPtr.Zero;

                // Note disposing has been done.
                _disposed = true;
            }

            base.Dispose(disposing);
        }


    }


}

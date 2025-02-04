using Containers;


namespace MinecraftServerEngine.Protocols
{
    public sealed class ConnectionListener : IConnectionListener
    {
        private bool _disposed = false;

        private readonly ConcurrentQueue<User> Users = new();

        ~ConnectionListener() => System.Diagnostics.Debug.Assert(false);

        public void AddUser(User user)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            Users.Enqueue(user);
        }

        public void Accept(World world)
        {
            System.Diagnostics.Debug.Assert(world != null);

            System.Diagnostics.Debug.Assert(!_disposed);

            while (Users.Dequeue(out User user) == true)
            {
                if (world.CanJoinWorld() == false)
                {
                    // TODO: Send message why disconnected.
                    user.Client.Dispose();

                    continue;
                }

                world.ConnectPlayer(user);
            }
        }

        public void Dispose()
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            // Assertion.

            // Release resources.

            // Finish
            System.GC.SuppressFinalize(this);
            _disposed = true;
        }

    }
}

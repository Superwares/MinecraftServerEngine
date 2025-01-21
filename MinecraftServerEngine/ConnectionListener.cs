using Containers;

using MinecraftPrimitives;

namespace MinecraftServerEngine
{
    public sealed class ConnectionListener : IConnectionListener
    {
        private bool _disposed = false;

        private readonly ConcurrentQueue<User> Users = new();

        ~ConnectionListener() => System.Diagnostics.Debug.Assert(false);

        public void AddUser(MinecraftClient client, System.Guid userId, string username)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            Users.Enqueue(new User(client, userId, username));
        }

        public void Accept(World world)
        {
            System.Diagnostics.Debug.Assert(world != null);

            System.Diagnostics.Debug.Assert(!_disposed);

            while (Users.Dequeue(out User user))
            {
                if (!world.CanJoinWorld())
                {
                    // TODO: Send message why disconnected.
                    user.Client.Dispose();

                    continue;
                }

                world.ConnectPlayer(user.Client, user.Username, user.Id);
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

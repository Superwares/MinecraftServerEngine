

namespace MinecraftServerEngine.Protocols
{
    public interface IConnectionListener : System.IDisposable
    {
        public void AddUser(User user);

    }
}

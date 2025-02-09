
namespace MinecraftServerEngine.Protocols
{
    internal readonly struct User
    {
        public readonly MinecraftClient Client;
        public readonly UserId Id;
        public readonly string Username;

        public readonly UserProperty[] Properties;

        internal User(
            MinecraftClient client,
            System.Guid userId, string username,
            params UserProperty[] properties)
        {
            System.Diagnostics.Debug.Assert(client != null);
            System.Diagnostics.Debug.Assert(userId != System.Guid.Empty);
            System.Diagnostics.Debug.Assert(username != null);
            System.Diagnostics.Debug.Assert(string.IsNullOrEmpty(username) == false);

            if (properties == null)
            {
                properties = [];
            }

            Client = client;
            Id = new UserId(userId);
            Username = username;

            Properties = new UserProperty[properties.Length];
            System.Array.Copy(properties, Properties, properties.Length);
        }

    }

}

using Common;
using Containers;

using MinecraftPrimitives;

namespace MinecraftServerEngine.Renderers
{
    internal sealed class PlayerListRenderer : Renderer
    {
        internal PlayerListRenderer(
            ConcurrentQueue<ClientboundPlayingPacket> outPackets)
            : base(outPackets) { }

        internal void AddPlayerWithLaytency(
            UserId userId, string username,
            UserProperty[] properties,
            long ticks)
        {
            System.Diagnostics.Debug.Assert(userId != UserId.Null);
            System.Diagnostics.Debug.Assert(username != "");
            System.Diagnostics.Debug.Assert(ticks <= int.MaxValue);
            System.Diagnostics.Debug.Assert(ticks >= int.MinValue);

            if (properties == null)
            {
                properties = [];
            }

            (string Name, string Value, string Signature)[] _properties =
                new (string Name, string Value, string Signature)[properties.Length];

            for (int i = 0; i < properties.Length; ++i)
            {
                UserProperty property = properties[i];
                _properties[i] = (property.Name, property.Value, property.Signature);
            }

            long ms = ticks * 50;
            System.Diagnostics.Debug.Assert(ms >= int.MinValue);
            System.Diagnostics.Debug.Assert(ms <= int.MaxValue);
            Render(new PlayerListItemAddPacket(userId.Value, username, _properties, (int)ms));
        }

        internal void RemovePlayer(UserId id)
        {
            System.Diagnostics.Debug.Assert(id != UserId.Null);

            Render(new PlayerListItemRemovePacket(id.Value));
        }

        internal void UpdatePlayerLatency(UserId id, long ticks)
        {
            System.Diagnostics.Debug.Assert(id != UserId.Null);
            System.Diagnostics.Debug.Assert(ticks <= int.MaxValue);
            System.Diagnostics.Debug.Assert(ticks >= int.MinValue);

            long ms = ticks * 50;
            System.Diagnostics.Debug.Assert(ms >= int.MinValue);
            System.Diagnostics.Debug.Assert(ms <= int.MaxValue);
            Render(new PlayerListItemUpdateLatencyPacket(id.Value, (int)ms));
        }
    }

}

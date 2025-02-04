using Common;
using Containers;

using MinecraftServerEngine.Protocols;
using MinecraftServerEngine.Physics;

namespace MinecraftServerEngine.Renderers
{
    internal sealed class WorldRenderer : Renderer
    {

        internal WorldRenderer(
            ConcurrentQueue<ClientboundPlayingPacket> outPackets)
            : base(outPackets) { }

        // title, worldboarder, chattings, sound, particles

        internal void PlaySound(string name, int category, Vector p, double volume, double pitch)
        {
            System.Diagnostics.Debug.Assert(name != null);
            System.Diagnostics.Debug.Assert(string.IsNullOrEmpty(name) == false);
            System.Diagnostics.Debug.Assert(volume >= 0.0F);
            System.Diagnostics.Debug.Assert(volume <= 1.0F);
            System.Diagnostics.Debug.Assert(pitch >= 0.5F);
            System.Diagnostics.Debug.Assert(pitch <= 2.0F);

            Render(new NamedSoundEffectPacket(
                //"entity.player.attack.strong", 7,
                name, category,
                (int)(p.X * 8), (int)(p.Y * 8), (int)(p.Z * 8),
                (float)volume, (float)pitch));
        }

        internal void DisplayTitle(int fadeIn, int stay, int fadeOut, string data)
        {
            System.Diagnostics.Debug.Assert(fadeIn >= 0);
            System.Diagnostics.Debug.Assert(stay >= 0);
            System.Diagnostics.Debug.Assert(fadeOut >= 0);

            System.Diagnostics.Debug.Assert(data != null);
            System.Diagnostics.Debug.Assert(string.IsNullOrEmpty(data) == false);

            Render(new SetTimesAndDisplayTitlePacket(fadeIn, stay, fadeOut));
            Render(new SetTitlePacket(data));
        }

        internal void OpenBossBar(System.Guid id, string title, double health,
            ProgressBarColor color, ProgressBarDivision division)
        {
            System.Diagnostics.Debug.Assert(id != System.Guid.Empty);
            System.Diagnostics.Debug.Assert(title != null);
            System.Diagnostics.Debug.Assert(health >= 0.0);
            System.Diagnostics.Debug.Assert(health <= 1.0);

            Render(new OpenBossBarPacket(id, title, (float)health, (int)color, (int)division, 0x01));
        }

        internal void UpdateBossBarHealth(System.Guid id, double health)
        {
            System.Diagnostics.Debug.Assert(id != System.Guid.Empty);
            System.Diagnostics.Debug.Assert(health >= 0.0);
            System.Diagnostics.Debug.Assert(health <= 1.0);

            Render(new UpdateBossBarHealthPacket(id, (float)health));
        }

        internal void CloseBossBar(System.Guid id)
        {
            System.Diagnostics.Debug.Assert(id != System.Guid.Empty);

            Render(new CloseBossBarPacket(id));
        }

        internal void WriteChatInChatBox(string data)
        {
            System.Diagnostics.Debug.Assert(data != null);
            System.Diagnostics.Debug.Assert(string.IsNullOrEmpty(data) == false);

            Render(new ClientboundChatmessagePacket(data, 0x00));
        }

        internal void InitWorldBorder(
            double centerX, double centerZ,
            double oldRadiusInMeters, double newRadiusInMeters,
            Time transitionTimePerMeter)
        {
            System.Diagnostics.Debug.Assert(oldRadiusInMeters > 0.0);
            System.Diagnostics.Debug.Assert(newRadiusInMeters > 0.0);
            System.Diagnostics.Debug.Assert(transitionTimePerMeter >= Time.Zero);

            Render(new WorldBorderInitPacket(
                centerX, centerZ,
                oldRadiusInMeters * 2.0, newRadiusInMeters * 2.0,
                transitionTimePerMeter.Amount / Time.FromMilliseconds(1).Amount
                ));
        }

        internal void SetWorldBorderCenter(double centerX, double centerZ)
        {
            Render(new WorldBorderCenterPacket(centerX, centerZ));
        }

        internal void SetWorldBorderRadius(double radiusInMeters)
        {
            System.Diagnostics.Debug.Assert(radiusInMeters > 0.0);

            Render(new WorldBorderSizePacket(radiusInMeters * 2.0));
        }

        internal void SetWorldBorderLerpSize(
            double oldRadiusInMeters, double newRadiusInMeters,
            Time transitionTimePerMeter)
        {
            System.Diagnostics.Debug.Assert(oldRadiusInMeters > 0.0);
            System.Diagnostics.Debug.Assert(newRadiusInMeters > 0.0);
            System.Diagnostics.Debug.Assert(transitionTimePerMeter >= Time.Zero);

            Render(new WorldBorderLerpSizePacket(
                oldRadiusInMeters * 2.0, newRadiusInMeters * 2.0,
                transitionTimePerMeter.Amount / Time.FromMilliseconds(1).Amount
                ));
        }

        internal void UpdateWorldTime(Time worldTime)
        {
            System.Diagnostics.Debug.Assert(worldTime >= Time.Zero);
            System.Diagnostics.Debug.Assert(worldTime <= MinecraftTimes.OneDay);

            // The world (or region) time, in ticks. If negative the sun will stop moving at the Math.abs of the time
            Render(new TimeUpdatePacket(0, -1 * MinecraftTimes.ToTicks(worldTime)));
        }
    }

}

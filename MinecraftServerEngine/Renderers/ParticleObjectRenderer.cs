using Common;
using Containers;
using MinecraftServerEngine.Protocols;
using MinecraftServerEngine.Blocks;
using MinecraftServerEngine.Items;
using MinecraftServerEngine.Physics;

namespace MinecraftServerEngine.Renderers
{
    internal sealed class ParticleObjectRenderer : PhysicsObjectRenderer
    {
        public ParticleObjectRenderer(
            ConcurrentQueue<ClientboundPlayingPacket> outPackets,
            ChunkLocation loc, int d)
            : base(outPackets, loc, d, false)
        {
        }

        internal void Move(
            Particle particle,
            Vector[] points,
            double speed, int count,
            double r, double g, double b)
        {
            System.Diagnostics.Debug.Assert(r >= 0.0D);
            System.Diagnostics.Debug.Assert(r <= 1.0D);
            System.Diagnostics.Debug.Assert(g >= 0.0D);
            System.Diagnostics.Debug.Assert(g <= 1.0D);
            System.Diagnostics.Debug.Assert(b >= 0.0D);
            System.Diagnostics.Debug.Assert(b <= 1.0D);
            System.Diagnostics.Debug.Assert(speed >= 0.0F);
            System.Diagnostics.Debug.Assert(speed <= 1.0F);
            System.Diagnostics.Debug.Assert(count >= 0);

            if (count == 0)
            {
                return;
            }

            if (particle != Particle.Reddust)
            {
                r = 0.0F;
                g = 0.0F;
                b = 0.0F;
            }

            foreach (Vector p in points)
            {
                System.Diagnostics.Debug.Assert(p.X >= double.MinValue);
                System.Diagnostics.Debug.Assert(p.X <= double.MaxValue);
                System.Diagnostics.Debug.Assert(p.Y >= double.MinValue);
                System.Diagnostics.Debug.Assert(p.Y <= double.MaxValue);
                System.Diagnostics.Debug.Assert(p.Z >= double.MinValue);
                System.Diagnostics.Debug.Assert(p.Z <= double.MaxValue);
                Render(new ParticlesPacket(
                    (int)particle, true,
                    (float)p.X, (float)p.Y, (float)p.Z,
                    (float)r, (float)g, (float)b,
                   (float)speed, count));
            }
        }

    }

}

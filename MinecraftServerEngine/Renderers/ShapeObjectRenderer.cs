using Containers;

using MinecraftServerEngine.Blocks;
using MinecraftServerEngine.Particles;
using MinecraftServerEngine.Protocols;
using MinecraftServerEngine.Physics;

namespace MinecraftServerEngine.Renderers
{
    internal sealed class ShapeObjectRenderer : PhysicsObjectRenderer
    {
        public ShapeObjectRenderer(
            ConcurrentQueue<ClientboundPlayingPacket> outPackets, 
            ChunkLocation loc, int d) 
            : base(outPackets, loc, d, false)
        {
        }

        internal void ShowParticles(
            Particle particle,
            Vector v,
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

            System.Diagnostics.Debug.Assert(v.X >= double.MinValue);
            System.Diagnostics.Debug.Assert(v.X <= double.MaxValue);
            System.Diagnostics.Debug.Assert(v.Y >= double.MinValue);
            System.Diagnostics.Debug.Assert(v.Y <= double.MaxValue);
            System.Diagnostics.Debug.Assert(v.Z >= double.MinValue);
            System.Diagnostics.Debug.Assert(v.Z <= double.MaxValue);
            Render(new ParticlesPacket(
                (int)particle, true,
                (float)v.X, (float)v.Y, (float)v.Z,
                (float)r, (float)g, (float)b,
                (float)speed, count));
        }
    }
}

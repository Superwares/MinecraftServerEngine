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
            double extra, int count,
            double offsetX, double offsetY, double offsetZ)
        {
            System.Diagnostics.Debug.Assert(Disconnected == false);

            System.Diagnostics.Debug.Assert(offsetX >= 0.0D);
            System.Diagnostics.Debug.Assert(offsetX <= 1.0D);
            System.Diagnostics.Debug.Assert(offsetY >= 0.0D);
            System.Diagnostics.Debug.Assert(offsetY <= 1.0D);
            System.Diagnostics.Debug.Assert(offsetZ >= 0.0D);
            System.Diagnostics.Debug.Assert(offsetZ <= 1.0D);
            System.Diagnostics.Debug.Assert(extra >= 0.0F);
            System.Diagnostics.Debug.Assert(extra <= 1.0F);
            System.Diagnostics.Debug.Assert(count >= 0);

            //if (count == 0)
            //{
            //    return;
            //}

            //if (particle != Particle.Reddust)
            //{
            //    r = 0.0F;
            //    g = 0.0F;
            //    b = 0.0F;
            //}

            System.Diagnostics.Debug.Assert(v.X >= double.MinValue);
            System.Diagnostics.Debug.Assert(v.X <= double.MaxValue);
            System.Diagnostics.Debug.Assert(v.Y >= double.MinValue);
            System.Diagnostics.Debug.Assert(v.Y <= double.MaxValue);
            System.Diagnostics.Debug.Assert(v.Z >= double.MinValue);
            System.Diagnostics.Debug.Assert(v.Z <= double.MaxValue);
            Render(new ParticlesPacket(
                (int)particle, true,
                (float)v.X, (float)v.Y, (float)v.Z,
                (float)offsetX, (float)offsetY, (float)offsetZ,
                (float)extra, count));
        }
    }
}

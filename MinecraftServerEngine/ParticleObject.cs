﻿
using Common;
using Containers;

namespace MinecraftServerEngine
{
    using PhysicsEngine;

    public abstract class ParticleObject : PhysicsObject
    {
        private sealed class RendererManager : System.IDisposable
        {
            private bool _disposed = false;

            private readonly ConcurrentTree<ParticleObjectRenderer> Renderers = new();  // Disposable

            public RendererManager()
            {
            }

            ~RendererManager() => System.Diagnostics.Debug.Assert(false);

            internal bool Apply(ParticleObjectRenderer renderer)
            {
                System.Diagnostics.Debug.Assert(renderer != null);

                System.Diagnostics.Debug.Assert(!_disposed);

                if (Renderers.Contains(renderer))
                {
                    return false;
                }

                System.Diagnostics.Debug.Assert(Renderers != null);
                Renderers.Insert(renderer);

                return true;
            }

            internal void HandleRendering(Vector p)
            {
                System.Diagnostics.Debug.Assert(!_disposed);

                using Queue<ParticleObjectRenderer> queue = new();

                System.Diagnostics.Debug.Assert(Renderers != null);
                foreach (ParticleObjectRenderer renderer in Renderers.GetKeys())
                {
                    System.Diagnostics.Debug.Assert(renderer != null);

                    if (renderer.CanRender(p))
                    {
                        continue;
                    }

                    queue.Enqueue(renderer);
                }

                while (!queue.Empty)
                {
                    ParticleObjectRenderer renderer = queue.Dequeue();

                    Renderers.Extract(renderer);
                }

            }


            internal void Move(Vector[] points, float r, float g, float b)
            {
                System.Diagnostics.Debug.Assert(points != null);

                System.Diagnostics.Debug.Assert(r > 0.0D);
                System.Diagnostics.Debug.Assert(r <= 1.0D);
                System.Diagnostics.Debug.Assert(g > 0.0D);
                System.Diagnostics.Debug.Assert(g <= 1.0D);
                System.Diagnostics.Debug.Assert(b > 0.0D);
                System.Diagnostics.Debug.Assert(b <= 1.0D);

                System.Diagnostics.Debug.Assert(!_disposed);

                System.Diagnostics.Debug.Assert(Renderers != null);
                foreach (ParticleObjectRenderer renderer in Renderers.GetKeys())
                {
                    System.Diagnostics.Debug.Assert(renderer != null);
                    renderer.Move(points, r, g, b);
                }
            
            }

            public void Dispose()
            {
                // Assertions.
                System.Diagnostics.Debug.Assert(!_disposed);

                // Release  resources.
                Renderers.Dispose();

                // Finish
                System.GC.SuppressFinalize(this);
                _disposed = true;
            }

        }

        private static double GetArea(double r)
        {
            System.Diagnostics.Debug.Assert(r > 0.0D);

            return 4 * System.Math.PI * r * r;
        }

        private const double MinRadius = 0.1D;
        private static readonly double MinArea = GetArea(MinRadius);

        private bool _disposed = false;

        private readonly double Radius;
        /*private Vector[] _points;*/
        private readonly float Red, Green, Blue;

        private RendererManager Manager = new();  // Disposable

        private static float Normalize(byte v)
        {
            return (v > 0) ? ((float)v / (float)byte.MaxValue) : 0.0001F;
        }

        private static Vector[] GetPoints(Vector p, double r)
        {
            System.Diagnostics.Debug.Assert(r >= MinRadius);

            if (r == MinRadius)
            {
                return [p];
            }

            int n = (int)(GetArea(r) / MinArea) / 3;
            if (n == 1)
            {
                return [p];
            }

            var points = new Vector[n];
            for (int i = 0; i < n; ++i)
            {
                double theta = 2 * System.Math.PI * Random.NextDouble(),
                    phi = System.Math.PI * Random.NextDouble();

                double d = r * Random.NextDouble();

                double x = (d * System.Math.Sin(phi) * System.Math.Cos(theta)) + p.X,
                    y = (d * System.Math.Sin(phi) * System.Math.Sin(theta)) + p.Y,
                    z = (d * System.Math.Cos(phi)) + p.Z;

                points[i] = new Vector(x, y, z);
            }

            return points;
        }

        private static Vector[] UpdatePoints(Vector[] points, Vector v)
        {
            int n = points.Length;
            var newPoints = new Vector[n];

            for (int i = 0; i < n; ++i)
            {
                newPoints[i] = (points[i] + v);
            }

            return newPoints;
        }

        private static double GetMiddleRadius(double r)
        {
            double a = Math.Sqrt(2);
            System.Diagnostics.Debug.Assert(((a - 1) * r) < 1.0D);
            return (2 * r) / (a + 1);
        }

        internal ParticleObject(
            Vector p, double r, double m,
            byte red, byte green, byte blue, 
            Movement movement)
            : base(m, AxisAlignedBoundingBox.Generate(p, GetMiddleRadius(r)), movement)
        {
            System.Diagnostics.Debug.Assert(movement != null);

            /*_points = GetPoints(p, r);*/
            /*Console.Printl($"length: {_points.Length}");*/

            Radius = r;

            Red = Normalize(red);
            Green = Normalize(green);
            Blue = Normalize(blue);
        }

        internal void ApplyRenderer(ParticleObjectRenderer renderer)
        {
            System.Diagnostics.Debug.Assert(renderer != null);

            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(Manager != null);
            
            Manager.Apply(renderer);
        }

        protected override BoundingVolume GenerateBoundingVolume()
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            // TODO: Apply force based on volume size.
            Forces.Enqueue(
                -1.0D *
                new Vector(1.0D - 0.9999d, 1.0D - 0.9999d, 1.0D - 0.9999d) *
                Velocity);  // Damping Force

            return BoundingVolume;
        }

        internal override void Move(BoundingVolume volume, Vector v)
        {
            System.Diagnostics.Debug.Assert(volume != null);

            System.Diagnostics.Debug.Assert(!_disposed);

            Vector p = volume.GetCenter();

            System.Diagnostics.Debug.Assert(Manager != null);
            Manager.HandleRendering(p);

            /*_points = UpdatePoints(_points, v);*/
            Manager.Move(GetPoints(p, Radius), Red, Green, Blue);

            base.Move(volume, v);
        }

        // TODO: set color

        public override void Dispose()
        {
            // Assertions.
            System.Diagnostics.Debug.Assert(!_disposed);

            // Release Resources.
            Manager.Dispose();

            // Finish.
            base.Dispose();
            _disposed = true;
        }

    }

    public abstract class FreeParticle : ParticleObject
    {
        protected FreeParticle(
            Vector p, double r, double m,
            byte red, byte green, byte blue)
            : base(p, r, m, red, green, blue, new WallPharsing())
        {

        }
    }

    public abstract class SimpleParticle : ParticleObject
    {
        protected SimpleParticle(
            Vector p, double r, double m,
            byte red, byte green, byte blue)
            : base(p, r, m, red, green, blue, new SimpleMovement())
        {

        }
    }

    public abstract class SmoothParticle : ParticleObject
    {
        protected SmoothParticle(
            Vector p, double r, double m,
            byte red, byte green, byte blue)
            : base(p, r, m, red, green, blue, new SmoothMovement())
        {

        }
    }

}

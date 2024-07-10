﻿using Common;

namespace MinecraftServerEngine.PhysicsEngine
{
    public readonly struct Vector : System.IEquatable<Vector>
    {
        public static bool operator !=(Vector v1, Vector v2)
        {
            return v1.X != v2.X || v1.Y != v2.Y || v1.Z != v2.Z;
        }

        public static bool operator ==(Vector v1, Vector v2)
        {
            return v1.X == v2.X && v1.Y == v2.Y && v1.Z == v2.Z;
        }

        public static Vector operator +(Vector v1, Vector v2)
        {
            return new(v1.X + v2.X, v1.Y + v2.Y, v1.Z + v2.Z);
        }

        public static Vector operator *(Vector v1, Vector v2)
        {
            return new(v1.X * v2.X, v1.Y * v2.Y, v1.Z * v2.Z);
        }

        public static Vector operator *(Vector v, double s)
        {
            return new(v.X * s, v.Y * s, v.Z * s);
        }

        public static Vector operator *(double s, Vector v)
        {
            return new(s * v.X, s * v.Y, s * v.Z);
        }

        public static Vector operator /(Vector v, double s)
        {
            return new(v.X / s, v.Y / s, v.Z / s);
        }

        public static Vector operator /(double s, Vector v)
        {
            return new(s / v.X, s / v.Y, s / v.Z);
        }

        public static double GetLengthSquared(Vector v1, Vector v2)
        {
            double dx = v1.X - v2.X,
                   dy = v1.Y - v2.Y,
                   dz = v1.Z - v2.Z;
            return (dx * dx) + (dy * dy) + (dz * dz);
        }

        public static double GetLength(Vector v1, Vector v2)
        {
            double s = GetLengthSquared(v1, v2);
            return Math.Sqrt(s);
        }

        public static readonly Vector Zero = new(0.0D, 0.0D, 0.0D);

        public readonly double X, Y, Z;

        public Vector(double x, double y, double z)
        {
            X = x; Y = y; Z = z;
        }

        public double GetLengthSquared()
        {
            return (X * X) + (Y * Y) + (Z * Z);
        }

        public double GetLength()
        {
            double s = GetLengthSquared();
            return Math.Sqrt(s);
        }

        public override readonly string ToString()
        {
            return $"( X: {X}, Y: {Y}, Z: {Z} )";
        }

        public readonly bool Equals(Vector other)
        {
            return this == other;
        }

        public override bool Equals(object obj)
        {
            return obj is Vector v && Equals(v);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

    }
}

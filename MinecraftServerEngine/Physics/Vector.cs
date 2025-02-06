using Common;

namespace MinecraftServerEngine.Physics
{
    public readonly struct Vector : System.IEquatable<Vector>
    {
        public const int Dimension = 3;


        public static readonly Vector Zero = new(0.0D, 0.0D, 0.0D);


        public readonly double X, Y, Z;


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

        public static Vector operator -(Vector v1, Vector v2)
        {
            return new(v1.X - v2.X, v1.Y - v2.Y, v1.Z - v2.Z);
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

        public static void Rotate(Angles angles, Vector[] vectors)
        {
            // Calculate the rotation matrix from the angles
            double cosRoll = System.Math.Cos(angles.Roll);
            double sinRoll = System.Math.Sin(angles.Roll);
            double cosPitch = System.Math.Cos(angles.Pitch);
            double sinPitch = System.Math.Sin(angles.Pitch);
            double cosYaw = System.Math.Cos(angles.Yaw);
            double sinYaw = System.Math.Sin(angles.Yaw);

            // Rotation matrix components
            double m11 = cosYaw * cosPitch;
            double m12 = cosYaw * sinPitch * sinRoll - sinYaw * cosRoll;
            double m13 = cosYaw * sinPitch * cosRoll + sinYaw * sinRoll;

            double m21 = sinYaw * cosPitch;
            double m22 = sinYaw * sinPitch * sinRoll + cosYaw * cosRoll;
            double m23 = sinYaw * sinPitch * cosRoll - cosYaw * sinRoll;

            double m31 = -sinPitch;
            double m32 = cosPitch * sinRoll;
            double m33 = cosPitch * cosRoll;

            for (int i = 0; i < vectors.Length; ++i)
            {
                vectors[i] = new Vector(
                    (vectors[i].X * m11) + (vectors[i].Y * m12) + (vectors[i].Z * m13),
                    (vectors[i].X * m21) + (vectors[i].Y * m22) + (vectors[i].Z * m23),
                    (vectors[i].X * m31) + (vectors[i].Y * m32) + (vectors[i].Z * m33)
                );
            }
        }

        public static Vector Rotate(Angles angles, Vector vector)
        {
            // Calculate the rotation matrix from the angles
            double cosRoll = System.Math.Cos(angles.Roll);
            double sinRoll = System.Math.Sin(angles.Roll);
            double cosPitch = System.Math.Cos(angles.Pitch);
            double sinPitch = System.Math.Sin(angles.Pitch);
            double cosYaw = System.Math.Cos(angles.Yaw);
            double sinYaw = System.Math.Sin(angles.Yaw);

            // Rotation matrix components
            double m11 = cosYaw * cosPitch;
            double m12 = cosYaw * sinPitch * sinRoll - sinYaw * cosRoll;
            double m13 = cosYaw * sinPitch * cosRoll + sinYaw * sinRoll;

            double m21 = sinYaw * cosPitch;
            double m22 = sinYaw * sinPitch * sinRoll + cosYaw * cosRoll;
            double m23 = sinYaw * sinPitch * cosRoll - cosYaw * sinRoll;

            double m31 = -sinPitch;
            double m32 = cosPitch * sinRoll;
            double m33 = cosPitch * cosRoll;

            return new Vector(
                (vector.X * m11) + (vector.Y * m12) + (vector.Z * m13),
                (vector.X * m21) + (vector.Y * m22) + (vector.Z * m23),
                (vector.X * m31) + (vector.Y * m32) + (vector.Z * m33)
            );
        }
    
        public static Vector CrossProduct(Vector v1, Vector v2)
        {
            return new Vector(
                (v1.Y * v2.Z) - (v1.Z * v2.Y),
                (v1.Z * v2.X) - (v1.X * v2.Z),
                (v1.X * v2.Y) - (v1.Y * v2.X)
            );
        }

        public static double DotProduct(Vector v1, Vector v2)
        {
            return (v1.X * v2.X) + (v1.Y * v2.Y) + (v1.Z * v2.Z);
        }


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

        public Vector Abs()
        {
            return new Vector(Math.Abs(X), Math.Abs(Y), Math.Abs(Z));
        }

        public Vector Rotate(Angles angles)
        {
            return Vector.Rotate(angles, this);
        }

        public Vector CrossProduct(Vector other)
        {
            return Vector.CrossProduct(this, other);
        }

        public double DotProduct(Vector other)
        {
            return Vector.DotProduct(this, other);
        }

        /// <summary>
        /// Clamps the vector's components to the specified minimum and maximum values.
        /// </summary>
        /// <param name="min">Inclusive minimum value</param>
        /// <param name="max">Inclusive maximum value</param>
        /// <returns>A new vector with clamped components</returns>
        /// <exception cref="System.ArgumentOutOfRangeException"></exception>
        public Vector Clamp(double min, double max)
        {
            if (min > max)
            {
                throw new System.ArgumentOutOfRangeException(nameof(min));
            }

            double x, y, z;

            if (X > max)
            {
                x = max;
            } else if (X < min)
            {
                x = min;
            } else
            {
                x = X;
            }

            if (Y > max)
            {
                y = max;
            }
            else if (Y < min)
            {
                y = min;
            }
            else
            {
                y = Y;
            }

            if (Z > max)
            {
                z = max;
            }
            else if (Z < min)
            {
                z = min;
            }
            else
            {
                z = Z;
            }

            return new Vector(x, y, z);
        }

        public static bool TryParse(string _x, string _y, string _z, out Vector v)
        {
            if (
                double.TryParse(_x, out double x) == false ||
                double.TryParse(_y, out double y) == false ||
                double.TryParse(_z, out double z) == false)
            {
                v = Zero;
                return false;
            }

            v = new Vector(x, y, z);
            return true;
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

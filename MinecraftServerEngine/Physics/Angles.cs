

namespace MinecraftServerEngine.Physics
{
    /// <summary>
    /// Represents the angles in a 3D space.
    /// </summary>
    public readonly struct Angles
    {

        /// <summary>
        /// Gets the roll angle (based on x axis).
        /// </summary>
        public double Roll { get; }

        /// <summary>
        /// Gets the yaw angle (based on y axis).
        /// </summary>
        public double Yaw { get; }

        /// <summary>
        /// Gets the pitch angle (based on z axis).
        /// </summary>
        public double Pitch { get; }

        public static Angles operator +(Angles angles1, Angles angles2)
        {
            return new(
                angles1.Roll + angles2.Roll,
                angles1.Yaw + angles2.Yaw,
                angles1.Pitch + angles2.Pitch
                );
        }

        public static Angles CreateByDegrees(double roll, double yaw, double pitch)
        {
            return new Angles(
                roll * (System.Math.PI / 180.0),
                yaw * (System.Math.PI / 180.0),
                pitch * (System.Math.PI / 180.0)
                );
        }

        /// <summary>
        /// Converts a unit vector to angles (roll, yaw, pitch).
        /// </summary>
        /// <param name="vector">The unit vector.</param>
        /// <returns>An instance of <see cref="Angles"/> representing the roll, yaw, and pitch.</returns>
        public static Angles FromUnitVector(Vector vector)
        {
            double x = vector.X;
            double y = vector.Y;
            double z = vector.Z;

            // Calculate yaw (rotation around the Y-axis)
            double yaw = System.Math.Atan2(y, x);

            // Calculate pitch (rotation around the X-axis)
            double pitch = System.Math.Asin(z);

            // Calculate roll (rotation around the Z-axis)
            double roll = 0; // Assuming roll is zero for a unit vector

            return new Angles(roll, yaw, pitch);
        }


        public static Angles GetFromStartToEnd(Angles from, Angles to)
        {
            double rollDifference = to.Roll - from.Roll;
            double yawDifference = to.Yaw - from.Yaw;
            double pitchDifference = to.Pitch - from.Pitch;

            return new Angles(rollDifference, yawDifference, pitchDifference);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Angles"/> struct.
        /// </summary>
        /// <param name="roll">The roll angle.</param>
        /// <param name="yaw">The yaw angle.</param>
        /// <param name="pitch">The pitch angle.</param>
        public Angles(double roll, double yaw, double pitch)
        {
            Roll = roll;
            Yaw = yaw;
            Pitch = pitch;
        }

        /// <summary>
        /// Gets the unit vector of the angles.
        /// </summary>
        /// <returns>A tuple representing the unit vector (x, y, z).</returns>
        public Vector ToUnitVector()
        {
            //double cosRoll = System.Math.Cos(Roll);
            //double sinRoll = System.Math.Sin(Roll);
            double cosYaw = System.Math.Cos(Yaw);
            double sinYaw = System.Math.Sin(Yaw);
            double cosPitch = System.Math.Cos(Pitch);
            double sinPitch = System.Math.Sin(Pitch);

            double x = cosYaw * cosPitch;
            double y = sinYaw * cosPitch;
            double z = sinPitch;

            return new Vector(x, y, z);
        }

        public override string ToString()
        {
            return $"[{Roll},{Yaw},{Pitch}]";
        }
    }
}

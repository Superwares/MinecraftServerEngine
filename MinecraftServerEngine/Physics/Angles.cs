

namespace MinecraftServerEngine.Physics
{
    /// <summary>
    /// Represents the angles in a 3D space.
    /// </summary>
    public readonly struct Angles
    {
        public static Angles operator +(Angles angles1, Angles angles2)
        {
            return new(
                angles1.Roll + angles2.Roll, 
                angles1.Yaw + angles2.Yaw, 
                angles1.Pitch + angles2.Pitch
                );
        }

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
    }
}



namespace MinecraftServerEngine
{
    using Physics;

    public readonly struct Angles : System.IEquatable<Angles>
    {
        public static readonly Angles Zero = new(0.0F, 0.0F);

        internal const double MaxYaw = 180, MinYaw = -180;
        internal const double MaxPitch = 90, MinPitch = -90;

        public readonly double Yaw, Pitch;

        private static double Frem(double angle)
        {
            double x = 360.0f;
            return angle - (x * (double)System.Math.Floor(angle / x));
        }

        public static bool TryParse(string _yaw, string _pitch, out Angles angles)
        {
            if (
                double.TryParse(_yaw, out double yaw) == false ||
                double.TryParse(_pitch, out double pitch) == false)
            {
                angles = Zero;
                return false;
            }

            if (yaw < MinYaw || MaxYaw < yaw)
            {
                angles = Zero;
                return false;
            }

            angles = new Angles(yaw, pitch);
            return true;
        }

        public static bool TryParse(double yaw, double pitch, out Angles angles)
        {
            if (yaw < MinYaw || MaxYaw < yaw)
            {
                angles = Zero;
                return false;
            }

            angles = new Angles(yaw, pitch);
            return true;
        }

        public Angles(double yaw, double pitch)
        {
            // TODO: map yaw from 180 to -180.
            /*System.Diagnostics.Debug.Assert(yaw >= MinYaw);
            System.Diagnostics.Debug.Assert(yaw <= MaxYaw);*/
            //System.Diagnostics.Debug.Assert(pitch >= MinPitch);
            //System.Diagnostics.Debug.Assert(pitch <= MaxPitch);

            if (pitch < MinPitch || MaxPitch < pitch)
            {
                throw new System.ArgumentOutOfRangeException(nameof(yaw));
            }

            Yaw = yaw;
            Pitch = pitch;
        }

        public Vector GetUnitVector()
        {
            /**
             * Reference: https://www.spigotmc.org/threads/converting-a-yaw-and-a-pitch-to-a-vector.639501/
             */

            double yaw = ((Yaw + 90.0D) * System.Math.PI) / 180;
            double pitch = ((Pitch + 90.0D) * System.Math.PI) / 180;

            double x = System.Math.Sin(pitch) * System.Math.Cos(yaw),
                y = System.Math.Cos(pitch),
                z = System.Math.Sin(pitch) * System.Math.Sin(yaw);

            return new Vector(x, y, z);
        }

        internal readonly (byte, byte) ConvertToProtocolFormat()
        {
            System.Diagnostics.Debug.Assert(Pitch >= MinPitch);
            System.Diagnostics.Debug.Assert(Pitch <= MaxPitch);

            double x = Frem(Yaw);
            double y = Frem(Pitch);

            return (
                (byte)((byte.MaxValue * x) / 360),
                (byte)((byte.MaxValue * y) / 360));
        }

        public readonly override string ToString()
        {
            throw new System.NotImplementedException();
        }

        public readonly bool Equals(Angles other)
        {
            return (Yaw == other.Yaw) && (Pitch == other.Pitch);
        }

    }
}

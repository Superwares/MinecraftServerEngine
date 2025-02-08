
using MinecraftServerEngine.Physics;

namespace MinecraftServerEngine.Entities
{

    public readonly struct EntityAngles : System.IEquatable<EntityAngles>
    {
        public static readonly EntityAngles Zero = new(0.0F, 0.0F);

        internal const double MaxYaw = 180, MinYaw = -180;
        internal const double MaxPitch = 90, MinPitch = -90;

        public readonly double Yaw, Pitch;

        private static double Frem(double angle)
        {
            double x = 360.0f;
            return angle - x * (double)System.Math.Floor(angle / x);
        }

        public static bool TryParse(string _yaw, string _pitch, out EntityAngles angles)
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

            angles = new EntityAngles(yaw, pitch);
            return true;
        }

        public static bool TryParse(double yaw, double pitch, out EntityAngles angles)
        {
            if (yaw < MinYaw || MaxYaw < yaw)
            {
                angles = Zero;
                return false;
            }

            angles = new EntityAngles(yaw, pitch);
            return true;
        }

        public EntityAngles(double yaw, double pitch)
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

        public Vector ToUnitVector()
        {
            /**
             * Reference: https://www.spigotmc.org/threads/converting-a-yaw-and-a-pitch-to-a-vector.639501/
             */

            double yaw = (Yaw + 90.0) * (System.Math.PI / 180.0);
            double pitch = (Pitch + 90.0) * (System.Math.PI / 180.0);

            double x = System.Math.Sin(pitch) * System.Math.Cos(yaw);
            double y = System.Math.Cos(pitch);
            double z = System.Math.Sin(pitch) * System.Math.Sin(yaw);

            return new Vector(x, y, z);
        }

        //public Angles ToAngles()
        //{
        //    Vector u = ToUnitVector();
        //    return Angles.FromUnitVector(u);
        //}

        internal readonly (byte, byte) ConvertToProtocolFormat()
        {
            System.Diagnostics.Debug.Assert(Pitch >= MinPitch);
            System.Diagnostics.Debug.Assert(Pitch <= MaxPitch);

            double x = Frem(Yaw);
            double y = Frem(Pitch);

            return (
                (byte)(byte.MaxValue * x / 360),
                (byte)(byte.MaxValue * y / 360));
        }

        public readonly override string ToString()
        {
            throw new System.NotImplementedException();
        }

        public readonly bool Equals(EntityAngles other)
        {
            return Yaw == other.Yaw && Pitch == other.Pitch;
        }

    }
}

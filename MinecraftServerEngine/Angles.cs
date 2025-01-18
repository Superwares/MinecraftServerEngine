

namespace MinecraftServerEngine
{
    using PhysicsEngine;

    public readonly struct Angles : System.IEquatable<Angles>
    {
        internal const float MaxYaw = 180, MinYaw = -180;
        internal const float MaxPitch = 90, MinPitch = -90;

        public readonly float Yaw, Pitch;

        private static float Frem(float angle)
        {
            float x = 360.0f;
            return angle - (x * (float)System.Math.Floor(angle / x));
        }

        public Angles(float yaw, float pitch)
        {
            // TODO: map yaw from 180 to -180.
            /*System.Diagnostics.Debug.Assert(yaw >= MinYaw);
            System.Diagnostics.Debug.Assert(yaw <= MaxYaw);*/
            System.Diagnostics.Debug.Assert(pitch >= MinPitch);
            System.Diagnostics.Debug.Assert(pitch <= MaxPitch);

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

            float x = Frem(Yaw);
            float y = Frem(Pitch);

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


namespace MinecraftServerEngine
{
    public readonly struct Look : System.IEquatable<Look>
    {
        internal const float MaxYaw = 180, MinYaw = -180;
        internal const float MaxPitch = 90, MinPitch = -90;

        public readonly float Yaw, Pitch;

        private static float Frem(float angle)
        {
            float y = 360.0f;
            return angle - (y * (float)System.Math.Floor(angle / y));
        }

        public Look(float yaw, float pitch)
        {
            // TODO: map yaw from 180 to -180.
            /*System.Diagnostics.Debug.Assert(yaw >= MinYaw);
            System.Diagnostics.Debug.Assert(yaw <= MaxYaw);*/
            System.Diagnostics.Debug.Assert(pitch >= MinPitch);
            System.Diagnostics.Debug.Assert(pitch <= MaxPitch);

            Yaw = yaw;
            Pitch = pitch;
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

        public readonly bool Equals(Look other)
        {
            return (Yaw == other.Yaw) && (Pitch == other.Pitch);
        }

    }
}


namespace MinecraftServerEngine
{
    public readonly struct Look : System.IEquatable<Look>
    {
        internal const float MaxYaw = 180, MinYaw = -180;
        internal const float MaxPitch = 90, MinPitch = -90;

        public readonly float YAW, PITCH;

        private static float Frem(float angle)
        {
            float y = 360.0f;
            return angle - (y * (float)System.Math.Floor(angle / y));
        }

        public Look(float yaw, float pitch)
        {
            // TODO: map yaw from 180 to -180.
            System.Diagnostics.Debug.Assert(pitch >= MinPitch);
            System.Diagnostics.Debug.Assert(pitch <= MaxPitch);

            YAW = yaw;
            PITCH = pitch;
        }

        internal readonly (byte, byte) ConvertToProtocolFormat()
        {
            System.Diagnostics.Debug.Assert(PITCH >= MinPitch);
            System.Diagnostics.Debug.Assert(PITCH <= MaxPitch);

            float x = Frem(YAW);
            float y = Frem(PITCH);

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
            return (YAW == other.YAW) && (PITCH == other.PITCH);
        }

    }
}

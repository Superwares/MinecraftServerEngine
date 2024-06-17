
namespace MinecraftServerEngine
{
    public readonly struct Look : System.IEquatable<Look>
    {
        internal const double MaxYaw = 180, MinYaw = -180;
        internal const double MaxPitch = 90, MinPitch = -90;

        private readonly double _YAW, _PITCH;

        private static double Frem(double angle)
        {
            double y = 360.0f;
            return angle - (y * (double)System.Math.Floor(angle / y));
        }

        public Look(double yaw, double pitch)
        {
            // TODO: map yaw from 180 to -180.
            System.Diagnostics.Debug.Assert(pitch >= MinPitch);
            System.Diagnostics.Debug.Assert(pitch <= MaxPitch);

            _YAW = yaw;
            _PITCH = pitch;
        }

        internal readonly (byte, byte) ConvertToProtocolFormat()
        {
            System.Diagnostics.Debug.Assert(_PITCH >= MinPitch);
            System.Diagnostics.Debug.Assert(_PITCH <= MaxPitch);

            double x = Frem(_YAW);
            double y = Frem(_PITCH);

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
            return (_YAW == other._YAW) && (_PITCH == other._PITCH);
        }

    }
}

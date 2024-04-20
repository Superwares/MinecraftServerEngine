using System;
using System.Diagnostics;

namespace Protocol
{
    public abstract class Control
    {
    }

    public class StandingControl : Control
    {
        public readonly bool OnGround;

        internal StandingControl(bool onGround)
        {
            OnGround = onGround;
        }

    }

    public class TransformationControl : Control
    {
        public readonly Player.Vector Pos;
        public readonly Player.Angles Look;
        public readonly bool OnGround;

        internal TransformationControl(
            double x, double y, double z,
            float yaw, float pitch, 
            bool onGround)
        {
            if (pitch < Player.Angles.MinPitch ||
                pitch > Player.Angles.MaxPitch)
                throw new UnexpectedValueException($"Entity.Pitch {pitch}");

            Pos = new(x, y, z);
            Look = new(yaw, pitch);
            OnGround = onGround;
        }
    }

    public class MovementControl : Control
    {
        public readonly Player.Vector Pos;
        public readonly bool OnGround;

        internal MovementControl(double x, double y, double z, bool onGround)
        {
            Pos = new(x, y, z);
            OnGround = onGround;
        }

    }

    public class RotationControl : Control
    {
        public readonly Player.Angles Look;
        public readonly bool OnGround;

        internal RotationControl(float yaw, float pitch, bool onGround)
        {
            if (pitch < Player.Angles.MinPitch ||
                pitch > Player.Angles.MaxPitch)
                throw new UnexpectedValueException($"Entity.Pitch {pitch}");

            Look = new(yaw, pitch);
            OnGround = onGround;
        }

    }

    public class SneakingControl : Control
    {
        internal SneakingControl() { }
    }

    public class UnsneakingControl : Control
    {
        internal UnsneakingControl() { }
    }

    public class SprintingControl : Control
    {
        internal SprintingControl() { }
    }

    public class UnsprintingControl : Control
    {
        internal UnsprintingControl() { }
    }

}

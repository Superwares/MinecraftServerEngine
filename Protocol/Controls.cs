using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Protocol
{
    public abstract class Control
    {
    }

    public class PlayerOnGroundControl : Control
    {
        public readonly bool OnGround;

        public PlayerOnGroundControl(bool onGround)
        {
            OnGround = onGround;
        }

    }

    public class PlayerPositionControl : Control
    {
        public readonly Entity.Vector Pos;

        public PlayerPositionControl(double x, double y, double z)
        {
            Pos = new(x, y, z);
        }

    }

    public class PlayerLookControl : Control
    {
        public readonly Entity.Look Look;

        public PlayerLookControl(float yaw, float pitch)
        {
            if (yaw < Entity.Look.MinYaw ||
                yaw > Entity.Look.MaxYaw)
                throw new UnexpectedValueException("Entity.Yaw");
            if (pitch < Entity.Look.MinPitch ||
                pitch > Entity.Look.MaxPitch)
                throw new UnexpectedValueException("Entity.Pitch");


            Look = new(yaw, pitch);
        }

    }
}

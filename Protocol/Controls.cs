using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Protocol
{
    public abstract class Control
    {
    }

    public class ClientSettingsControl : Control
    {
        public readonly byte RenderDistance;

        internal ClientSettingsControl(byte renderDistance)
        {
            RenderDistance = renderDistance;
        }

    }

    public class PlayerOnGroundControl : Control
    {
        public readonly bool OnGround;

        public PlayerOnGroundControl(bool onGround)
        {
            OnGround = onGround;
        }

    }

    public class PlayerMovementControl : Control
    {
        public readonly Entity.Position Pos;

        public PlayerMovementControl(double x, double y, double z)
        {
            Pos = new(x, y, z);
        }

    }

    public class PlayerLookControl : Control
    {
        public readonly Entity.Look Look;

        public PlayerLookControl(float yaw, float pitch)
        {
            Look = new(yaw, pitch);
        }

    }
}

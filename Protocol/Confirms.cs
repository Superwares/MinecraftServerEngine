using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Protocol
{
    /*public abstract class Confirm
    {
    }

    internal class TeleportConfirm : Confirm
    {
        public readonly int Payload;

        internal TeleportConfirm(int payload)
        {
            Payload = payload;
        }

    }

    internal class ClientSettingsConfirm : Confirm
    {
        public readonly Connection.ClientsideSettings Settings;

        internal ClientSettingsConfirm(
            Connection.ClientsideSettings settings)
        {
            if (settings.renderDistance < Connection.ClientsideSettings.MinRenderDistance ||
                settings.renderDistance > Connection.ClientsideSettings.MaxRenderDistance)
                throw new UnexpectedValueException("RenderDistance");

            Settings = settings;
        }
    }

    internal class KeepaliveConfirm : Confirm
    {
        public readonly long Payload;

        internal KeepaliveConfirm(long payload)
        {
            Payload = payload;
        }

    }*/

}

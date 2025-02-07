using MinecraftServerEngine;

namespace TestMinecraftServerApplication.Configs
{

    internal class ConfigWorld 
    {
        internal readonly double CenterX;
        internal readonly double CenterZ;
        internal readonly double DefaultWorldBorderRadiusInMeters;

        internal readonly double RespawningX;
        internal readonly double RespawningY;
        internal readonly double RespawningZ;
        internal readonly double RespawningYaw;
        internal readonly double RespawningPitch;

        internal ConfigWorld(System.Xml.XmlNode node)
        {
            foreach (System.Xml.XmlNode _node in node.ChildNodes)
            {
                if (_node.NodeType != System.Xml.XmlNodeType.Element)
                {
                    continue;
                }

                switch (_node.Name)
                {
                    case nameof(CenterX):
                        if (double.TryParse(_node.InnerText, out CenterX) == false)
                        {
                            throw new System.InvalidOperationException($"Invalid double value for \"{nameof(CenterX)}\"");
                        }
                        break;
                    case nameof(CenterZ):
                        if (double.TryParse(_node.InnerText, out CenterZ) == false)
                        {
                            throw new System.InvalidOperationException($"Invalid double value for \"{nameof(CenterZ)}\"");
                        }
                        break;
                    case nameof(DefaultWorldBorderRadiusInMeters):
                        if (double.TryParse(_node.InnerText, out DefaultWorldBorderRadiusInMeters) == false)
                        {
                            throw new System.InvalidOperationException($"Invalid double value for \"{nameof(DefaultWorldBorderRadiusInMeters)}\"");
                        }
                        break;

                    case nameof(RespawningX):
                        if (double.TryParse(_node.InnerText, out RespawningX) == false)
                        {
                            throw new System.InvalidOperationException($"Invalid double value for \"{nameof(RespawningX)}\"");
                        }
                        break;
                    case nameof(RespawningY):
                        if (double.TryParse(_node.InnerText, out RespawningY) == false)
                        {
                            throw new System.InvalidOperationException($"Invalid double value for \"{nameof(RespawningY)}\"");
                        }
                        break;
                    case nameof(RespawningZ):
                        if (double.TryParse(_node.InnerText, out RespawningZ) == false)
                        {
                            throw new System.InvalidOperationException($"Invalid double value for \"{nameof(RespawningZ)}\"");
                        }
                        break;
                    case nameof(RespawningYaw):
                        if (double.TryParse(_node.InnerText, out RespawningYaw) == false)
                        {
                            throw new System.InvalidOperationException($"Invalid double value for \"{nameof(RespawningYaw)}\"");
                        }
                        break;
                    case nameof(RespawningPitch):
                        if (double.TryParse(_node.InnerText, out RespawningPitch) == false)
                        {
                            throw new System.InvalidOperationException($"Invalid double value for \"{nameof(RespawningPitch)}\"");
                        }
                        break;
                }
            }

        }
    }

}

using MinecraftServerEngine;

namespace TestMinecraftServerApplication.Configs
{

    internal class ConfigServer 
    {
        internal readonly ushort Port;

        internal ConfigServer(System.Xml.XmlNode node)
        {
            foreach (System.Xml.XmlNode _node in node.ChildNodes)
            {
                if (_node.NodeType != System.Xml.XmlNodeType.Element)
                {
                    continue;
                }

                switch (_node.Name)
                {
                    case nameof(Port):
                        if (ushort.TryParse(_node.InnerText, out Port) == false)
                        {
                            throw new System.InvalidOperationException($"Invalid ushort value for \"{nameof(Port)}\"");
                        }
                        break;
                }
            }

        }
    }

}

namespace TestMinecraftServerApplication.Configs
{



    internal class Config
    {
        internal readonly ConfigServer Server;
        internal readonly ConfigWorld World;
        internal readonly ConfigGame Game;


        internal Config(System.Xml.XmlNode node)
        {
            foreach (System.Xml.XmlNode _node in node.ChildNodes)
            {
                if (_node.NodeType != System.Xml.XmlNodeType.Element)
                {
                    continue;
                }

                switch (_node.Name)
                {
                    case nameof(Server):
                        Server = new ConfigServer(_node);
                        break;
                    case nameof(World):
                        World = new ConfigWorld(_node);
                        break;
                    case nameof(Game):
                        Game = new ConfigGame(_node);
                        break;
                }
            }

            if (Server == null)
            {
                throw new System.InvalidOperationException($"\"{nameof(Server)}\" element not found");
            }

            if (World == null)
            {
                throw new System.InvalidOperationException($"\"{nameof(World)}\" element not found");
            }

            if (Game == null)
            {
                throw new System.InvalidOperationException($"\"{nameof(Game)}\" element not found");
            }
        }
        
    }
}

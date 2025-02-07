using MinecraftServerEngine;

namespace TestMinecraftServerApplication.Configs
{

    internal class ConfigGame
    {
        internal readonly ConfigGameScoreboard Scoreboard;

        internal readonly int KillCoins;
        internal readonly int InitCoins;

        internal readonly ConfigGameRound Round;

        internal ConfigGame(System.Xml.XmlNode node)
        {
            foreach (System.Xml.XmlNode _node in node.ChildNodes)
            {
                if (_node.NodeType != System.Xml.XmlNodeType.Element)
                {
                    continue;
                }

                switch (_node.Name)
                {
                    case nameof(Scoreboard):
                        Scoreboard = new ConfigGameScoreboard(_node);
                        break;
                    case nameof(KillCoins):
                        if (int.TryParse(_node.InnerText, out KillCoins) == false)
                        {
                            throw new System.InvalidOperationException($"Invalid integer value for \"{nameof(KillCoins)}\"");
                        }
                        break;
                    case nameof(InitCoins):
                        if (int.TryParse(_node.InnerText, out InitCoins) == false)
                        {
                            throw new System.InvalidOperationException($"Invalid integer value for \"{nameof(InitCoins)}\"");
                        }
                        break;
                    case nameof(Round):
                        Round = new ConfigGameRound(_node);
                        break;
                }
            }

            if (Scoreboard == null)
            {
                throw new System.InvalidOperationException($"\"{nameof(Scoreboard)}\" element not found");
            }

            if (Round == null)
            {
                throw new System.InvalidOperationException($"\"{nameof(Round)}\" element not found");
            }
        }
    }

}

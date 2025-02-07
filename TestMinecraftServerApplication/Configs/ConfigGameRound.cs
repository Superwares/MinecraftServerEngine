namespace TestMinecraftServerApplication.Configs
{

    internal sealed class ConfigGameRound
    {

        internal readonly int NormalTimeInSeconds;
        internal readonly int BurningTimeInSeconds;

        internal readonly int SurvivingCoins;
        
        internal readonly int SeekerWinAdditionalPoints;
        internal readonly int SeekerWinCoins;
        internal readonly int HiderWinAdditionalPoints;
        internal readonly int HiderWinCoins;

        internal ConfigGameRound(System.Xml.XmlNode node)
        {
            foreach (System.Xml.XmlNode _node in node.ChildNodes)
            {
                if (_node.NodeType != System.Xml.XmlNodeType.Element)
                {
                    continue;
                }

                switch (_node.Name)
                {
                    case nameof(NormalTimeInSeconds):
                        if (int.TryParse(_node.InnerText, out NormalTimeInSeconds) == false)
                        {
                            throw new System.InvalidOperationException($"Invalid integer value for \"{nameof(NormalTimeInSeconds)}\"");
                        }
                        break;
                    case nameof(BurningTimeInSeconds):
                        if (int.TryParse(_node.InnerText, out BurningTimeInSeconds) == false)
                        {
                            throw new System.InvalidOperationException($"Invalid integer value for \"{nameof(BurningTimeInSeconds)}\"");
                        }
                        break;
                    case nameof(SurvivingCoins):
                        if (int.TryParse(_node.InnerText, out SurvivingCoins) == false)
                        {
                            throw new System.InvalidOperationException($"Invalid integer value for \"{nameof(SurvivingCoins)}\"");
                        }
                        break;
                    case nameof(SeekerWinAdditionalPoints):
                        if (int.TryParse(_node.InnerText, out SeekerWinAdditionalPoints) == false)
                        {
                            throw new System.InvalidOperationException($"Invalid integer value for \"{nameof(SeekerWinAdditionalPoints)}\"");
                        }
                        break;
                    case nameof(SeekerWinCoins):
                        if (int.TryParse(_node.InnerText, out SeekerWinCoins) == false)
                        {
                            throw new System.InvalidOperationException($"Invalid integer value for \"{nameof(SeekerWinCoins)}\"");
                        }
                        break;
                    case nameof(HiderWinAdditionalPoints):
                        if (int.TryParse(_node.InnerText, out HiderWinAdditionalPoints) == false)
                        {
                            throw new System.InvalidOperationException($"Invalid integer value for \"{nameof(HiderWinAdditionalPoints)}\"");
                        }
                        break;
                    case nameof(HiderWinCoins):
                        if (int.TryParse(_node.InnerText, out HiderWinCoins) == false)
                        {
                            throw new System.InvalidOperationException($"Invalid integer value for \"{nameof(HiderWinCoins)}\"");
                        }
                        break;
                }
            }
        }

    }

}

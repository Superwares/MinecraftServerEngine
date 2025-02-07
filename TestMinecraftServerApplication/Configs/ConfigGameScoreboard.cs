namespace TestMinecraftServerApplication.Configs
{

    internal sealed class ConfigGameScoreboard 
    {
        internal readonly int PointsPerKill;
        internal readonly int PointsPerDeath;
        internal readonly int PoinsPerSurviving;
        internal readonly int DefaultAdditionalPoints;

        internal ConfigGameScoreboard(System.Xml.XmlNode node)
        {
            foreach (System.Xml.XmlNode _node in node.ChildNodes)
            {
                if (_node.NodeType != System.Xml.XmlNodeType.Element)
                {
                    continue;
                }

                switch (_node.Name)
                {
                    case nameof(PointsPerKill):
                        if (int.TryParse(_node.InnerText, out PointsPerKill) == false)
                        {
                            throw new System.InvalidOperationException($"Invalid integer value for \"{nameof(PointsPerKill)}\"");
                        }
                        break;
                    case nameof(PointsPerDeath):
                        if (int.TryParse(_node.InnerText, out PointsPerDeath) == false)
                        {
                            throw new System.InvalidOperationException($"Invalid integer value for \"{nameof(PointsPerDeath)}\"");
                        }
                        break;
                    case nameof(PoinsPerSurviving):
                        if (int.TryParse(_node.InnerText, out PoinsPerSurviving) == false)
                        {
                            throw new System.InvalidOperationException($"Invalid integer value for \"{nameof(PoinsPerSurviving)}\"");
                        }
                        break;
                    case nameof(DefaultAdditionalPoints):
                        if (int.TryParse(_node.InnerText, out DefaultAdditionalPoints) == false)
                        {
                            throw new System.InvalidOperationException($"Invalid integer value for \"{nameof(DefaultAdditionalPoints)}\"");
                        }
                        break;
                }
            }
        }
    }

}

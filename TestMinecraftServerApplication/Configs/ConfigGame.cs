namespace TestMinecraftServerApplication.Configs
{
    public interface IConfigGame
    {
        public IConfigGameScoreboard Scoreboard { get; }


        public int KillCoins { get; }
        public int InitCoins { get; }


        public IConfigGameRound Round { get; }

    }

    public class ConfigGame : IConfigGame
    {
        [System.Xml.Serialization.XmlElement("Scoreboard")]
        public ConfigGameScoreboard Scoreboard { get; set; }
        IConfigGameScoreboard IConfigGame.Scoreboard => Scoreboard;


        [System.Xml.Serialization.XmlElement("KillCoins")]
        public int KillCoins { get; set; }

        [System.Xml.Serialization.XmlElement("InitCoins")]
        public int InitCoins { get; set; }


        [System.Xml.Serialization.XmlElement("Round")]
        public ConfigGameRound Round { get; set; }
        IConfigGameRound IConfigGame.Round => Round;
    }

}



namespace TestMinecraftServerApplication
{
    public interface IConfigGame
    {
        public IConfigGameScroeboard Scroeboard { get; }

        public int KillCoins { get; }
        public int DefaultCoins { get; }

        public int SurvivingRoundCoins { get; }

        public int RoundSeekerWinAdditionalPoints { get; }
        public int RoundSeekerWinCoins { get; }
        public int RoundHiderWinAdditionalPoints { get; }
        public int RoundHiderWinCoins { get; }
    }

    public class ConfigGame : IConfigGame
    {
        [System.Xml.Serialization.XmlElement("Scroeboard ")]
        public ConfigGameScroeboard Scroeboard { get; set; }
        IConfigGameScroeboard IConfigGame.Scroeboard => Scroeboard;


        [System.Xml.Serialization.XmlElement("KillCoins ")]
        public int KillCoins { get; set; }

        [System.Xml.Serialization.XmlElement("DefaultCoins ")]
        public int DefaultCoins { get; set; }


        [System.Xml.Serialization.XmlElement("SurvivingRoundCoins ")]
        public int SurvivingRoundCoins { get; set; }


        [System.Xml.Serialization.XmlElement("RoundSeekerWinAdditionalPoints ")]
        public int RoundSeekerWinAdditionalPoints { get; set; }

        [System.Xml.Serialization.XmlElement("RoundSeekerWinCoins ")]
        public int RoundSeekerWinCoins { get; set; }

        [System.Xml.Serialization.XmlElement("RoundHiderWinAdditionalPoints ")]
        public int RoundHiderWinAdditionalPoints { get; set; }

        [System.Xml.Serialization.XmlElement("RoundHiderWinCoins ")]
        public int RoundHiderWinCoins { get; set; }
    }

}

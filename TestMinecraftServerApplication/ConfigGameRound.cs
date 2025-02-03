

namespace TestMinecraftServerApplication
{
    public interface IConfigGameRound
    {
        public int SurvivingCoins { get; }

        public int SeekerWinAdditionalPoints { get; }
        public int SeekerWinCoins { get; }
        public int HiderWinAdditionalPoints { get; }
        public int HiderWinCoins { get; }
    }

    public class ConfigGameRound : IConfigGameRound
    {


        [System.Xml.Serialization.XmlElement("SurvivingCoins")]
        public int SurvivingCoins { get; set; }


        [System.Xml.Serialization.XmlElement("SeekerWinAdditionalPoints")]
        public int SeekerWinAdditionalPoints { get; set; }

        [System.Xml.Serialization.XmlElement("SeekerWinCoins")]
        public int SeekerWinCoins { get; set; }

        [System.Xml.Serialization.XmlElement("HiderWinAdditionalPoints")]
        public int HiderWinAdditionalPoints { get; set; }

        [System.Xml.Serialization.XmlElement("HiderWinCoins")]
        public int HiderWinCoins { get; set; }
    }

}

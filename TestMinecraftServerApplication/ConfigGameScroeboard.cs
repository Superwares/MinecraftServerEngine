

namespace TestMinecraftServerApplication
{
    public interface IConfigGameScroeboard
    {
        public int PointsPerKill { get; }
        public int PointsPerDeath { get; }
        public int PoinsPerSurviving { get; }
        public int DefaultAdditionalPoints { get; }
    }

    public class ConfigGameScroeboard : IConfigGameScroeboard
    {
        [System.Xml.Serialization.XmlElement("PointsPerKill")]
        public int PointsPerKill { get; set; }

        [System.Xml.Serialization.XmlElement("PointsPerDeath")]
        public int PointsPerDeath { get; set; }

        [System.Xml.Serialization.XmlElement("PoinsPerSurviving")]
        public int PoinsPerSurviving { get; set; }

        [System.Xml.Serialization.XmlElement("DefaultAdditionalPoints")]
        public int DefaultAdditionalPoints { get; set; }


    }

}

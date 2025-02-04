namespace TestMinecraftServerApplication.Configs
{
    public interface IConfigWorld
    {
        public double CenterX { get; }
        public double CenterZ { get; }
        public double DefaultWorldBorderRadiusInMeters { get; }

        public double RespawningX { get; }
        public double RespawningY { get; }
        public double RespawningZ { get; }
        public double RespawningYaw { get; }
        public double RespawningPitch { get; }
    }

    public class ConfigWorld : IConfigWorld
    {
        [System.Xml.Serialization.XmlElement("CenterX")]
        public double CenterX { get; set; }

        [System.Xml.Serialization.XmlElement("CenterZ")]
        public double CenterZ { get; set; }

        [System.Xml.Serialization.XmlElement("DefaultWorldBorderRadiusInMeters")]
        public double DefaultWorldBorderRadiusInMeters { get; set; }



        [System.Xml.Serialization.XmlElement("RespawningX")]
        public double RespawningX { get; set; }

        [System.Xml.Serialization.XmlElement("RespawningY")]
        public double RespawningY { get; set; }

        [System.Xml.Serialization.XmlElement("RespawningZ")]
        public double RespawningZ { get; set; }
        [System.Xml.Serialization.XmlElement("RespawningYaw")]
        public double RespawningYaw { get; set; }
        [System.Xml.Serialization.XmlElement("RespawningPitch")]
        public double RespawningPitch { get; set; }


    }

}



using Common;
using MinecraftServerEngine;

namespace TestMinecraftServerApplication
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

    public interface IConfig
    {
        public IConfigWorld World { get; }
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

    [System.Xml.Serialization.XmlRoot("Config")]
    public class Config : IConfig
    {
        [System.Xml.Serialization.XmlElement("World")]
        public ConfigWorld World { get; set; }
        IConfigWorld IConfig.World => World;




        private static Config _instance = null;
        public static IConfig Instance
        {
            get
            {
                if (_instance == null)
                {
                    throw new System.InvalidOperationException("Configuration has not been deserialized.");
                }
                return _instance;
            }
        }

        public static void Deserialize(string filename)
        {
            Config config = null;

            try
            {
                using System.IO.StreamReader reader = new(filename, System.Text.Encoding.UTF8);

                config = new System.Xml.Serialization.XmlSerializer(typeof(Config)).Deserialize(reader) as Config;
            }
            //catch (System.InvalidOperationException e) { }
            finally
            {

            }

            _instance = config;
        }
    }
}

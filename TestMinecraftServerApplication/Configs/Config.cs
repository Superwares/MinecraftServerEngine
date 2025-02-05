namespace TestMinecraftServerApplication.Configs
{

    public interface IConfig
    {
        public IConfigWorld World { get; }

        public IConfigGame Game { get; }
    }


    [System.Xml.Serialization.XmlRoot("Config")]
    public class Config : IConfig
    {
        [System.Xml.Serialization.XmlElement("World")]
        public ConfigWorld World { get; set; }
        IConfigWorld IConfig.World => World;


        [System.Xml.Serialization.XmlElement("Game")]
        public ConfigGame Game { get; set; }
        IConfigGame IConfig.Game => Game;

        
    }
}

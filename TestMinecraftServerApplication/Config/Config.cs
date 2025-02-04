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

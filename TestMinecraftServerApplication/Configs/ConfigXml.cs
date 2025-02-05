

namespace TestMinecraftServerApplication.Configs
{
    internal class ConfigXml
    {
        private static Config _config = null;
        public static IConfig Config
        {
            get
            {
                if (_config == null)
                {
                    throw new System.InvalidOperationException("Configuration has not been deserialized.");
                }
                return _config;
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

            _config = config;
        }
    }
}

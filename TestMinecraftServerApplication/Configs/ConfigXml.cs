

namespace TestMinecraftServerApplication.Configs
{
    internal static class ConfigXml
    {
        private static Config _config = null;

        public static void Deserialize(string filename)
        {
            Config config = null;

            try
            {
                using System.IO.StreamReader reader = new(filename, System.Text.Encoding.UTF8);

                System.Xml.XmlDocument doc = new System.Xml.XmlDocument();
                string xmlContent = reader.ReadToEnd();
                doc.LoadXml(xmlContent);

                //config = new System.Xml.Serialization.XmlSerializer(typeof(Config)).Deserialize(reader) as Config;

                foreach (System.Xml.XmlNode node in doc.ChildNodes)
                {
                    if (node.NodeType != System.Xml.XmlNodeType.Element)
                    {
                        continue;
                    }

                    switch (node.Name)
                    {
                        case nameof(Config):
                            config = new Config(node);
                            break;
                    }
                }

                if (config == null)
                {
                    throw new System.InvalidOperationException($"\"{nameof(Config)}\" element not found");
                }
            }
            catch(System.InvalidOperationException e)
            {
                throw new System.InvalidOperationException(
                        $"Failed to deserialize the configuration file: {e.Message}"
                        );
            }

            _config = config;
        }

        internal static Config GetConfig()
        {
            System.Diagnostics.Debug.Assert(_config != null);

            return _config;
        }
    }
}

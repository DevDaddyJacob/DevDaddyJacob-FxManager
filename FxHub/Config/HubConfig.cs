using DevDaddyJacob.FxHub.Config.Models;
using Newtonsoft.Json;

namespace DevDaddyJacob.FxHub.Config
{
    internal class HubConfig
    {
        #region Fields

        private string _configFilePath;
        private ConfigFile _config;

        #endregion Fields

        #region JSON File Properties

        public string Label
        {
            get => _config.HubLabel;
            set => _config.HubLabel = value;
        }

        public string HubPrivateKeyPath
        {
            get => _config.HubPrivateKeyPath;
        }

        public SocketConfig Socket
        {
            get => _config.Socket;
            set => _config.Socket = value;
        }

        public Dictionary<string, NodeConfig> Nodes
        {
            get => _config.Nodes;
            set => _config.Nodes = value;
        }

        #endregion JSON File Properties

        public HubConfig(string configPath)
        {
            // Check if the config file exists
            _configFilePath = configPath;
            if (!File.Exists(_configFilePath)) { throw new FileNotFoundException(_configFilePath); }

            _config = LoadConfigFromFile(_configFilePath);
        }

        private ConfigFile LoadConfigFromFile(string configFilePath)
        {
            using (StreamReader reader = new StreamReader(configFilePath))
            {
                string fileContent = reader.ReadToEnd();
                return JsonConvert.DeserializeObject<ConfigFile>(fileContent);
            }
        }

        public void SaveToFile()
        {
            using (StreamWriter writer = new StreamWriter(_configFilePath))
            {
                string jsonConfig = JsonConvert.SerializeObject(_config, Formatting.Indented);
                writer.WriteLine(jsonConfig);
            }
        }
    }
}

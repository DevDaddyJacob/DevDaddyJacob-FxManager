using DevDaddyJacob.FxManager.Node.Config.Models;
using Newtonsoft.Json;

namespace DevDaddyJacob.FxManager.Node.Config
{
    internal class NodeConfig
    {
        #region Fields

        private string _configFilePath;
        private ConfigFile _config;

        #endregion Fields

        #region JSON File Properties

        public string Label
        {
            get => _config.NodeLabel;
            set => _config.NodeLabel = value;
        }

        public string NodePrivateKeyPath
        {
            get => _config.NodePrivateKeyPath;
        }

        public FivemRunnerConfig FivemRunner
        {
            get => _config.FivemRunner;
            set => _config.FivemRunner = value;
        }

        public FxHubConfig Hub
        {
            get => _config.HubConfig;
            set => _config.HubConfig = value;
        }

        #endregion JSON File Properties

        public NodeConfig(string profilePath)
        {
            // Check if the config file exists
            _configFilePath = Path.Combine(profilePath, "config.json");
            if (!File.Exists(_configFilePath)) { throw new FileNotFoundException(_configFilePath); }

            _config = LoadConfigFromFile(_configFilePath);
        }

        private ConfigFile LoadConfigFromFile(string configFilePath)
        {
            using (StreamReader reader = new StreamReader(configFilePath))
            {
                string fileContent = reader.ReadToEnd();
#pragma warning disable CS8603 // Possible null reference return.
                return JsonConvert.DeserializeObject<ConfigFile>(fileContent);
#pragma warning restore CS8603 // Possible null reference return.
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

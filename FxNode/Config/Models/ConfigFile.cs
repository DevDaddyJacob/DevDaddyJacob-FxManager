using Newtonsoft.Json;

namespace DevDaddyJacob.FxManager.Node.Config.Models
{
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    internal class ConfigFile
    {
        [JsonProperty("nodeLabel", Order = 1)]
        public required string NodeLabel { get; set; }

        [JsonProperty("nodePrivateKeyPath", Order = 2)]
        public required string NodePrivateKeyPath { get; set; }

        [JsonProperty("fxHost", Order = 3)]
        public required FxHubConfig HubConfig { get; set; }

        [JsonProperty("fivemRunner", Order = 4)]
        public required FivemRunnerConfig FivemRunner { get; set; }
    }
}

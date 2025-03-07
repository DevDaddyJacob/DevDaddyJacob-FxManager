using Newtonsoft.Json;

namespace DevDaddyJacob.FxManager.Hub.Config.Models
{
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    internal class ConfigFile
    {
        [JsonProperty("hubLabel", Order = 1)]
        public required string HubLabel { get; set; }

        [JsonProperty("hubPrivateKeyPath", Order = 2)]
        public required string HubPrivateKeyPath { get; set; }

        [JsonProperty("socket", Order = 3)]
        public required SocketConfig Socket { get; set; }

        [JsonProperty("nodes", Order = 4)]
        public required Dictionary<string, NodeConfig> Nodes { get; set; }
    }
}

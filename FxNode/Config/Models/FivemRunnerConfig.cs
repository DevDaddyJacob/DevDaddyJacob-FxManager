using Newtonsoft.Json;

namespace DevDaddyJacob.FxNode.Config.Models
{
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    internal class FivemRunnerConfig
    {
        [JsonProperty("artifactsPath", Order = 1)]
        public required string ArtifactsPath { get; set; }

        [JsonProperty("serverDataPath", Order = 3)]
        public required string ServerDataPath { get; set; }

        [JsonProperty("serverCfgPath", Order = 2)]
        public required string ServerCfgPath { get; set; }
    }
}

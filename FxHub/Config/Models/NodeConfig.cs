using Newtonsoft.Json;

namespace DevDaddyJacob.FxHub.Config.Models
{
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    internal class NodeConfig
    {
        [JsonProperty("nodeIpAddress", Order = 1)]
        public required string IpAddress { get; set; }

        [JsonProperty("nodePublicKey", Order = 2)]
        public required string PublicKey { get; set; }
    }
}

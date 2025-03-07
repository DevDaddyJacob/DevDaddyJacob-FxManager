using Newtonsoft.Json;

namespace DevDaddyJacob.FxManager.Node.Config.Models
{
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    internal class FxHubConfig
    {
        [JsonProperty("socketAddress", Order = 1)]
        public required string SocketAddress { get; set; }

        [JsonProperty("socketPort", Order = 2)]
        public required int SocketPort { get; set; }

        [JsonProperty("hostPublicKey", Order = 3)]
        public required string HostPublicKey { get; set; }
    }
}

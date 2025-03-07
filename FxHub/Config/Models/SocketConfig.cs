using Newtonsoft.Json;

namespace DevDaddyJacob.FxManager.Hub.Config.Models
{
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    internal class SocketConfig
    {
        [JsonProperty("socketPort", Order = 1)]
        public required int SocketPort { get; set; }

        [JsonProperty("maxTimeoutMs", Order = 2)]
        public required int MaxTimeoutMs { get; set; }
    }
}

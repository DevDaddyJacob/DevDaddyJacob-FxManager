using MessagePack;

namespace DevDaddyJacob.FxSocket.Payloads
{
    public delegate void OnNodeAttachRequest(string nodeLabel);

    [MessagePackObject]
    public class NodeAttachRequest : ISocketPayload
    {
        [Key(0)]
        public required string NodeLabel { get; set; }
    }
}

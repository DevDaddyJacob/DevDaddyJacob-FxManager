using MessagePack;

namespace DevDaddyJacob.FxManager.Socket.Payloads.General
{
    public delegate void OnNodeAttach(string nodeLabel);

    [MessagePackObject]
    public class NodeAttachFrame : SocketPayload
    {
        [Key(1)]
        public override string Event { get; set; } = "NodeAttachFrame";

        [Key(2)]
        public required string NodeLabel { get; set; }
    }
}

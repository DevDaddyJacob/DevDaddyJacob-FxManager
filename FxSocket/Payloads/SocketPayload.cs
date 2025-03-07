using DevDaddyJacob.FxManager.Socket.Payloads.General;
using MessagePack;

namespace DevDaddyJacob.FxManager.Socket.Payloads
{
    [Union(0, typeof(NodeAttachFrame))]
    [Union(1, typeof(HeartbeatFrame))]
    [MessagePackObject]
    public abstract class SocketPayload
    {
        [Key(0)]
        public DateTime Timestamp { get; set; } = DateTime.Now;

        [Key(1)]
        public virtual string Event { get; set; } = "Unknown";
    }
}
using MessagePack;

namespace DevDaddyJacob.FxSocket.Payloads.Frames
{
    [Union(0, typeof(HeartbeatFrame))]
    [MessagePackObject]
    public abstract class SocketFrame
    {
        [Key(0)]
        public DateTime Timestamp { get; set; } = DateTime.Now;

        [Key(1)]
        public virtual string Event { get; set; } = "Unknown";
    }
}
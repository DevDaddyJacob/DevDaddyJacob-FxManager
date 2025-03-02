using MessagePack;

namespace DevDaddyJacob.FxSocket.Payloads.Frames
{
    public delegate void OnHeartbeatFrame();

    [MessagePackObject]
    public class HeartbeatFrame : SocketFrame
    {
        [Key(1)]
        public override string Event { get; set; } = "HeartbeatFrame";
    }
}
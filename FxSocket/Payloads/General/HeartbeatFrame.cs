using MessagePack;

namespace DevDaddyJacob.FxManager.Socket.Payloads.General
{
    public delegate void OnHeartbeatFrame();

    [MessagePackObject]
    public class HeartbeatFrame : SocketPayload
    {
        [Key(1)]
        public override string Event { get; set; } = "HeartbeatFrame";
    }
}
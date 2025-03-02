using MessagePack;

namespace DevDaddyJacob.FxSocket.Payloads
{
    [Union(0, typeof(NodeAttachRequest))]
    [Union(1, typeof(AuthChallenge))]
    public interface ISocketPayload
    {
    }
}
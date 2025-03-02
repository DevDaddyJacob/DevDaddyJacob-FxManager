using MessagePack;

namespace DevDaddyJacob.FxSocket.Payloads
{
    public delegate void OnAuthChallenge(bool isSolving, string challenge);

    [MessagePackObject]
    public class AuthChallenge : ISocketPayload
    {
        [Key(0)]
        public required bool IsSolving { get; set; }

        [Key(1)]
        public required string Challenge { get; set; }
    }
}

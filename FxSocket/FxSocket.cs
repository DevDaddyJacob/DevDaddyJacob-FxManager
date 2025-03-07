using DevDaddyJacob.FxManager.Socket.Payloads;
using MessagePack;
using System.Net.WebSockets;
using System.Text;

namespace DevDaddyJacob.FxManager.Socket
{
    internal abstract class FxSocket
    {
        #region Public Methods

        public abstract void Start();

        #endregion Public Methods

        #region Protected Methods
        
        protected async Task<Tuple<SocketMessageType, byte[]>?> NextMessage(WebSocket socket)
        {
            if (socket == null || socket.State == WebSocketState.Closed) { return null; }

            byte[] buffer = new byte[1024];
            while (socket.State == WebSocketState.Open)
            {
                try
                {
                    WebSocketReceiveResult result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);
                        return null;
                    }

                    byte[] data = new byte[result.Count];
                    for (int i = 0; i < result.Count; i++)
                    {
                        data[i] = buffer[i];
                    }

                    return new((SocketMessageType)result.MessageType, data);
                }
                catch
                {
                    return null;
                }
            }

            return null;
        }

        protected byte[] PackMessage<T>(T message)
        {
            return MessagePackSerializer.Serialize<T>(message);
        }

        protected T UnpackMessage<T>(byte[] message)
        {
            return MessagePackSerializer.Deserialize<T>(message);
        }

        protected byte[] PackPayload(SocketPayload payload)
        {
            return PackMessage<SocketPayload>(payload);
        }

        protected SocketPayload UnpackPayload(byte[] payload)
        {
            return UnpackMessage<SocketPayload>(payload);
        }

        #endregion Protected Methods
    }
}

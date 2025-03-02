#if IS_FX_NODE

using DevDaddyJacob.FxNode;
using DevDaddyJacob.FxNode.Config;
using DevDaddyJacob.FxSocket.Payloads;
using DevDaddyJacob.FxSocket.Payloads.Frames;
using MessagePack;
using System;
using System.Net.WebSockets;
using System.Text;

namespace DevDaddyJacob.FxSocket.Node
{
    internal class NodeSocket
    {
        #region Fields

        private FxKeyAuth _keyAuth;
        private NodeConfig _config;
        private ClientWebSocket? _socketClient = null;

        #endregion Fields

        #region Constructors

        public NodeSocket(NodeConfig config)
        {
            _config = config;
            _keyAuth = new(config);
        }

        #endregion Constructors

        #region Public Methods

        // C->S     Connect
        // C->S     Send attach request
        // S->C     Parse request, validate, and send challenge
        // C->S     Decrypt challenge, re-encrypt it, and return it
        // S->C     Decrypt challenge and return results
        public async void ConnectToHub2()
        {
            // If there is already an existing socket open
            if (_socketClient != null && _socketClient.State == WebSocketState.Open) { return; }
            _socketClient = new ClientWebSocket();


            // Establish a connection
            Program.Logger.Info($"Establishing connection to hub socket");
            await _socketClient.ConnectAsync(new Uri($"ws://{_config.Hub.SocketAddress}:{_config.Hub.SocketPort}"), CancellationToken.None);
            Program.Logger.Info($"Starting auth handshake...");


            // Send the attach request
            try
            {
                Program.Logger.Debug($"Packing payload data");

                NodeAttachRequest payload = new() { NodeLabel = _config.Label };
                string serializedPayload = MessagePackSerializer.SerializeToJson(payload);
                if (serializedPayload.Length == 0) { throw new Exception("Failed to serialize and payload data"); }

                Program.Logger.Debug($"Sending payload data");
                await _socketClient.SendAsync(new(Encoding.UTF8.GetBytes(serializedPayload)), WebSocketMessageType.Text, true, CancellationToken.None);
            }
            catch (Exception ex)
            {
                Program.Logger.Info($"Auth handshake failed");
                Program.Logger.Error(ex);
                return;
            }


            // Await the server's challenge
            string challengeStr = string.Empty;
            try
            {
                Program.Logger.Debug($"Awaiting server challenge");

                byte[] _buffer = new byte[1024];
                WebSocketReceiveResult result = await _socketClient.ReceiveAsync(new ArraySegment<byte>(_buffer), CancellationToken.None);
                string resultMsg = Encoding.UTF8.GetString(_buffer, 0, result.Count);

                Program.Logger.Debug($"Unpacking server message: \"{resultMsg}\"");
                AuthChallenge serverChallenge = UnpackMessage<AuthChallenge>(resultMsg);

                Program.Logger.Debug($"Unpacked: challenge={serverChallenge.Challenge}");
                challengeStr = serverChallenge.Challenge;
            }
            catch (Exception ex)
            {
                Program.Logger.Info($"Auth handshake failed");
                Program.Logger.Error(ex);
                return;
            }


            // Send the solved challenge
            try
            {
                Program.Logger.Debug($"Packing payload data");

                AuthChallenge payload = new() { IsSolving = true, Challenge = challengeStr };
                string? serializedPayload = PackMessage(payload);
                if (serializedPayload == null) { throw new Exception("Failed to serialize and pack payload data"); }

                Program.Logger.Debug($"Sending auth solve payload data");
                await _socketClient.SendAsync(new(Encoding.UTF8.GetBytes(serializedPayload)), WebSocketMessageType.Text, true, CancellationToken.None);
            }
            catch (Exception ex)
            {
                Program.Logger.Info($"Auth handshake failed");
                Program.Logger.Error(ex);
                return;
            }


            // Await the server's approval
            try
            {
                Program.Logger.Debug($"Awaiting server challenge result");

                byte[] _buffer = new byte[1024];
                WebSocketReceiveResult result = await _socketClient.ReceiveAsync(new ArraySegment<byte>(_buffer), CancellationToken.None);
                string resultMsg = Encoding.UTF8.GetString(_buffer, 0, result.Count);

                Program.Logger.Debug($"Unpacking server message: \"{resultMsg}\"");
                AuthChallenge authChallenge = UnpackMessage<AuthChallenge>(resultMsg);

                if (!authChallenge.Challenge.Equals("Handshake Complete, Auth Ok!"))
                {
                    await _socketClient.CloseAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);
                    return;
                }
            }
            catch (Exception ex)
            {
                Program.Logger.Info($"Auth handshake failed");
                Program.Logger.Error(ex);
                return;
            }


            // Listen for messages
            Program.Logger.Info("Auth handshake with server complete, listening for messages");
            byte[] buffer = new byte[1024];
            while (_socketClient.State == WebSocketState.Open)
            {
                WebSocketReceiveResult result = await _socketClient.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                if (result.MessageType == WebSocketMessageType.Text)
                {
                    string message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    SocketFrame frame;
                    try
                    {
                        frame = UnpackMessage<SocketFrame>(message);
                    }
                    catch (Exception ex)
                    {
                        continue;
                    }

                    await ProcessFrame(frame);
                }
            }

            return;
        }

        public async void ConnectToHub()
        {
            // If there is already an existing socket open
            if (_socketClient != null && _socketClient.State == WebSocketState.Open) { return; }
            _socketClient = new ClientWebSocket();


            // Establish a connection
            Program.Logger.Info($"Establishing connection to hub socket");
            await _socketClient.ConnectAsync(new Uri($"ws://{_config.Hub.SocketAddress}:{_config.Hub.SocketPort}"), CancellationToken.None);


            // Send the attach request
            try
            {
                await SendMessage(new NodeAttachRequest() 
                { 
                    NodeLabel = _config.Label 
                });
            }
            catch (Exception ex)
            {
                Program.Logger.Error(ex);
                return;
            }


            // Listen for messages
            Program.Logger.Info("Handshake with server complete, listening for messages");
            byte[] buffer = new byte[1024];
            while (_socketClient.State == WebSocketState.Open)
            {
                WebSocketReceiveResult result = await _socketClient.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                if (result.MessageType == WebSocketMessageType.Text)
                {
                    string message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    Program.Logger.Debug($"[HubSocket] {message}");
                    SocketFrame frame;
                    try
                    {
                        frame = UnpackMessage<SocketFrame>(message);
                    }
                    catch (Exception ex)
                    {
                        continue;
                    }

                    await ProcessFrame(frame);
                }
            }

            return;
        }

        #endregion Public Methods

        #region Private Methods

        //private async Task<WebSocketReceiveResult> AwaitNextTextMessage(ClientWebSocket socket)
        //{
        //    WebSocketReceiveResult? result = null;
        //    byte[] buffer = new byte[1024];

        //    while (result == null)
        //    {
        //        result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

        //        if (result.MessageType != WebSocketMessageType.Text)
        //        {
        //            result = null;
        //        }
        //    }

        //    return result;
        //}

        private async Task ProcessFrame(SocketFrame frame)
        {

        }

        public async Task SendMessage<T>(T message)
        {
            if (_socketClient == null || _socketClient.State == WebSocketState.Closed) { return; }

            string? packedMsg = PackMessage(message);
            if (packedMsg == null) { return; }

            await _socketClient.SendAsync(new(Encoding.UTF8.GetBytes(packedMsg)), WebSocketMessageType.Text, true, CancellationToken.None);
        }

        private string? PackMessage<T>(T message, FxKeyAuth auth)
        {
            byte[] serializedMsg = MessagePackSerializer.Serialize(message);
            string? encryptedMsg = auth.Encrypt(serializedMsg);
            return encryptedMsg;
        }

        private string? PackMessage<T>(T message)
        {
            return PackMessage(message, _keyAuth);
        }

        private T UnpackMessage<T>(string message, FxKeyAuth auth)
        {
            byte[]? decryptedMsg = auth.Decrypt(message);
            T deserializedMsg = MessagePackSerializer.Deserialize<T>(decryptedMsg);
            return deserializedMsg;
        }

        private T UnpackMessage<T>(string message)
        {
            return UnpackMessage<T>(message, _keyAuth);
        }

        #endregion Private Methods
    }
}

#endif
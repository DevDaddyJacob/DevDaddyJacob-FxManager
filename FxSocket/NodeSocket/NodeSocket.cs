#if IS_FX_NODE

using DevDaddyJacob.FxManager.Node;
using DevDaddyJacob.FxManager.Node.Config;
using DevDaddyJacob.FxManager.Shared.Logger;
using DevDaddyJacob.FxManager.Socket.Payloads;
using DevDaddyJacob.FxManager.Socket.Payloads.General;
using System.Net.WebSockets;
using System.Text;

namespace DevDaddyJacob.FxManager.Socket.Node
{
    internal class NodeSocket : FxSocket
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

        public override async void Start()
        {
            // If there is already an existing socket open
            if (_socketClient != null && _socketClient.State == WebSocketState.Open) { return; }
            _socketClient = new ClientWebSocket();


            // Establish a connection
            FxNode.Logger.Info($"Establishing connection to hub socket");
            await _socketClient.ConnectAsync(new Uri($"ws://{_config.Hub.SocketAddress}:{_config.Hub.SocketPort}"), CancellationToken.None);


            // Send the attach request
            try
            {
                await SendMessage(new NodeAttachFrame()
                {
                    NodeLabel = _config.Label
                });
            }
            catch (Exception ex)
            {
                FxNode.Logger.Error(ex);
                return;
            }


            // Listen for messages
            FxNode.Logger.Info("Listening for messages from hub");
            while (_socketClient.State == WebSocketState.Open)
            {
                Tuple<SocketMessageType, byte[]>? msgData = await NextMessage(_socketClient);
                if (msgData == null)
                {
                    FxNode.Logger.Debug($"[HubSocket] Received Message: null");
                    continue;
                }

                SocketPayload payload = UnpackPayload(msgData.Item2);
                await ProcessFrame(payload);
            }

            FxNode.Exit("FxHub terminated socket connection!", LogLevel.Critical);
        }

        #endregion Public Methods

        #region Private Methods

        private async Task ProcessFrame(SocketPayload frame)
        {

        }

        //private async Task<string?> NextMessage()
        //{
        //    if (_socketClient == null || _socketClient.State == WebSocketState.Closed) { return null; }

        //    byte[] buffer = new byte[1024];
        //    while (_socketClient.State == WebSocketState.Open)
        //    {
        //        try
        //        {
        //            WebSocketReceiveResult result = await _socketClient.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

        //            if (result.MessageType == WebSocketMessageType.Close)
        //            {

        //            }

        //            if (result.MessageType == WebSocketMessageType.Text)
        //            {
        //                return Encoding.UTF8.GetString(buffer, 0, result.Count);
        //            }
        //        }
        //        catch
        //        {
        //            return null;
        //        }
        //    }

        //    return null;
        //}

        public async Task SendMessage(SocketPayload message)
        {
            if (_socketClient == null || _socketClient.State == WebSocketState.Closed) { return; }

            byte[] packedMsg = PackPayload(message);
            await _socketClient.SendAsync(new(packedMsg), WebSocketMessageType.Binary, true, CancellationToken.None);
        }

        #endregion Private Methods
    }
}

#endif
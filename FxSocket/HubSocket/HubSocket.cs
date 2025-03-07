#if IS_FX_HUB

using DevDaddyJacob.FxManager.Hub;
using DevDaddyJacob.FxManager.Hub.Config;
using DevDaddyJacob.FxManager.Hub.Config.Models;
using DevDaddyJacob.FxManager.Socket.Payloads;
using DevDaddyJacob.FxManager.Socket.Payloads.General;
using System.Collections.Concurrent;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Xml.Linq;

namespace DevDaddyJacob.FxManager.Socket.Hub
{
    internal class HubSocket : FxSocket
    {
        #region Fields

        private FxKeyAuth _keyAuth;
        private HubConfig _config;
        private HttpListener? _listener = null;
        private ConcurrentDictionary<string, FxKeyAuth> _nodeEncrypters = new();
        private ConcurrentDictionary<string, NodeEventPool> _nodeEventPools = new();

        private ConcurrentDictionary<WebSocket, string> _socketNodeLabel = new();
        private ConcurrentDictionary<WebSocket, HttpListenerContext> _socketContext = new();
        private ConcurrentDictionary<WebSocket, bool> _socketAuthStatus = new();
        private ConcurrentDictionary<WebSocket, Thread> _socketThread = new();
        private ConcurrentDictionary<WebSocket, CancellationTokenSource> _socketCancel = new();

        #endregion Fields

        #region Properties

        public NodeEventPool? this[string label] 
        {
            get
            {
                if (!_nodeEventPools.TryGetValue(label, out NodeEventPool pool))
                {
                    return null;
                }

                return pool;
            }
        }

        #endregion Properties

        #region Constructors

        public HubSocket(HubConfig config)
        {
            _config = config;
            _keyAuth = new(config);

            foreach (KeyValuePair<string, NodeConfig> node in _config.Nodes)
            {
                _nodeEncrypters[node.Key] = new(pubKeyData: node.Value.PublicKey);
            }
        }

        #endregion Constructors

        #region Public Methods

        public override async void Start()
        {
            // Check if there is already an existing socket open
            FxHub.Logger.Debug($"Trying to start socket");
            if (_listener != null) { return; }


            // Create the http listener and other variables
            FxHub.Logger.Debug($"Creating http listener at \"http://+:{_config.Socket.SocketPort}/\"");
            _listener = new HttpListener();
            _listener.Prefixes.Add($"http://+:{_config.Socket.SocketPort}/");


            // Start the http listener
            FxHub.Logger.Debug($"Starting listener");
            try
            {
                _listener.Start();
            }
            catch (HttpListenerException ex)
            {
                if (ex.Message.Equals("Access is denied."))
                {
                    FxHub.Logger.Alert(ex);
                    FxHub.Logger.Alert("Use the following command in a command prompt run as administrator to allow use of the websocket server:\n" +
                        $"netsh http add urlacl url=\"http://+:{_config.Socket.SocketPort}/\" user={System.Security.Principal.WindowsIdentity.GetCurrent().Name}\n");
                    return;
                }

                throw;
            }


            // Handle the connections
            while (_listener.IsListening)
            {
                HttpListenerContext context = await _listener.GetContextAsync();
                if (context.Request.IsWebSocketRequest)
                {
                    _ = ReceiveSocketConnection(context);
                }
                else
                {
                    context.Response.StatusCode = 400;
                    context.Response.Close();
                }
            }
        }

        public Task SendAsync(string nodeLabel, SocketPayload message)
        {
            return Task.Run(() =>
            {
                // Pack the payload
                byte[] packedMsg = PackPayload(message);


                // Send the message to each applicable node
                Task[] sendTasks = new Task[0];
                foreach (KeyValuePair<WebSocket, string> entry in _socketNodeLabel)
                {
                    if (nodeLabel.Equals("*") || nodeLabel.Equals(entry.Value))
                    {
                        int index = sendTasks.Length;
                        Array.Resize(ref sendTasks, index + 1);
                        sendTasks[index] = Task.Run(async () =>
                        {
                            await entry.Key.SendAsync(new(packedMsg), WebSocketMessageType.Text, true, CancellationToken.None);
                        });
                    }
                }

                Task.WaitAll(sendTasks);
            });
        }

        #endregion Public Methods

        #region Private Methods

        private void ResetSocket(WebSocket socket)
        {
            if (_socketCancel.TryRemove(socket, out CancellationTokenSource cancel))
            {
                cancel.Cancel();
            }

            _socketAuthStatus.TryRemove(socket, out _);
            _socketThread.TryRemove(socket, out _);
            _socketContext.TryRemove(socket, out _);
        }

        private async Task ReceiveSocketConnection(HttpListenerContext context)
        {
            HttpListenerWebSocketContext socketContext = await context.AcceptWebSocketAsync(null);
            WebSocket socket = socketContext.WebSocket;

            _socketContext[socket] = context;
            _socketAuthStatus[socket] = false;
            _socketCancel[socket] = new();
            _socketThread[socket] = new Thread(() => HandleSocket(socket));
            _socketThread[socket].Start();
        }

        private async void HandleSocket(WebSocket socket)
        {
            if (!_socketContext.TryGetValue(socket, out HttpListenerContext? context)) { ResetSocket(socket); return; }
            if (!_socketAuthStatus.TryGetValue(socket, out bool authStatus)) { ResetSocket(socket); return; }
            if (!_socketCancel.TryGetValue(socket, out CancellationTokenSource? cancel)) { ResetSocket(socket); return; }

            byte[] buffer = new byte[1024];
            FxHub.Logger.Info($"Listening for messages from node at {context.Request.RemoteEndPoint}");
            while (socket.State == WebSocketState.Open && !cancel.IsCancellationRequested)
            {
                Tuple<SocketMessageType, byte[]>? msgData = await NextMessage(socket);
                if (msgData == null)
                {
                    FxHub.Logger.Debug($"[NodeSocket:{context.Request.RemoteEndPoint}] Received Message: null");
                    continue;
                }

                SocketPayload payload = UnpackPayload(msgData.Item2);
                await ProcessFrame(socket, payload);
            }

            ResetSocket(socket);
        }

        private async Task ProcessFrame(WebSocket socket, SocketPayload frame)
        {
            if (!_socketAuthStatus.TryGetValue(socket, out bool authStatus)) { return; }
            FxHub.Logger.Debug($"Processing frame with event: '{frame.Event}'");

            // Check if we have yet to auth and handle the frame
            if (!authStatus)
            {
                if (frame.Event.Equals("NodeAttachFrame"))
                {
                    NodeAttachFrame attachFrame = (NodeAttachFrame)frame;

                    bool isLabelValid = false;
                    foreach (KeyValuePair<string, FxKeyAuth> entry in _nodeEncrypters)
                    {
                        if (entry.Key.Equals(attachFrame.NodeLabel))
                        {
                            isLabelValid = true;
                            break;
                        }
                    }

                    if (!isLabelValid)
                    {
                        await socket.CloseAsync(WebSocketCloseStatus.InvalidPayloadData, $"No Such Node Label! (label='{attachFrame.NodeLabel}')", CancellationToken.None);
                        ResetSocket(socket);
                        return;
                    }

                    _socketAuthStatus[socket] = true;
                    _socketNodeLabel[socket] = attachFrame.NodeLabel;
                }
                else
                {
                    await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);
                    ResetSocket(socket);
                }

                return;
            }


            if (!_socketNodeLabel.TryGetValue(socket, out string? label)) { return; }

            _nodeEventPools[label]?.InvokeFrame(frame);
        }

        #endregion Private Methods
    }
}

#endif
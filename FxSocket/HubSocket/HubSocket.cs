#if IS_FX_HUB

using DevDaddyJacob.FxHub;
using DevDaddyJacob.FxHub.Config;
using DevDaddyJacob.FxHub.Config.Models;
using DevDaddyJacob.FxShared;
using DevDaddyJacob.FxSocket.Payloads;
using DevDaddyJacob.FxSocket.Payloads.Frames;
using MessagePack;
using System.Collections.Concurrent;
using System.Net;
using System.Net.WebSockets;
using System.Text;

namespace DevDaddyJacob.FxSocket.Hub
{
    internal class HubSocket
    {
        #region Fields

        private FxKeyAuth _keyAuth;
        private HubConfig _config;
        private ConcurrentDictionary<string, FxKeyAuth> _nodeEncrypters = new();
        private ConcurrentDictionary<string, WebSocket> _socketNodeMapping = new();
        private ConcurrentDictionary<string, NodeEventPool> _nodeEventPools = new();

        private CancellationTokenSource? _listenerCancel = null;
        private Thread? _listenerThread = null;
        private HttpListener? _listener = null;

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

        public void StartSocket()
        {
            // Check if there is already an existing socket open
            Program.Logger.Debug($"Trying to start socket");
            if (_listener != null) { return; }


            // Create the http listener and other variables
            Program.Logger.Debug($"Creating http listener at \"http://+:{_config.Socket.SocketPort}/\"");
            _listenerCancel = new();
            _listenerThread = new Thread(RunListener);
            _listener = new HttpListener();
            _listener.Prefixes.Add($"http://+:{_config.Socket.SocketPort}/");


            // Start the http listener
            Program.Logger.Debug($"Starting listener");
            try
            {
                _listener.Start();
                _listenerThread.Start();
            }
            catch (HttpListenerException ex)
            {
                if (ex.Message.Equals("Access is denied."))
                {
                    Program.Logger.Alert(ex);
                    Program.Logger.Alert("Use the following command in a command prompt run as administrator to allow use of the websocket server:\n" +
                        $"netsh http add urlacl url=\"http://+:{_config.Socket.SocketPort}/\" user={System.Security.Principal.WindowsIdentity.GetCurrent().Name}\n");
                    return;
                }

                throw ex;
            }
        }

        #endregion Public Methods

        #region Private Methods

        private void Reset()
        {
            _listener = null;
            _listenerCancel = null;
            _listenerThread = null;
        }

        private async void RunListener()
        {
            Program.Logger.Debug($"Entering thread for \"RunListener\" (Managed Thread ID: {Thread.CurrentThread.ManagedThreadId})");

            while (true)
            {
                if (_listener == null) { break; }
                if (_listenerCancel == null) { break; }
                if (_listenerThread == null) { break; }
                if (_listenerCancel.IsCancellationRequested) { break; }

                HttpListenerContext context = await _listener.GetContextAsync();
                if (context.Request.IsWebSocketRequest)
                {
                    ProcessSocketConnection(context);
                }
                else
                {
                    context.Response.StatusCode = 400;
                    context.Response.Close();
                }
            }

            Program.Logger.Debug($"Exiting thread for \"RunListener\" (Managed Thread ID: {Thread.CurrentThread.ManagedThreadId})");
        }

        private async Task ProcessSocketConnection(HttpListenerContext context)
        {
            HttpListenerWebSocketContext socketContext = await context.AcceptWebSocketAsync(null);
            WebSocket socket = socketContext.WebSocket;
            byte[] buffer = new byte[1024];


            // Await the attach request
            WebSocketReceiveResult attachResult = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            NodeAttachRequest attachRequest = UnpackMessage<NodeAttachRequest>(Encoding.UTF8.GetString(buffer, 0, attachResult.Count));

            bool isLabelValid = false;
            foreach (KeyValuePair<string, FxKeyAuth> entry in _nodeEncrypters)
            {
                if (entry.Key.Equals(attachRequest.NodeLabel))
                {
                    isLabelValid = true;
                    break;
                }
            }

            if (!isLabelValid)
            {
                await socket.CloseAsync(WebSocketCloseStatus.InvalidPayloadData, $"Auth Failed - Stage 1 (label='{attachRequest.NodeLabel}')", CancellationToken.None);
                return;
            }

            _socketNodeMapping[attachRequest.NodeLabel] = socket;


            // Listen for messages
            string label = attachRequest.NodeLabel;
            _nodeEventPools[label] = new(label);
            
            Program.Logger.Info($"Handshake with node '{label}' complete, listening for messages");
            while (socket.State == WebSocketState.Open)
            {
                WebSocketReceiveResult result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                if (result.MessageType == WebSocketMessageType.Text)
                {
                    string message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    Program.Logger.Debug($"[NodeSocket:{label}] {message}");
                    SocketFrame frame;
                    try
                    {
                        frame = UnpackMessage<SocketFrame>(message);
                    }
                    catch (Exception ex)
                    {
                        Program.Logger.Error(ex.Message);
                        Program.Logger.Error(ex.InnerException);
                        Program.Logger.Error(ex.Source);
                        Program.Logger.Error(ex.StackTrace);
                        continue;
                    }

                    _nodeEventPools[label]?.InvokeFrame(frame);
                }
                else if (result.MessageType == WebSocketMessageType.Close)
                {
                    await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);
                }
            }


            // Handle closing
            _nodeEventPools.TryRemove(label, out _);
        }

        // C->S     Connect
        // C->S     Send attach request
        // S->C     Parse request, validate, and send challenge
        // C->S     Decrypt challenge, re-encrypt it, and return it
        // S->C     Decrypt challenge and return results
        private async Task ProcessSocketConnection2(HttpListenerContext context)
        {
            HttpListenerWebSocketContext socketContext = await context.AcceptWebSocketAsync(null);
            WebSocket socket = socketContext.WebSocket;
            byte[] buffer = new byte[1024];


            // Track the socket's source
            Program.Logger.Info($"Received socket connection from endpoint: {context.Request.RemoteEndPoint}");
            Program.Logger.Debug($"Context info: {context.Request.RemoteEndPoint}");


            // Await the attach request
            buffer = new byte[1024];
            WebSocketReceiveResult attachResult = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            NodeAttachRequest attachRequest = MessagePackSerializer.Deserialize<NodeAttachRequest>(MessagePackSerializer.ConvertFromJson(Encoding.UTF8.GetString(buffer, 0, attachResult.Count)));

            bool isLabelValid = false;
            foreach (KeyValuePair<string, FxKeyAuth> entry in _nodeEncrypters)
            {
                if (entry.Key.Equals(attachRequest.NodeLabel))
                {
                    isLabelValid = true;
                    break;
                }
            }

            if (!isLabelValid)
            {
                await socket.CloseAsync(WebSocketCloseStatus.InvalidPayloadData, $"Auth Failed - Stage 1 (label='{attachRequest.NodeLabel}')", CancellationToken.None);
                return;
            }

            _socketNodeMapping[attachRequest.NodeLabel] = socket;


            // Generate and send the challenge
            string challenge = Utils.GetUniqueKey(32);
            try
            {
                AuthChallenge serverChallenge = new AuthChallenge() { IsSolving = false, Challenge = challenge };
                string? packedChallengePayload = PackMessage(serverChallenge, _nodeEncrypters[attachRequest.NodeLabel]);
                if (packedChallengePayload == null) { return; }
                await socket.SendAsync(new(Encoding.UTF8.GetBytes(packedChallengePayload)), WebSocketMessageType.Text, true, CancellationToken.None);
            }
            catch
            {
                await socket.CloseAsync(WebSocketCloseStatus.Empty, "Auth Failed - Stage 2", CancellationToken.None);
                return;
            }


            // Await the solved challenge
            try
            {
                buffer = new byte[1024];
                WebSocketReceiveResult solveResult = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                AuthChallenge solveChallenge = UnpackMessage<AuthChallenge>(Encoding.UTF8.GetString(buffer, 0, attachResult.Count));
                if (!solveChallenge.IsSolving || !solveChallenge.Challenge.Equals(challenge))
                {
                    await socket.CloseAsync(WebSocketCloseStatus.InvalidPayloadData, "Bad auth challenge solve", CancellationToken.None);
                    return;
                }
            }
            catch
            {
                await socket.CloseAsync(WebSocketCloseStatus.Empty, "Auth Failed - Stage 3", CancellationToken.None);
                return;
            }


            // Send the auth challenge 'ok'
            try
            {
                AuthChallenge challengeOk = new AuthChallenge() { IsSolving = true, Challenge = "Handshake Complete, Auth Ok!" };
                string? packedChallengeOkPayload = PackMessage(challengeOk, _nodeEncrypters[attachRequest.NodeLabel]);
                if (packedChallengeOkPayload == null) { return; }
                await socket.SendAsync(new(Encoding.UTF8.GetBytes(packedChallengeOkPayload)), WebSocketMessageType.Text, true, CancellationToken.None);
            }
            catch
            {
                await socket.CloseAsync(WebSocketCloseStatus.Empty, "Auth Failed - Stage 4", CancellationToken.None);
                return;
            }


            // Listen for messages
            string label = attachRequest.NodeLabel;
            Program.Logger.Info($"Handshake with node '{label}' complete, listening for messages");
            _nodeEventPools[label] = new(label);
            while (socket.State == WebSocketState.Open)
            {
                WebSocketReceiveResult result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
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
                    
                    _nodeEventPools[label]?.InvokeFrame(frame);
                }
                else if (result.MessageType == WebSocketMessageType.Close)
                {
                    await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);
                }
            }


            // Handle closing
            _nodeEventPools.TryRemove(label, out _);
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
#if IS_FX_HUB

using DevDaddyJacob.FxHub;
using DevDaddyJacob.FxSocket.Payloads.Frames;
using MessagePack;

namespace DevDaddyJacob.FxSocket.Hub
{
    internal class NodeEventPool
    {
        #region Fields

        private readonly string _nodeLabel;
        private event OnHeartbeatFrame _heartbeatEvent;

        #endregion Fields

        #region Properties

        public string NodeLabel
        {
            get => _nodeLabel;
        }

        public event OnHeartbeatFrame OnHeartbeat
        {
            add => _heartbeatEvent += value;
            remove => _heartbeatEvent -= value;
        }

        #endregion Properties

        #region Constructors

        public NodeEventPool(string nodeLabel)
        {
            _nodeLabel = nodeLabel;
        }

        #endregion Constructors

        #region Methods

        public void InvokeFrame(SocketFrame frame)
        {
            Program.Logger.Info($"[{_nodeLabel}:{frame.Event}] {MessagePackSerializer.SerializeToJson(frame)}");
            switch (frame.Event)
            {
                case "HeartbeatFrame": 
                    _heartbeatEvent?.Invoke(); 
                    break;
                
                default: break;
            }
        }

        #endregion Methods
    }
}

#endif
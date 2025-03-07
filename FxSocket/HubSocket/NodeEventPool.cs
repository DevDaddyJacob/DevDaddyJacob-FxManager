#if IS_FX_HUB

using DevDaddyJacob.FxManager.Hub;
using DevDaddyJacob.FxManager.Socket.Payloads;
using DevDaddyJacob.FxManager.Socket.Payloads.General;
using MessagePack;

namespace DevDaddyJacob.FxManager.Socket.Hub
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

#pragma warning disable CS8618
        public NodeEventPool(string nodeLabel)
#pragma warning restore CS8618
        {
            _nodeLabel = nodeLabel;
        }

        #endregion Constructors

        #region Methods

        public void InvokeFrame(SocketPayload frame)
        {
            FxHub.Logger.Info($"[{_nodeLabel}:{frame.Event}] {MessagePackSerializer.SerializeToJson(frame)}");
            switch (frame)
            {
                case HeartbeatFrame _frame:
                    _heartbeatEvent?.Invoke(); 
                    break;
                
                default: break;
            }
        }

        #endregion Methods
    }
}

#endif
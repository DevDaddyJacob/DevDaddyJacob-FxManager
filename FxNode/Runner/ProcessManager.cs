using DevDaddyJacob.FxManager.Node.Runner.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevDaddyJacob.FxManager.Node.Runner
{
    /// <summary>
    /// Manages the lifecycle of a child process, isolating process management logic from the main application.
    /// </summary>
    internal class ProcessManager
    {
        #region Fields

        private readonly int _processId;
        private readonly string _mutex;
        private readonly string _netEndpoint;
        private readonly Action _statusCallback;
        
        private readonly long _timestampStart = DateTime.Now.Ticks;
        private long? _timestampKill = null;
        private long? _timestampExit = null;
        private long? _timestampClose = null;
        private FxProcess? _fxs = null;
        private readonly Action? _exitCallback = null;

        #endregion Fields

        #region Properties

        public int PID
        {
            get => _processId;
        }

        public string Mutex
        {
            get => _mutex;
        }

        public string NetEndpoint
        {
            get => _netEndpoint;
        }

        /// <summary>
        /// Get the proc info/stats for the history
        /// 
        /// https://github.com/tabarra/txAdmin/blob/1a5c2745d275e311cdb0d7884cc8ae60215c5394/core/modules/FxRunner/ProcessManager.ts#L133
        /// </summary>
        public ChildProcessStateInfo StateInfo
        {
            get => throw new NotImplementedException();
        }

        /// <summary>
        /// If the child process is alive, meaning it has process running and the pipes open
        /// 
        /// https://github.com/tabarra/txAdmin/blob/1a5c2745d275e311cdb0d7884cc8ae60215c5394/core/modules/FxRunner/ProcessManager.ts#L154
        /// </summary>
        public bool IsAlive
        {
            get => throw new NotImplementedException();
        }

        /// <summary>
        /// The overall state of the child process
        /// 
        /// https://github.com/tabarra/txAdmin/blob/1a5c2745d275e311cdb0d7884cc8ae60215c5394/core/modules/FxRunner/ProcessManager.ts#L162
        /// </summary>
        public ChildProcessState Status
        {
            get => throw new NotImplementedException();
        }

        /// <summary>
        /// Uptime of the child process, until now or the last event
        /// 
        /// https://github.com/tabarra/txAdmin/blob/1a5c2745d275e311cdb0d7884cc8ae60215c5394/core/modules/FxRunner/ProcessManager.ts#L172
        /// </summary>
        public long Uptime
        {
            get => throw new NotImplementedException();
        }

        /// <summary>
        /// The stdin of the child process, should be writable if this.IsAlive
        /// 
        /// https://github.com/tabarra/txAdmin/blob/1a5c2745d275e311cdb0d7884cc8ae60215c5394/core/modules/FxRunner/ProcessManager.ts#L185
        /// </summary>
        public StreamWriter? StdIn
        {
            get => throw new NotImplementedException();
        }

        #endregion Properties

        #region Constructor

        /// <summary>
        /// https://github.com/tabarra/txAdmin/blob/1a5c2745d275e311cdb0d7884cc8ae60215c5394/core/modules/FxRunner/ProcessManager.ts#L41
        /// </summary>
        public ProcessManager(FxProcess childProcess, ChildStateProps props)
        {
            _processId = childProcess.Process.Id;
            _fxs = childProcess;
            _mutex = props.Mutex;
            _netEndpoint = props.NetEndpoint;
            _statusCallback = props.OnStatusUpdate;


            // Handle exit event, ensuring to wait for the io streams
            _fxs.Process.Exited += (sender, args) =>
            {
                _timestampExit = DateTime.Now.Ticks;
                string info = $"0x{_fxs.Process.ExitCode.ToString("X").ToUpper()}";
                FxNode.Logger.Warning($"FXServer Exited ({info})");

                _exitCallback?.Invoke();
                TriggerStatusUpdate();
                if (_timestampExit - _timestampStart <= 5000)
                {
                    FxNode.Logger.Warning("FXServer didn\'t start. This is not a FxManager issue.");
                }
            
            };

        }

        #endregion Constructor

        #region Static Public Methods
        #endregion Static Public Methods

        #region Static Private Methods
        #endregion Static Private Methods

        #region Public Methods

        /// <summary>
        /// Ensures we did everything we can to send a kill signal to the child process
        /// and that we are freeing up resources
        /// 
        /// https://github.com/tabarra/txAdmin/blob/1a5c2745d275e311cdb0d7884cc8ae60215c5394/core/modules/FxRunner/ProcessManager.ts#L109
        /// </summary>
        public void Destroy()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Registers a callback to be called when the child process is destroyed
        /// 
        /// https://github.com/tabarra/txAdmin/blob/1a5c2745d275e311cdb0d7884cc8ae60215c5394/core/modules/FxRunner/ProcessManager.ts#L126
        /// </summary>
        public void OnExit(Action callback)
        {
            throw new NotImplementedException();
        }

        #endregion Public Methods

        #region Private Methods

        /// <summary>
        /// Safely triggers the status update callback
        /// 
        /// https://github.com/tabarra/txAdmin/blob/1a5c2745d275e311cdb0d7884cc8ae60215c5394/core/modules/FxRunner/ProcessManager.ts#L96
        /// </summary>
        private void TriggerStatusUpdate()
        {
            throw new NotImplementedException();
        }

        #endregion Private Methods
    }
}

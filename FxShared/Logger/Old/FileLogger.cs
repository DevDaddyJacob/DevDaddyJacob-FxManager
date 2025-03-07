//using System.Diagnostics;
//using System.IO;
//using System.Text;

//namespace DevDaddyJacob.FxManager.Shared.Logger
//{
//    internal delegate void OnLoggerWriteLine(string line);

//    internal abstract class FileLogger
//    {
//        #region Fields

//        private static readonly TimeSpan _MAX_IDLE_TIME = new TimeSpan(0, 0, 15);
//        private bool _PreventAppLogging;
//        private OnLoggerWriteLine? _OnWriteLineEvent;
//        private string _LogDir;
//        private bool _UseZuluTime;
//        private StreamWriter? _LogStream = null;
//        private string? _CurrentLogFileName = null;
//        private DateTime _LastWrite;
//        private CancellationTokenSource? _IdleTimerCancelTokenSrc = null;
//        private Task? _IdleTimer = null;

//        #endregion Fields

//        #region Properties

//        #region Protected

//        protected TimeSpan TimeSinceWrite
//        {
//            get => DateTimeNow() - _LastWrite;
//        }

//        #endregion Protected

//        #region Public

//        public string LogDir
//        {
//            get => _LogDir;
//            private set => _LogDir = value;
//        }

//        public bool UseZuluTime
//        {
//            get => _UseZuluTime;
//            private set => _UseZuluTime = value;
//        }

//        public bool LogStreamOpen
//        {
//            get => _LogStream != null;
//        }

//        public event OnLoggerWriteLine OnWriteLine
//        {
//            add => _OnWriteLineEvent += value;
//            remove => _OnWriteLineEvent -= value;
//        }

//        #endregion Public

//        #endregion Properties

//        #region Constructor

//        public FileLogger(string logDir, bool usesZulu = true, bool preventAppLogging = false)
//        {
//            _LogDir = logDir;
//            _UseZuluTime = usesZulu;
//            _PreventAppLogging = preventAppLogging;
//            _LastWrite = DateTimeNow();
//            StartIdleTimer();
//        }

//        #endregion Constructor

//        #region Methods

//        #region Private

//        private void IdleTimer()
//        {
//            if (_IdleTimerCancelTokenSrc == null || _IdleTimerCancelTokenSrc.IsCancellationRequested)
//            {
//                return;
//            }

//            if (TimeSinceWrite > _MAX_IDLE_TIME)
//            {
//                int hours = TimeSinceWrite.Hours;
//                int minutes = TimeSinceWrite.Minutes;
//                int seconds = TimeSinceWrite.Seconds;
//                StringBuilder idleTimeStrBld = new StringBuilder();
//                if (hours > 0)
//                {
//                    idleTimeStrBld.Append($"{hours} hour{(hours > 1 ? "s" : "")}");
//                }

//                if (minutes > 0)
//                {
//                    if (idleTimeStrBld.Length > 0)
//                    {
//                        idleTimeStrBld.Append(" ");
//                    }

//                    idleTimeStrBld.Append($"{minutes} minute{(minutes > 1 ? "s" : "")}");
//                }

//                if (seconds > 0)
//                {
//                    if (idleTimeStrBld.Length > 0)
//                    {
//                        idleTimeStrBld.Append(" ");
//                    }

//                    idleTimeStrBld.Append($"{seconds} second{(seconds > 1 ? "s" : "")}");
//                }

//                if (!_PreventAppLogging)
//                {
//                    AppLogger.Info($"Closing logger stream after {idleTimeStrBld}");
//                }
//                CloseStream();

//                return;
//            }

//            Task.Delay(5000).ContinueWith((_) => IdleTimer(), _IdleTimerCancelTokenSrc.Token);
//        }

//        private void StartIdleTimer()
//        {
//            if (_IdleTimer != null)
//            {
//                return;
//            }

//            lock (_IdleTimerCancelTokenSrc = new CancellationTokenSource())
//            {
//                _IdleTimer = Task.Run(IdleTimer, _IdleTimerCancelTokenSrc.Token);
//            }
//        }

//        private void StopIdleTimer()
//        {
//            if (!_PreventAppLogging)
//            {
//                AppLogger.Info("Stopping the idle timer for the Logger");
//            }

//            if (_IdleTimerCancelTokenSrc == null)
//            {
//                _IdleTimer = null;
//                return;
//            }

//            lock (_IdleTimerCancelTokenSrc)
//            {
//                _IdleTimerCancelTokenSrc.Cancel();
//                _IdleTimerCancelTokenSrc = null;

//                _IdleTimer = null;
//            }
//        }

//        #endregion Private

//        #region Protected

//        protected DateTime DateTimeNow()
//        {
//            return UseZuluTime ? DateTime.UtcNow : DateTime.Now;
//        }

//        protected string GetFormatedDate(DateTime date)
//        {
//            return date.ToString("yyyy-MM-dd");
//        }

//        protected virtual string GetFileName()
//        {
//            string fileName = $"{GetFormatedDate(DateTimeNow())}.log";
//            return Path.Join(LogDir, fileName);
//        }

//        protected virtual string GetLogPrefix()
//        {
//            if (UseZuluTime)
//            {
//                return DateTimeNow().ToString($"HH:mm:ss'Z'");
//            }
//            else
//            {
//                return DateTimeNow().ToString($"HH:mm:ss UTCz");
//            }
//        }

//        protected void Write(string line)
//        {
//            if (_LogStream == null)
//            {
//                OpenStream();
//                Debug.Assert(_LogStream != null);
//            }
//            else if (_CurrentLogFileName != GetFileName())
//            {
//                CloseStream();
//                OpenStream();
//            }

//            string message = Utils.StripColour($"[{GetLogPrefix()}] {line}");
//            lock (_LogStream)
//            {
//                _LogStream.WriteLine(message);
//                _LogStream.Flush();
//            }
//            _LastWrite = DateTimeNow();
//            _OnWriteLineEvent?.Invoke(message);
//        }

//        #endregion Protected

//        #region Public

//        public void CloseStream()
//        {
//            if (_LogStream == null)
//            {
//                return;
//            }

//            lock (_LogStream)
//            {
//                _LogStream.Flush();
//                _LogStream.Close();
//                StopIdleTimer();

//                _LogStream = null;
//                _CurrentLogFileName = null;
//            }
//        }

//        public void OpenStream()
//        {
//            // Check if a file is already open
//            if (_LogStream != null)
//            {
//                return;
//            }

//            // Check if the directory exists
//            if (!Utils.ValidatePath(LogDir, true, preventAppLogging: _PreventAppLogging))
//            {
//                Directory.CreateDirectory(LogDir);
//            }

//            // Open the file in append mode
//            _CurrentLogFileName = GetFileName();
//            _LogStream = File.AppendText(_CurrentLogFileName);
//        }

//        #endregion Public

//        #endregion Methods
//    }
//}

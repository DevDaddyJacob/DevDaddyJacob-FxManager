using DevDaddyJacob.FxManager.Node.Config;
using DevDaddyJacob.FxManager.Shared.Logger;
using DevDaddyJacob.FxManager.Socket.Node;

namespace DevDaddyJacob.FxManager.Node
{
    internal class FxNode
    {
        #region Fields

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        private static FxNode _instance;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

        private readonly string[] _programArgs;
        private readonly AppLogger _appLogger;
        private readonly NodeConfig _nodeConfig;
        private readonly NodeSocket _nodeSocket;

        private ManualResetEvent _quitEvent;
        private string _quitReason = string.Empty;
        private LogLevel _quitSeverity = LogLevel.Informational;

        #endregion Fields

        #region Properties

        public static FxNode Instance
        {
            get => _instance;
        }

        public static AppLogger Logger
        {
            get => _instance._appLogger;
        }

        public static NodeConfig Config
        {
            get => _instance._nodeConfig;
        }

        public static NodeSocket Socket
        {
            get => _instance._nodeSocket;
        }

        #endregion Properties

        #region Constructor

        public FxNode(string[] programArgs)
        {
            _instance = this;
            _programArgs = programArgs;
            _quitEvent = new ManualResetEvent(false);
            _appLogger = new AppLogger(Console.Out, Args.LogLevel);
            _appLogger.Info("Creating FxNode...");


            // Get and validate the profile folder path for the config
            string profilePath = Args.ProfilePath;
            if (!Directory.Exists(profilePath)) { throw new DirectoryNotFoundException(profilePath); }
            _nodeConfig = new(profilePath);


            // Create the socket
            _nodeSocket = new(_nodeConfig);
        }

        #endregion Constructor

        #region Public Methods

        public static void Run(string[] args)
        {
            // Create the instance and re-title the node
            Console.Title = "FxManager - FxNode";
            _instance = new(args);
            Console.Title = $"FxManager - FxNode ({Config.Label})";


            // Setup keep alive rutine
            Console.CancelKeyPress += (_, args) =>
            {
                args.Cancel = true;
                Exit("Console cancel key pressed");
            };


            // Start the node processes
            Socket.Start();


            // Hold keep alive
            _instance._quitEvent.WaitOne();
            _instance._appLogger.Log(
                _instance._quitSeverity,
                "Application exiting{0}",
                _instance._quitReason.Equals(string.Empty) ? "." : $": {_instance._quitReason}");

            Thread.Sleep(5000);
        }

        public static void Exit(string reason, LogLevel severity)
        {
            _instance._quitReason = reason;
            _instance._quitSeverity = severity;
            _instance._quitEvent.Set();
        }

        public static void Exit(string reason)
        {
            Exit(reason, LogLevel.Informational);
        }

        public static void Exit(LogLevel severity)
        {
            Exit(string.Empty, severity);
        }

        public static void Exit()
        {
            Exit(string.Empty, LogLevel.Informational);
        }

        #endregion Public Methods

        #region Private Methods

        #endregion Private Methods

        #region Args Class

        public static class Args
        {
            public static string ProfilePath { get => GetString("--profile"); }

            public static LogLevel LogLevel { get => (LogLevel)TryGetInt("--loglevel", (int)LogLevel.Informational); }

            #region Private Methods

            /// <exception cref="ArgumentNullException">Thrown when the specified argument isn't present</exception>
            private static string GetString(string key)
            {
                string value = TryGetDefinedArg(Instance._programArgs, key, string.Empty);

                if (value.Equals(string.Empty))
                {
                    throw new ArgumentNullException($"Missing argument '{key}'");
                }

                return value;
            }

            private static string TryGetString(string key, string defaultVal)
            {
                string val;
                try { val = GetString(key); }
                catch { val = defaultVal; }
                return val;
            }

            /// <exception cref="ArgumentNullException">Thrown when the specified argument isn't present</exception>
            private static int GetInt(string key)
            {
                string value = GetString(key);

                if (!int.TryParse(value, out int intVal))
                {
                    throw new Exception($"Cannot convert '{value}' to integer!");
                }

                return intVal;
            }

            private static int TryGetInt(string key, int defaultVal)
            {
                int val;
                try { val = GetInt(key); }
                catch { val = defaultVal; }
                return val;
            }

            private static string TryGetDefinedArg(string[] args, string key, string defaultVal)
            {
                for (int i = 0; i < args.Length; i++)
                {
                    if (args[i].Equals(key))
                    {
                        if (i + 1 < args.Length)
                        {
                            return args[i + 1];
                        }
                        else
                        {
                            return defaultVal;
                        }
                    }
                }

                return defaultVal;
            }

            #endregion Private Methods
        }

        #endregion Args Class
    }
}

using DevDaddyJacob.FxManager.Hub.Config;
using DevDaddyJacob.FxManager.Shared.Logger;
using DevDaddyJacob.FxManager.Socket.Hub;

namespace DevDaddyJacob.FxManager.Hub
{
    internal class FxHub
    {
        #region Fields

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        private static FxHub _instance;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

        private readonly string[] _programArgs;
        private readonly AppLogger _appLogger;
        private readonly HubConfig _hubConfig;
        private readonly HubSocket _hubSocket;

        private ManualResetEvent _quitEvent;
        private string _quitReason = string.Empty;
        private LogLevel _quitSeverity = LogLevel.Informational;

        #endregion Fields

        #region Properties

        public static FxHub Instance
        {
            get => _instance;
        }

        public static AppLogger Logger
        {
            get => _instance._appLogger;
        }

        public static HubConfig Config
        {
            get => _instance._hubConfig;
        }

        public static HubSocket Socket
        {
            get => _instance._hubSocket;
        }

        #endregion Properties

        #region Constructor

        public FxHub(string[] programArgs)
        {
            _instance = this;
            _programArgs = programArgs;
            _quitEvent = new ManualResetEvent(false);
            _appLogger = new AppLogger(Console.Out, Args.LogLevel);
            _appLogger.Info("Creating FxHub...");


            // Get and validate the profile folder path for the config
            string configPath = Args.ConfigPath;
            if (!File.Exists(configPath)) { throw new FileNotFoundException(configPath); }
            _hubConfig = new(configPath);


            // Create the socket
            _hubSocket = new(_hubConfig);
        }

        #endregion Constructor

        #region Public Methods

        public static void Run(string[] args)
        {
            // Create the instance and re-title the hub
            Console.Title = "FxManager - FxHub";
            _instance = new(args);
            Console.Title = $"FxManager - FxHub ({Config.Label})";


            // Setup keep alive rutine
            Console.CancelKeyPress += (_, args) =>
            {
                args.Cancel = true;
                Exit("Console cancel key pressed");
            };


            // Start the hub processes
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
            public static string ConfigPath { get => GetString("--config"); }

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

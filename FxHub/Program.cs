using DevDaddyJacob.FxHub.Config;
using DevDaddyJacob.FxShared.Logger;
using DevDaddyJacob.FxSocket.Hub;

namespace DevDaddyJacob.FxHub
{
    internal class Program
    {
        #region Fields

#pragma warning disable CS8618
        private static HubConfig _hubConfig;
        private static HubSocket _hubSocket;
        private static AppLogger _appLogger;
        private static ManualResetEvent _quitEvent;
#pragma warning restore CS8618

        #endregion Fields

        #region Properties

        public static AppLogger Logger
        {
            get => _appLogger;
        }

        #endregion Properties

        #region Public Methods

        public static void Main(string[] args)
        {
            // Setup keep alive stuff and other prep
            Console.Title = "FxManager - FxHub";
            _appLogger = new AppLogger(Console.Out, LogLevel.Debug);
            Logger.Debug($"Starting App");

            _quitEvent = new ManualResetEvent(false);
            Console.CancelKeyPress += (_, args) =>
            {
                _quitEvent.Set();
                args.Cancel = true;
            };


            // Get and validate the config file path
            Logger.Debug($"Trying to find \"--config\" argument");
            string configPath = TryGetDefinedArg(args, "--config", string.Empty);
            if (configPath.Equals(string.Empty)) { throw new ArgumentNullException("Missing required argument '--config'"); }
            if (!File.Exists(configPath)) { throw new FileNotFoundException(configPath); }


            // Read the config
            Logger.Debug($"Creating hub config using path \"{configPath}\"");
            _hubConfig = new(configPath);
            Console.Title = $"FxManager - FxHub ({_hubConfig.Label})";


            // Create and start the socket
            Logger.Debug($"Creating socket from hub config");
            _hubSocket = new(_hubConfig);
            _hubSocket.StartSocket();


            _quitEvent.WaitOne();
        }

        #endregion Public Methods

        #region Private Methods

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
}

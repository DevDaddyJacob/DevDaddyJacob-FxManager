using DevDaddyJacob.FxNode.Config;
using DevDaddyJacob.FxNode.Config.Models;
using DevDaddyJacob.FxShared.Logger;
using DevDaddyJacob.FxSocket;
using DevDaddyJacob.FxSocket.Node;
using DevDaddyJacob.FxSocket.Payloads.Frames;
using MessagePack;

namespace DevDaddyJacob.FxNode
{
    internal class Program
    {
        #region Fields
        
#pragma warning disable CS8618
        private static NodeConfig _nodeConfig;
        private static NodeSocket _nodeSocket;
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
            Console.Title = "FxManager - FxNode";
            _appLogger = new AppLogger(Console.Out, LogLevel.Debug);
            Logger.Debug($"Starting App");

            _quitEvent = new ManualResetEvent(false);
            Console.CancelKeyPress += (_, args) =>
            {
                _quitEvent.Set();
                args.Cancel = true;
            };


            // Get and validate the profile folder path
            Logger.Debug($"Trying to find \"--profile\" argument");
            string profilePath = TryGetDefinedArg(args, "--profile", string.Empty);
            if (profilePath.Equals(string.Empty)) { throw new ArgumentNullException("Missing required argument '--profile'"); }
            if (!Directory.Exists(profilePath)) { throw new DirectoryNotFoundException(profilePath); }


            // Read the config
            Logger.Debug($"Creating node config using profile path \"{profilePath}\"");
            _nodeConfig = new(profilePath);
            Console.Title = $"FxManager - FxNode ({_nodeConfig.Label})";


            // Create and start the socket
            Logger.Debug($"Creating socket from node config");
            _nodeSocket = new(_nodeConfig);
            _nodeSocket.ConnectToHub();

            Task.Run(async () =>
            {
                await Task.Delay(3000);
                await _nodeSocket.SendMessage(new HeartbeatFrame());
            });


            _quitEvent.WaitOne();
        }

        private static void KeyCryptTest()
        {
            FxKeyAuth nodeAuth = new(pubKeyPath: FxKeyAuth.DefaultKeyPath("fxhub_1_pub.pem"), privKeyPath: FxKeyAuth.DefaultKeyPath("fxnode_1"));
            Console.WriteLine($"[-] nodeAuth.CanEncrypt={nodeAuth.CanEncrypt};nodeAuth.CanDecrypt={nodeAuth.CanDecrypt}");

            FxKeyAuth hubAuth = new(pubKeyPath: FxKeyAuth.DefaultKeyPath("fxnode_1_pub.pem"), privKeyPath: FxKeyAuth.DefaultKeyPath("fxhub_1"));
            Console.WriteLine($"[-] hubAuth.CanEncrypt={hubAuth.CanEncrypt};hubAuth.CanDecrypt={hubAuth.CanDecrypt}");

            T1(nodeAuth, hubAuth);
            T2(nodeAuth, hubAuth);
        }

        private static void T1(FxKeyAuth nodeAuth, FxKeyAuth hubAuth)
        {
            string data = "Secret Data!";
            Console.WriteLine($"[-] data={data}");

            string? nodeEncryptedData = nodeAuth.Encrypt(nodeAuth.StringToBytes(data));
            Console.WriteLine($"[{(nodeEncryptedData == null ? "Y" : "N")}] nodeEncryptedData={nodeEncryptedData}");

            byte[]? hubDecryptedData = hubAuth.Decrypt(nodeEncryptedData);
            Console.WriteLine($"[{(hubDecryptedData == null ? "Y" : "N")}] hubDecryptedData={nodeAuth.BytesToString(hubDecryptedData)}");


            string? hubEncryptedData = hubAuth.Encrypt(nodeAuth.StringToBytes(data));
            Console.WriteLine($"[{(hubEncryptedData == null ? "Y" : "N")}] hubEncryptedData={hubEncryptedData}");

            byte[]? nodeDecryptedData = nodeAuth.Decrypt(hubEncryptedData);
            Console.WriteLine($"[{(nodeDecryptedData == null ? "Y" : "N")}] nodeDecryptedData={MessagePackSerializer.SerializeToJson(nodeDecryptedData)}");
        }

        private static void T2(FxKeyAuth nodeAuth, FxKeyAuth hubAuth)
        {
            byte[] serializedData = MessagePackSerializer.Serialize("Secret Data!");
            Console.WriteLine($"\n[-] serializedData={MessagePackSerializer.SerializeToJson(serializedData)}");

            string? nodeEncryptedData = nodeAuth.Encrypt(serializedData);
            Console.WriteLine($"[{(nodeEncryptedData == null ? "Y" : "N")}] nodeEncryptedData={nodeEncryptedData}");

            byte[]? hubDecryptedData = hubAuth.Decrypt(nodeEncryptedData);
            Console.WriteLine($"[{(hubDecryptedData == null ? "Y" : "N")}] hubDecryptedData={MessagePackSerializer.SerializeToJson(hubDecryptedData)}");


            string? hubEncryptedData = hubAuth.Encrypt(serializedData);
            Console.WriteLine($"[{(hubEncryptedData == null ? "Y" : "N")}] hubEncryptedData={hubEncryptedData}");

            byte[]? nodeDecryptedData = nodeAuth.Decrypt(hubEncryptedData);
            Console.WriteLine($"[{(nodeDecryptedData == null ? "Y" : "N")}] nodeDecryptedData={MessagePackSerializer.SerializeToJson(nodeDecryptedData)}");
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

// 1. Node starts, and sends node_public_key to host
// 2. Host validates node_public_key against host config file
//      2.1 Reject if mismatch
// 3. Host sends host_public_key to node
// 4. Node validates host_public_key against node config file
//      4.1 Reject if mismatch
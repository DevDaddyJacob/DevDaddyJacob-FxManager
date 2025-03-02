using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Crypto.Engines;
using System.Text;
using Org.BouncyCastle.Security;

#if IS_FX_NODE 

using DevDaddyJacob.FxNode.Config;

#endif

#if IS_FX_HUB

using DevDaddyJacob.FxHub.Config;

#endif

namespace DevDaddyJacob.FxSocket
{
    // Commands to create RSA key pair:
    // ssh-keygen -t rsa -b 4096 -m PEM -f %USERPROFILE%/.ssh/KEY_NAME_HERE
    // ssh-keygen -f %USERPROFILE%/.ssh/KEY_NAME_HERE.pub -e -m PKCS8 > %USERPROFILE%/.ssh/KEY_NAME_HERE_pub.pem
    internal class FxKeyAuth
    {
        #region Fields

        private static string _sshFolder = string.Empty;

        private RsaKeyParameters? _privKey = null;
        private RsaKeyParameters? _pubKey = null;

        #endregion Fields

        #region Properties

        public static string KeyFolderPath
        {
            get
            {
                if (_sshFolder == string.Empty)
                {
                    _sshFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".ssh");
                }

                return _sshFolder;
            }
        }

        public bool CanEncrypt 
        { 
            get => _pubKey != null;
        }

        public bool CanDecrypt
        {
            get => _privKey != null;
        }

        #endregion Properties

        #region Constructors

        public FxKeyAuth(string? pubKeyPath = null, string? privKeyPath = null, string? pubKeyData = null)
        {
            if (pubKeyPath != null)
            {
                _pubKey = ReadPublicKey(pubKeyPath);
            }
            else if (pubKeyData != null)
            {
                _pubKey = MakePublicKey(pubKeyData);
            }

            if (privKeyPath != null)
            {
                _privKey = ReadPrivateKey(privKeyPath);
            }
        }

#if IS_FX_NODE

        public FxKeyAuth(NodeConfig config) 
            : this(pubKeyData: config.Hub.HostPublicKey, privKeyPath: config.NodePrivateKeyPath) { }

#endif

#if IS_FX_HUB

        public FxKeyAuth(HubConfig config)
            : this(privKeyPath: config.HubPrivateKeyPath) { }

        public FxKeyAuth(HubConfig config, string nodeLabel)
            : this(pubKeyData: config.Nodes[nodeLabel].PublicKey, privKeyPath: config.HubPrivateKeyPath) { }

#endif

        #endregion Constructors

        #region Public Methods

        public static string DefaultKeyPath(string file)
        {
            return Path.Combine(KeyFolderPath, file);
        }

        public string? Encrypt(byte[] data)
        {
            if (!CanEncrypt) { Console.WriteLine("Cannot Encrypt"); return null; }

            RsaEngine engine = new();
            engine.Init(true, _pubKey);

            byte[] encryptedBytes = engine.ProcessBlock(data, 0, data.Length);
            return Convert.ToBase64String(encryptedBytes);
        }

        public byte[]? Decrypt(string data)
        {
            if (!CanDecrypt) { Console.WriteLine("Cannot Decrypt"); return null; }

            RsaEngine engine = new();
            engine.Init(false, _privKey);

            byte[] encryptedBytes = Convert.FromBase64String(data);
            return engine.ProcessBlock(encryptedBytes, 0, encryptedBytes.Length);
        }

        public byte[] StringToBytes(string data)
        {
            return Encoding.UTF8.GetBytes(data);
        }

        public string BytesToString(byte[] data)
        {
            return Encoding.UTF8.GetString(data);
        }

        #endregion Public Methods

        #region Private Methods

        private RsaKeyParameters ReadPrivateKey(string privKeyPath)
        {
            if (!File.Exists(privKeyPath)) { throw new FileNotFoundException($"Missing private key file: \"{privKeyPath}\""); }

            using (StreamReader reader = new(privKeyPath))
            {
                PemReader pemReader = new(reader);
                AsymmetricCipherKeyPair keyPair = (AsymmetricCipherKeyPair)pemReader.ReadObject();
                return (RsaKeyParameters)keyPair.Private;
            }
        }

        private RsaKeyParameters ReadPublicKey(string pubKeyPath)
        {
            if (!File.Exists(pubKeyPath)) { throw new FileNotFoundException($"Missing public key file: \"{pubKeyPath}\""); }

            using (StreamReader reader = new(pubKeyPath))
            {
                PemReader pemReader = new(reader);
                return (RsaKeyParameters)pemReader.ReadObject();
            }
        }

        private RsaKeyParameters MakePublicKey(string pubKeyData)
        {
            pubKeyData = pubKeyData
                .Replace("-----BEGIN PUBLIC KEY-----", "")
                .Replace("-----END PUBLIC KEY-----", "")
                .Replace("\n", "")
                .Replace("\r", "")
                .Trim();

            byte[] keyBytes = Convert.FromBase64String(pubKeyData);
            return (RsaKeyParameters)PublicKeyFactory.CreateKey(keyBytes);
        }

        #endregion Private Methods
    }
}
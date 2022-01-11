using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;
using NewSocket.Core;
using NewSocket.Models;
using NewSocket.Security.Interfaces;
using NewSocket.Security.Models;

namespace NewSocket.Security.Protocols
{
    /// <summary>
    /// Exchanges certificates with the remote server to create an encrypted RSA tunnel.
    /// NOTE: Since this is full trust in certificates, this would not prevent a man-in-the-middle attack.
    /// For better security, Use ...
    /// </summary>
    public class RSAProtocol : ISecurityProtocal
    {
        public bool Authenticated { get; private set; } = false;
        public bool Preauthenticate { get; } = true;

        private RSAStream? RSAStream;

        public IRSACertShare CertShare;

        internal RSAProtocol(IRSACertShare certShare)
        {
            CertShare = certShare;
        }

        public async Task Authenticate(Stream network)
        {
            using (var relay = new NetworkMessageRelay(network))
            {
                await CertShare.ExchangeCertificates(relay);
                var encrypt = RSA.Create();
                encrypt.ImportParameters(CertShare.RemotePubkey);
                var decrypt = RSA.Create();
                decrypt.ImportParameters(CertShare.LocalPrivKey);

                RSAStream = new RSAStream(network, encrypt, decrypt);
                await relay.Synchronize();
                Authenticated = true;
            }
        }

        public Stream? GetDownStream(Stream networkDown)
        {
            return RSAStream;
        }

        public Stream? GetUpStream(Stream networkUp)
        {
            return RSAStream;
        }

        public Task OnSocketStarted(BaseSocketClient socket)
        {
            return Task.CompletedTask;
        }

        public async Task MessageSent()
        {
            if (RSAStream != null)
            {
                await RSAStream.PushCurrentBlockAsync();
            }
        }

        /// <summary>
        /// Exchanges certificates with the remote server to create an encrypted tunnel.
        /// Note: This protocol doesn't verify the remoet certificate, 
        /// making it vulnerable to man-in-the-middle attacks
        /// </summary>
        public static RSAProtocol CreateCertExchange()
        {
            return new RSAProtocol(new RSACertExchange());
        }

    }
}
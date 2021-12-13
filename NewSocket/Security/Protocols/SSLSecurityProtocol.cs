using System;
using System.IO;
using System.Net.Security;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using NewSocket.Core;
using NewSocket.Security.Interfaces;
using NewSocket.Security.Models;

namespace NewSocket.Security.Protocols
{
    public class SSLSecurityProtocol : ISecurityProtocal
    {
        public bool Authenticated { get; private set; } = false;
        public bool Preauthenticate { get; } = true;

        private SslStream? SSL;

        public bool IsServer { get; }

        public X509Certificate? ServerCertificate { get; }

        public string? TargetHost { get; }

        public static SSLSecurityProtocol AsServer(X509Certificate cert)
        {
            return new SSLSecurityProtocol(cert);
        }

        /// <param name="cert">The server's certificate</param>
        internal SSLSecurityProtocol(X509Certificate cert)
        {
            ServerCertificate = cert;
            IsServer = true;
        }

        /// <param name="targetHost">The domain name and port of the attempted connection</param>
        public static SSLSecurityProtocol AsClient(string targetHost)
        {
            return new SSLSecurityProtocol(targetHost);
        }

        internal SSLSecurityProtocol(string targetHost)
        {
            TargetHost = targetHost;
            IsServer = false;
        }

        public async Task Authenticate(Stream network)
        {
            try
            {
                SSL = new SslStream(network);
                if (IsServer)
                {
                    if (ServerCertificate == null)
                    {
                        throw new ArgumentNullException("Server Certificate");
                    }
                    await SSL.AuthenticateAsServerAsync(ServerCertificate);
                    Authenticated = true;
                }
                else
                {
                    if (TargetHost == null)
                    {
                        throw new ArgumentNullException("TargetHost");
                    }
                    await SSL.AuthenticateAsClientAsync(TargetHost);
                    Authenticated = true;
                }
            }
            catch (Exception ex)
            {
                Authenticated = false;
                throw new AuthenticationFailedException(ex);
            }
        }

        public Stream? GetDownStream(Stream networkDown) => SSL;

        public Stream? GetUpStream(Stream networkUp) => SSL;

        public Task OnSocketStarted(BaseSocketClient socket)
        {
            return Task.CompletedTask;
        }
    }
}
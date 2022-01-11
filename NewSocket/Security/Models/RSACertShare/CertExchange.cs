using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using NewSocket.Models;
using NewSocket.Security.Interfaces;

namespace NewSocket.Security.Models
{
    public class RSACertExchange : IRSACertShare
    {
        public RSAParameters RemotePubkey { get; private set; }
        public RSAParameters LocalPrivKey { get; private set; }
        private RSAParameters m_LocalPubKey;

        public RSACertExchange()
        {
            using (var rsa = RSA.Create()) // Genrate priv key
            {
                LocalPrivKey = rsa.ExportParameters(true);
                m_LocalPubKey = rsa.ExportParameters(false);
            }
        }


        public async Task ExchangeCertificates(NetworkMessageRelay relay)
        {
            var modulus = m_LocalPubKey.Modulus;
            var exponent = m_LocalPubKey.Exponent;

            if (modulus == null)
                throw new AuthenticationFailedException("Failed to obtain local RSA modulus");
            if (exponent == null)
                throw new AuthenticationFailedException("Failed to obtain local RSA exponent");

            var modSend = relay.SendMessageAsync("Modulus", modulus);
            var modRead = await relay.ReadMessageAsync();
            await modSend;
            if (modRead == null)
            {
                throw new AuthenticationFailedException("Remote party failed to provide RSA Modulus");
            }
            var remoteModulus = modRead.ToArray();


            var expSend = relay.SendMessageAsync("Exponent", exponent);
            var expRead = await relay.ReadMessageAsync();
            await expSend;
            if (expRead == null)
            {
                throw new AuthenticationFailedException("Remote party failed to provide RSA Exponent");
            }

            var remoteExponent = expRead.ToArray();


            RemotePubkey = new RSAParameters() { Modulus = remoteModulus, Exponent = remoteExponent };

        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using NewSocket.Models;

namespace NewSocket.Security.Interfaces
{
    public interface IRSACertShare
    {
        RSAParameters RemotePubkey { get; }

        RSAParameters LocalPrivKey { get; }

        Task ExchangeCertificates(NetworkMessageRelay relay);

    }
}

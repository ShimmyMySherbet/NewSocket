using System.IO;
using System.Security.Cryptography;

namespace NewSocket.Security.Models
{
    public class RSAStream : BlockCryptoStream
    {
        public override ushort MaxLegalDownBlockSize => 1024;
        public override ushort MaxLegalUpBlockSize => 117;
        public override ushort PreferedUpBlockSize => 100;

        public RSAStream(Stream data, RSA encrypt, RSA decrypt) : base(data,
                x => encrypt.Encrypt(x, RSAEncryptionPadding.Pkcs1),
                x => decrypt.Decrypt(x, RSAEncryptionPadding.Pkcs1))
        {
        }
    }
}
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using NewSocket.Core;
using NewSocket.Security.Interfaces;
using NewSocket.Security.Models;

namespace NewSocket.Security.Protocols
{
    public class AESPresharedKeyProtocol : ISecurityProtocal
    {
        public bool Authenticated { get; private set; } = false;
        public bool Preauthenticate { get; } = true;

        private Aes AES { get; }

        private Stream? Down;
        private Stream? Up;

        public AESPresharedKeyProtocol(string password)
        {
            using (var sha = SHA256.Create()) // Compute the SHA256 hash of the password to use as the key for AES256
            {
                var d = Encoding.UTF8.GetBytes(password);
                AES = Aes.Create();
                AES.Key = sha.ComputeHash(d);
                Array.Clear(d, 0, d.Length);
            }
        }

        public async Task Authenticate(Stream network)
        {
            try
            {
                var IV = new byte[16];    // Generate an IV for the socket to use when sending data
                using (var rng = RNGCryptoServiceProvider.Create())
                    rng.GetBytes(IV);

                await network.WriteAsync(IV, 0, 16);  // Send the IV to the remote client

                var remoteIV = new byte[16];
                await network.NetReadAsync(remoteIV, 16); // Read the remote client's IV for the socket to use when reading data

                // Create the up and down streams using the 2 IVs

                remoteIV = new byte[16];
                IV = new byte[16];
                var decTransform = AES.CreateDecryptor(AES.Key, remoteIV);
                var encTransform = AES.CreateEncryptor(AES.Key, IV);

                var upStream = new CryptoStream(network, encTransform, CryptoStreamMode.Write);
                var downStream = new CryptoStream(network, decTransform, CryptoStreamMode.Read);

                var testBytes = new byte[128];   // Generate 128 random bytes for testing the connection
                using (var rng = RNGCryptoServiceProvider.Create())
                    rng.GetBytes(testBytes);

                byte[] testHash;
                using (var sha = SHA256.Create())
                    testHash = sha.ComputeHash(testBytes); // Compute the hash of the random bytes

                await upStream.WriteAsync(testBytes, 0, 128);  // Send the test data to the remote client, over the encrypted connection

                // flush bytes
                upStream.WriteByte(0x0);
                upStream.WriteByte(0x0);

                var remoteBytes = new byte[128];
                //await downStream.ReadAsync(remoteBytes, 0, 128); // Read the remote client's random data over the encrypted connection

                for (int i = 0; i < 128; i++)
                {
                    var byt = network.ReadByte();
                    if (byt != -1)
                    {
                        remoteBytes[i] = (byte)byt;
                    }
                }

                byte[] remoteHash;
                using (var sha = SHA256.Create())
                    remoteHash = sha.ComputeHash(remoteBytes); // Compute the remote client's random data hash

                await upStream.WriteAsync(remoteHash, 0, 32); // Send the hash back to the remote client over the encrypted connection;
                await upStream.WriteAsync(remoteHash, 0, 32); // Send the hash back to the remote client over the encrypted connection;

                // consume flushed bytes
                downStream.ReadByte();
                downStream.ReadByte();

                var returnedHash = new byte[32];
                //await downStream.ReadAsync(returnedHash, 0, 32); // Recieve the hash the remote client computed for the random data sent to it
                for (int i = 0; i < 32; i++)
                {
                    var byt = network.ReadByte();
                    if (byt != -1)
                    {
                        returnedHash[i] = (byte)byt;
                    }
                }

                for (int i = 0; i < 32; i++) // Verify the hash matches the hash computed locally
                {
                    if (testHash[i] != returnedHash[i])
                    {
                        // Hash does not match, meaning data is being corrupted over the network
                        // Meaning the password is incorrect.
                        throw new AuthenticationFailedException("Verification Failed, Is the password incorrect?");
                    }
                }

                // All the data came back as expected, meaning the password and the IVs are correct.

                Authenticated = true;
            }
            catch (Exception ex)
            {
                // An error occoured while trying to encrypt or decrypt data, meaning the password is incorrect
                Authenticated = false;
                throw new AuthenticationFailedException(ex);
            }
        }

        public Stream? GetDownStream(Stream networkDown) => Down;

        public Stream? GetUpStream(Stream networkUp) => Up;

        public Task OnSocketStarted(BaseSocketClient socket)
        {
            return Task.CompletedTask;
        }

        public Task MessageSent()
        {
            return Task.CompletedTask;
        }
    }
}
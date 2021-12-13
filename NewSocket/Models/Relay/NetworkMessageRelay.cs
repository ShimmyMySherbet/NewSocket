using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NewSocket.Models.Relay;

namespace NewSocket.Models
{
    /// <summary>
    /// A primitive thread unsafe messaging socket that supports network synchronization.
    /// Meant to be used for security protocols authenticating before switching to the NewSocket messaging protocol
    /// </summary>
    public class NetworkMessageRelay : IDisposable
    {
        public Stream Network { get; }

        /// <summary>
        /// A list of previous messages this socket has received
        /// </summary>
        public List<RelayMessage> PreviousMessages { get; } = new List<RelayMessage>();

        public bool LeaveStreamOpen { get; set; }

        /// <param name="network">The base network stream</param>
        /// <param name="leaveStreamOpen">Leave the stream open when this instance is disposed</param>
        public NetworkMessageRelay(Stream network, bool leaveStreamOpen = true)
        {
            Network = network;
            LeaveStreamOpen = leaveStreamOpen;
        }

        /// <summary>
        /// Reads the next message from the network
        /// </summary>
        public async Task<RelayMessage?> ReadMessageAsync(CancellationToken token = default)
        {
            var headerStr = await Network.NetReadString(token);
            if (token.IsCancellationRequested)
            {
                return null;
            }
            var dataLength = await Network.NetReadInt32(token);
            if (token.IsCancellationRequested)
            {
                return null;
            }
            var data = new MarshalAllocMemoryStream(dataLength);
            var dataRead = 0;

            var buffer = new byte[32];
            while (dataRead < dataLength || token.IsCancellationRequested)
            {
                var segment = Math.Min(32, dataLength - dataRead);
                var read = await Network.ReadAsync(buffer, 0, segment, token);
                if (token.IsCancellationRequested)
                    return null;
                dataRead += read;
                await data.WriteAsync(buffer, 0, read, token);
            }
            if (token.IsCancellationRequested)
            {
                return null;
            }
            var msg = new RelayMessage(headerStr, data);
            PreviousMessages.Add(msg);
            return msg;
        }

        /// <summary>
        /// Writes a message to the network
        /// </summary>
        public async Task SendMessageAsync(string header, byte[] data)
        {
            await Network.NetWriteString(header);
            var lenBytes = BitConverter.GetBytes(data.Length);
            await Network.WriteAsync(lenBytes, 0, lenBytes.Length);
            await Network.WriteAsync(data, 0, data.Length);
        }

        /// <summary>
        /// Writes a message to the network with a specified header and text content
        /// </summary>
        public async Task SendMessageAsync(string header, string content)
        {
            await SendMessageAsync(header, Encoding.UTF8.GetBytes(content));
        }

        /// <summary>
        /// Writes an empty message to the network with the specified header
        /// </summary>
        public async Task SendMessageAsync(string header)
        {
            await SendMessageAsync(header, new byte[0]);
        }

        /// <summary>
        /// Synchronizes the network to prepare it for a protocol change.
        /// </summary>
        /// <returns>True if the stream was sucessfully synchronized</returns>
        public async Task<bool> Synchronize(CancellationToken token = default)
        {
            await SendMessageAsync("SYNC");

            while (!token.IsCancellationRequested)
            {
                var next = await ReadMessageAsync(token);

                if (token.IsCancellationRequested || next == null)
                    return false;

                if (next.Header == "SYNC")
                {
                    return true;
                }
                else
                {
                    PreviousMessages.Add(next);
                }
            }

            return false;
        }

        /// <summary>
        /// Disposes all relay messages received by this instance.
        /// </summary>
        public void Dispose()
        {
            if (PreviousMessages.Count > 0)
            {
                foreach (var msg in PreviousMessages)
                {
                    msg?.Dispose();
                }
            }

            if (!LeaveStreamOpen)
            {
                Network?.Dispose();
            }
        }
    }
}
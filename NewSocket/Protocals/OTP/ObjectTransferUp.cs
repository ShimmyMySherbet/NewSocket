using NewSocket.Core;
using NewSocket.Interfaces;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NewSocket.Protocals.OTP
{
    /*  POTOCAL:
     *
     *  [Byte]  Type
     *  [Ulong] ID
     *
     *  <- init->
     *
     *  [String] Channel;
     *  [long] ContentLength
     *
     *  <-body->
     *
     *  [long] TransferSize
     *  [Bytes...] TransferData
     *
     *
     */

    public class ObjectTransferUp : IMessageUp
    {
        public ulong MessageID { get; }

        public byte MessageType => 0;

        public BaseSocketClient Socket { get; }

        private Stream m_MessageContent;

        public string Channel { get; }

        public long RemainingBytes => m_MessageContent.Length - m_MessageContent.Position;

        private int m_MaxTransferSize => Socket.UpTransferSize;

        public bool Complete { get; private set; } = false;
        public bool WantsToWrite => true;

        /// <summary>
        /// Creates a message with a JSON serialized object
        /// </summary>
        /// <param name="id">Message ID</param>
        /// <param name="socket">Source Socket</param>
        /// <param name="channel">Channel Name</param>
        /// <param name="obj">Object to send</param>
        public ObjectTransferUp(ulong id, BaseSocketClient socket, string channel, object obj)
        {
            MessageID = id;
            Socket = socket;
            Channel = channel;
            var json = JsonConvert.SerializeObject(obj);
            m_MessageContent = new MemoryStream(Encoding.UTF8.GetBytes(json));
            m_Buffer = new byte[socket.UpBufferSize];
        }

        /// <summary>
        /// Transmits the provided stream to the remote socket
        /// </summary>
        /// <param name="id">Message ID</param>
        /// <param name="socket">Source Socket</param>
        /// <param name="channel">Channel Name</param>
        /// <param name="stream">Stream to transmit</param>
        public ObjectTransferUp(ulong id, BaseSocketClient socket, string channel, Stream stream)
        {
            MessageID = id;
            Socket = socket;
            Channel = channel;
            m_Buffer = new byte[socket.UpBufferSize];
            m_MessageContent = stream;
        }

        public void Dispose()
        {
            m_MessageContent?.Dispose();
        }

        private bool m_init = true;
        private byte[] m_Buffer;

        /*  POTOCAL:
        *
        *  [Byte]  Type
        *  [Ulong] ID
        *
        *  <- init->
        *
        *  [String] Channel;
        *  [int] ContentLength
        *
        *  <-body->
        *
        *  [long] TransferSize
        *  [Bytes...] TransferData
        *
        *
        */

        public async Task<bool> Write(Stream stream)
        {
            if (Complete)
            {
                throw new InvalidOperationException("The message has already been uploaded.");
            }
            //System.Console.WriteLine($"[{Thread.CurrentThread.ManagedThreadId} {Socket.Name}] [OTP Up] Start Write");
            if (m_init)
            {
                //System.Console.WriteLine($"[{Thread.CurrentThread.ManagedThreadId} {Socket.Name}] [OTP Up] Message Init; Write Headers");
                m_init = false;
                await stream.Write(Channel);
                await stream.Write(m_MessageContent.Length);
            }

            var transferSize = RemainingBytes < m_MaxTransferSize ? RemainingBytes : m_MaxTransferSize;
            //System.Console.WriteLine($"[{Thread.CurrentThread.ManagedThreadId} {Socket.Name}] [OTP Up] This transfer size: {transferSize}");
            await stream.Write(transferSize);
            var remaining = transferSize;
            //System.Console.WriteLine($"[{Thread.CurrentThread.ManagedThreadId} {Socket.Name}] [OTP Up] Writing Segment...");
            while (remaining > 0)
            {
                var segmentSize = remaining < m_Buffer.Length ? remaining : m_Buffer.Length;
                var read = await m_MessageContent.ReadAsync(m_Buffer, 0, (int)segmentSize, CancellationToken.None);
                await stream.WriteAsync(m_Buffer, 0, read, CancellationToken.None);
                remaining -= read;
            }
            //System.Console.WriteLine($"[{Thread.CurrentThread.ManagedThreadId} {Socket.Name}] [OTP Up] Segment Wrote, Wrote {transferSize}, Remaining: {RemainingBytes}/{m_MessageContent.Length}");
            Complete = RemainingBytes == 0;
            return Complete;
        }
    }
}
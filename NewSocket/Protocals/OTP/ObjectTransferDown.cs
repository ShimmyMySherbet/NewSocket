using NewSocket.Core;
using NewSocket.Interfaces;
using NewSocket.Models;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace NewSocket.Protocals.OTP
{
    public class ObjectTransferDown : IMessageDown
    {
        public ulong MessageID { get; }
        public BaseSocketClient Client { get; }

        public bool WantsToDispatch { get; private set; }

        public string Channel { get; private set; } = "";

        public byte MessageType => 0;

        private Stream? m_Read;
        private long m_BytesRead = -1;

        public long MessageSize { get; private set; } = 0;

        public long BytesRemaining => MessageSize - m_BytesRead;

        private byte[] m_Buffer;

        public ObjectTransferProtocal Protocal { get; }

        public bool Complete { get; private set; } = false;

        public ObjectTransferDown(ulong id, BaseSocketClient client, ObjectTransferProtocal otp)
        {
            MessageID = id;
            Client = client;
            Protocal = otp;
            m_Buffer = new byte[client.DownBufferSize];
        }

        private bool m_Init = true;

        public async Task<bool> Read(Stream stream, CancellationToken token)
        {
            if (Complete)
            {
                throw new InvalidOperationException("The message has already been sent.");
            }
            //System.Console.WriteLine($"[{Thread.CurrentThread.ManagedThreadId} {Client.Name}] [OTP Down] Start Read");
            if (m_Init)
            {
                m_Init = false;
                Channel = await stream.NetReadString();
                MessageSize = await stream.NetReadInt64();
                m_Read = new MarshalAllocMemoryStream((int)MessageSize);
                //System.Console.WriteLine($"[{Thread.CurrentThread.ManagedThreadId} {Client.Name}] [OTP Down] [Init] Total Message Size: {MessageSize}");
                m_BytesRead = 0;
            }

            var transferSize = await stream.NetReadInt64();
            var remaining = transferSize;
            //System.Console.WriteLine($"[{Thread.CurrentThread.ManagedThreadId} {Client.Name}] [OTP Down] Transfer Size: {transferSize}, Remaining: {BytesRemaining}");
            if (m_Read == null)
            {
                throw new InvalidOperationException();
            }

            while (remaining > 0)
            {
                var nextRead = remaining < m_Buffer.Length ? remaining : m_Buffer.Length;
                var read = await stream.ReadAsync(m_Buffer, 0, (int)nextRead);
                await m_Read.WriteAsync(m_Buffer, 0, read, token);
                m_BytesRead += read;
                remaining -= read;
            }

            //System.Console.WriteLine($"[{Thread.CurrentThread.ManagedThreadId} {Client.Name}] [OTP Down] Remaining bytes in message: {BytesRemaining}");
            Complete = BytesRemaining == 0;
            if (Complete)
            {
                //System.Console.WriteLine($"[{Thread.CurrentThread.ManagedThreadId} {Client.Name}] [OTP Down] Finished Message Download; send dispatch");
                WantsToDispatch = true;
            }
            return Complete;
        }

        public Task Dispatch()
        {
            WantsToDispatch = false;
            ThreadPool.QueueUserWorkItem(async (_) =>
            {
                if (m_Read == null)
                {
                    return;
                }

                try
                {
                    await Protocal.RaiseMessage(Channel, m_Read);
                }
                finally
                {
                    await m_Read.DisposeAsync();
                }
            });
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            if (WantsToDispatch)
            {
                WantsToDispatch = false;
                if (m_Read != null)
                {
                    m_Read.Dispose();
                }
            }
        }
    }
}
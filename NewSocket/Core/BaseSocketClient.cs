using NewSocket.Interfaces;
using NewSocket.Models;
using NewSocket.Models.Exceptions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

// TODO: Rename this project to something else like god dam, what a fucking stupid name "NewSocket", how origonal and creative

namespace NewSocket.Core
{
    public class BaseSocketClient : ISocketClient
    {
        public string Name { get; set; } = "Socket"; // for debug logging
        public Stream? UpStream { get; private set; }
        public Stream? DownStream { get; private set; }
        public int DownBufferSize { get; set; } = 1024 * 2;
        public int UpBufferSize { get; set; } = 1024 * 2;
        public int UpTransferSize { get; set; } = 1024 * 20;

        protected IDictionary<byte, IMessageProtocal> m_Protocals = new ConcurrentDictionary<byte, IMessageProtocal>();

        protected MessageCache m_Cache = new MessageCache();

        public IDAssigner MessageIDAssigner { get; } = new IDAssigner();

        protected IScheduler<IMessageUp> m_MessageScheduler = new RotaryScheduler<IMessageUp>();

        protected CancellationTokenSource? m_TokenSource;

        protected bool AllowPartialSocket { get; set; } = false;

        public T? GetProtocal<T>() where T : class, IMessageProtocal
        {
            var vals = m_Protocals.Values.OfType<T>();
            if (vals.Any())
            {
                return vals.First();
            }
            return null;
        }

        public T RegisterProtocal<T>(T instance) where T : IMessageProtocal
        {
            m_Protocals[instance.ID] = instance;
            return instance;
        }

        public void SetStream(ESocketStream direction, Stream stream)
        {
            if (direction == ESocketStream.Both)
            {
                UpStream = stream;
                DownStream = stream;
            }
            else if (direction == ESocketStream.Down)
            {
                DownStream = stream;
            }
            else if (direction == ESocketStream.Up)
            {
                UpStream = stream;
            }
        }

        public void Start()
        {
            if (m_TokenSource != null)
            {
                m_TokenSource.Cancel();
            }
            m_TokenSource = new CancellationTokenSource();

            if (UpStream == null && DownStream == null)
            {
                throw new MissingStreamException(ESocketStream.Both);
            }
            else if (!AllowPartialSocket)
            {
                if (UpStream == null)
                {
                    throw new MissingStreamException(ESocketStream.Up);
                }
                if (DownStream == null)
                {
                    throw new MissingStreamException(ESocketStream.Down);
                }
            }
            if (UpStream != null)
            {
                Task.Run(() => MessageUpload(UpStream, m_TokenSource.Token), m_TokenSource.Token);
            }
            if (DownStream != null)
            {
                Task.Run(() => MessageDownload(DownStream, m_TokenSource.Token), m_TokenSource.Token);
            }
        }

        public void Stop()
        {
            if (m_TokenSource != null)
            {
                m_TokenSource.Cancel();
            }
        }

        public BaseSocketClient(Stream network)
        {
            UpStream = network;
            DownStream = network;
        }

        public BaseSocketClient(Stream up, Stream down)
        {
            UpStream = up;
            DownStream = down;
        }

        public BaseSocketClient()
        {
        }

        public void Enqueue(IMessageUp message)
        {
            m_MessageScheduler.Enqueue(message);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        protected async Task MessageUpload(Stream network, CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                //Cout.Write($"{Name} Up", "Get Next Message");

                var message = await m_MessageScheduler.GetNext(token);
                //Cout.Write($"{Name} Up", $"Got message of ID {message.MessageID}; write type byte");
                network.WriteByte(message.MessageType);
                //Cout.Write($"{Name} Up", "Write Message ID");
                await network.Write(message.MessageID);
                //Cout.Write($"{Name} Up", "Send Protocal Write");
                bool complete = await message.Write(network);
                //Cout.Write($"{Name} Up", $"Protocal Write finished, Message complete: {complete}");
                if (complete)
                {
                    //Cout.Write($"{Name} Up", "Messaged completed, sending finalize...");
                    m_MessageScheduler.Finalize(message);
                }
                //Cout.Write($"{Name} Up", "Checking Token Throw...");
                token.ThrowIfCancellationRequested();
            }
        }

        /*
         *     Format:
         *     [Byte]      Message Type
         *     [ULong]      Message ID
         *
         *     [Bytes..]   Protocal message body
         */

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        protected async Task MessageDownload(Stream network, CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                using (var headerStream = new MemoryStream())
                {
                    //while (!token.IsCancellationRequested)
                    //{
                    //Cout.Write($"{Name} Down", "Start read of next message");
                    var messageType = network.NetReadByte();
                    //Cout.Write($"{Name} down", $"Next Message Type Byte: {messageType}");
                    token.ThrowIfCancellationRequested();
                    var messageID = await network.NetReadUInt64();
                    //Cout.Write($"{Name} down", $"Next Message ID: {messageID}");

                    var newMessage = m_Cache.IsNewMessage(messageID);
                    //Cout.Write($"{Name} down", $"Is New Message: {newMessage}");

                    IMessageDown? down;
                    if (newMessage)
                    {
                        down = await m_Protocals[messageType].CreateDown(messageID, this);
                        m_Cache.Register(down);
                    }
                    else if (!m_Cache.TryGetDownload(messageID, out down) || down == null)
                    {
                        //Cout.Write($"{Name} down", $"Failed to get message");
                        throw new Exception("Message ID was known, but couldn't be retrived.");
                    }
                    //Cout.Write($"{Name} down", $"Has IMessageDown");

                    token.ThrowIfCancellationRequested();
                    //Cout.Write($"{Name} down", $"Running Protocal Read...");

                    var complete = await down.Read(network, token);
                    //Cout.Write($"{Name} down", $"Protocal Read complete. Message complete: {complete}, Wants Dispatch: {down.WantsToDispatch}");

                    if (down.WantsToDispatch)
                    {
                        //Cout.Write($"{Name} down", $"Sending Protocal Message Dispatch");

                        await down.Dispatch();
                    }
                    if (complete)
                    {
                        //Cout.Write($"{Name} down", $"Destroying MessageDown (Message is complete)");

                        m_Cache.Destroy(down);
                    }
                    //Cout.Write($"{Name} down", $"Check Cancellation State...");

                    token.ThrowIfCancellationRequested();
                    //}
                }
            }
        }
    }
}
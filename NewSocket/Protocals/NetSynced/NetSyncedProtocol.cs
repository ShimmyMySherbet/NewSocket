using System;
using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Threading.Tasks;
using NewSocket.Core;
using NewSocket.Interfaces;
using NewSocket.Models;
using NewSocket.Protocals.NetSynced.Models;

namespace NewSocket.Protocals.NetSynced
{
    public class NetSyncedProtocol : IMessageProtocal
    {
        public byte ID => 2;

        private ConcurrentDictionary<ulong, NetSyncedStream> m_StreamsNetID = new ConcurrentDictionary<ulong, NetSyncedStream>();
        private ConcurrentDictionary<ulong, TaskCompletionSource<NetSyncedStream>> m_StreamAwaiters = new ConcurrentDictionary<ulong, TaskCompletionSource<NetSyncedStream>>();
        private ISocketClient m_SocketClient { get; }

        public int DefaultMaxBufferLength { get; set; } = 1024 * 1024 * 16;

        public NetSyncedProtocol(ISocketClient socketClient)
        {
            m_SocketClient = socketClient;
        }

        public NetSyncedStream? GetExistingOrNull(ulong netID)
        {
            if (m_StreamsNetID.TryGetValue(netID, out var obj))
            {
                return obj;
            }
            return null;
        }

        public NetSyncedStream GetOrCreateDown(ulong netID, bool readable, bool writable, bool requireStart)
        {
            if (m_StreamsNetID.TryGetValue(netID, out var stream))
            {
                if (m_StreamAwaiters.TryGetValue(netID, out var handle2))
                {
                    if (!handle2.Task.IsCompleted)
                    {
                        handle2.SetResult(stream);
                    }
                }
                return stream;
            }

            NetSyncedDownBuffer? down = null;
            NetSyncedUpBuffer? up = null;

            if (readable)
            {
                down = new NetSyncedDownBuffer();
            }

            if (writable)
            {
                up = new NetSyncedUpBuffer();
            }

            stream = new NetSyncedStream(netID, requireStart, down, up);
            stream.MarkInitComplete();
            stream.MaxNetworkBuffer = DefaultMaxBufferLength;
            m_StreamsNetID[netID] = stream;

            m_SocketClient.Enqueue(new NetSyncedUp(stream, false));

            if (m_StreamAwaiters.TryGetValue(netID, out var handle))
            {
                if (!handle.Task.IsCompleted)
                {
                    handle.SetResult(stream);
                }
            }

            return stream;
        }

        private ulong AssignRandomNetID()
        {
            var buffer = new byte[8];
            using (var rng = RandomNumberGenerator.Create())
                rng.GetBytes(buffer);

            return BitConverter.ToUInt64(buffer, 0);
        }

        public Task<IMessageDown> CreateDown(ulong messageID, BaseSocketClient client)
        {
            return Task.FromResult<IMessageDown>(new NetSyncedDown(messageID, this));
        }

        public NetSyncedStream CreateStream(bool readable, bool writable, bool requireStart = true)
        {
            var id = AssignRandomNetID();

            var stream = GetOrCreateDown(id, readable, writable, requireStart);

            m_StreamsNetID[id] = stream;

            m_SocketClient.Enqueue(new NetSyncedUp(stream));

            return stream;
        }

        public async Task<NetSyncedStream> GetStream(ulong streamID)
        {
            if (m_StreamsNetID.TryGetValue(streamID, out var val))
            {
                return val;
            }

            if (m_StreamAwaiters.TryGetValue(streamID, out var awaiter))
            {
                return await awaiter.Task;
            }

            var waiter = new TaskCompletionSource<NetSyncedStream>();
            m_StreamAwaiters[streamID] = waiter;
            return await waiter.Task;
        }

        public Task OnSocketDisconnect(DisconnectContext context)
        {
            return Task.CompletedTask;
        }
    }
}
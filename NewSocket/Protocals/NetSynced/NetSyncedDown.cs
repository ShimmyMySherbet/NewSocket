using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using NewSocket.Interfaces;

namespace NewSocket.Protocals.NetSynced
{
    public class NetSyncedDown : IMessageDown
    {
        public bool WantsToDispatch { get; private set; }
        public ulong MessageID { get; }
        public byte MessageType => 2;
        public bool Complete { get; private set; }

        public NetSyncedProtocol NetSynced { get; }

        public Task Dispatch()
        {
            return Task.CompletedTask;
        }

        public void Dispose()
        {
        }

        public async Task<bool> Read(Stream stream, CancellationToken token)
        {
            var type = stream.NetReadByte();

            if (type == 0)
            {
                var NetSyncedID = await stream.NetReadUInt64();
                var writable = await stream.NetReadBool();
                var readable = await stream.NetReadBool();
                NetSynced.GetOrCreateDown(NetSyncedID, readable, writable);
                return false;
            }
            else
            {
                var netID = await stream.NetReadUInt64();
                var isOpen = await stream.NetReadBool();

                var netSyncedStream = NetSynced.GetOrCreateDown(netID, true, false);

                if (!isOpen)
                {
                    netSyncedStream.Dispose();
                    return true;
                }

                var blockLength = await stream.NetReadInt64();

                if (blockLength == 0)
                {
                    return false;
                }

                if (netSyncedStream.DownBuffer == null)
                {
                    // Discard
                    await stream.ConsumeBytes(blockLength);
                }
                else
                {
                    await netSyncedStream.DownBuffer.ReceiveBlockAsync(stream, (int)blockLength);
                }

                return false;
            }
        }

        public NetSyncedDown(ulong messageID, NetSyncedProtocol protocol)
        {
            MessageID = messageID;
            NetSynced = protocol;
        }
    }
}
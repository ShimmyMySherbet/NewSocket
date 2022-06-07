using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using NewSocket.Interfaces;
using NewSocket.Protocals.NetSynced.Models;

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

        /*
         * [NetSyncedID] : ulong
         *
         * [Type] : Byte
         *
         * <Type>
         *      0: Init Message
         *        - [Readable] : bool
         *        - [Writable] : bool
         *        - [RequireStart] : bool
         *        #return <false>
         *
         *      1: Data Block
         *        - [Data Length] : int
         *        - [Data Block] : Bytes(Data Length)
         *        #return <false>
         *
         *      2: Stream Close
         *        #return <true>
         *      
         *      3: Stream Start
         *        #return <false>
         *      
         *      4: Invalid State
         *        #return <false>
         *        
         *      5+: # Stream-Desync
         *
         *
         */

        public async Task<bool> Read(Stream stream, CancellationToken token)
        {
            var netSyncedID = await stream.NetReadUInt64();

            var type = stream.NetReadByte();

            NetSyncedStream? netSyncedStream;

            switch (type)
            {
                case 0:
                    var writable = await stream.NetReadBool();
                    var readable = await stream.NetReadBool();
                    var requireStart = await stream.NetReadBool();
                    NetSynced.GetOrCreateDown(netSyncedID, readable, writable, requireStart);
                    return false;

                case 1:

                    var blockLength = await stream.NetReadInt32();

                    if (blockLength == 0)
                    {
                        return false;
                    }

                    netSyncedStream = NetSynced.GetExistingOrNull(netSyncedID);
                    if (netSyncedStream == null || netSyncedStream.DownBuffer == null)
                    {
                        await stream.ConsumeBytes(blockLength);
                    }
                    else
                    {
                        await netSyncedStream.DownBuffer.ReceiveBlockAsync(stream, blockLength);
                    }

                    return false;

                case 2:
                    netSyncedStream = NetSynced.GetExistingOrNull(netSyncedID);
                    if (netSyncedStream != null)
                    {
                        netSyncedStream.Dispose();
                    }
                    return true;

                case 3:
                    netSyncedStream = NetSynced.GetExistingOrNull(netSyncedID);
                    netSyncedStream?.MarkRemoteReady();

                    return false;

                case 4:
                    return false;

                default:
                    Debug.WriteLine("Possible desync in NetSycnedDown");
                    return true;
            }
        }

        public NetSyncedDown(ulong messageID, NetSyncedProtocol protocol)
        {
            MessageID = messageID;
            NetSynced = protocol;
        }
    }
}
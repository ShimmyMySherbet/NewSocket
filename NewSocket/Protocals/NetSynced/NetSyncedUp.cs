using System.IO;
using System.Threading.Tasks;
using NewSocket.Interfaces;
using NewSocket.Protocals.NetSynced.Models;

namespace NewSocket.Protocals.NetSynced
{
    public class NetSyncedUp : IMessageUp
    {
        public ulong MessageID { get; }
        public byte MessageType => 2;
        public bool Complete { get; private set; }

        public ulong NetSyncedID { get; private set; }

        public NetSyncedUpBuffer? UpBuffer { get; }
        public bool WantsToWrite => (UpBuffer?.HasBlocks ?? false) || (UpBuffer?.IsClosed ?? false) || !IsInited;

        public bool IsInited { get; private set; } = false;

        public bool Writable { get; }
        public bool Readable { get; }

        public void Dispose()
        {
        }

        public async Task<bool> Write(Stream stream)
        {
            if (!IsInited)
            {
                stream.WriteByte(1);
                await stream.Write(NetSyncedID);
                await stream.Write(Readable);
                await stream.Write(Writable);
                IsInited = true;
                return UpBuffer == null;
            }
            else
            {
                stream.WriteByte(0);
            }

            if (UpBuffer == null)
            {
                await stream.Write(false);
                return true;
            }

            await stream.Write(NetSyncedID);

            var block = UpBuffer.GetBlock();

            if (block == null && UpBuffer.IsClosed)
            {
                await stream.Write(false);
                return true;
            }
            else
            {
                await stream.Write(true);
            }
            if (block == null || block.Length == 0)
            {
                await stream.Write((long)0);
                return false;
            }

            await block.Write(block.Length);
            await block.CopyToAsync(stream);
            block.Dispose();
            return false;
        }

        public NetSyncedUp(ulong msgID, NetSyncedUpBuffer up)
        {
            MessageID = msgID;
            UpBuffer = up;
        }

        public NetSyncedUp(ulong msgID)
        {
            MessageID = msgID;
            UpBuffer = null;
        }
    }
}
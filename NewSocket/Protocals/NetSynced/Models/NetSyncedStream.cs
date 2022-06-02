using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NewSocket.Protocals.NetSynced.Models
{
    public class NetSyncedStream : Stream
    {
        public ulong NetSyncedID { get; }
        public NetSyncedStream(ulong netID, NetSyncedDownBuffer? down = null, NetSyncedUpBuffer? up = null)
        {
            CanRead = down != null;
            CanWrite = up != null;
            NetSyncedID = netID;
        }


        public override bool CanRead { get; }
        public override bool CanSeek => false;
        public override bool CanWrite { get; }
        public override long Length => -1;
        public override long Position { get; set; }

        public NetSyncedDownBuffer? DownBuffer { get; }
        public NetSyncedUpBuffer? UpBuffer { get; }

        public override void Flush()
        {
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (DownBuffer != null)
            {
                return DownBuffer.Read(buffer, offset, count);
            }
            throw new NotSupportedException("This NetSyncedStream does not support reading.");
        }

        public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            if (DownBuffer != null)
            {
                return await DownBuffer.ReadAsync(buffer, offset, count);
            }
            throw new NotSupportedException("This NetSyncedStream does not support reading.");
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException("NetSyncedStreams do not support seeking.");
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException("NetSyncedStreams do not support Length Setting.");
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            if (UpBuffer != null)
            {
                UpBuffer.Write(buffer, offset, count);
            }
            throw new NotSupportedException("This NetSyncedStream does not support reading.");
        }

        public override async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            if (UpBuffer != null)
            {
                await UpBuffer.WriteAsync(buffer, offset, count);
            }
            throw new NotSupportedException("This NetSyncedStream does not support reading.");
        }

        public void Dispose()
        {

        }

    }
}

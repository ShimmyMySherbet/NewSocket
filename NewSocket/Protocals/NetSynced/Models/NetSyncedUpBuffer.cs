using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using NewSocket.Models;

namespace NewSocket.Protocals.NetSynced.Models
{
    public class NetSyncedUpBuffer : Stream, IDisposable
    {
        private ConcurrentQueue<MarshalAllocMemoryStream> m_Blocks = new();
        public override bool CanRead => true;
        public override bool CanSeek => false;
        public override bool CanWrite => true;
        public override long Length => -1;
        public override long Position { get; set; }
        public bool HasBlocks => !m_Blocks.IsEmpty;
        public long AvailableBytes => (m_Blocks.Sum(x => x.Length));

        public bool IsClosed { get; private set; } = false;

        public override void Flush()
        {
        }

        public MarshalAllocMemoryStream? GetBlock()
        {
            if (m_Blocks.TryDequeue(out var newBlock))
            {
                Position += newBlock.Length;
                return newBlock;
            }
            return null;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException("Reading isn't supported on this buffer");
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException("Seeking isn't supported on this buffer");
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException("Seeking isn't supported on this buffer");
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            var netBuffer = new MarshalAllocMemoryStream(count);
            netBuffer.Write(buffer, offset, count);
            netBuffer.Position = 0;
            m_Blocks.Enqueue(netBuffer);
        }

        public new void Dispose()
        {
            IsClosed = true;
        }
    }
}
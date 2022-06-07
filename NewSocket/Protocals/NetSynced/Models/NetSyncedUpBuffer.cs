using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using NewSocket.Models;

namespace NewSocket.Protocals.NetSynced.Models
{
    public class NetSyncedUpBuffer : Stream, IDisposable
    {
        private ConcurrentQueue<MarshalAllocMemoryStream> m_Blocks = new();
        public override bool CanRead => false;
        public override bool CanSeek => false;
        public override bool CanWrite => SourceReplacement == null;
        public override long Length => -1;
        private long m_position = 0;

        public bool IsSourceReplaced => m_SourceReplacement != null;
        private AsyncWaitHandle m_FlushWait = new AsyncWaitHandle();

        public new async Task FlushAsync()
        {
            await m_FlushWait.WaitAsync();
        }

        public override void Flush()
        {
            m_FlushWait.Wait();
        }

        public override long Position
        {
            get => m_SourceReplacement != null ? m_SourceReplacement.Position : m_position;
            set
            {
                if (m_SourceReplacement != null)
                {
                    m_SourceReplacement.Position = value;
                    return;
                }
                m_position = value;
            }
        }

        public bool HasBlocks => !m_Blocks.IsEmpty;

        public bool HasAvailableData
        {
            get
            {
                if (IsSourceReplaced)
                {
                    return true;
                }

                return HasBlocks;
            }
        }

        public long AvailableBytes => (m_Blocks.Sum(x => x.Length));
        public bool IsClosed { get; private set; } = false;

        public bool ThrowDisposedExceptions { get; set; } = true;

        private Stream? m_SourceReplacement;

        public int BufferSize { get; set; } = 1024 * 1024;

        public Stream? SourceReplacement
        {
            get => m_SourceReplacement;
        }

        public void SetSourceReplacement(Stream stream)
        {
            if (IsSourceReplaced)
            {
                throw new InvalidOperationException("Stream source has already been replaced.");
            }
            m_SourceReplacement = stream;
        }

        public void MarkBlockWritten(long written)
        {
            if (IsSourceReplaced)
            {
                if (written == 0)
                {
                    m_FlushWait.Release();
                } else
                {
                    m_FlushWait.Reset();
                }
            }
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
            if (SourceReplacement != null)
            {
                throw new NotSupportedException("Writing isn't supported on a source replaced stream.");
            }

            if (IsClosed)
            {
                if (ThrowDisposedExceptions)
                {
                    throw new ObjectDisposedException("The net stream has been closed.");
                }
                else
                {
                    return;
                }
            }

            Debug.WriteLine($"Writing block of size {count} bytes");
            var netBuffer = new MarshalAllocMemoryStream(count);
            netBuffer.Write(buffer, offset, count);
            netBuffer.Position = 0;
            m_FlushWait.Reset();
            m_Blocks.Enqueue(netBuffer);
        }

        public new void Dispose()
        {
            IsClosed = true;
            m_FlushWait.Release();
        }

        public NetSyncedUpBuffer(Stream? source = null, int bufferSize = 1024 * 1024)
        {
            m_SourceReplacement = source;
            BufferSize = bufferSize;
        }
    }
}
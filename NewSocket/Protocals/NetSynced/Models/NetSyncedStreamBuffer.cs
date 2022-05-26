using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using NewSocket.Models;

namespace NewSocket.Protocals.NetSynced.Models
{
    public class NetSyncedStreamBuffer : Stream
    {
        private SemaphoreSlim m_BlockSemaphore = new(0);
        private ConcurrentQueue<MarshalAllocMemoryStream> m_Blocks = new();
        public MarshalAllocMemoryStream? CurrentBlock { get; private set; }
        public Stream? RedirectStream { get; private set; }
        public override bool CanRead => true;
        public override bool CanSeek => false;
        public override bool CanWrite => false;
        public override long Length => -1;
        public override long Position { get; set; }

        public void SetStreamRedirect(Stream stream)
        {
            if (RedirectStream != null)
            {
                throw new InvalidOperationException("Stream is already being redirected.");
            }
            RedirectStream = stream;
        }

        public async Task ReceiveBlockAsync(Stream network, int bufferLength)
        {
            var buffer = new byte[1024];
            var remainingBytes = bufferLength;

            if (RedirectStream != null)
            {
                while (remainingBytes > 0)
                {
                    var currentBlock = Math.Min(remainingBytes, buffer.Length);

                    var read = await network.ReadAsync(buffer, 0, currentBlock);

                    await RedirectStream.WriteAsync(buffer, 0, read);

                    remainingBytes -= read;
                }
                return;
            }

            var block = new MarshalAllocMemoryStream(bufferLength);

            while (remainingBytes > 0)
            {
                var currentBuffer = Math.Min(remainingBytes, buffer.Length);

                var read = await network.ReadAsync(buffer, 0, currentBuffer);

                await block.WriteAsync(buffer, 0, read);

                remainingBytes -= read;
            }

            m_Blocks.Enqueue(block);
            m_BlockSemaphore.Release();
        }

        public override void Flush()
        {
        }

        private async Task<MarshalAllocMemoryStream> WaitForBlockAsync()
        {
            if (CurrentBlock == null || CurrentBlock.Position >= CurrentBlock.Length)
            {
                await m_BlockSemaphore.WaitAsync();
                if (!m_Blocks.TryDequeue(out var cBlock))
                {
                    throw new InvalidDataException("Semaphore released when no blocks were available.");
                }
                CurrentBlock?.Dispose();
                CurrentBlock = cBlock;
            }
            return CurrentBlock;
        }

        private MarshalAllocMemoryStream WaitForBlock()
        {
            if (CurrentBlock == null || CurrentBlock.Position >= CurrentBlock.Length)
            {
                m_BlockSemaphore.Wait();
                if (!m_Blocks.TryDequeue(out var cBlock))
                {
                    throw new InvalidDataException("Semaphore released when no blocks were available.");
                }
                CurrentBlock?.Dispose();
                CurrentBlock = cBlock;
            }
            return CurrentBlock;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            var read = 0;
            while (count > read)
            {
                var block = WaitForBlock();
                var blockTransfer = Math.Min(block.RemainingLength, count - read);
                var blockRead = block.Read(buffer, offset + read, (int)blockTransfer);
                read += blockRead;

                if (blockRead < blockTransfer)
                {
                    return read;
                }
            }
            return read;
        }

        public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            var read = 0;
            while (count > read)
            {
                var block = await WaitForBlockAsync();
                var blockTransfer = Math.Min(block.RemainingLength, count - read);
                var blockRead = await block.ReadAsync(buffer, offset + read, (int)blockTransfer);
                read += blockRead;
                Position += read;
                if (blockRead < blockTransfer)
                {
                    return read;
                }
            }
            return read;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException("Seeking not supported in NetSyncedBuffers");
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException("Writing not supported in NetSyncedBuffers");
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException("Writing not supported in NetSyncedBuffers");
        }
    }
}
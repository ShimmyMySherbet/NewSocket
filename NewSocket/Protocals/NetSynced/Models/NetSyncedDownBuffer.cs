using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using NewSocket.Models;

namespace NewSocket.Protocals.NetSynced.Models
{
    public class NetSyncedDownBuffer : Stream, IDisposable
    {
        private SemaphoreSlim m_BlockSemaphore = new(0);
        private ConcurrentQueue<NetSyncedBlock> m_Blocks = new();
        public MarshalAllocMemoryStream? CurrentBlock { get; private set; }
        public Stream? RedirectStream { get; private set; }
        public override bool CanRead => true;
        public override bool CanSeek => false;
        public override bool CanWrite => false;
        public override long Length => -1;
        public override long Position { get; set; }

        public bool IsClosed { get; set; } = false;
        private CancellationTokenSource m_TokenSource = new CancellationTokenSource();
        public bool EndOfStream => (CurrentBlock != null ? CurrentBlock.Length == 0 : true) && m_Blocks.Count == 0 && IsClosed;

        /// <summary>
        /// Sets a redirect stream, and flushes any buffered data to it.
        /// The redirect stream will also be disposed when the NetSycnedStream is disposed.
        /// </summary>
        public async Task SetStreamRedirect(Stream stream)
        {
            if (RedirectStream != null)
            {
                throw new InvalidOperationException("Stream is already being redirected.");
            }

            while (m_Blocks.Count > 0)
            {
                if (m_Blocks.TryDequeue(out var block))
                {
                    if (block.Disposal || block.MemoryBlock == null)
                    {
                        stream.Dispose();
                        Dispose();
                        return;
                    }
                    else
                    {
                        await block.MemoryBlock.CopyToAsync(stream);
                        block.MemoryBlock.Dispose();
                    }
                }
            }

            RedirectStream = stream;
        }

        /// <summary>
        /// Alternate to <seealso cref="SetStreamRedirect(Stream)"/> that doesn't flush buffered data
        /// </summary>
        public void SetDownstream(Stream stream)
        {
            if (RedirectStream != null)
            {
                throw new InvalidOperationException("Stream is already being redirected.");
            }
            RedirectStream = stream;    
        }


        public new void Dispose()
        {
            IsClosed = true;
            RedirectStream?.Dispose();
            m_TokenSource.Cancel();

            m_Blocks.Enqueue(NetSyncedBlock.Dispose());
            m_BlockSemaphore.Release();
        }

        public async Task ReceiveBlockAsync(Stream network, int blockLength)
        {
            var buffer = new byte[1024];
            var remainingBytes = blockLength;

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

            var block = new MarshalAllocMemoryStream(blockLength);

            while (remainingBytes > 0)
            {
                var currentBuffer = Math.Min(remainingBytes, buffer.Length);

                var read = await network.ReadAsync(buffer, 0, currentBuffer);

                await block.WriteAsync(buffer, 0, read);

                remainingBytes -= read;
            }
            block.Position = 0;

            m_Blocks.Enqueue(NetSyncedBlock.Block(block));
            m_BlockSemaphore.Release();
        }

        public override void Flush()
        {
        }

        private async Task<MarshalAllocMemoryStream?> WaitForBlockAsync()
        {
            if (IsClosed && m_Blocks.Count == 0)
            {
                return null;
            }

            if (CurrentBlock != null && CurrentBlock.RemainingLength > 0)
            {
                return CurrentBlock;
            }

            if (m_BlockSemaphore.CurrentCount == 0 && IsClosed)
            {
                return null;
            }

            await m_BlockSemaphore.WaitAsync();

            if (!m_Blocks.TryDequeue(out var cBlock))
            {
                throw new InvalidDataException("Semaphore released when no blocks were available.");
            }

            if (cBlock.Disposal || cBlock.MemoryBlock == null)
            {
                return null;
            }

            CurrentBlock?.Dispose();
            CurrentBlock = cBlock.MemoryBlock;

            return CurrentBlock;
        }

        private MarshalAllocMemoryStream? WaitForBlock()
        {
            if (IsClosed && m_Blocks.Count == 0)
            {
                return null;
            }

            if (CurrentBlock != null && CurrentBlock.RemainingLength > 0)
            {
                return CurrentBlock;
            }

            if (m_BlockSemaphore.CurrentCount == 0 && IsClosed)
            {
                return null;
            }

            m_BlockSemaphore.Wait();

            if (!m_Blocks.TryDequeue(out var cBlock))
            {
                throw new InvalidDataException("Semaphore released when no blocks were available.");
            }

            if (cBlock.Disposal || cBlock.MemoryBlock == null)
            {
                return null;
            }

            CurrentBlock?.Dispose();
            CurrentBlock = cBlock.MemoryBlock;

            return CurrentBlock;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            var read = 0;
            bool streamEnded = false;
            while (count > read)
            {
                var block = WaitForBlock();
                if (block == null)
                {
                    return read;
                }
                var blockTransfer = Math.Min(block.RemainingLength, count - read);
                var blockRead = block.Read(buffer, offset + read, (int)blockTransfer);
                read += blockRead;
                Position += blockRead;

                streamEnded = IsClosed
                    && m_Blocks.IsEmpty
                    && CurrentBlock != null
                    && CurrentBlock.RemainingLength == 0;

                if (blockRead < blockTransfer)
                {
                    if (streamEnded)
                    {
                        CurrentBlock?.Dispose();
                        CurrentBlock = null;
                    }
                    return read;
                }
            }

            if (streamEnded)
            {
                CurrentBlock?.Dispose();
                CurrentBlock = null;
            }
            return read;
        }

        public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            var read = 0;
            bool streamEnded = false;

            while (count > read)
            {
                var block = await WaitForBlockAsync();
                if (block == null)
                {
                    return read;
                }
                var blockTransfer = Math.Min(block.RemainingLength, count - read);
                var blockRead = await block.ReadAsync(buffer, offset + read, (int)blockTransfer);
                read += blockRead;
                Position += read;

                streamEnded = IsClosed
                                    && m_Blocks.IsEmpty
                                    && CurrentBlock != null
                                    && CurrentBlock.RemainingLength == 0;

                if (blockRead < blockTransfer)
                {
                    if (streamEnded)
                    {
                        CurrentBlock?.Dispose();
                        CurrentBlock = null;
                    }
                    return read;
                }
            }

            if (streamEnded)
            {
                CurrentBlock?.Dispose();
                CurrentBlock = null;
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
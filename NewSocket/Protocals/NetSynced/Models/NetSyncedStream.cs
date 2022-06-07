using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using NewSocket.Models;

namespace NewSocket.Protocals.NetSynced.Models
{
    public class NetSyncedStream : Stream
    {
        public ulong NetSyncedID { get; }
        public bool RequireSync { get; }

        public bool Ready => !RequireSync || (LocalClientReady && RemoteClientReady);
        public bool NeedsToWriteSync => LocalClientReady && !m_LocalStateWritten && RequireSync;

        public bool LocalClientReady { get; private set; } = false;
        public bool RemoteClientReady { get; private set; } = false;

        private bool m_LocalStateWritten = false;
        private AsyncWaitHandle m_RemoteReadyWait = new AsyncWaitHandle();
        private AsyncWaitHandle m_InitWait = new AsyncWaitHandle();
        private AsyncWaitHandle m_StateWait = new AsyncWaitHandle();

        public async Task StartAsync()
        {
            LocalClientReady = true;
            if (RequireSync)
            {
                await m_StateWait.WaitAsync();
                await m_RemoteReadyWait.WaitAsync();
            }
        }

        public void Start()
        {
            StartNonBlocking();
            if (RequireSync)
            {
                m_StateWait.Wait();
                m_RemoteReadyWait.Wait();
            }
        }

        public void MarkStateWritten()
        {
            m_LocalStateWritten = true;
            m_StateWait.Release();
        }

        public void MarkRemoteReady()
        {
            RemoteClientReady = true;
            m_RemoteReadyWait.Release();
        }

        /// <summary>
        /// Alternate to <seealso cref="StartAsync"/> or <seealso cref="Start"/>. Starts the socket non-blocking
        /// </summary>
        public void StartNonBlocking()
        {
            LocalClientReady = true;
        }

        public void MarkInitComplete()
        {
            m_InitWait.Release();
        }

        /// <summary>
        /// The maximum bytes that can be allowed in the net buffer.
        /// When the limit is reached, the socket is suspended until there is more space
        /// </summary>
        public int MaxNetworkBuffer { get; set; }

        public NetSyncedStream(ulong netID, bool requireSync, NetSyncedDownBuffer? down = null, NetSyncedUpBuffer? up = null)
        {
            CanRead = down != null;
            CanWrite = up != null;
            UpBuffer = up;
            DownBuffer = down;
            NetSyncedID = netID;
            RequireSync = requireSync;
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
            UpBuffer?.Flush();
        }

        public new async Task FlushAsync()
        {
            if (UpBuffer != null)
            {
                await UpBuffer.FlushAsync();
            }
        }

        public async Task<NetSyncedStream> WaitForInitAsync()
        {
            await m_InitWait.WaitAsync();
            return this;
        }

        public async Task ReplaceDownstream(Stream stream)
        {
            if (DownBuffer != null)
            {
                await DownBuffer.SetStreamRedirect(stream);
            }
        }

        public void ReplaceUpstream(Stream stream)
        {
            if (UpBuffer != null)
            {
                UpBuffer.SetSourceReplacement(stream);
            }
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
                return;
            }
            throw new NotSupportedException("This NetSyncedStream does not support reading.");
        }

        public override async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            if (UpBuffer != null)
            {
                await UpBuffer.WriteAsync(buffer, offset, count);
                return;
            }
            throw new NotSupportedException("This NetSyncedStream does not support reading.");
        }

        public new void Dispose()
        {
            UpBuffer?.Dispose();
            DownBuffer?.Dispose();
            m_InitWait.Dispose();
            m_RemoteReadyWait.Dispose();
            m_StateWait.Dispose();
        }
    }
}
using System;
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

        public ulong NetSyncedID => Stream.NetSyncedID;

        public NetSyncedStream Stream { get; }
        public NetSyncedUpBuffer? UpBuffer => Stream?.UpBuffer;
        public bool WantsToWrite => ((UpBuffer?.HasAvailableData ?? false) && Stream.Ready) || Closed || !IsInited || Stream.NeedsToWriteSync;

        public bool IsInited { get; private set; } = false;

        public bool Writable => Stream.UpBuffer != null;
        public bool Readable => Stream.DownBuffer != null;

        private bool m_ManualClose = false;

        public bool Closed => (UpBuffer?.IsClosed ?? false) || m_ManualClose;

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
        private byte[] Buffer = new byte[0];

        public async Task<bool> Write(Stream stream)
        {
            await stream.Write(NetSyncedID);

            if (!IsInited)
            {
                // Init Message
                stream.WriteByte(0);
                await stream.Write(Readable);
                await stream.Write(Writable);
                await stream.Write(Stream.RequireSync);
                IsInited = true;
                Stream.MarkInitComplete();
                return false;
            }


            if (Stream.NeedsToWriteSync)
            {
                // State
                stream.WriteByte(3);
                Stream.MarkStateWritten();
                return false;
            }

            var bufferHasBlocks = UpBuffer != null && (UpBuffer.IsSourceReplaced ? !m_ManualClose : UpBuffer.HasAvailableData);

            if (Closed && !bufferHasBlocks)
            {
                // Close Message
                stream.WriteByte(2);
                return true;
            }

            if (UpBuffer == null)
            {
                // Invalid State
                stream.WriteByte(4);
                return false;
            }

            // Data Block

            stream.WriteByte(1);

            // Read from source
            if (UpBuffer.SourceReplacement != null)
            {
                if (Buffer.Length != UpBuffer.BufferSize)
                {
                    Buffer = new byte[UpBuffer.BufferSize]; // Re-use buffer to reduce memory pressure
                }

                int blockSize;

                try
                {
                    blockSize = await UpBuffer.SourceReplacement.ReadAsync(Buffer, 0, Buffer.Length);
                }
                catch (Exception)
                {
                    await stream.Write((int)0);
                    UpBuffer.Dispose();
                    m_ManualClose = true;
                    return false;
                }


                await stream.Write((int)blockSize);

                if (blockSize == 0)
                {
                    UpBuffer.Dispose();
                    m_ManualClose = true;
                    return false;
                }

                await stream.WriteAsync(Buffer, 0, blockSize);

                if (blockSize < Buffer.Length)
                {
                    UpBuffer.Dispose();
                    m_ManualClose = true;


                }

                return false;
            }

            var block = UpBuffer.GetBlock();

            if (block == null)
            {
                await stream.Write((int)0);
                return false;
            }
            await stream.Write((int)block.Length);

            block.Position = 0;
            await block.CopyToAsync(stream);
            block.Dispose();

            return false;
        }

        public void Dispose()
        {
        }

        public NetSyncedUp(NetSyncedStream stream, bool needsInit = true)
        {
            Stream = stream;
            IsInited = !needsInit;
        }
    }
}
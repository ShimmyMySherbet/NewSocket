using System;
using System.Diagnostics;
using System.IO;

namespace NewSocket.Security.Models
{
    /// <summary>
    /// For use in streaming set block size asymetric encryption
    /// </summary>
    public class SetBlockBufferStream : Stream
    {
        private Stream DataStream;
        public int UploadBlockSize { get; } = 64;
        public int DownloadBlockSize { get; } = 64;
        public override bool CanRead { get; } = true;
        public override bool CanSeek { get; } = false;
        public override bool CanWrite { get; } = true;
        public override long Length { get; } = -1;

        public override long Position
        {
            get => DataStream.Position;
            set => throw new NotSupportedException();
        }

        protected byte[] DownBuffer;
        protected ushort DownBufferIndex = 0;
        protected virtual int DownBufferLength { get; set; }
        private bool DownBufferNeedsInit = true;

        protected byte[] UpBuffer;
        protected ushort UpBufferIndex;
        protected virtual int UpBufferLength => UpBuffer.Length;


        public SetBlockBufferStream(Stream data)
        {
            DataStream = data;
            UpBuffer = new byte[UploadBlockSize];
            DownBuffer = new byte[DownloadBlockSize];
        }

        public override void Flush()
        {
        }


        protected virtual void GetNextDownBlock()
        {
            Debug.WriteLine("Read next block");
            var r = DataStream.Read(DownBuffer, 0, DownloadBlockSize);
            DownBufferIndex = 0;
            DownBufferNeedsInit = false;
            DownBufferLength = r;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (DownBufferNeedsInit)
            {
                GetNextDownBlock();
            }

            int wrote = 0;

            while (true)
            {
                var cBufCanRead = DownloadBlockSize - DownBufferLength;
                var shouldRead = Math.Min(cBufCanRead, count - wrote);
                Buffer.BlockCopy(DownBuffer, DownBufferIndex, buffer, offset + wrote, shouldRead);
                wrote += shouldRead;
                DownBufferIndex += (ushort)shouldRead;

                if (wrote >= count)
                {
                    return wrote;
                }

                if (DownBufferIndex >= DownloadBlockSize)
                {
                    GetNextDownBlock();
                }
            }
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return Position;
        }

        public override void SetLength(long value)
        {
        }

        protected virtual void PushCurrentBuffer()
        {
            Debug.WriteLine("Push buffer");
            DataStream.Write(UpBuffer, 0, UploadBlockSize);
            UpBufferIndex = 0;
        }

        public virtual void FlushBufferToLegalSize(byte flushByte = 0x0)
        {
            var remBytes = UploadBlockSize - UpBufferIndex;
            for (int i = 0; i < remBytes; i++)
            {
                UpBuffer[UpBufferIndex + i] = flushByte;
            }
            PushCurrentBuffer();
        }

        public virtual void FlushPartialBuffer()
        {
            DataStream.Write(UpBuffer, 0, UpBufferIndex);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            int remaining = count;
            var wrote = 0;
            while (true)
            {
                var blockSizeRemaining = UpBufferLength - UpBufferIndex;

                var shouldWrite = Math.Min(blockSizeRemaining, remaining);

                Buffer.BlockCopy(buffer, offset + wrote, UpBuffer, UpBufferIndex, shouldWrite);

                UpBufferIndex += (ushort)shouldWrite;
                remaining -= shouldWrite;
                wrote += shouldWrite;
                if (UpBufferIndex >= UpBufferLength)
                {
                    PushCurrentBuffer();
                }

                if (remaining == 0)
                {
                    break;
                }
            }
        }
    }
}
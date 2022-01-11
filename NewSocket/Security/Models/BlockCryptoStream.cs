using System;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace NewSocket.Security.Models
{
    // Seems to cause a significant performance decrease when running with
    // the debugger attached.
    public class BlockCryptoStream : Stream
    {
        public override bool CanRead => DataStream.CanRead;
        public override bool CanSeek => false;
        public override bool CanWrite => DataStream.CanWrite;
        public override long Length => DataStream.Length;

        public override long Position
        {
            get => DataStream.Position;
            set => throw new NotSupportedException("Seeking is not supported for this stream");
        }

        public virtual ushort MaxLegalDownBlockSize => 1024 * 4;
        public virtual ushort MaxLegalUpBlockSize => 1024 * 4;

        public virtual ushort PreferedUpBlockSize => 1024 * 3;

        public Stream DataStream { get; }

        // Decrypted data read from stream
        protected byte[] DownBuffer;

        protected int DownBufferLength = 0;
        protected int DownBufferIndex = 0;

        // Unencrypted data to be encrypted and wrote in block
        protected byte[] UpBuffer;

        protected int UpBufferIndex = 0;

        protected byte[] UpBufferTemp;
        protected byte[] DownBufferTemp;

        protected Func<byte[], byte[]> m_Encrypt;
        protected Func<byte[], byte[]> m_Decrypt;

        public BlockCryptoStream(Stream data, Func<byte[], byte[]> encrypt, Func<byte[], byte[]> decrypt)
        {
            DataStream = data;
            DownBuffer = new byte[MaxLegalDownBlockSize];
            UpBuffer = new byte[PreferedUpBlockSize];
            UpBufferTemp = new byte[0];
            DownBufferTemp = new byte[0];
            m_Encrypt = encrypt;
            m_Decrypt = decrypt;
        }

        public byte[] Encrypt(byte[] buffer)
        {
#if DEBUG
            var sw = new Stopwatch();
            sw.Start();
            var b = m_Encrypt(buffer);
            sw.Stop();
            Debug.WriteLine($"[TIMER] <> [{Math.Round(sw.ElapsedTicks / 10000f, 2)}] Encrypt took {sw.ElapsedTicks} / {sw.ElapsedTicks / 10000f}ms");
            return b;
#else
            return m_Encrypt(buffer);
#endif
        }

        public byte[] Decrypt(byte[] buffer)
        {
#if DEBUG

            var sw = new Stopwatch();
            sw.Start();
            var b = m_Decrypt(buffer);
            sw.Stop();
            Debug.WriteLine($"[TIMER] <> [{Math.Round(sw.ElapsedTicks / 10000f, 2)}] Decrypt took {sw.ElapsedTicks} / {sw.ElapsedTicks / 10000f}ms");
            return b;

            Debug.WriteLine($"Decrypt buffer of length {buffer.Length}");
            return m_Decrypt(buffer);
#else

            return m_Decrypt(buffer);

#endif
        }

        public void GetNextDownBuffer()
        {
#if DEBUG

            var sw = new Stopwatch();
            sw.Start();
            try
            {
#endif
                var sizeBuffer = new byte[2];
                DataStream.Read(sizeBuffer, 0, 2);

                var cBlockSize = BitConverter.ToUInt16(sizeBuffer, 0);

                if (cBlockSize > MaxLegalDownBlockSize)
                {
                    throw new InvalidDataException("Remote party sent an invalid block size, or the input data is in an invalid format");
                }
                else if (cBlockSize == 0)
                {
                    DownBufferLength = 0;
                    DownBufferIndex = 0;
                    return;
                }

                // Most of the time encrypted block sizes will be the same,
                // so try to re-use the buffer to reduce memory usage
                if (DownBufferTemp == null || DownBuffer.Length != cBlockSize)
                {
                    DownBufferTemp = new byte[cBlockSize];
                }


                DataStream.Read(DownBufferTemp, 0, cBlockSize);


                // .NET 4.8 and Net Standard's RSA only allows for buffer in buffer out, so no way around creating a new buffer every time.
                var decrypted = Decrypt(DownBufferTemp);

                DownBufferLength = decrypted.Length;
                DownBufferIndex = 0;
                Buffer.BlockCopy(decrypted, 0, DownBuffer, 0, DownBufferLength);
#if DEBUG

            }
            finally
            {
                sw.Stop();
                Debug.WriteLine($"[TIMER] <> [{Math.Round(sw.ElapsedTicks / 10000f, 2)}] Pull Block took {sw.ElapsedTicks}");
            }
#endif
        }

        public void PushCurrentBlock()
        {
#if DEBUG

            var sw = new Stopwatch();
            sw.Start();
            try
            {
#endif
                if (UpBufferIndex == 0)
                    return;

                if (UpBufferTemp == null || UpBufferTemp.Length != UpBufferIndex)
                {
                    UpBufferTemp = new byte[UpBufferIndex];
                }

                Buffer.BlockCopy(UpBuffer, 0, UpBufferTemp, 0, UpBufferIndex);
                var encrypted = Encrypt(UpBufferTemp);
                DataStream.Write(BitConverter.GetBytes((ushort)encrypted.Length), 0, 2);
                DataStream.Write(encrypted, 0, encrypted.Length);
                UpBufferIndex = 0;
#if DEBUG

            }
            finally
            {
                Debug.WriteLine($"[TIMER] <> [{Math.Round(sw.ElapsedTicks / 10000f, 2)}] Push sync took {sw.ElapsedTicks} {sw.ElapsedTicks / 10000f}ms");
            }
#endif
        }

        public async Task PushCurrentBlockAsync()
        {
#if DEBUG

            var sw = new Stopwatch();
            sw.Start();
            try
            {
#endif
                if (UpBufferIndex == 0)
                    return;

                if (UpBufferTemp == null || UpBufferTemp.Length != UpBufferIndex)
                {
                    UpBufferTemp = new byte[UpBufferIndex];
                }

                Buffer.BlockCopy(UpBuffer, 0, UpBufferTemp, 0, UpBufferIndex);
                var encrypted = Encrypt(UpBufferTemp);
                await DataStream.WriteAsync(BitConverter.GetBytes((ushort)encrypted.Length), 0, 2);
                await DataStream.WriteAsync(encrypted, 0, encrypted.Length);
                UpBufferIndex = 0;
#if DEBUG

            }
            finally
            {
                sw.Stop();
                Debug.WriteLine($"[TIMER] <> [{Math.Round(sw.ElapsedTicks / 10000f, 2)}] PushAsync took {sw.ElapsedTicks} {sw.ElapsedTicks / 10000f}");
            }
#endif
        }

        public override void Flush()
        {
            DataStream.Flush();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
#if DEBUG
            var sw = new Stopwatch();
            sw.Start();
            try
            {
#endif
                if (DownBufferLength == 0 || DownBufferIndex >= DownBufferLength)
                {
                    GetNextDownBuffer();
                    if (DownBufferLength == 0)
                    {
                        return 0;
                    }
                }

                int wrote = 0;

                while (true)
                {
                    var bufferRem = DownBufferLength - DownBufferIndex;

                    if (bufferRem == 0)
                    {
                        GetNextDownBuffer();
                        if (DownBufferLength == 0)
                        {
                            return wrote;
                        }
                        continue;
                    }

                    var remByts = count - wrote;

                    var cblock = Math.Min(bufferRem, remByts);

                    Buffer.BlockCopy(DownBuffer, DownBufferIndex, buffer, offset + wrote, cblock);
                    DownBufferIndex += cblock;
                    wrote += cblock;

                    if (wrote >= count)
                    {
                        return wrote;
                    }
                }
#if DEBUG

            }
            finally
            {
                sw.Stop();
                Debug.WriteLine($"[TIMER] <> [{Math.Round(sw.ElapsedTicks / 10000f, 2)}] Read {count} bytes took {sw.ElapsedTicks / 10000f}");
            }
#endif
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return 0;
        }

        public override void SetLength(long value)
        {
        }

        public override void Write(byte[] buffer, int offset, int count)
        {

#if DEBUG
            var sw = new Stopwatch();
            sw.Start();
            try
            {
#endif
                int remaining = count;
                var wrote = 0;
                while (true)
                {
                    var blockSizeRemaining = UpBuffer.Length - UpBufferIndex;

                    var shouldWrite = Math.Min(blockSizeRemaining, Math.Min(remaining, PreferedUpBlockSize));

                    Buffer.BlockCopy(buffer, offset + wrote, UpBuffer, UpBufferIndex, shouldWrite);

                    UpBufferIndex += shouldWrite;
                    remaining -= shouldWrite;
                    wrote += shouldWrite;

                    if (UpBufferIndex >= PreferedUpBlockSize)
                    {
                        PushCurrentBlock();
                    }

                    if (remaining == 0)
                    {
                        break;
                    }
                }
#if DEBUG
            }
            finally
            {
                Debug.WriteLine($"[TIMER] <> [{Math.Round(sw.ElapsedTicks / 10000f, 2)}] Write {count} bytes took {sw.ElapsedTicks} {sw.ElapsedTicks / 10000f}ms");
            }
#endif
        }
    }
}
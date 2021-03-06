using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NewSocket
{
    public static class NetworkExtensions
    {
        public static byte NetReadByte(this Stream stream)
        {
            var byt = stream.ReadByte();
            if (byt == -1)
                throw new InvalidOperationException("No data could be read from the stream");
            return (byte)byt;
        }

        public static async Task<short> NetReadInt16(this Stream stream)
        {
            var buffer = new byte[2];
            await stream.NetReadAsync(buffer);
            return BitConverter.ToInt16(buffer, 0);
        }

        public static async Task<ushort> NetReadUInt16(this Stream stream)
        {
            var buffer = new byte[2];
            await stream.NetReadAsync(buffer);
            return BitConverter.ToUInt16(buffer, 0);
        }

        public static async Task<int> NetReadInt32(this Stream stream, CancellationToken token = default)
        {
            var buffer = new byte[4];
            await stream.NetReadAsync(buffer, token);
            if (token.IsCancellationRequested)
                return 0;
            return BitConverter.ToInt32(buffer, 0);
        }

        public static async Task<uint> NetReadUInt32(this Stream stream, CancellationToken token = default)
        {
            var buffer = new byte[4];
            await stream.NetReadAsync(buffer, token);
            return BitConverter.ToUInt32(buffer, 0);
        }

        public static async Task<long> NetReadInt64(this Stream stream)
        {
            var buffer = new byte[8];
            await stream.NetReadAsync(buffer);
            return BitConverter.ToInt64(buffer, 0);
        }

        public static async Task<ulong> NetReadUInt64(this Stream stream)
        {
            var buffer = new byte[8];
            await stream.NetReadAsync(buffer);
            return BitConverter.ToUInt64(buffer, 0);
        }

        public static async Task<float> NetReadFloat(this Stream stream)
        {
            var buffer = new byte[8];
            await stream.NetReadAsync(buffer);
            return BitConverter.ToSingle(buffer, 0);
        }

        public static async Task<double> NetReadDouble(this Stream stream)
        {
            var buffer = new byte[8];
            await stream.NetReadAsync(buffer);
            return BitConverter.ToDouble(buffer, 0);
        }

        public static async Task<char> NetReadChar(this Stream stream)
        {
            var buffer = new byte[2];
            await stream.NetReadAsync(buffer);
            return BitConverter.ToChar(buffer, 0);
        }

        public static async Task<string> NetReadString(this Stream stream, uint length)
        {
            var buffer = new byte[length];
            await stream.NetReadAsync(buffer);
            return Encoding.UTF8.GetString(buffer);
        }

        public static async Task NetWriteString(this Stream stream, string content)
        {
            var buf = Encoding.UTF8.GetBytes(content);
            var lenByt = BitConverter.GetBytes((uint)buf.Length);
            await stream.WriteAsync(lenByt, 0, lenByt.Length);
            await stream.WriteAsync(buf, 0, buf.Length);
        }

        public static async Task<string> NetReadString(this Stream stream, CancellationToken token = default)
        {
            var length = await stream.NetReadUInt32(token);
            if (token.IsCancellationRequested)
                return string.Empty;
            var buffer = new byte[length];
            await stream.NetReadAsync(buffer, token);
            if (token.IsCancellationRequested)
                return string.Empty;
            return Encoding.UTF8.GetString(buffer);
        }

        public static async Task<bool> NetReadBool(this Stream stream)
        {
            var buffer = new byte[1];
            await stream.NetReadAsync(buffer);
            return BitConverter.ToBoolean(buffer, 0);
        }

        public static async Task Write(this Stream stream, short value)
        {
            var buffer = BitConverter.GetBytes(value);
            await stream.WriteAsync(buffer, 0, buffer.Length);
        }

        public static async Task Write(this Stream stream, ushort value)
        {
            var buffer = BitConverter.GetBytes(value);
            await stream.WriteAsync(buffer, 0, buffer.Length);
        }

        public static async Task Write(this Stream stream, int value)
        {
            var buffer = BitConverter.GetBytes(value);
            await stream.WriteAsync(buffer, 0, buffer.Length);
        }

        public static async Task Write(this Stream stream, uint value)
        {
            var buffer = BitConverter.GetBytes(value);
            await stream.WriteAsync(buffer, 0, buffer.Length);
        }

        public static async Task Write(this Stream stream, long value)
        {
            var buffer = BitConverter.GetBytes(value);
            await stream.WriteAsync(buffer, 0, buffer.Length);
        }

        public static async Task Write(this Stream stream, ulong value)
        {
            var buffer = BitConverter.GetBytes(value);
            await stream.WriteAsync(buffer, 0, buffer.Length);
        }

        public static async Task Write(this Stream stream, float value)
        {
            var buffer = BitConverter.GetBytes(value);
            await stream.WriteAsync(buffer, 0, buffer.Length);
        }

        public static async Task Write(this Stream stream, double value)
        {
            var buffer = BitConverter.GetBytes(value);
            await stream.WriteAsync(buffer, 0, buffer.Length);
        }

        public static async Task Write(this Stream stream, bool value)
        {
            var buffer = BitConverter.GetBytes(value);
            await stream.WriteAsync(buffer, 0, buffer.Length);
        }

        public static async Task Write(this Stream stream, string? value, bool includeHeader = true)
        {
            var upBuffer = Encoding.UTF8.GetBytes(value ?? "");
            if (includeHeader)
            {
                await stream.Write((uint)upBuffer.Length);
            }
            await stream.WriteAsync(upBuffer, 0, upBuffer.Length);
        }

        public static async Task NetReadAsync(this Stream stream, byte[] buffer, int bytes, CancellationToken token = default)
        {
            int remaining = bytes;
            int offset = 0;
            while (remaining > 0 || token.IsCancellationRequested)
            {
                var read = await stream.ReadAsync(buffer, offset, remaining, token);
                if (token.IsCancellationRequested) return;
                remaining -= read;
                offset += read;
            }
        }

        public static async Task NetReadAsync(this Stream stream, byte[] buffer, CancellationToken token = default)
        {
            await stream.NetReadAsync(buffer, buffer.Length, token);
        }
    }
}
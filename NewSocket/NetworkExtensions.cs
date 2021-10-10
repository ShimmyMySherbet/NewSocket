using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace NewSocket
{
    public static class NetworkExtensions
    {
        public static byte NetReadByte(this Stream stream)
        {
            var byt = stream.ReadByte();
            if (byt == -1)
                throw new SocketException((int)SocketError.NoData);
            return (byte)byt;
        }

        public static async Task<short> NetReadInt16(this Stream stream)
        {
            var buffer = new byte[2];
            await stream.NetReadAsync(buffer);
            return BitConverter.ToInt16(buffer);
        }

        public static async Task<ushort> NetReadUInt16(this Stream stream)
        {
            var buffer = new byte[2];
            await stream.NetReadAsync(buffer);
            return BitConverter.ToUInt16(buffer);
        }

        public static async Task<int> NetReadInt32(this Stream stream)
        {
            var buffer = new byte[4];
            await stream.NetReadAsync(buffer);
            return BitConverter.ToInt32(buffer);
        }

        public static async Task<uint> NetReadUInt32(this Stream stream)
        {
            var buffer = new byte[4];
            await stream.NetReadAsync(buffer);
            return BitConverter.ToUInt32(buffer);
        }

        public static async Task<long> NetReadInt64(this Stream stream)
        {
            var buffer = new byte[8];
            await stream.NetReadAsync(buffer);
            return BitConverter.ToInt64(buffer);
        }

        public static async Task<ulong> NetReadUInt64(this Stream stream)
        {
            var buffer = new byte[8];
            await stream.NetReadAsync(buffer);
            return BitConverter.ToUInt64(buffer);
        }

        public static async Task<float> NetReadFloat(this Stream stream)
        {
            var buffer = new byte[8];
            await stream.NetReadAsync(buffer);
            return BitConverter.ToSingle(buffer);
        }

        public static async Task<double> NetReadDouble(this Stream stream)
        {
            var buffer = new byte[8];
            await stream.NetReadAsync(buffer);
            return BitConverter.ToDouble(buffer);
        }

        public static async Task<char> NetReadChar(this Stream stream)
        {
            var buffer = new byte[2];
            await stream.NetReadAsync(buffer);
            return BitConverter.ToChar(buffer);
        }

        public static async Task<string> NetReadString(this Stream stream, uint length)
        {
            var buffer = new byte[length];
            await stream.NetReadAsync(buffer);
            return Encoding.UTF8.GetString(buffer);
        }

        public static async Task<string> NetReadString(this Stream stream)
        {
            var length = await stream.NetReadUInt32();
            var buffer = new byte[length];
            await stream.NetReadAsync(buffer);
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
            await stream.WriteAsync(BitConverter.GetBytes(value));
        }

        public static async Task Write(this Stream stream, ushort value)
        {
            await stream.WriteAsync(BitConverter.GetBytes(value));
        }

        public static async Task Write(this Stream stream, int value)
        {
            await stream.WriteAsync(BitConverter.GetBytes(value));
        }

        public static async Task Write(this Stream stream, uint value)
        {
            await stream.WriteAsync(BitConverter.GetBytes(value));
        }

        public static async Task Write(this Stream stream, long value)
        {
            await stream.WriteAsync(BitConverter.GetBytes(value));
        }

        public static async Task Write(this Stream stream, ulong value)
        {
            await stream.WriteAsync(BitConverter.GetBytes(value));
        }

        public static async Task Write(this Stream stream, float value)
        {
            await stream.WriteAsync(BitConverter.GetBytes(value));
        }

        public static async Task Write(this Stream stream, double value)
        {
            await stream.WriteAsync(BitConverter.GetBytes(value));
        }

        public static async Task Write(this Stream stream, bool value)
        {
            await stream.WriteAsync(BitConverter.GetBytes(value));
        }

        public static async Task Write(this Stream stream, string? value, bool includeHeader = true)
        {
            var upBuffer = Encoding.UTF8.GetBytes(value ?? "");
            if (includeHeader)
            {
                await stream.Write((uint)upBuffer.Length);
            }
            await stream.WriteAsync(upBuffer);
        }

        public static async Task NetReadAsync(this Stream stream, byte[] buffer, int bytes)
        {
            int remaining = bytes;
            int offset = 0;
            while (remaining > 0)
            {
                var read = await stream.ReadAsync(buffer, offset, remaining);
                remaining -= read;
                offset += read;
            }
        }

        public static async Task NetReadAsync(this Stream stream, byte[] buffer)
        {
            await stream.NetReadAsync(buffer, buffer.Length);
        }
    }
}
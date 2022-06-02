using System;
using System.IO;
using System.Threading.Tasks;

namespace NewSocket
{
    public static class Extensions
    {
        /// <summary>
        /// Discards the specified number of bytes from teh stream
        /// </summary>
        public static async Task ConsumeBytes(this Stream stream, long count)
        {
            var buffer = new byte[1024];
            var remainingBytes = count;

            while (remainingBytes > 0)
            {
                var block = Math.Min(buffer.Length, remainingBytes);
                await stream.ReadAsync(buffer, 0, (int)block);
            }
        }
    }
}
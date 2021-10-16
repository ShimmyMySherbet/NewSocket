using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace NewSocket.Models
{
    /// <summary>
    /// Wrapper class to allocate and manage Unmanaged memory as a stream.
    /// Allows for the memory to be force released. Can also be finalized by GC to avoid memory leaks
    /// </summary>
    public class MarshalAllocMemoryStream : UnmanagedMemoryStream
    {
        private IntPtr m_Handle;
        private bool m_Disposed = false;

        public MarshalAllocMemoryStream(int length)
        {
            m_Handle = Marshal.AllocHGlobal(length);
            unsafe
            {
                Initialize((byte*)m_Handle.ToPointer(), length, length, FileAccess.ReadWrite);
            }
        }

        public MarshalAllocMemoryStream(byte[] buffer)
        {
            m_Handle = Marshal.AllocHGlobal(buffer.Length);
            unsafe
            {
                Initialize((byte*)m_Handle.ToPointer(), buffer.Length, buffer.Length, FileAccess.ReadWrite);
            }
            Write(buffer, 0, buffer.Length);
            Position = 0;
        }

        ~MarshalAllocMemoryStream()
        {
            if (!m_Disposed)
            {
                FreeMemory();
            }
        }

#if NET5_0_OR_GREATER

        public override ValueTask DisposeAsync()
        {
            if (m_Disposed) return ValueTask.CompletedTask;
            m_Disposed = true;
            FreeMemory();
            return base.DisposeAsync();
        }

#endif

        public override void Close()
        {
            if (m_Disposed) return;
            m_Disposed = true;
            FreeMemory();
            base.Close();
        }

        public new void Dispose()
        {
            if (m_Disposed) return;
            m_Disposed = true;
            FreeMemory();
            base.Dispose();
        }

        private void FreeMemory() => Marshal.FreeHGlobal(m_Handle);
    }
}
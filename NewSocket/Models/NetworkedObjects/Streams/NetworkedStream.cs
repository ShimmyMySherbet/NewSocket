using System;
using System.Data;
using System.IO;
using NewSocket.Core;
using Newtonsoft.Json;

namespace NewSocket.Models.NetworkedObjects.Streams
{
    public class NetworkedStream : Stream, INetworkedObject
    {
        private bool m_Readable = false;
        private bool m_Writable = false;
        private long m_Length = 0;
        private EStreamMode m_Mode;

        private ulong m_NetID { get; set; }

        public EStreamMode Mode
        {
            get => m_Mode;
            set
            {
                if (Client != null)
                    throw new ReadOnlyException("Cannot write EStreamMode after the stream is initialized.");
                m_Mode = value;
            }
        }




        [JsonIgnore]
        public override bool CanRead => m_Readable;

        [JsonIgnore]
        public override bool CanSeek => false;

        [JsonIgnore]
        public override bool CanWrite => m_Writable;

        [JsonIgnore]
        public override long Length => m_Length;

        [JsonIgnore]
        public override long Position { get; set; }
        [JsonIgnore]

        public NewSocketClient? Client { get; private set; }

        public override void Flush()
        {
            throw new NotImplementedException();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (!CanRead)
            {
                throw new InvalidOperationException("Cannot read a write-only NetworkedStream");
            }



        }

        public void RecieveClient(NewSocketClient client)
        {
            if (Client != null)
            {
                Client = client;
            }
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new InvalidOperationException("Cannot seek a NetworkedStream");
        }

        public override void SetLength(long value)
        {
            throw new InvalidOperationException("Cannot set length of a NetworkedStream");
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            if (!CanWrite)
            {
                throw new InvalidOperationException("Cannot write a read-only NetworkedStream");
            }
            throw new NotImplementedException();
        }
    }
}
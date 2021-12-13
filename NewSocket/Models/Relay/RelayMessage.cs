using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewSocket.Models.Relay
{
    public class RelayMessage : IDisposable
    {
        public string Header { get; }

        public MarshalAllocMemoryStream Data { get; }

        public RelayMessage(string messageTag, MarshalAllocMemoryStream data)
        {
            Header = messageTag;
            Data = data;
        }

        public byte[] ToArray()
        {
            Data.Position = 0;
            var byt = new byte[Data.Length];
            Data.Read(byt, 0, byt.Length);
            return byt;
        }

        public void Dispose()
        {
            Data?.Dispose();
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NewSocket.Interfaces;

namespace NewSocket.Protocals.NetSynced
{
    public class NetSyncedDown : IMessageDown
    {
        public bool WantsToDispatch { get; }
        public ulong MessageID { get; }
        public byte MessageType { get; }
        public bool Complete { get; }

        public Task Dispatch()
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public Task<bool> Read(Stream stream, CancellationToken token)
        {
            throw new NotImplementedException();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NewSocket.Core;
using NewSocket.Interfaces;
using NewSocket.Models;

namespace NewSocket.Protocals.NetSynced
{
    public class NetSyncedProtocol : IMessageProtocal
    {
        public byte ID { get; }

        public Task<IMessageDown> CreateDown(ulong messageID, BaseSocketClient client)
        {
            throw new NotImplementedException();
        }

        public Task OnSocketDisconnect(DisconnectContext context)
        {
            throw new NotImplementedException();
        }
    }
}

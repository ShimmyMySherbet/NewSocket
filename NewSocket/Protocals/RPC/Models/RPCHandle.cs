using NewSocket.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewSocket.Protocals.RPC.Models
{
    public class RPCHandle
    {
        public ulong MessageID { get; }
        public ulong RPCID { get; }
        public AsyncWaitHandle<RPCData> Handle { get; }

        public RPCHandle(ulong messageID, ulong rpcID)
        {
            MessageID = messageID;
            RPCID = rpcID;
            Handle = new AsyncWaitHandle<RPCData>();
        }
    }
}

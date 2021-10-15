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

        public string RPCName { get; }

        public RPCHandle(ulong messageID, ulong rpcID, string name)
        {
            MessageID = messageID;
            RPCID = rpcID;
            Handle = new AsyncWaitHandle<RPCData>();
            RPCName = name;
        }
        public void Release(RPCData data)
        {
            Handle.Release(data);
        }

        public async Task<RPCData> WaitAsync()
        {
            return await Handle.WaitAsync();
        }
    }
}

using NewSocket.Protocals.RPC.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewSocket.Protocals.RPC.Models.Registry
{
    public class RPCRequestRegistry : IRPCRequestRegistry
    {
        private ConcurrentDictionary<ulong, RPCHandle> m_RequestHandles = new ConcurrentDictionary<ulong, RPCHandle>();

        public void RegisterRequest(RPCHandle handle)
        {
            m_RequestHandles[handle.RPCID] = handle;
        }

        public void ReleaseRequest(ulong RPCID, RPCData data)
        {
            if (m_RequestHandles.Remove(RPCID, out var handle))
            {
                handle.Release(data);
            }
        }
    }
}

using System.Collections.Concurrent;
using System.Collections.Generic;

namespace NewSocket.Protocals.RPC.Models
{
    public class RPCRequestRegistry
    {
        private ConcurrentDictionary<ulong, RPCHandle> m_RequestHandles = new ConcurrentDictionary<ulong, RPCHandle>();

        public void RegisterRequest(RPCHandle handle)
        {
            m_RequestHandles[handle.RPCID] = handle;
        }

        public void ReleaseRequest(ulong RPCID, RPCParameters response)
        {
            if (m_RequestHandles.Remove(RPCID, out var handle))
            {
                handle.Handle.Release(response);
            }
        }
    }
}
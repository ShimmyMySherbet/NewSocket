using System.Collections.Concurrent;
using NewSocket.Models;
using NewSocket.Protocals.RPC.Interfaces;

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
            if (m_RequestHandles.TryRemove(RPCID, out var handle))
            {
                handle.Release(data);
            }
        }

        public void SendShutdown(DisconnectContext context)
        {
        }
    }
}
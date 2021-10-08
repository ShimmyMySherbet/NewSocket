using NewSocket.Protocals.RPC.Models;

namespace NewSocket.Protocals.RPC.Interfaces
{
    public interface IRPCRequestRegistry
    {
        void RegisterRequest(RPCHandle handle);

        void ReleaseRequest(ulong RPCID, RPCData data);
    }
}
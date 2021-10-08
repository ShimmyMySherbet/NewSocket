using NewSocket.Models;

namespace NewSocket.Protocals.RPC.Models
{
    public class RPCHandle
    {
        public ulong MessageID { get; }
        public ulong RPCID { get; }
        public AsyncWaitHandle<RPCParameters> Handle { get; }

        public RPCHandle(ulong messageID, ulong rpcID)
        {
            MessageID = messageID;
            RPCID = rpcID;
            Handle = new AsyncWaitHandle<RPCParameters>();
        }
    }
}
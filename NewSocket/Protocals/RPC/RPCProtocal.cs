using NewSocket.Core;
using NewSocket.Interfaces;
using System.Threading.Tasks;

namespace NewSocket.Protocals.RPC
{
    public class RPCProtocal : IMessageProtocal
    {
        public byte ID => 1;

        public Task<IMessageDown> CreateDown(ulong messageID, BaseSocketClient client)
        {
            return Task.FromResult((IMessageDown)new RPCDown(messageID));
        }

        public IMessageUp CreateRPCCall(BaseSocketClient client, string method, params object[] parameters)
        {
            var msg = new RPCUp(client.UpIDAssigner.AssignID(), method, parameters, client.UpBufferSize);
            return msg;
        }

        public IMessageUp CreateRPCResponse(BaseSocketClient client, ulong replyingTo, string method, object response)
        {
            var msg = new RPCUp(client.UpIDAssigner.AssignID(), method, replyingTo, response, client.UpBufferSize);
            return msg;
        }
    }
}
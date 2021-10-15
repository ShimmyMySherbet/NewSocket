using NewSocket.Core;
using NewSocket.Interfaces;
using NewSocket.Models;
using NewSocket.Protocals.RPC;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace NewSocket.Protocals.OTP
{
    public class ObjectTransferProtocal : IMessageProtocal
    {
        public byte ID => 0;

        public event OTPMessageRecivedArgs? MessageRecieved;

        public Task<IMessageDown> CreateDown(ulong messageID, BaseSocketClient client)
        {
            IMessageDown msg = new ObjectTransferDown(messageID, client, this);
            return Task.FromResult(msg);
        }

        public IMessageUp CreateUp(ulong messageID, BaseSocketClient client, string channel, object obj)
        {
            return new ObjectTransferUp(messageID, client, channel, obj);
        }

        public IMessageUp CreateUp(ulong messageID, BaseSocketClient client, string channel, Stream stream)
        {
            return new ObjectTransferUp(messageID, client, channel, stream);
        }

        public Task OnSocketDisconnect(DisconnectContext context)
        {
            return Task.CompletedTask;
        }

        internal Task RaiseMessage(string channel, Stream stream)
        {
            if (MessageRecieved != null)
            {
                ThreadPool.QueueUserWorkItem(async (_) => await MessageRecieved(channel, stream));
            }
            return Task.CompletedTask;
        }
    }
}
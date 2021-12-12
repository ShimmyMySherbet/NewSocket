using NewSocket.Models;
using NewSocket.Protocals.RPC;
using System.IO;

namespace NewSocket.Core
{
    public class NewSocketClient : BaseSocketClient
    {
        public EClientRole Role { get; }
        public bool RPCEnabled { get; }
        public RPCProtocal? RPC { get; }

        public NewSocketClient(Stream network, SocketClientConfig config) : base(network)
        {
            DownBufferSize = config.DownBufferSize;
            UpBufferSize = config.UpBufferSize;
            UpTransferSize = config.UpTransferSize;
            RPCEnabled = config.RPCEnabled;
            Role = config.Role;
            AllowPartialSocket = config.PartialSocket;
            AllowSocketReuse = config.AllowSocketReuse;
            AllowQueueForReuse = config.AllowSocketReuse;
            AllowQueueBeforeStart = config.AllowQueueBeforeStart;

            if (config.MessageScheduler != null)
            {
                m_MessageScheduler = config.MessageScheduler;
            }

            foreach (var protocal in config.Protocals)
            {
                m_Protocals[protocal.ID] = protocal;
            }

            if (RPCEnabled)
            {
                RPC = RegisterProtocal(new RPCProtocal(this));
            }
            else
            {
                RPC = new NullRPCProtocal(this);
            }
        }
    }
}
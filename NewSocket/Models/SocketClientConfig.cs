using NewSocket.Interfaces;
using System.Collections.Generic;

namespace NewSocket.Models
{
    public class SocketClientConfig
    {
        public EClientRole Role { get; init; }
        public bool RPCEnabled { get; init; } = true;
        public bool OTPEnabled { get; init; } = true;

        public bool PartialSocket { get; init; } = false;
        public int DownBufferSize { get; init;  } = 1024 * 2;
        public int UpBufferSize { get; init;  } = 1024 * 2;
        public int UpTransferSize { get; init;  } = 1024 * 20;
        public List<IMessageProtocal> Protocals { get; } = new List<IMessageProtocal>();

        public IScheduler<IMessageUp>? MessageScheduler { get; } = null;

        public SocketClientConfig(EClientRole role = EClientRole.Any, bool rpcEnabled = false)
        {
            Role = role;
            RPCEnabled = RPCEnabled;
        }
    }
}
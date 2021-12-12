using NewSocket.Interfaces;
using System.Collections.Generic;

namespace NewSocket.Models
{
    public class SocketClientConfig
    {
        public EClientRole Role { get; set; }
        public bool RPCEnabled { get; set; } = true;
        public bool OTPEnabled { get; set; } = true;

        public bool PartialSocket { get; set; } = false;
        public int DownBufferSize { get; set; } = 1024 * 2;
        public int UpBufferSize { get; set; } = 1024 * 2;
        public int UpTransferSize { get; set; } = 1024 * 20;
        public bool AllowSocketReuse { get; set; } = false;
        public bool AllowQueueBeforeStart { get; set; } = false;

        public List<IMessageProtocal> Protocals { get; } = new List<IMessageProtocal>();

        public IScheduler<IMessageUp>? MessageScheduler { get; } = null;

        public SocketClientConfig(EClientRole role = EClientRole.Any, bool rpcEnabled = true)
        {
            Role = role;
            RPCEnabled = RPCEnabled;
        }
    }
}
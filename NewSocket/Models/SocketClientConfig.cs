using NewSocket.Interfaces;
using System.Collections.Generic;

namespace NewSocket.Models
{
    public class SocketClientConfig
    {
        public EClientRole Role { get; }
        public bool RPCEnabled { get; } = true;
        public bool OTPEnabled { get; } = true;

        public bool PartialSocket { get; } = false;
        public int DownBufferSize { get; } = 1024 * 2;
        public int UpBufferSize { get; } = 1024 * 2;
        public int UpTransferSize { get; } = 1024 * 20;
        public List<IMessageProtocal> Protocals { get; } = new List<IMessageProtocal>();

        public IScheduler<IMessageUp> MessageScheduler { get; } = null;

        public SocketClientConfig(EClientRole role = EClientRole.Any)
        {
            Role = role;
        }
    }
}
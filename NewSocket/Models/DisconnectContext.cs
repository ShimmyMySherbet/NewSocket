using NewSocket.Core;
using NewSocket.Interfaces;
using System;

namespace NewSocket.Models
{
    public struct DisconnectContext
    {
        public bool Unexpected { get; }
        public bool IsFaulted { get; }

        public Exception? Exception { get; }

        public EChannelDirection Stream { get; }

        public BaseSocketClient Socket { get; }

        public DisconnectContext(BaseSocketClient client,  bool unexpected, EChannelDirection stream, Exception? exception = null)
        {
            Socket = client;
            Unexpected = unexpected;
            IsFaulted = exception != null;
            Exception = exception;
            Stream = stream;
        }
    }
}
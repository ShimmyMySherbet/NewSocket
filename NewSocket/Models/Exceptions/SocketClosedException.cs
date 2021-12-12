using System;

namespace NewSocket.Models.Exceptions
{
    public sealed class SocketClosedException : Exception
    {
        public override string Message => "The socket has been closed";
    }
}
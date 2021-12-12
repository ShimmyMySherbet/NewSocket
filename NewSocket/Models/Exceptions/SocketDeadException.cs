using System;

namespace NewSocket.Models.Exceptions
{
    public sealed class SocketDeadException : Exception
    {
        public override string Message => "The socket is dead. The socket has been disconnected and is not marked for re-use.";
    }
}
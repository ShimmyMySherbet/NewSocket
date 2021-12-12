using System;

namespace NewSocket.Models.Exceptions
{
    public sealed class SocketNotStartedException : Exception
    {
        public override string Message => "The socket has pre-start message queuing disabled, and has not yet started.";
    }
}
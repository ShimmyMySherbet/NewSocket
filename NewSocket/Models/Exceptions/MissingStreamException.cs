using System;

namespace NewSocket.Models.Exceptions
{
    public sealed class MissingStreamException : Exception
    {
        public MissingStreamException(ESocketStream stream) : base($"Socket missing strem/s: {stream}.")
        {
        }
    }
}
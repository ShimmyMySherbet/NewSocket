using System;

namespace NewSocket.Security.Models
{
    public class AuthenticationFailedException : Exception
    {
        public AuthenticationFailedException()
        {
        }

        public AuthenticationFailedException(Exception underlying) : base("Authentication to remote server failed", underlying)
        {
        }

        public AuthenticationFailedException(string message) : base(message)
        {
        }

        public AuthenticationFailedException(string message, Exception underlying) : base(message, underlying)
        {
        }
    }
}
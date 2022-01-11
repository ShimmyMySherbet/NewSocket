using System;

namespace NewSocket.Security.Models
{
    /// <summary>
    /// Indicates that authentication to remote server failed.
    /// This could indicate differing security protocols, an invalid password/certificate
    /// Or, it could potentially indicate a bad actor tampering with the connection.
    /// </summary>
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
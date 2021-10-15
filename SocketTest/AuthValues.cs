using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocketTest
{
    public class AuthValues
    {
        public string Username = "";
        public string Password = "";

        public AuthValues(string username, string password)
        {
            Username = username;
            Password = password;
        }

        public AuthValues()
        {
        }
    }
}

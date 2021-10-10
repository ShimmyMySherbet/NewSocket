using NewSocket.Interfaces;
using NewSocket.Security.Interfaces;

namespace NewSocket.Security.Models
{
    public delegate void AuthorizationCallback(bool authorized, ISecurityProtocal protocal, ISocketClient client);
}
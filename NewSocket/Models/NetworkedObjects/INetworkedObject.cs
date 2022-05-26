using NewSocket.Core;

namespace NewSocket.Models.NetworkedObjects
{
    public interface INetworkedObject
    {
        void RecieveClient(NewSocketClient client);
    }
}
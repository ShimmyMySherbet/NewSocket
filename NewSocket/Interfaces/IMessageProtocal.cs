using NewSocket.Core;
using NewSocket.Models;
using System.Threading.Tasks;

namespace NewSocket.Interfaces
{
    public interface IMessageProtocal
    {
        byte ID { get; }

        Task<IMessageDown> CreateDown(ulong messageID, BaseSocketClient client);

        Task OnSocketDisconnect(DisconnectContext context);
    }
}
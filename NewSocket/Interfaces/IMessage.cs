using System.Threading.Tasks;

namespace NewSocket.Interfaces
{
    public interface IMessage
    {
        ulong MessageID { get; }
        byte MessageType { get; }
        bool Complete { get; }
    }
}
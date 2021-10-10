using NewSocket.Models;

namespace NewSocket.Interfaces
{
    public interface ISocketClient
    {
        int DownBufferSize { get; }
        int UpBufferSize { get; }
        int UpTransferSize { get; }
        IDAssigner MessageIDAssigner { get; }

        void Enqueue(IMessageUp message);

    }
}
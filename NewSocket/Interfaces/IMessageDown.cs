using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace NewSocket.Interfaces
{
    public interface IMessageDown : IDisposable, IMessage
    {
        bool WantsToDispatch { get; }

        Task<bool> Read(Stream stream, CancellationToken token);

        Task Dispatch();
    }
}
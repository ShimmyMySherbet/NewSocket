using System.IO;
using System.Threading.Tasks;
using NewSocket.Core;

namespace NewSocket.Security.Interfaces
{
    public interface ISecurityProtocal
    {
        bool Authenticated { get; }
        bool Preauthenticate { get; }

        Stream? GetDownStream(Stream networkDown);

        Stream? GetUpStream(Stream networkUp);

        /// <summary>
        /// Preauthenticates to the remote client before switching into the NewSocket messaging protocol.
        /// </summary>
        Task Authenticate(Stream network);

        Task OnSocketStarted(BaseSocketClient socket);
    }
}
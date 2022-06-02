using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewSocket.Interfaces
{
    public interface IMessageUp : IDisposable, IMessage
    {
        bool WantsToWrite { get; }
        Task<bool> Write(Stream stream);
    }
}

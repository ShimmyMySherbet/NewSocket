using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewSocket.Models
{
    public enum ENetSyncedMode : byte
    {
        Read = 0,
        Write = 1,
        ReadWrite = 2
    }
}

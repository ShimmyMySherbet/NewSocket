﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NewSocket.Interfaces;

namespace NewSocket.Protocals.NetSynced
{
    public class NetSyncedUp : IMessageUp
    {
        public ulong MessageID { get; }
        public byte MessageType { get; }
        public bool Complete { get; }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public Task<bool> Write(Stream stream)
        {
            throw new NotImplementedException();
        }
    }
}

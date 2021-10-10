﻿using NewSocket.Models;
using NewSocket.Protocals.RPC;
using System.IO;

namespace NewSocket.Core
{
    public class SocketClient : BaseSocketClient
    {
        public EClientRole Role { get; }
        public bool RPCEnabled { get; }
        public RPCProtocal RPC { get; }
        public SocketClient(Stream network, SocketClientConfig config) : base(network)
        {
            DownBufferSize = config.DownBufferSize;
            UpBufferSize = config.UpBufferSize;
            UpTransferSize = config.UpTransferSize;
            RPCEnabled = config.RPCEnabled;
            Role = config.Role;
            AllowPartialSocket = config.PartialSocket;

            if (config.MessageScheduler != null)
            {
                m_MessageScheduler = config.MessageScheduler;
            }

            foreach (var protocal in config.Protocals)
            {
                m_Protocals[protocal.ID] = protocal;
            }

            if (RPCEnabled)
            {
                RPC = RegisterProtocal(new RPCProtocal(this));
            }
        }
    }
}
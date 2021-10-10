﻿using NewSocket.Core;
using NewSocket.Interfaces;
using NewSocket.Protocals.RPC.Models;
using System;
using System.Threading.Tasks;

namespace NewSocket.Protocals.RPC
{
    public class NullRPCProtocal : RPCProtocal
    {
        public override byte ID => byte.MaxValue;

        public NullRPCProtocal(ISocketClient socketClient) : base(socketClient)
        {
        }

        public override Task<IMessageDown> CreateDown(ulong messageID, BaseSocketClient client)
        {
            throw new InvalidOperationException("This socket doesn't support the RPC protocal.");
        }

        public override IMessageUp CreateRPCCall(string method, out RPCHandle handle, params object?[] parameters)
        {
            throw new InvalidOperationException("This socket doesn't support the RPC protocal.");
        }

        public override IMessageUp CreateRPCResponse(ulong parentRPCID)
        {
            throw new InvalidOperationException("This socket doesn't support the RPC protocal.");
        }

        public override IMessageUp CreateRPCResponse(ulong parentRPCID, object? response)
        {
            throw new InvalidOperationException("This socket doesn't support the RPC protocal.");
        }

        public override void DispatchRPC(ulong RPCID, string? method, RPCData parameters)
        {
            throw new InvalidOperationException("This socket doesn't support the RPC protocal.");
        }

        public override void RegisterFrom<T>(T instance)
        {
            throw new InvalidOperationException("This socket doesn't support the RPC protocal.");
        }

        public override void Subscribe(string name, Delegate handler)
        {
            throw new InvalidOperationException("This socket doesn't support the RPC protocal.");
        }
    }
}
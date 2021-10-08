using NewSocket.Core;
using NewSocket.Interfaces;
using NewSocket.Models;
using NewSocket.Protocals.RPC.Models;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace NewSocket.Protocals.RPC
{
    public class RPCProtocal : IMessageProtocal
    {
        public byte ID => 1;

        private IDAssigner RPCAssigner = new IDAssigner();

        public ISocketClient SocketClient { get; }

        public RPCRequestRegistry RequestRegistry = new RPCRequestRegistry();

        public RPCHandlerRegistry HandlerRegistry = new RPCHandlerRegistry();

        public RPCProtocal(ISocketClient socketClient)
        {
            SocketClient = socketClient;
        }

        public async Task<T> QueryAsync<T>(string method, params object[] parameters)
        {
            var msg = CreateRPCCall(method, out var handle, parameters: parameters);
            SocketClient.Enqueue(msg);

            var resp = await handle.Handle.WaitAsync();

            if (resp.Objects.Count > 0)
            {
                return resp.ReadObject<T>(0);
            }

            throw new InvalidOperationException("Remote RPC didn't return anything");
        }

        public Task<IMessageDown> CreateDown(ulong messageID, BaseSocketClient client)
        {
            return Task.FromResult((IMessageDown)new RPCDown(messageID, this));
        }

        public IMessageUp CreateRPCCall(string method, out RPCHandle handle, params object[] parameters)
        {
            handle = new RPCHandle(SocketClient.MessageIDAssigner.AssignID(), RPCAssigner.AssignID());
            RequestRegistry.RegisterRequest(handle);
            var msg = new RPCUp(SocketClient, handle, method, parameters);
            return msg;
        }

        public IMessageUp CreateRPCResponse(ulong parentRPCID, object response)
        {
            var context = new RPCHandle(SocketClient.MessageIDAssigner.AssignID(), parentRPCID);
            var msg = new RPCUp(SocketClient, context, response);
            return msg;
        }

        public IMessageUp CreateRPCResponse(ulong parentRPCID)
        {
            var context = new RPCHandle(SocketClient.MessageIDAssigner.AssignID(), parentRPCID);
            var msg = new RPCUp(SocketClient, context);
            return msg;
        }

        public void DispatchRPC(ulong RPCID, string method, RPCParameters parameters)
        {
            ThreadPool.QueueUserWorkItem(async (_) => await HandleRPC(RPCID, method, parameters));
        }

        private async Task HandleRPC(ulong id, string method, RPCParameters parameters)
        {
            var handler = HandlerRegistry.GetHandler(method);
            if (handler != null)
            {
                var paramList = new List<object>();
                if (parameters.Objects.Count >= handler.Parameters.Length)
                {
                    for (int i = 0; i < handler.Parameters.Length; i++)
                    {
                        var pType = handler.Parameters[i];
                        paramList.Add(parameters.ReadObject(i, pType));
                    }
                    try
                    {
                        var returnValue = await handler.Execute(paramList.ToArray());

                        if (handler.HasReturn)
                        {
                            var outBound = CreateRPCResponse(id, returnValue);
                            SocketClient.Enqueue(outBound);
                        }
                        return;
                    }
                    catch (System.Exception)
                    {
                        // TODO: Exception proxy logic
                    }
                }
            }
            var msg = CreateRPCResponse(id);
            SocketClient.Enqueue(msg);
        }
    }
}